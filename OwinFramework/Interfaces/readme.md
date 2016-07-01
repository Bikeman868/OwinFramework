# Unversioned interfaces

Interfaces in this folder are not versioned because either 
they can't change without fundamentally altering the nature
of the Owin Framework, or they are designed to be used 
by the builder and the application.

Since the application developer deliberatelt chooses to
upgrade their version of the framework, it is easy for 
them to alter their application code to accomodate breaking 
changes.