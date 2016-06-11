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

If you are writing middleware components to share with others please read
[the middleware development guide](middleware_developer.md).

If you are writing an OWIN application and want to consume components 
and configure your OWIN pipeline please read
[the application development guide](application_developer.md).

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
