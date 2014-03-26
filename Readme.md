RestGenerator
=============

RestGenerator is a DSL package (a plugin module) for [Rhetos development platform](https://github.com/Rhetos/Rhetos).
It automatically generates **REST interface** for all data structures and actions that are defined in a Rhetos application.

See [rhetos.org](http://www.rhetos.org/) for more information on Rhetos.

Features
========

RestGenerator provides one service for every data structure and action.
That way it is not possible to get one wsdl that combines all services, but initialization of each service and usage is faster.

Deployment
==========

### Prerequisites

* *CommonConcepts* package must be deployed along with *RestGenerator*.

Building binaries from source
=============================

### Prerequisites

* Build utilities in this project are based on relative path to Rhetos repository.
  [Rhetos source](https://github.com/Rhetos/Rhetos) should be downloaded to a folder
  with relative path `..\..\Rhetos` and compiled (use `Build.bat`),
  before this package's `Build.bat` script is executed.

Sample folder structure:
 
	\ROOT
		\Rhetos
		\RhetosPackages
			\RestGenerator

### Build

1. Build this package by executing `Build.bat`. The script will pause in case of an error.
   * The script automatically copies all needed dll files from Rhetos folder and builds the Rhetos.RestGenerator.sln using Visual Studio (command-line).

### Create installation package

1. Set the new version number in `ChangeVersion.bat` and execute it.
2. Execute `Build.bat`.
3. Execute `CreatePackage.bat`. It creates installation package (.zip) in parent directory of RestGenerator.
