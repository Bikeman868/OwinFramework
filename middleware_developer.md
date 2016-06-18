# OWIN Framework For Middleware Developers

As a middleware developer you get these benefits from using this framework:

* You don't have ot write an entire suite of middleware components that work
  together to make them useful. You can author one small piece of the whole
  picture and any application developer can use your middleware nomatter what
  other middleware choices they made.

* You don't need to be aware of all the other middleware available to the 
  application developer and write versions of your middleware or adapters
  to make it work with those other implementations. You can write your
  middleware once and have it work correctly alongside anyone elses.

* You don't have to support multiple configuration mechanisms to fit in
  with the ways that different different application developers chose to
  store their configuration data. The OWIN Framework provides an
  abstraction between your middleware and the configuration mechanism.

* You don't have to provide configuration to select different mechanisms
  for other middleware functionallity because the OWIN framework allows
  the application developer to define routes that split and join
  the OWIN pipeline and have different middleware on each route.

* In short you can focus on just writing the functionallity that your
  middleware provides and achieve maximum reach with very little 
  extra effort.

The OWIN Framework has the following features that you can take advangate of:

* Simple one line declaration of dependency on other middleware functionallity.
  For example you can specify that your middleware needs session to run, or
  you can specify that your middleware can use session when available. In both
  cases the framework will ensure that the middleware that provides session will
  run prior to your middleware.

* You can specify that your middleware is designed to run at the front or at
  the back of the OWIN pipeline. This reduces configuration problems and
  increases adoption of your middleware by application developers.

* You can inject funcionallity into the OWIN pipeline and this can be picked
  up and used by other middleware further down the pipe. If for example your
  middleware provides session then it should impement `IMiddleware<ISession>`
  which tells the framework to put is in front of any middleware that has
  a dependency on `ISession`.

* You can provide an upsream communication so that downstream middleware can
  configure your middleware specifically for each request. For example if
  your middleware provides session storage you can provide a mechanism for
  downstream middleware to indicate whether it needs session or not for a
  specific request.

* You can define your configuration options as a simple class heirachy
  without concern for whether it is stored in a config file, a database
  or retrieved from a web service.

Please look at the `ExampleUsage` project in this solution for examples of
different styles of middleware implementation.

## Middleware communications

This project defines some simple extensible standards that allow OWIN middleware
components to communicate with each other and work together without knowing anything
about each other.

This project defines some interfaces that allow middleware developers to make their
middleware interoperable with other middleware from other authors. The list of standard
interfaces can be extended over time, and application developers can also define 
interfaces within their application and use them with the builder too.

The `Buider` in this project is completely agnostic to the interfaces that define the
features provided by a middleware component. This project simply defines `IMiddleware<T>`
where `T` can be any interface you like.

### Downstream communication

When a class implements `IMiddleware<T>` it must inject `T` into the OWIN context when it
is invoked. Other middleware components that depend on `T` can add this to their
dependencies, and the `Builder` will ensure that the middleware that injects `T` into the
context will be invoked before any middleware that depends on it.

This project defines two extension methods of `IOwinContext` called `SetFeature<T>()` and
`GetFeature<T>()` that middleware can use to inject these types into the OWIN context 
and later retrieve them.

This mechanism is simple, and provides forwards communication, i.e. as the request is 
processed through the OWIN pipeline, any interfaces injected into the OWIN context 
by a middleware component are available to other middleware further down the pipe. I
refer to this as downstream communication.

### Upstream communication

The downstream communication mechanism is great, it allows session to be established at the
start of the request that other middleware further down the pipe can make use of, but
what about the case where the presentation middleware needs to tell the authentication
middleware what level of permissiotns are required for this request, but the authentication 
middleware runs before the presentation middleware in the OWIN pipeline? This requires
communication back up the pipeline, which is a bit more complicated. I refer to this as
upstream communication.

This project defines a routing mechanism that splits and joins the OWIN pipeline into
a set of parallel paths (referred to as routes). This routing mechanism also provides
the upstream communication.

When you use the OWIN Framework request processing is a two stage process. In the first
stage the routing components figure out which middleware components are going to be
used to process this request and in what order. They do this with routing filters that 
are defined by the application developer (routing filters are very simple Lambda 
expressions). In the second stage the OWIN pipeline is executed in the usual OWIN fashion. 
Upstream communication happens during the first stage, downstream communication happens
during the second stage.

For an example of upstream communication see the `InProcessSession` class in the 
`ExampleUsage` project.

# FAQ

## How can I convert my existing middleware to work with OwinFramework?

This Framework depends on the Microsoft OWIN package. Assuming you based your
middleware implementation on this pattern then all you have to do is implement 
`IMiddleware<T>`.

Starting from existing middleware like this:
```
    using System;
    using System.Threading.Tasks;
    using Microsoft.Owin;

    public class MyMiddleware: IMiddleware<object>
    {
	  public Task Invoke(IOwinContext context, Func<Task> next)
	  {
	    // Do my stuff here
		//
	    return next();
	  }
	}
```
Make this code work with OwinFramework by changing it to this:
```
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using OwinFramework.Interfaces.Builder;

    public class MyMiddleware: IMiddleware<object>
    {
	  private readonly IList<IDependency> _dependencies = new List<IDependency>();
	  public IList<IDependency> Dependencies { get { return _dependencies; } }

	  public string Name { get; set; }

	  public Task Invoke(IOwinContext context, Func<Task> next)
	  {
	    // Do my stuff here
		//
	    return next();
	  }
	}
```
Now you can register this with the OwinFramework builder, give it a name and
make other middleware depend on it by name.