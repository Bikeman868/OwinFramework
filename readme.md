# OWIN Framework

If you are writing middleware components to share with others please read
[the middleware development guide](middleware_developer.md).

If you are writing an OWIN application and want to consume components 
and configure your OWIN pipeline please read
[the application development guide](application_developer.md).

Note that there is a comprehensive [Wiki for this project](https://github.com/Bikeman868/OwinFramework/wiki).

## Why does this project exist

OWIN represents a great leap forward in standardizing the interface bewteen
the hosting environment and the web application. It allows any web application
to run on any hosting platform, and this standardization is helpful to
everyone involved in web development.

Another great thing about OWIN is that it defines only one very straightforward
interface that does not place any constraints on the shape of the application
or the hosting platform, constraining only those things that absolutely have
to be constrained to make OWIN possible.

I wanted to extend these great OWIN concepts up to the next layer, and define
a set of interfaces that place no unnecessary constraints on the shape
of the middleware components whilst at the same time allowing middleware
components to be switched out transparently in the same way that you can
switch out the hosting service.

Using only the OWIN specification I can't use any session or authentication 
middleware I like and have it work with any output rendering framework I like
because these things don't know how to talk to each other. In addition as a
developer I have to understand all the dependencies bewteen middleware
components from different sources to be able to configure a working application.

I think we can do much better than this. I want to be able to choose any middleware
I want, and have something else build me an OWIN pipeline that will work. If I am
missing something that is essential then tell me what I am missing and tell me
what my options are.

Consider the following scenario: I have a UI and API way of accessing the
functionallity of my web site. The UI and the API share functionallity at the
model and business logic level and need shared caching, but they use different 
rendering frameworks. I want to build this with OWIN because OWIN is great! The
problem is that I want forms authentication for the UI, and I want certificate 
based authentication for the API. I want the UI and the API share the same session 
mechanism. Each UI component and each API endpoint specifies whether it needs 
session or not, it also specifies the user permissions required to access the 
functionallity it provides. My UI is a composition built from multiple UI components.

Katana can do this I hear you say! But for me Katana missed the whole point 
of the open architecture that OWIN promised. Katana is an evolution of ASP.Net
and as such is a huge suite of middleware components that work together but do
not work with anything else. Sure Microsoft provided specific points of
extensibility like creating your own view engine, but these things are
extremely complex and it takes many hours to understand enough about the
Katana ecosystem to extend it in any meaningful way. I don't think Katana
is a good platform for everyone to use as a model for writing interoperable
middleware components becuase it is full of details specific to the Microsoft
implementation. In short I don't think Microsoft designed Katana to be
the foundation of an open architecture for building middleware components, and
I see that as a missed opportunity.

This project set out to define an open architecture for building middleware
components that work together, and in the spirit of the original OWIN
design it comprises mostly interfaces that do not place constraints on the
shape of the middleware components.

Using this framework I can source UI components from other developers that 
know how to specify session and authentication without knowing which session 
provider or authentication provider I chose, and the session provider knows 
whether it needs to establish session or not without knowing anything about
the middleware downstream. This is just one example of the class of problems
that this framework solves.

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

Note that this is a work in progress, and I would welcome contributions 
from  as many other developers as possible. The more people who use this
framework, the more useful it will be to everyone so please join in.

## Projects in this solution

`ExampleUsage.csproj` is a project that demonstrates some of what is possible
with this framework. It is by no means trying to be an exhastive list
of what can be done, but just a few variations to give a flavour.

`OwinFramework.Net40.csproj` compiles to the DLL that gets installed from NuGet.
This is the core framework itself and consists mainly of interfaces but
also has an OWIN pipeline builder that resolves dependencies and provides
routing. There is also a Net45 version of this project.

`OwinFramework.Configuration.ConfigurationManager.csproj` is distributed as an
optional extra NuGet package that provides middleware configuration
via the web.config file using the `ConfigurationManager` class. I only built
the .Net45 version so far.

`OwinFramework.Configuration.Urchin.Net40.csproj` is distributed as an
optional extra NuGet package that provides middleware configuration via
the [Urchin](https://github.com/Bikeman868/urchin) rules based configuration 
management system. There is also a Net45 version of this project.

`UnitTests.csproj` contains what you would expect!

`OwinFramework.Mocks.Net40.csproj` contains mock implementations of the OWIN Framework
interfaces. You can use these mocks in your unit tests to mock the framework
itself. There is also a >net 4.5 version of this project in the solution.

## Roadmap

These are the next batch of NuGet packages that are in the pipeline:

1. A token store facility that uses Prius

2. Authorization middleware for managing users, groups and permissions

3. Identification middleware that uses IP address and can be used for 2 factor authentication

4. An identity store facility that uses SQLite

5. A token store facility thay uses SQLite

6. A cache facility that uses MemcacheD

7. A cache facility that uses ElastiCache

8. Identification middleware that uses shared secrets

9. Certificate based identification middleware

10. Social login (Google, Facebook, LinkedIn, Microsoft etc)
