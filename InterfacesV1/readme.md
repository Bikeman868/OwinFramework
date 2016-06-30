# Versioned Interfaces
This folder contains version 1 of the versioned interfaces. These
interfaces are versioned so that older middleware and newer 
middleware can both run against a newer version of the Owin
Framework in the same application.

When you are writing code that implements these interfaces 
you should update your code to implement the newest version.

When you are writing code that consumes these interfaces
you will need to check which version it implemented. Note that
the V2 interfaces will inherit from the V1 interfaces etc
so that an `is` check against the V1 interface will be true 
for any newer version of the interface also.

Note that you can easily switch to a newer version by altering
your `using` statements. This is why the interface namespace
is versioned rather than the individual interfaces.

The sub-folders have the following meanings:

## Capability
These are interfaces that can optionally be implemented
by middleware to specify that it poseses some ability.
for example the ability to self-document or the ability
to be configured.

## Facility
These are shared blocks of functionallity that could be 
used by multiple middleware, for example distrubuted
caching or localization.

## Middleware
These interfaces are added to the Owin context during
request processing to pass information to other
middleware further downstream.

Middleware must implement `IMiddleware<T>` where `T`
is one of the interfaces in this folder. The middleware
must add an implementation of `T` to the Owin context
when it handles the request. If the middleware does
not want to add anything to the Owin context at request
processing time because it has nothing to communicate
downstream then is should implement `IMiddleware<object>`.

## Upstream
These interfaces are added to the Owin context during
the request routing phase to allow downstream middleware
to communicate with middleware further upstream prior
to request processing.

Middleware that has an upstream communication channel
must implement `IUpstreamCommunicator<T>` where `T` is
one of the interfaces in this folder. At runtime the
middleware must add an implementation of `T` to the
Owin context during request routing.