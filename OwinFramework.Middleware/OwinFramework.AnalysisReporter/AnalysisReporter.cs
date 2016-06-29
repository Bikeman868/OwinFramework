﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Middleware;
using OwinFramework.Interfaces.Routing;
using Newtonsoft.Json.Linq;

namespace OwinFramework.AnalysisReporter
{
    public class AnalysisReporter:
        IMiddleware<object>, 
        IConfigurable, 
        ISelfDocumenting
    {
        private const string ConfigDocsPath = "/docs/configuration";

        private readonly IList<IDependency> _dependencies = new List<IDependency>();
        public IList<IDependency> Dependencies { get { return _dependencies; } }

        public string Name { get; set; }

        public AnalysisReporter()
        {
            this.RunAfter<IAuthorization>(null, false);
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            string path;
            if (!IsForThisMiddleware(context, out path))
                return next();

            if (context.Request.Path.Value.Equals(path, StringComparison.OrdinalIgnoreCase))
                return ReportAnalysis(context);

            if (context.Request.Path.Value.Equals(path + ConfigDocsPath, StringComparison.OrdinalIgnoreCase))
                return DocumentConfiguration(context);

            throw new Exception("This request looked like it was for the analysis reporter middleware, but the middleware did not know how to handle it.");
        }

        // Note that these two lists must be 1-1 and in the same order
        private enum ReportFormat 
        { 
            Html = 0, 
            Text = 1, 
            Markdown = 2, 
            Json = 3, 
            Xml = 4 
        }
        private readonly List<string> _supportedFormats = new List<string>()
        { 
            "text/html", 
            "text/plain", 
            "text/markdown", 
            "application/json", 
            "application/xml" 
        };
     
        private Task ReportAnalysis(IOwinContext context)
        {
            string mimeType = null;
            if (string.IsNullOrEmpty(context.Request.Accept))
            {
                mimeType = _configuration.DefaultFormat;
            }
            else
            {
                var acceptFormats = context.Request.Accept
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s));
                foreach (var acceptFormat in acceptFormats)
                {
                    if (acceptFormat == "*/*")
                    {
                        mimeType = _configuration.DefaultFormat;
                        break;
                    }
                    if (_supportedFormats.Contains(acceptFormat))
                    {
                        mimeType = acceptFormat;
                        break;
                    }
                }
            }

            if (mimeType == null)
            {
                context.Response.StatusCode = 406;
                context.Response.ReasonPhrase = "Not Acceptable";
                return context.Response.WriteAsync(
                    "The analysis reporter supports the following MIME types: " + 
                    string.Join(",", _supportedFormats));
            }
            context.Response.ContentType = mimeType;
            var reportFormat = (ReportFormat)_supportedFormats.IndexOf(mimeType);

            var analysis = GetAnalysisData(context);

            switch (reportFormat)
            {
                case ReportFormat.Html:
                    return RenderHtml(context, analysis);
                case ReportFormat.Text:
                    return RenderText(context, analysis);
                case ReportFormat.Markdown:
                    return RenderMarkdown(context, analysis);
                case ReportFormat.Json:
                    return RenderJson(context, analysis);
                case ReportFormat.Xml:
                    return RenderXml(context, analysis);
            }

