# OWIN Framework For Application Developers

As an application developer you get these benefits from using this framework:

* Middleware components from different authors that provide the same functionallity
  can be interchanged without changing any of your application code. For example
  you can choose any middleware component that provides session management, and you
  can change your mind later without changing any of your application code.

* Middleware components that depend on other functionallity in the OWIN
  pipeline don't have to come from the same author. For example if an
  Authorization middleware you are using depends on Identification middleware
  you can choose any implementation of Identification, the Authorization
  middleware does not have to support is explicitly.

* When middleware components have dependencies these are resolved automatically
  and the OWIN pipeline will be built in a way that works without you having
  to understand the inner workings of the middleware you are using.

* You can split and join the OWIN pipeline creating different paths (routes)
  through the middleware for different kinds of requests. For example you
  might need Identification/Authorization only in certain areas of your site,
  or you might want an entirely different security scheme for certain types
  of request.

## Configuring the OWIN pipeline

This project defines a fluid syntax for defining and chaining OWIN middleware 
components. The framework has a 'builder' that can be added to the OWIN pipeline
using the familiar OWIN syntax and the builder then builds the rest of the
OWIN pipeline for you based on the following principals:

* Each OWIN middleware component will know that it depends on specific
  types of OWIN middleware, and has a way to declare what these dependencies are. 
  For example if an Auhorization middleware needs Session storage then
  it can declare this dependency, then if the application developer configures
  this middleware but does not define a Session mechanism for the
  application then an error will be produced. To fix the error the application
  developer needs to choose a Session mechanism, but the application developer
  does not need to know what depends on Session, or what Session depends on
  and figure out the correct execution order, the builder does that for you.

* Each OWIN middleware component will know what other OWIN middleware
  components it can make use of if they are present, but will still
  function correctly without them. For example a View Engine might define
  that is will use Session state if available, but it is not an error
  to configure the View Engine without a Session mechanism. In this case
  the builder will ensure that Session runs before the View Engine if
  it is configured.

* Some middleware is designed to run before any other middleware. An example
  of this type is one that catches exceptions and returns error reports. It
  needs to run first to catch errors in all subsequent middleware components.

* Some middleware is designed to run after all other middleware. An example
  of this type of middleware is one that returns 404 (not found) responses
  when no other middleware component handled the rrequest.

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

## Example application code to configure the OWIN pipeline

This is an example of how an appllication can use the builder. Note that
applications do not have to use the builder for all middleware, but it 
makes dependencyresolution much easier.

Instead of the developer trying to figure our the order of chaining to make 
everything work as expected, the developer just has to specify the things 
they care about which are their choice of implementation for each piece 
of functionallity (which session provider, which authentication provider etc) 
and the places where they care about the order of chaining (which is never 
required just to make it work).

A very basic configuration with a single chain can be configured very simply.
Note that order is not specified here but forms identification knows that it
needs session and MVC defines that it can use authentication if present.
The builder uses this dependency information to build the chain in the order
session -> authentication -> mvc.

```
    public static Configuration(IAppBuilder app)
    {
	  var builder = new Builder();
	  var configuration = new WebConfigConfiguration();

	  builder.Register(new FormsIdentification())
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

Note that this example does not use IoC for simplicity. In any real
world application I strongly suggest that you should use IoC. All of
the classes in this project support constructor injection.

Note that you can switch the implementation of session, authentication
or presentation independantly of each other without changing any other 
code.

Note that this framework allows you to supply any configuration mechanism 
that implements a very simple configuration provider interfrace. You
don't have to use web.config, implementing an alternate scheme is very
straightforward.

## A more complex example

A more complex eample might be a scenario where your web application
serves UI pages but also has a REST interface. The REST interface
and the UI pages use different identification mechanisms, also some
of the UI is secure and some is public.

This example involves two splits in the OWIN pipeline, firstly between
UI and API, and secondly between the secire and public parts of the UI.
It also involves a join because the secure and public routes both end
in the same page rendering middleware.

This very complex configuration might look like this:

```
    public static Configuration(IAppBuilder app)
    {
      var builder = new Builder();
      var urchin = new UrchinConfiguration();
      var webConfig = new WebConfigConfiguration();
	  
      builder.Register(new FormsIdentification())
          .As("FormsIdentification")
          .ConfigureWith(webConfig, "/owin/auth/forms")
          .RunAfter<IRoute>("SecureUI");
	  
      builder.Register(new CertificateIdentification())
          .As("CertificateIdentification")
          .ConfigureWith(urchin, "/owin/auth/cert")
          .RunAfter<IRoute>("API");
	  
      builder.Register(new InProcessSession())
          .ConfigureWith(configuration, "/owin/session");
	  
      builder.Register(new TemplatePageRendering())
          .RunAfter<IRoute>("PublicUI")
          .RunAfter<IRoute>("SecureUI")
          .ConfigureWith(webConfig, "/owin/templates");
	  
      builder.Register(new RestServiceMapper())
          .RunAfter<IAuthentication>("CertificateAuthentication")
          .ConfigureWith(urchin, "/owin/rest");
	  
      builder.Register(new Router())
          .AddRoute("UI", context => context.Request.Path.Value.EndsWith(".aspx"))
          .AddRoute("API", context => true)
          .RunAfter<ISession>();
	  
      builder.Register(new Router())
          .AddRoute("SecureUI", context => context.Request.Path.Value.StartsWith("/secure"))
          .AddRoute("PublicUI", context => true)
          .RunAfter<IRoute>("UI");

	  builder.Register(new ExceptionReporter())
	      .RunFirst();
	  
	  builder.Register(new NotFoundResponse())
	      .RunLast();

      app.UseBuilder(builder);
    }
