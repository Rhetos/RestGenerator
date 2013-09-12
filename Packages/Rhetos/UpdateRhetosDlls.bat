DEL /Q %~dp0*.dll
DEL /Q %~dp0*.xml
DEL /Q %~dp0*.pdb

XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\Source\Rhetos\bin\Rhetos.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\Source\Rhetos.Compiler.Interfaces\bin\Debug\Rhetos.Compiler.Interfaces.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\Source\Rhetos.Extensibility.Interfaces\bin\Debug\Rhetos.Extensibility.Interfaces.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\Source\Rhetos.Dsl.Interfaces\bin\Debug\Rhetos.Dsl.Interfaces.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\Source\Rhetos.Utilities\bin\Debug\Rhetos.Utilities.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\Source\Rhetos.Extensibility\bin\Debug\Rhetos.Extensibility.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\Source\Rhetos.Interfaces\bin\Debug\Rhetos.Interfaces.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\Source\Rhetos.Logging.Interfaces\bin\Debug\Rhetos.Logging.Interfaces.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\Source\Rhetos.Logging\bin\Debug\Rhetos.Logging.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\Source\Rhetos.Processing.Interfaces\bin\Debug\Rhetos.Processing.Interfaces.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\Source\Rhetos.Security.Interfaces\bin\Debug\Rhetos.Security.Interfaces.??? %~dp0 || EXIT /B 1

XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\CommonConcepts\Plugins\Rhetos.Dsl.DefaultConcepts\bin\Debug\Rhetos.Dsl.DefaultConcepts.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\CommonConcepts\Plugins\Rhetos.Dom.DefaultConcepts.Interfaces\bin\Debug\Rhetos.Dom.DefaultConcepts.Interfaces.??? %~dp0 || EXIT /B 1
XCOPY /Y/D/R %~dp0..\..\..\..\Rhetos\CommonConcepts\Plugins\Rhetos.Processing.DefaultCommands.Interfaces\bin\Debug\Rhetos.Processing.DefaultCommands.Interfaces.??? %~dp0 || EXIT /B 1

