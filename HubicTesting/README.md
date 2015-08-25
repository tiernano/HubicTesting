# HubicTesting
testing of hubic. code based on https://github.com/Vachounet/SwiftLib_Hubic

#whats it do?
[HuibC][1] is a storage system which gives you 25Gb storage for free and 10Tb for a fiver a month. I wanted to upload and download files using C#, and found a library, https://github.com/oderwat/hubic2swiftgate, to use it. But i needed to do some tweaking. Here are the tweaks.
The Exe, when built, takes 2 parameters: 

* container you want to upload to
* directory with the files you want to upload

It will kick off a parallel upload for these files. 

The original code does have stuff for downloading and deleting files, but im not suing that yet... just uploads.

#hows it work?
Magic. Most of it is taken from https://github.com/oderwat/hubic2swiftgate, which was modified slightly to build and get embedded into the app... I am planning on making some minor changes to their code and sending it on as a Pull Request, but just havent done it yet.

[1]:http://www.hubic.com
