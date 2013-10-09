ECHO Target folder = [%1]
ECHO $(ConfigurationName) = [%2]

SET ThisScriptFolder="%~dp0"

XCOPY /Y/D/R %ThisScriptFolder%Plugins\Rhetos.RestGenerator\bin\%2\Rhetos.RestGenerator.dll %1 || EXIT /B 1
XCOPY /Y/D/R %ThisScriptFolder%Plugins\Rhetos.RestGenerator\bin\%2\Rhetos.RestGenerator.pdb %1 || EXIT /B 1
XCOPY /Y/D/R %ThisScriptFolder%Plugins\Rhetos.RestGenerator.DefaultConcepts\bin\%2\Rhetos.RestGenerator.DefaultConcepts.dll %1 || EXIT /B 1
XCOPY /Y/D/R %ThisScriptFolder%Plugins\Rhetos.RestGenerator.DefaultConcepts\bin\%2\Rhetos.RestGenerator.DefaultConcepts.pdb %1 || EXIT /B 1

EXIT /B 0
