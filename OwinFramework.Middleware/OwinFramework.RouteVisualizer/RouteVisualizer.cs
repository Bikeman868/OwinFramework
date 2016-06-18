using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using Svg;
using Svg.Transforms;

namespace OwinFramework.RouteVisualizer
{
    public class RouteVisualizer: IMiddleware<object>, IConfigurable, ISelfDocumenting, IAnalysable
    {
        private const float _textHeight = 12;
        private const float _textLineSpacing = 15;

        private const float _boxLeftMargin = 5;
        private const float _boxTopMargin = 5;

        private readonly IList<IDependency> _dependencies = new List<IDependency>();
        public IList<IDependency> Dependencies { get { return _dependencies; } }

        public string Name { get; set; }

        private IDisposable _configurationRegistration;
        private Configuration _configuration;

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            if (!string.IsNullOrEmpty(_configuration.Path) 
                && context.Request.Path.HasValue
                && string.Equals(context.Request.Path.Value, _configuration.Path, StringComparison.OrdinalIgnoreCase))
            {
                _requestCount++;
                return VisualizeRouting(context);
            }
            return next();
        }

        void IConfigurable.Configure(IConfiguration configuration, string path)
        {
            _configurationRegistration = configuration.Register(
                path, cfg => _configuration = cfg, new Configuration());
        }

        private Task VisualizeRouting(IOwinContext context)
        {
            var document = CreateDocument();

            SvgUnit width;
            SvgUnit height;
            DrawRoutes(document, context, 20, 20, out width, out height);

            SetDocumentSize(document, width, height);

            return Svg(context, document);
        }

        #region Creating SVG drawing

        protected SvgDocument CreateDocument()
        {
            var document = new SvgDocument
            {
                FontFamily = "Arial",
                FontSize = _textHeight
            };

            var styles = GetScriptResource("svg.css");
            if (!string.IsNullOrEmpty(styles))
            {
                var styleElement = new NonSvgElement("style");
                styleElement.Content = "\n" + styles;
                document.Children.Add(styleElement);
            }

            var script = GetScriptResource("svg.js");
            if (!string.IsNullOrEmpty(script))
            {
                document.CustomAttributes.Add("onload", "init(evt)");
                var scriptElement = new NonSvgElement("script");
                scriptElement.CustomAttributes.Add("type", "text/ecmascript");
                scriptElement.Content = "\n" + script;
                document.Children.Add(scriptElement);
            }

            return document;
        }

        private void SetDocumentSize(SvgDocument document, SvgUnit width, SvgUnit height)
        {
            document.Width = width;
            document.Height = height;
            document.ViewBox = new SvgViewBox(0, 0, width, height);
        }

