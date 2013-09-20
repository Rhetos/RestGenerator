RestGenerator
=================

RestGenerator is a DSL package (a plugin module) for [Rhetos development platform](https://github.com/Rhetos/Rhetos).

RestGenerator automatically generates REST interface for all data structures and actions that are defined in a Rhetos application.

See [rhetos.org](http://www.rhetos.org/) for more information on Rhetos.

Features
========

RestGenerator provides one service for every data structure and action.
That way it is not possible to get one wsdl that combines all services, but initialization of each service and usage is faster.

Prerequisites
=============

Utilities in this project are based on relative path to Rhetos repository. [Rhetos source](https://github.com/Rhetos/Rhetos) must be downloaded to a folder with relative path `..\..\Rhetos`.

Sample folder structure:
 
	\ROOT
		\Rhetos
		\RhetosPackages
			\RestGenerator


Build and Installation
======================

Build package with `Build.bat`. Check BuildError.log for errors.

Instalation package creation:

1. Set the new version number in `ChangeVersion.bat` and start it.
2. Start `CreatePackage.bat`. Instalation package (.zip) is going to be created in parent directory of RestGenerator.
