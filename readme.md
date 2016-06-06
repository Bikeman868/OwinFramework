# OWIN Framework

OWIN represents a great leap forward in enabling application developers to build 
their applications from components from multiple sources, but OWIN does not go
far enough and web applications still have nowhere near enough flexibility.

Consider the following scenario: I have a UI and API way of accessing the
functionallity of my web site. The UI and the API share functionallity at the
model and business logic level and need shared caching, but they use different 
rendering frameworks. We want to build this with OWIN because OWIN is great! The
problem is that the UI uses forms authentication that depends on session and
the API use certificate based authentication that does not require session.
The UI and the API share the same session mechanism. Each UI component end 
each API endpoint specifies whether it needs session or not, it also specifies 
the user permissions required to access the functionallity it provides. The UI 
is a composition build from multiple UI components.

In this scenario how can I source UI components from other developers that 
know how to specify session and authentication without knowing which
session provider or authentication provider I chose. How does the session
provider know whether it needs to establish session or not. The rules for
establishing session for the API are just whether the endpont requires
session or not, but for the UI if the page contains any components that
need authentication then session is required, but if I switch my authentication
to a different provider that is not session based, then session is only
needed when the UI components need session.

This scenario is not so far fetched. I can't do this today with OWIN and 
have the flexibility to switch any OWIN component for any other implementation
without making sbnstantial throughout my application.

This project is an effort to fix this problem.

## What is in this project

This project mostly consists of interface definitions. These interfaces 
extend the original OWIN concept of not restricting the building blocks
of the application by enforcing constraints on what their shape should be
whilst at the same time providing enough structure to allow different
implementations of the same functionallity to be switched out without
having impact anywhere else.

The areas of standardization covered by this project are:
* Configuring the OWIN chain to satisfy dependencies
* Communication between OWIN middleware components

### Configuring the OWIN chain

This project defines a fluid syntax for defining and chaining OWIN middleware 
components. The builder works on the following principals:

* Each OWIN middleware component will know that it depends on specific
  types of OWIN middleware, and has a way to declare what these dependencies are. 
  For example if an authentication middleware needs session storage then
  it can declare this dependency then if the application developer configures
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
  then builds the OWIN middleware chain so that all dependencies are
  satisfied.

* There can be routing middleware components that split the OWIN chain
  into multiple chains and each of these chains can be configured with
  different implementations. Continuing from the example at the start 
  of this document, I can specify that my routing middleware must run 
  after session and that it split the chain in two, one for the UI and
  one for the API. These two chains need to be configured differently.

#### Example application code to configure the OWIN chain

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
    public static Startup()
    {
	  var builder = new Builder();
	  var configuration = new UrchinConfiguration();

	  var auth = new FormsAuthentication(builder)
		.ConfiguredWith(configuration, "/owin/authentication/forms");

	  var mvc = new MvcFramework(builder)
		.ConfiguredWith(configuration, "/owin/mvc");

	  var session = new AspSession(builder)
		.ConfiguredWith(configuration, "/owin/aspSession");
	  
	  builder.Build();
    }
```

Note that you don't have to use the standard builder, any class that
implements IBuilder could be used instead.

Note that you can swith the implementation of session, authentication
or presentation without changing anything else.

The example presented at the start of this document is a bit more
complicated because there are multiple chains and several implementations
of the same functionallity that have to be named to distinguish them. This
configuration would look like this:

```
    public static Startup()
    {
	  // Note that Builder can also be registered as a singleton
	  // in an IOC container and this container can be used to
	  // construct the other middleware components

	  var builder = new Builder();
	  var configuration = new WebConfigConfiguration();

	  var session = new AspSession(builder)
		.ConfiguredWith(configuration, "/owin/aspSession");

	  var router = new Router(builder)
	    .As("router1")
		.RunsAfter<ISession>();
		.ConfiguredWith(configuration, "/owin/router1");

	  // Note that the router configuration adds two
	  // IRoute middleware components to the builder
	  // called 'apiRoute' and 'uiRoute'

	  var auth1 = new FormsAuthentication(builder)
		.As("formsAuthentication")
		.RunsAfter<IRoute>("uiRoute")
		.ConfiguredWith(configuration, "/owin/authentication/forms");

	  var auth2 = new CertificateAuthentication(builder)
		.As("certificateAuthentication")
		.RunsAfter<IRoute>("apiRoute")
		.ConfiguredWith(configuration, "/owin/authentication/cert");

	  var rest = new RestAPIMapper(builder)
		.RunsAfter<IAuthentication>("certificateAuthentication")
		.ConfiguredWith(configuration, "/owin/restAPIMapper");

	  var mvc = new MvcFramework(builder)
		.RunsAfter<IAuthentication>("formsAuthentication")
		.ConfiguredWith(configuration, "/owin/mvc");

	  builder.Build();
    }
```
