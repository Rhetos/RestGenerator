SETLOCAL

@REM During the Build process we are not executing the dbupdate command so we must explicitly call it here
test\TestApp\bin\Debug\net5.0\rhetos.exe dbupdate test\TestApp\bin\Debug\net5.0\TestApp.dll || GOTO Error0

@REM Using "no-build" option as optimization, because Test.bat should always be executed after Build.bat.
dotnet test --no-build || GOTO Error0

@REM ================================================

@ECHO.
@ECHO %~nx0 SUCCESSFULLY COMPLETED.
@EXIT /B 0

:Error0
@ECHO.
@ECHO %~nx0 FAILED.
@EXIT /B 1