```

## Legacy Middleware

In order to support all of the features of the OWIN Framework, middleware components
need to implement `IMiddleware<T>`. This is great going forward but what about existing
middleware that wasn't written with OWIN Framework in mind?

The OWIN Framework contains a `LegacyMiddlewareWrapper` class that can be used to bridge
the gap between earlier standards and the OWIN Framework.

In OWIN itself middleware is defined as a function taking `IDictionary<string, object>` and 
returning `Task`. Katana adds `IAppBuilder` extension methods to supprt 5 different ways of 
adding middleware to the pipeline see
(http://benfoster.io/blog/how-to-write-owin-middleware-in-5-different-steps) and
all 5 of these are supported by the `LegacyMiddlewareWrapper` class.

If you have existing code like the sample below that uses Katana middleware or middleware
that does not directly support the OWIN Framework:

```
    public static Configuration(IAppBuilder app)
    {
      app.UseWelcomePage("/")
    }
```

You can use it with the OWIN Framework using the `LegacyMiddlewareWrapper` class like this:


```
    public static Configuration(IAppBuilder app)
    {
      var builder = new Builder();
	  
      builder.Register(new LegacyMiddlewareWrapper().UseWelcomePage("/"));
	  
      app.UseBuilder(builder);
    }
```

This code can be used in conjunction with the fluid syntax for setting up the pipeline:

```
    public static Configuration(IAppBuilder app)
    {
      var builder = new Builder();
	  
      builder.Register(new LegacyMiddlewareWrapper().UseWelcomePage("/"))
		.As("OWIN welcome page")
		.RunFirst();
	  
      app.UseBuilder(builder);
    }
```

> Note that legacy middleware will not play nicely with the OWIN Framework because the OWIN
> Framework doesn't know anything about this middleware. Legacy middleware is just a function 
> with an AppFunc signature. This means for example that if you include authentication 
> middleware in your pipeline the OWIN Framework will not know that it provides authentication 
> and will not automatically ensure that the dependencies of other middleware components have been met.

## Pipeline Configuration Syntax

The steps to configuring the OWIN pipeline are

1. Construct an instance of the builder (using IoC).
2. Call the `Register()` method for each middleware.
3. call `app.UseBuilder()` to add the builder to the OWIN pipeline.

The `Register()` method of the builder returns `IMiddleware<T>`. There
are a number of fluid extensions to this interface that allow you to
configure various aspects of the middleware registration as follows:

### `AddRoute(string routeName, Func<IOwinContext, bool> filterExpression)`

This only applies when you register a router middleware. These must be 
the first statements after the call to `builder.Register()` because that's
the only place where the compiler knows that middleware is `IMiddleware<IRoute>`.

The route name can be used in the configuration of other middleware to
specify that they must be chained into this route. You would do this by adding
`.RunAfter<IRoute>(routeName)` to the middleware that should be in this route.

### `As(string name)`

Gives a name to this middleware. This makes diagnostic messages more useful, but
also makes it possible to add a dependency on this middleware from another when
there is more than one middleware of the same type.

When there is only one middleware of a specific type on a route then names are
not required, the type already uniquely identifies which middleware the 
dependency is on.

### `RunFirst()`

Indicates that this middleware should be at the front of the pipeline and
execute before any other middleware. If this middleware has dependencies 
then the dependent middleware will be run before this one.

If there are multiple middleware with the `RunFirst()` flag then they will
be executed in an unspecified order unless the have internal dependencies
or you add some `RunAfter()` statements to specify the order.

### `RunAfter<T>(string name = null, bool required = true)`

Specifies that this middleware has a dependency on some other middleware and
therefore should be placed after it in the OWIN pipeline.

The optional `name` parameter is only required if there are multiple middleware
components providing functionallity `T`. In this case `T` is not the type
of the middleware component, but the interface type of the functionallity
that it provides. For example an `InProcessSession` middleware and an `AspSession`
middleware will both provide `ISession` functionallity. When you add a
`RunAfter<ISession>()` statement you are telling the builder that this middleware
uses session and needs session to be executed earlier in the pipeline, but
either `InProcessSession` or `AspSession` could provide this.

The optional `required` parameter tells the builder whether to throw an
execution and terminate the build if the dependant middleware is not
configured. For example if a middleware component has `RunAfter<ISession>()` 
statement and no session middleware is configured then an exception
will be thrown if the `required` parameter was `true`.

### `RunLast()`

Indicates that this middleware should be at the back of the pipeline and
execute after all other middleware. If this middleware has things that
depend on it then these dependants will be run after this one.

If there are multiple middleware with the `RunLast()` flag then they will
be executed in an unspecified order unless the have internal dependencies
or you add some `RunAfter()` statements to specify the order.

### `ConfigureWith(IConfiguration configuration, string path)`

Passes configuration data to the middleware component. This framework
assumes that configuration data is heirachical and that it can be
de-serialized to a specific type. It does not make any assumption about
the format of persistence mechanism, so you can use XML, Json or any other
heirachical structure and you can store it in a file, database etc or
pull it from a web service.

The `path` part navigates the heirachical configuration structure just
like the path part of a URL can specify a file location in the
heirachical file system.

If you have the following Json:

```
    {
	  "myApp":{
	    "setting1":"some value"
	  }
	}
```

Then you would refer to setting1 with the path `/myApp/setting1`.

Although middleware components are interchangeble and implement the
same interfaces, different implementations can offer very different
implementations and these will typically surface through the 
configuration options available.

When you switch from one implementation to another, you will typically
need to create configuration specific to that implementation. For
example in-process session middleware might only support session 
timeout configuration, but a session middleware that persists
session to a shared session server will also need the url of the
shared server at the very least.
