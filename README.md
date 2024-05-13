# Quick Link

![Build Workflow](https://github.com/Periapsises/QuickLink/actions/workflows/dotnet.yml/badge.svg)

QuickLink is small library to easily create clients and servers with basic communication.  
It uses a concept of messages, featuring a type, to which clients and the server can subscribe to and handle.

The library manages network streams to ensure data is received in its entirety before being dispatched to the subscribed methods.
