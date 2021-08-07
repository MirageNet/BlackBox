[![Discord](https://img.shields.io/discord/809535064551456888.svg)](https://discordapp.com/invite/DTBPBYvexy)
[![release](https://img.shields.io/github/release/Miragenet/BlackBox.svg)](https://github.com/MirageNet/BlackBox/releases/latest)
[![Build](https://github.com/MirageNet/BlackBox/workflows/CI/badge.svg)](https://github.com/MirageNet/BlackBox/actions?query=workflow%3ACI)
[![GitHub issues](https://img.shields.io/github/issues/MirageNet/BlackBox.svg)](https://github.com/MirageNet/BlackBox/issues)
![GitHub last commit](https://img.shields.io/github/last-commit/MirageNet/BlackBox.svg) ![MIT Licensed](https://img.shields.io/badge/license-MIT-green.svg)
[![openupm](https://img.shields.io/npm/v/com.miragenet.blackbox?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.miragenet.blackbox/)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=BlackBox&metric=alert_status)](https://sonarcloud.io/dashboard?id=BlackBox)
[![SonarCloud Coverage](https://sonarcloud.io/api/project_badges/measure?project=BlackBox&metric=coverage)](https://sonarcloud.io/component_measures?id=BlackBox&metric=coverage)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=BlackBox&metric=ncloc)](https://sonarcloud.io/dashboard?id=BlackBox)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=BlackBox&metric=sqale_index)](https://sonarcloud.io/dashboard?id=BlackBox)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=BlackBox&metric=code_smells)](https://sonarcloud.io/dashboard?id=BlackBox)


BlackBox is a plugin for Mirage to allow encrypting and decrypting of messages.

## Warning
This does not protect you from any type of MITM attacks. This still requires end users to have there servers behind some type of ssl connection to securely pass around the public key between clients and server.
Each time a user connects to the server a new public key is assigned and server matches and creates a shared key with each client. Each new instance of server or even upn reboot will recreate a new private / public
key that the server will use to generate a shared key. So if there is a MITM attack its only for that single instance user is connected and or server is not rebooted. 

In the future I do plan to add feature to have a ssl connection to pass the key around to protect it fully from MITM attacks. At this time mirage is still in progress towards that goal.

## Installation
The preferred installation method is Unity Package manager.

If you are using unity 2019.3 or later: 

1) Open your project in unity
2) Install [Mirage](https://github.com/MirageNet/Mirage)
3) Click on Windows -> Package Manager
4) Click on the plus sign on the left and click on "Add package from git URL..."
5) enter https://github.com/MirageNet/BlackBox.git?path=/Assets/BlackBox
6) Unity will download and install BlackBox
7) Make sure .NET 4.X set for compiler level.

## Basic Usage

1) In Unity create a NetworkManager gameobject from the GameObject -> Networking -> NetworkManager.
2) Attach BlackboxFactory to same gameobject the network manager is on.
3) After this just get reference to the blackboxfactory script and call BlackBoxServer.Send or BlackBoxClient.Send to send encrypted messages from server side or client side.

## Authenticator Usage
1) Attach BlackboxAuthenticator to same object as network manager is attached to. After that everything will link on its own.
2) In inspector or in code change the username / password to authenticate user's using encryption method. At this time server just accepts all authentication.
if you want to change that please inherit from BlackboxAuthenticator and override the method OnMessageResponse to take control checking on server if username / password is correct.

## Contributing

There are several ways to contribute to this project:

* Pull requests for bug fixes and features are always appreciated.
* Pull requests to improve the documentation is also welcome
* Make tutorials on how to use this
* Test it and open issues
* Review existing pull requests
* Donations

When contributing code, please keep these things in mind:

* [KISS](https://en.wikipedia.org/wiki/KISS_principle) principle. Everything needs to be **as simple as possible**. 
* An API is like a joke,  if you have to explain it is not a good one.  Do not require people to read the documentation if you can avoid it.
* Follow [C# code conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions).
* Follow [SOLID principles](https://en.wikipedia.org/wiki/SOLID) as much as possible. 
* Keep your pull requests small and obvious,  if a PR can be split into several small ones, do so.
