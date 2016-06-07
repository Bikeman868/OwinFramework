# OWIN Framework

OWIN represents a great leap forward in enabling application developers to build 
their applications from components from multiple sources, but OWIN is just the
starting point and we need more standardization if we want to be able to mix and
match any OWIN middleware we want and have a working application with minimal
knowledge of how the parts work.

Using only the OWIN specification I can't use any session or authentication 
middleware I like and have it work with any output rendering framework I like
because these things don't know how to talk to each other. In addition as a
developer I have to undestand all the dependencies bewteen middleware
components from different sources to be able to configure a working application.

I think we can do much better than this. I want to be able to choose any middleware
I want, and have something else build me an OWIN pipeline that will work. If I am
missing something that is essential then tell me what I am missing and tell me
what my options are.

Consider the following scenario: I have a UI and API way of accessing the
functionallity of my web site. The UI and the API share functionallity at the
model and business logic level and need shared caching, but they use different 
rendering frameworks. We want to build this with OWIN because OWIN is great! The
problem is that I want forms authentication for the UI and that depends on 
session, but I want certificate based authentication for the API and that does 
not require session. I want the UI and the API share the same session mechanism. 
Each UI component and each API endpoint specifies whether it needs session or not, 
it also specifies the user permissions required to access the functionallity 
it provides. My UI is a composition built from multiple UI components.

How can I use redily available OWIN components to realize this design?

How can I source UI components from other developers that know how to specify 
session and authentication without knowing which session provider or authentication 
provider I chose, and how does the session provider know whether it needs to 
establish session or not. The rules for establishing session for the API are just 
whether the endpont requires session or not, but for the UI if the page contains 
any components that need authentication then session is required. If I switch 
my authenticationto a different provider that is not session based, then session 
is only needed when the UI components need session.

I can't configure this with palin OWIN and have the flexibility to switch any 
OWIN middleware component for any other implementation without making substantial 
changes throughout my application.

This project is an effort to fix this problem.

## What is in this project

This project mostly consists of interface definitions. These interfaces 
extend the original OWIN concept of not restricting the building blocks
of the application by enforcing constraints on what their shape should be,
whilst at the same time providing enough structure to allow different
implementations of the same functionallity to be switched out without
having impact elsewhere.

The areas of standardization covered by this project are:
* Configuring the OWIN pipeline to satisfy dependencies between middleware
* Defining routes and splitting/joining the OWIN pipeline
* Communication between OWIN middleware components

## Configuring the OWIN pipeline

This project defines a fluid syntax for defining and chaining OWIN middleware 
components. The builder works on the following principals:

* Each OWIN middleware component will know that it depends on specific
  types of OWIN middleware, and has a way to declare what these dependencies are. 
  For example if an authentication middleware needs session storage then
  it can declare this dependency, then if the application developer configures
  this middleware but does not define a session mechanism for the
  application then an error will be produced. To fix the error the application
  developer needs to choose a specific session mechanism implementation.

* Each OWIN middleware component will know what other OWIN middleware
  components it can make use of if they are present, but will still
  function correctly without them. For example a view engine might define
  that is will use session state if available, but it is not an error
  to configure the view engine without a session mechanism.

* The application author might want to specify some constraints in the 
  order that OWIN middleware is chained regardless of their dependencies.
  For example the OWIN middleware that serves static file content might
  have no dependency on the authentication module or visa versa but the
  application developer wants the static file middleware to run first
  for performance reasons.

* The different types of dependencies are examined by the builder which
  then builds the OWIN middleware pipeline so that all dependencies are
  satisfied.

* There can be routing middleware components that split the OWIN pipeline
  into multiple routes and each of these routes can be configured with
  different implementations. Continuing from the example at the start 
  of this document, I can specify that my routing middleware must run 
  after session and that it split the pipeline in two, one for the UI and
  one for the API. These two pipelines need to be configured differently.

### Example application code to configure the OWIN pipeline

This is an example of how an appllication can use the builder. Note that
applications do not have to use the builder, but it makes dependency
resolution much easier. Instead of the developer trying to figure our
the order of chaining to make everything work as expected, the developer
just has to specify the things they care about which are their choice
of implementation for each piece of functionallity (which session
provider, which authentication provider etc) and the places where they
care about the order of chaining (which is never required just to 
make it work).

A very basic configuration with a single chain can be configured very simply.
Note that order is not specified here but forms authentication knows that it
needs session and MVC defines that it can use authentication if present.
The builder uses this dependency information to build the chain in the order
session -> authentication -> mvc.

```
    public static Configuration(IAppBuilder app)
    {
      var dependencyTreeFactory = new DependencyTreeFactory();
	  var builder = new Builder(dependencyTreeFactory);
	  var configuration = new WebConfigConfiguration();

	  builder.Register(new FormsAuthentication())
		.ConfiguredWith(configuration, "/owin/authentication/forms");

	  builder.Register(new MvcFramework())
		.ConfiguredWith(configuration, "/owin/mvc");

	  builder.Register(new AspSession())
		.ConfiguredWith(configuration, "/owin/aspSession");
	  
      app.UseBuilder(builder);
    }
```

Note that you don't have to use the standard builder, any class that
implements `IBuilder` could be used instead.

Note that you can switch the implementation of session, authentication
or presentation independantly of each other without changing any other 
code (configuration options will be different for each middleware).

The example presented at the start of this document is a bit more
complicated because there are multiple splits in the OWIN pipeline as
well as a join. There are also several implementations of the 
authentication functionallity that have to be named to distinguish them.
This is about as complicated a scenario as I can envisage at this point.

This very complex configuration might look like this:

```
    public static Configuration(IAppBuilder app)
    {
      var dependencyTreeFactory = new DependencyTreeFactory();
      var builder = new Builder(dependencyTreeFactory);
      var configuration = new UrchinConfiguration();
	  
      builder.Register(new FormsAuthentication())
          .As("FormsAuthentication")
          .ConfigureWith(configuration, "/owin/auth/forms")
          .RunAfter<IRoute>("SecureUI");
	  
      builder.Register(new CertificateAuthentication())
          .As("CertificateAuthentication")
          .ConfigureWith(configuration, "/owin/auth/cert")
          .RunAfter<IRoute>("API");
	  
      builder.Register(new InProcessSession())
          .ConfigureWith(configuration, "/owin/session");
	  
      builder.Register(new TemplatePageRendering())
          .RunAfter<IRoute>("PublicUI")
          .RunAfter<IRoute>("SecureUI")
          .ConfigureWith(configuration, "/owin/templates");
	  
      builder.Register(new RestServiceMapper())
          .RunAfter<IAuthentication>("CertificateAuthentication")
          .ConfigureWith(configuration, "/owin/rest");
	  
      builder.Register(new Router())
          .AddRoute("UI", context => context.Request.Path.Value.EndsWith(".aspx"))
          .AddRoute("API", context => true)
          .RunAfter<ISession>();
	  
      builder.Register(new Router())
          .AddRoute("SecureUI", context => context.Request.Path.Value.StartsWith("/secure"))
          .AddRoute("PublicUI", context => true)
          .RunAfter<IRoute>("UI");
	  
      app.UseBuilder(builder);
    }
```