            context.Response.StatusCode = 406;
            context.Response.ReasonPhrase = "Not Acceptable";
            return context.Response.WriteAsync("The " + reportFormat + " format is currently under development.");
        }

        private Task RenderText(IOwinContext context, IEnumerable<AnalysableInfo> analysisData)
        {
            var pageTemplate = GetScriptResource("pageTemplate.txt");
            var analysableTemplate = GetScriptResource("analysableTemplate.txt");
            var statisticTemplate = GetScriptResource("statisticTemplate.txt");

            return RenderTemplates(context, analysisData, pageTemplate, analysableTemplate, statisticTemplate);
        }

        private Task RenderMarkdown(IOwinContext context, IEnumerable<AnalysableInfo> analysisData)
        {
            var pageTemplate = GetScriptResource("pageTemplate.md");
            var analysableTemplate = GetScriptResource("analysableTemplate.md");
            var statisticTemplate = GetScriptResource("statisticTemplate.md");

            return RenderTemplates(context, analysisData, pageTemplate, analysableTemplate, statisticTemplate);
        }

        private Task RenderHtml(IOwinContext context, IEnumerable<AnalysableInfo> analysisData)
        {
            var pageTemplate = GetScriptResource("pageTemplate.html");
            var analysableTemplate = GetScriptResource("analysableTemplate.html");
            var statisticTemplate = GetScriptResource("statisticTemplate.html");

            return RenderTemplates(context, analysisData, pageTemplate, analysableTemplate, statisticTemplate);
        }

        private Task RenderTemplates(
            IOwinContext context, 
            IEnumerable<AnalysableInfo> analysisData,
            string pageTemplate,
            string analysableTemplate,
            string statisticTemplate)
        {
            var analysablesContent = new StringBuilder();
            var statisticsContent = new StringBuilder();

            foreach (var analysable in analysisData)
            {
                statisticsContent.Clear();
                foreach (var statistic in analysable.Statistics)
                {
                    statistic.Statistic.Refresh();
                    var statisticHtml = statisticTemplate
                        .Replace("{name}", statistic.Name)
                        .Replace("{units}", statistic.Units)
                        .Replace("{description}", statistic.Description)
                        .Replace("{value}", statistic.Statistic.Formatted);
                    statisticsContent.AppendLine(statisticHtml);
                }

                var analysableHtml = analysableTemplate
                    .Replace("{name}", analysable.Name)
                    .Replace("{type}", analysable.Type)
                    .Replace("{description}", analysable.Description)
                    .Replace("{statistics}", statisticsContent.ToString());
                analysablesContent.AppendLine(analysableHtml);
            }

            return context.Response.WriteAsync(pageTemplate.Replace("{analysables}", analysablesContent.ToString()));
        }

        private Task RenderJson(IOwinContext context, IEnumerable<AnalysableInfo> analysisData)
        {
            var json = new JObject();
            var analysablesArray = new JArray();
            json.Add("middleware", analysablesArray);

            foreach (var analysable in analysisData)
            {
                var analysableJson = new JObject();
                analysableJson.Add("name", analysable.Name);
                analysableJson.Add("type", analysable.Type);
                analysableJson.Add("description", analysable.Description);
                analysablesArray.Add(analysableJson);

                var statisticsArray = new JArray();
                analysableJson.Add("statistics", statisticsArray);

                foreach (var statistic in analysable.Statistics)
                {
                    statistic.Statistic.Refresh();
                    var statisticJson = new JObject();
                    statisticJson.Add("name", statistic.Name);
                    statisticJson.Add("units", statistic.Units);
                    statisticJson.Add("description", statistic.Description);
                    statisticJson.Add("value", statistic.Statistic.Value);
                    statisticJson.Add("denominator", statistic.Statistic.Denominator);
                    statisticJson.Add("formatted", statistic.Statistic.Formatted);
                    statisticsArray.Add(statisticJson);
                }
            }

            return context.Response.WriteAsync(json.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        private Task RenderXml(IOwinContext context, IEnumerable<AnalysableInfo> analysisData)
        {
            var document = new XDocument(new XDeclaration("1.0", "utf-8", "true"));
            var rootElement = new XElement("Analytics");
            document.Add(rootElement);

            foreach (var analysable in analysisData)
            {
                var middlewareElement = new XElement(
                    "Middleware",
                    new XAttribute("name", analysable.Name),
                    new XAttribute("type", analysable.Type),
                    new XElement("Description", analysable.Description)
                );
                rootElement.Add(middlewareElement);

                var statisticsElement = new XElement("Statistics");
                middlewareElement.Add(statisticsElement);

                foreach (var statistic in analysable.Statistics)
                {
                    statistic.Statistic.Refresh();

                    statisticsElement.Add(
                        new XElement(
                            "Statistic",
                            new XAttribute("name", statistic.Name),
                            new XElement("units", statistic.Units ?? ""),
                            new XElement("value", statistic.Statistic.Value),
                            new XElement("denominator", statistic.Statistic.Denominator),
                            new XElement("formatted", statistic.Statistic.Formatted),
                            new XElement("description", statistic.Description)));
                }
            }

            return context.Response.WriteAsync(document.ToString(SaveOptions.None));
        }

        #region Gathering analysis information

        private IList<AnalysableInfo> _stats;

        private IList<AnalysableInfo> GetAnalysisData(IOwinContext context)
        {
            if (_stats != null) return _stats;

            var router = context.Get<IRouter>("OwinFramework.Router");
            if (router == null)
                throw new Exception("The analysis reporter can only be used if you used OwinFramework to build your OWIN pipeline.");

            var stats = new List<AnalysableInfo>();
            AddStats(stats, router);

            _stats = stats;
            return stats;
        }

        private void AddStats(IList<AnalysableInfo> stats, IRouter router)
        {
            if (router.Segments != null)
            {
                foreach (var segment in router.Segments)
                {
                    if (segment.Middleware != null)
                    {
                        foreach (var middleware in segment.Middleware)
                        {
                            AddStats(stats, middleware);
                        }
                    }
                }
            }
        }

        private void AddStats(IList<AnalysableInfo> stats, IMiddleware middleware)
        {
            var analysable = middleware as IAnalysable;
            if (analysable != null)
            {
                var analysableInfo = new AnalysableInfo
                {
                    Name = middleware.Name,
                    Type = middleware.GetType().FullName,
                    Statistics = new List<StatisticInfo>()
                };
                stats.Add(analysableInfo);

                var selfDocumenting = middleware as ISelfDocumenting;
                if (selfDocumenting != null)
                {
                    analysableInfo.Description = selfDocumenting.ShortDescription;
                }

                foreach (var availableStatistic in analysable.AvailableStatistics)
                {
                    var statisticInfo = new StatisticInfo
                    {
                        Name = availableStatistic.Name,
                        Description = availableStatistic.Description,
                        Units = availableStatistic.Units,
                        Statistic = analysable.GetStatistic(availableStatistic.Id)
                    };
                    analysableInfo.Statistics.Add(statisticInfo);
                }
            }

            var router = middleware as IRouter;
            if (router != null) AddStats(stats, router);
        }

        private class AnalysableInfo
        {
            public string Name;
            public string Description;
            public string Type;
            public List<StatisticInfo> Statistics;
        }

        private class StatisticInfo
        {
            public string Name;
            public string Units;
            public string Description;
            public IStatistic Statistic;
        }

        #endregion

        #region IConfigurable

        private IDisposable _configurationRegistration;
        private Configuration _configuration = new Configuration();

        private bool IsForThisMiddleware(IOwinContext context, out string path)
        {
            // Note that the configuration can be changed at any time by another thread
            path = _configuration.Path;

            return _configuration.Enabled
                   && !string.IsNullOrEmpty(path)
                   && context.Request.Path.HasValue
                   && context.Request.Path.Value.StartsWith(path, StringComparison.OrdinalIgnoreCase);
        }

        void IConfigurable.Configure(IConfiguration configuration, string path)
        {
            _configurationRegistration = configuration.Register(
                path, cfg => _configuration = cfg, new Configuration());
        }


        #endregion

        #region ISelfDocumenting

        private Task DocumentConfiguration(IOwinContext context)
        {
            var document = GetScriptResource("configuration.html");

            document = document.Replace("{path}", _configuration.Path);
            document = document.Replace("{enabled}", _configuration.Enabled.ToString());
            document = document.Replace("{requiredPermission}", _configuration.RequiredPermission ?? "<none>");
            document = document.Replace("{defaultFormat}", _configuration.DefaultFormat);

            var defaultConfiguration = new Configuration();
            document = document.Replace("{path.default}", defaultConfiguration.Path);
            document = document.Replace("{enabled.default}", defaultConfiguration.Enabled.ToString());
            document = document.Replace("{requiredPermission.default}", defaultConfiguration.RequiredPermission ?? "<none>");
            document = document.Replace("{defaultFormat.default}", defaultConfiguration.DefaultFormat);

            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync(document);
        }

        Uri ISelfDocumenting.GetDocumentation(DocumentationTypes documentationType)
        {
            switch (documentationType)
            {
                case DocumentationTypes.Configuration:
                    return new Uri(_configuration.Path + ConfigDocsPath, UriKind.Relative);
                case DocumentationTypes.Overview:
                    return new Uri(_configuration.Path + "https://github.com/Bikeman868/OwinFramework/tree/master/OwinFramework.Middleware", UriKind.Absolute);
            }
            return null;
        }

        string ISelfDocumenting.LongDescription
        {
            get { return "Allows you to extract analysis information about your middleware for visual inspection or for use in other applications such as health monitoring, trend analysis and system dashboard"; }
        }

        string ISelfDocumenting.ShortDescription
        {
            get { return "Generates a report with analytic information from middleware"; }
        }

        #endregion

        #region Embedded resources

        private string GetScriptResource(string filename)
        {
            var scriptResourceName = Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .FirstOrDefault(n => n.Contains(filename));
            if (scriptResourceName == null)
                throw new Exception("Failed to find embedded resource " + filename);

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(scriptResourceName))
            {
                if (stream == null)
                    throw new Exception("Failed to open embedded resource " + scriptResourceName);

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        #endregion
    }
}
