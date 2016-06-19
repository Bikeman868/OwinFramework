using System;
using System.IO;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Configuration.Urchin;
using OwinFramework.RouteVisualizer;
using OwinFramework.Utility;

namespace TestServer
{
    // You can use this as a template for the Owin Statup class in your application.
    public class Startup
    {
        /// <summary>
        /// This is used to hold onto a reference to the Urchin file store. If the file
        /// store is disposed by the garbage collector then it will no longer notice
        /// changes in the configuration file.
        /// </summary>
        private static IDisposable _configurationFileSource;

        public void Configuration(IAppBuilder app)
        {
            // Craete an Urchin configuration store
            var configurationStore = new Urchin.Client.Data.ConfigurationStore().Initialize();;

            // Tell urchin to get its configuration from the config.json file in this project. Note that if
            // you edit this file whilst the application is running the changes will be applied without 
            // restarting the application.
            var configFile = new FileInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.json");
            _configurationFileSource = new Urchin.Client.Sources.FileSource(configurationStore).Initialize(configFile, TimeSpan.FromSeconds(5));

            // Construct an adapter between the Owin Framework and Urchin
            var urchin = new UrchinConfiguration(configurationStore);

            // Use the Owin Framework builder to build the Owin pipeline (in your application you should use IoC)
            var dependencyGraphFactory = new DependencyGraphFactory();
            var segmenterFactory = new SegmenterFactory(dependencyGraphFactory);
            var builder = new Builder(dependencyGraphFactory, segmenterFactory);

            // The route visualizer middleware will produce an SVG showing the Owin pipeline configuration
            builder.Register(new RouteVisualizer())
                .As("RouteVisualizer")
                .ConfigureWith(urchin, "/middleware/visualizer")
                .RunFirst();

            // Tell Owin to add our Owin Framework middleware to the Owin pipeline
            app.UseBuilder(builder);
        }
    }
}
