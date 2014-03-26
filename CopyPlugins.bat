ECHO Target folder = [%1]
ECHO $(ConfigurationName) = [%2]

SET ThisScriptFolder="%~dp0"

XCOPY /Y/D/R %ThisScriptFolder%Plugins\Rhetos.RestGenerator\bin\%2\Rhetos.RestGenerator.dll %1 || EXIT /B 1
XCOPY /Y/D/R %ThisScriptFolder%Plugins\Rhetos.RestGenerator\bin\%2\Rhetos.RestGenerator.pdb %1 || EXIT /B 1
XCOPY /Y/D/R %ThisScriptFolder%Packages\Newtonsoft.Json.6.0.1\lib\net40\Newtonsoft.Json.dll %1 || EXIT /B 1

EXIT /B 0