        protected Task Svg(IOwinContext context, SvgDocument document)
        {
            try
            {
                string svg;
                using (var stream = new MemoryStream())
                {
                    document.Write(stream);
                    svg = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
                }

                if (string.IsNullOrEmpty(svg))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                    return context.Response.WriteAsync("");
                }

                context.Response.ContentType = "image/svg+xml";
                return context.Response.WriteAsync(svg);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return context.Response.WriteAsync("Exception serializing SVG response: " + ex.Message);
            }
        }

        private string GetScriptResource(string filename)
        {
            var scriptResourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(n => n.Contains(filename));
            if (scriptResourceName == null)
                throw new Exception("Failed to find embedded resource " + filename);
            
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(scriptResourceName))
            {
                if (stream == null)
                    throw new Exception("Failed to find embedded resource " + filename);

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private void DrawRoutes(
            SvgDocument document, 
            IOwinContext context, 
            SvgUnit x,
            SvgUnit y,
            out SvgUnit width, 
            out SvgUnit height)
        {
            var router = context.Get<IRouter>("OwinFramework.Router");
            if (router == null)
                throw new Exception("The route vizualizer can only be used if you used OwinFramework to build your OWIN pipeline.");

            var root = PositionRouter(router);
            root.X = x;
            root.Y = y;

            Arrange(root);
            Draw(document, root);

            width = root.X + root.TreeWidth + 50;
            height = root.Y + root.TreeHeight + 50;
        }

        void Arrange(Positioned root)
        {
            const float childHorizontalOffset = 20;
            const float childVericalSpacing = 10;
            const float siblingHorizontalSpacing = 10;

            root.TreeHeight = root.Height;
            root.TreeWidth = root.Width;

            var childX = root.X + childHorizontalOffset;
            var childY = root.Y + root.Height + childVericalSpacing;

            var siblingX = root.X + root.Width + siblingHorizontalSpacing;
            var siblingY = root.Y;

            var x = childX;
            var y = childY;
            foreach (var child in root.Children)
            {
                child.X = x;
                child.Y = y;
                Arrange(child);
                y += child.TreeHeight + childVericalSpacing;
                if (child.X + child.TreeWidth > root.X + root.TreeWidth)
                    root.TreeWidth = child.TreeWidth + child.X - root.X;
            }
            if (y - root.Y - childVericalSpacing > root.TreeHeight)
            root.TreeHeight = y - root.Y - childVericalSpacing;

            x = siblingX;
            y = siblingY;
            foreach (var sibling in root.Siblings)
            {
                sibling.X = x;
                sibling.Y = y;
                Arrange(sibling);
                x += sibling.TreeWidth + siblingHorizontalSpacing;
                if (sibling.Y + sibling.TreeHeight > root.Y + root.TreeHeight)
                    root.TreeHeight = sibling.TreeHeight + sibling.Y - root.Y;
            }
            if (x - root.X - siblingHorizontalSpacing > root.TreeWidth)
                root.TreeWidth = x - root.X - siblingHorizontalSpacing;
        }

        private void Draw(SvgDocument document, Positioned root)
        {
            root.DrawAction(document, root);

            foreach (var child in root.Children)
                Draw(document, child);

            foreach (var sibling in root.Siblings)
                Draw(document, sibling);
        }

        private class Positioned
        {
            public SvgUnit X;
            public SvgUnit Y;
            public SvgUnit Width;
            public SvgUnit Height;
            public SvgUnit TreeWidth;
            public SvgUnit TreeHeight;
            public Action<SvgDocument, Positioned> DrawAction;
            public IList<Positioned> Children = new List<Positioned>();
            public IList<Positioned> Siblings = new List<Positioned>();
        }

        private Positioned PositionRouter(IRouter router)
        {
            var positioned = new Positioned
            {
                Width = 120,
                Height = _textHeight * 4,
                DrawAction = (d, p) => 
                    {
                        DrawBox(
                            d, 
                            p.X, 
                            p.Y, 
                            p.Width,
                            new List<string> 
                            { 
                                "Router", 
                                router.Name ?? "<anonymous>" 
                            }, 
                            "router", 
                            2f);
                    }
            };

            if (router.Segments != null)
            {
                positioned.Children = router
                    .Segments
                    .Select(PositionSegment)
                    .ToList();
            }

            return positioned;
        }

        private Positioned PositionSegment(IRoutingSegment segment)
        {
            var lines = new List<string>
            {
                "Route: " + (segment.Name ?? "<anonymous>")
            };
            
            var positioned = new Positioned
            {
                Width = 160,
                Height = _textHeight * (lines.Count + 2),
                DrawAction = (d, p) =>
                    {
                        DrawBox(d,p.X,p.Y,p.Width,lines,"segment",2f);
                    }
            };

            if (segment.Middleware != null)
            {
                positioned.Siblings = segment
                    .Middleware
                    .Select(PositionMiddleware)
                    .ToList();
            }

            return positioned;
        }

        private Positioned PositionMiddleware(IMiddleware middleware)
        {
            var router = middleware as IRouter;
            if (router != null)
                return PositionRouter(router);

            var lines = new List<string>();

            lines.Add("Middleware " + middleware.GetType().Name);

            if (string.IsNullOrEmpty(middleware.Name))
                lines.Add("<anonymous>");
            else
                lines.Add("\"" + middleware.Name + "\"");

            var selfDocumenting = middleware as ISelfDocumenting;
            if (selfDocumenting != null)
            {
                lines.Add(selfDocumenting.ShortDescription);
            }

            var configurable = middleware as IConfigurable;
            if (configurable != null)
            {
                var line = "Configurable";
                if (selfDocumenting != null)
                {
                    var configurationUrl = selfDocumenting.GetDocumentation(DocumentationTypes.Configuration);
                    if (configurationUrl != null)
                        line += " (see " + configurationUrl + ")";
                }
                lines.Add(line);
            }

            var analysable = middleware as IAnalysable;
            if (analysable != null)
            {
                foreach (var stat in analysable.AvailableStatistics)
                {
                    var value  = analysable.GetStatistic(stat.Id);
                    if (value != null)
                    {
                        lines.Add(stat.Name + " " + value.Formatted);
                    }
                }
            }

            var longestLine = lines.Select(l => l.Length).Max();

            return new Positioned
            {
                Width = longestLine * 6.5f,
                Height = _textHeight * (lines.Count + 2),
                DrawAction = (d, p) =>
                    {
                        DrawBox(d, p.X, p.Y, p.Width, lines,"middleware", 2f);
                    }
            };
        }

        private void DrawLine(
            SvgDocument document,
            SvgUnit x1,
            SvgUnit y1,
            SvgUnit x2,
            SvgUnit y2,
            string cssClass)
        {
            var line = new SvgLine
            {
                StartX = x1,
                StartY = y1,
                EndX = x2,
                EndY = y2
            };
            if (!string.IsNullOrEmpty(cssClass))
                line.CustomAttributes.Add("class", cssClass);
            document.Children.Add(line);
        }

        private float DrawBox(
            SvgDocument document,
            SvgUnit x,
            SvgUnit y,
            SvgUnit width,
            IList<string> lines,
            string cssClass,
            SvgUnit cornerRadius)
        {
            var group = new SvgGroup();
            group.Transforms.Add(new SvgTranslate(x, y));

            if (!string.IsNullOrEmpty(cssClass))
                group.CustomAttributes.Add("class", cssClass);

            document.Children.Add(group);

            var height = _textLineSpacing * lines.Count + _boxTopMargin * 2;

            var rectangle = new SvgRectangle
            {
                Height = height,
                Width = width,
                CornerRadiusX = cornerRadius,
                CornerRadiusY = cornerRadius
            };
            group.Children.Add(rectangle);

            for (var lineNumber = 0; lineNumber < lines.Count; lineNumber++)
            {
                var text = new SvgText(lines[lineNumber]);
                text.Transforms.Add(new SvgTranslate(_boxLeftMargin, _textHeight + _textLineSpacing * lineNumber + _boxTopMargin));
                text.Children.Add(new SvgTextSpan());
                group.Children.Add(text);
            }

            return height;
        }

        #endregion

        #region ISelfDocumenting

        Uri ISelfDocumenting.GetDocumentation(DocumentationTypes documentationType)
        {
            switch (documentationType)
            {
                case DocumentationTypes.Configuration:
                    return new Uri("https://github.com/Bikeman868/OwinFramework");
            }
            return null;
        }

        string ISelfDocumenting.LongDescription
        {
            get { return "Allows you to visually inspect your OWIN pipeline configuration to ensure that your application will work as expected."; }
        }

        string ISelfDocumenting.ShortDescription
        {
            get { return "Produces an SVG visualization of the OWIN pipeline"; }
        }

        #endregion

        #region IAnalysable

        private volatile int _requestCount;

        IList<IStatisticInformation> IAnalysable.AvailableStatistics
        {
            get
            {
                return new List<IStatisticInformation>
                {
                    new StatisticInformation
                    {
                        Id = "RequestCount",
                        Name = "Total requests",
                        Description = "The total number of requests processed by the visualazer midldeware since the application was restarted",
                    }
                };
            }
        }

        IStatistic IAnalysable.GetStatistic(string id)
        {
            switch (id)
            {
                case "RequestCount":
                    return new RequestCountStatistic(this).Refresh();
            }
            return null;
        }

        private class StatisticInformation: IStatisticInformation
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Explanation { get; set; }
            public string Units { get; set; }
        }

        private abstract class Statistic: IStatistic
        {
            public float Value { get; protected set; }
            public float Denominator { get; protected set; }
            public string Formatted { get; protected set; }

            public abstract IStatistic Refresh();
        }

        private class RequestCountStatistic: Statistic
        {
            private readonly RouteVisualizer _routeVisualizer;

            public RequestCountStatistic(RouteVisualizer routeVisualizer)
            {
                _routeVisualizer = routeVisualizer;
            }

            public override IStatistic Refresh()
            {
                Value = _routeVisualizer._requestCount;
                Denominator = 1;

                if (Value < 2000)
                    Formatted = Value.ToString(CultureInfo.InvariantCulture);
                else if (Value < 2000000)
                    Formatted = (Value / 1000f).ToString("f1", CultureInfo.InvariantCulture) + "K";
                else
                    Formatted = (Value / 1000000f).ToString("f1", CultureInfo.InvariantCulture) + "M";

                return this;
            }
        }

        #endregion
    }
}
