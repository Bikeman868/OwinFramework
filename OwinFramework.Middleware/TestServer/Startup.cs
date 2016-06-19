using System;
using System.IO;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Configuration.Urchin;
using OwinFramework.RouteVisualizer;
using OwinFramework.Utility;

namespace TestServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Craete an Urchin configuration store
            var configurationStore = new Urchin.Client.Data.ConfigurationStore();
            configurationStore.Initialize();

            // Tell urchin to get its configuration from the config.json file in this project. Note that if
            // you edit this file whilst the application is running the changes will be immediately applied
            // without restarting the application.
            var fileSource = new Urchin.Client.Sources.FileSource(configurationStore);
            var configFile = new FileInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.json");
            fileSource.Initialize(configFile, TimeSpan.FromSeconds(10));

            // Tell the Owin Framework to use Urchin for its middleware configuration
            var configuration = new UrchinConfiguration(configurationStore);

            // Use the Owin Framework  builder to build the Owin pipeline
            var dependencyGraphFactory = new DependencyGraphFactory();
            var segmenterFactory = new SegmenterFactory(dependencyGraphFactory);
            var builder = new Builder(dependencyGraphFactory, segmenterFactory);

            // The route visualizer middleware will produce an SVG showing the Owin pipeline configuration
            builder.Register(new RouteVisualizer())
                .As("RouteVisualizer")
                .ConfigureWith(configuration, "/middleware/visualizer")
                .RunFirst();

            // Tell Owin to add our Owin Framework middleware to the Owin pipeline
            app.UseBuilder(builder);
        }
    }
}
