ECHO Target folder = [%1]
ECHO $(ConfigurationName) = [%2]

REM "%~dp0" is this script's folder.

XCOPY /Y/D/R "%~dp0"Plugins\Rhetos.RestGenerator\bin\%2\Rhetos.RestGenerator.dll %1 || EXIT /B 1
XCOPY /Y/D/R "%~dp0"Plugins\Rhetos.RestGenerator\bin\%2\Rhetos.RestGenerator.pdb %1 || EXIT /B 1
XCOPY /Y/D/R "%~dp0"Packages\Newtonsoft.Json.6.0.1\lib\net40\Newtonsoft.Json.dll %1 || EXIT /B 1

EXIT /B 0
