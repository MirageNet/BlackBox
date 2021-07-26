## Usage

To create a plugin for MirrorNG, follow these steps:

7) update README.txt to describe your project
8) While not strictly required,  I suggest creating a symbolic link from Assets/MyPlugin/Samples~ to Assets/Samples. This will allow you to open and edit your examples in unity.
9) Add your samples to package.json, so upm can install them.

Once your plugin is working the way you like,  you can add it to openupm by going to https://openupm.com/packages/add/

[![Discord](https://img.shields.io/discord/343440455738064897.svg)](https://discordapp.com/invite/N9QVxbM)
[![release](https://img.shields.io/github/release/Miragenet/BlackBox.svg)](https://github.com/MirageNet/BlackBox/releases/latest)

[![Build](https://github.com/MirrorNG/Discovery/workflows/CI/badge.svg)](https://github.com/MirageNet/BlackBox/actions?query=workflow%3ACI)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=BlackBox&metric=alert_status)](https://sonarcloud.io/dashboard?id=BlackBox)
[![SonarCloud Coverage](https://sonarcloud.io/api/project_badges/measure?project=BlackBox&metric=coverage)](https://sonarcloud.io/component_measures?id=BlackBox&metric=coverage)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=BlackBox&metric=ncloc)](https://sonarcloud.io/dashboard?id=BlackBox)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=BlackBox&metric=sqale_index)](https://sonarcloud.io/dashboard?id=BlackBox)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=BlackBox&metric=code_smells)](https://sonarcloud.io/dashboard?id=BlackBox)


BlackBox is a plugin for Mirage to allow encrypting and decrypting of messages.

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

## Usage

1) In Unity create a NetworkManager gameobject from the GameObject -> Networking -> NetworkManager.

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