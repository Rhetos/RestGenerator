@IF [%1] == [] ECHO Using default Rhetos location. & "%~f0" "%~dp0..\..\..\..\Rhetos"
@IF /I [%1] == [/NOPAUSE] ECHO Using default Rhetos location. & "%~f0" "%~dp0..\..\..\..\Rhetos" %1
@IF EXIST "%~f1\Source\Rhetos.Utilities\bin\Debug\Rhetos.Utilities.dll" GOTO RhetosFolderExists
@ECHO.
@ECHO ERROR: Rhetos binaries are not available. Please download Rhetos source to "%~f1" and build it using Build.bat, or provide an alternative path.
@GOTO Error0
:RhetosFolderExists

PUSHD "%~dp0"
DEL /Q /F "*.txt" || GOTO Error1
DEL /Q /F "*.dll" || GOTO Error1
DEL /Q /F "*.xml" || GOTO Error1
DEL /Q /F "*.pdb" || GOTO Error1

@CALL :SafeCopy ..\..\..\..\Rhetos\Source\Rhetos.Web\bin\Debug\Rhetos.Web.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\Source\Rhetos.Compiler.Interfaces\bin\Debug\Rhetos.Compiler.Interfaces.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\Source\Rhetos.Extensibility.Interfaces\bin\Debug\Rhetos.Extensibility.Interfaces.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\Source\Rhetos.Dsl.Interfaces\bin\Debug\Rhetos.Dsl.Interfaces.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\Source\Rhetos.Utilities\bin\Debug\Rhetos.Utilities.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\Source\Rhetos.Extensibility\bin\Debug\Rhetos.Extensibility.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\Source\Rhetos.Interfaces\bin\Debug\Rhetos.Interfaces.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\Source\Rhetos.Logging.Interfaces\bin\Debug\Rhetos.Logging.Interfaces.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\Source\Rhetos.Logging\bin\Debug\Rhetos.Logging.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\Source\Rhetos.Processing.Interfaces\bin\Debug\Rhetos.Processing.Interfaces.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\Source\Rhetos.Security.Interfaces\bin\Debug\Rhetos.Security.Interfaces.??? || GOTO Error1

@CALL :SafeCopy ..\..\..\..\Rhetos\CommonConcepts\Plugins\Rhetos.Dsl.DefaultConcepts\bin\Debug\Rhetos.Dsl.DefaultConcepts.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\CommonConcepts\Plugins\Rhetos.Dom.DefaultConcepts.Interfaces\bin\Debug\Rhetos.Dom.DefaultConcepts.Interfaces.??? || GOTO Error1
@CALL :SafeCopy ..\..\..\..\Rhetos\CommonConcepts\Plugins\Rhetos.Processing.DefaultCommands.Interfaces\bin\Debug\Rhetos.Processing.DefaultCommands.Interfaces.??? || GOTO Error1

@Goto Done

:SafeCopy
@IF NOT EXIST %1 ECHO. && ECHO ERROR: Missing "%~f1" && EXIT /B 1
XCOPY /Y/R %1 . || EXIT /B 1
@EXIT /B 0

:Done
PowerShell.exe -Command "dir *.dll,*.exe | %%{gi $_.FullName} | select -Property Name, Length, @{Name=\"LastWriteTime\"; Expression={$_.LastWriteTime.ToString(\"yyyy-MM-dd HH:mm:ss\")}}, @{Name=\"FileVersion\"; Expression={$_.VersionInfo.FileVersion}} | fl | Out-File FileVersions.txt -Width 1000"
POPD

@REM ================================================

@ECHO.
@ECHO %~nx0 SUCCESSFULLY COMPLETED.
@EXIT /B 0

:Error1
@POPD
:Error0
@ECHO.
@ECHO %~nx0 FAILED.
@IF /I [%2] NEQ [/NOPAUSE] @PAUSE
@EXIT /B 1
