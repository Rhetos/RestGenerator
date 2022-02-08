SETLOCAL

@REM During the Build process we are not executing the dbupdate command so we must explicitly call it here
test\TestApp\bin\Debug\net5.0\rhetos.exe dbupdate test\TestApp\bin\Debug\net5.0\TestApp.dll || GOTO Error0

@REM Using "no-build" option as optimization, because Test.bat should always be executed after Build.bat.
dotnet test --no-build || GOTO Error0

@REM Running the TestApp for the curl tests below.
start "Rhetos.RestGenerator test app. Close manually after tests are done." dotnet run --project test\TestApp --no-build || GOTO Error0
@REM Waiting for TestApp to start.
timeout /t 5

@REM ================================================

@REM Prerequisites:
@REM  - "CURL" command-line utility added to system path
@REM  - Rhetos packages Rhetos.CommonConceptsTest and Rhetos.RestGenerator deployed to Rhetos application at http://localhost:5000, with Windows authentication or anonymous access.
@REM
@REM Test URL samples:
@REM  - Standard URL escape codes: quotes (") => %22 and plus (+) => %2B
@REM  - Batch-file escape code: percent (%) => %%

@WHERE /Q curl || (ECHO Missing prerequisite: CURL. Please install 'CURL' command line utility and add it to system path. & EXIT /B 1)

@REM Generic filter:
curl "http://localhost:5000/rest/Common/Claim/?filters=[{%%22Property%%22:%%22ID%%22,%%22Operation%%22:%%22in%%22,%%22Value%%22:[%%224a5c23ff-6525-4e17-b848-185e082a6974%%22,%%220f6618b1-074a-482f-be74-c6e394641209%%22]}]" --ntlm --fail --globoff -u : || GOTO Error0

@REM Specific filter with simplified class name:
curl "http://localhost:5000/rest/Common/Claim/?filters=[{%%22Filter%%22:%%22Guid[]%%22,%%22Value%%22:[%%224a5c23ff-6525-4e17-b848-185e082a6974%%22,%%220f6618b1-074a-482f-be74-c6e394641209%%22]}]" --ntlm --fail --globoff -u : || GOTO Error0
curl "http://localhost:5000/rest/Common/Claim/?filters=[{%%22Filter%%22:%%22System.Guid[]%%22,%%22Value%%22:[%%224a5c23ff-6525-4e17-b848-185e082a6974%%22,%%220f6618b1-074a-482f-be74-c6e394641209%%22]}]" --ntlm --fail --globoff -u : || GOTO Error0
curl "http://localhost:5000/rest/Common/Claim/?filters=[{%%22Filter%%22:%%22System.Collections.Generic.IEnumerable`1[[System.Guid]]%%22,%%22Value%%22:[%%224a5c23ff-6525-4e17-b848-185e082a6974%%22,%%220f6618b1-074a-482f-be74-c6e394641209%%22]}]" --ntlm --fail --globoff -u : || GOTO Error0

@REM Legacy filters:
curl "http://localhost:5000/rest/Common/Claim/?filter=Guid[]&fparam=[%%224a5c23ff-6525-4e17-b848-185e082a6974%%22,%%220f6618b1-074a-482f-be74-c6e394641209%%22]" --ntlm --fail --globoff -u : || GOTO Error0
curl "http://localhost:5000/rest/Common/Claim/?filter=System.Guid[]&fparam=[%%224a5c23ff-6525-4e17-b848-185e082a6974%%22,%%220f6618b1-074a-482f-be74-c6e394641209%%22]" --ntlm --fail --globoff -u : || GOTO Error0
curl "http://localhost:5000/rest/Common/Claim/?filter=System.Collections.Generic.IEnumerable`1[[System.Guid]]&fparam=[%%224a5c23ff-6525-4e17-b848-185e082a6974%%22,%%220f6618b1-074a-482f-be74-c6e394641209%%22]" --ntlm --fail --globoff -u : || GOTO Error0

@REM Specific filter with parameter name:
curl "http://localhost:5000/rest/Common/MyClaim/?filters=[{%%22Filter%%22:%%22Common.Claim%%22,%%22Value%%22:{%%22ClaimResource%%22:%%22Common.RolePermission%%22,%%22ClaimRight%%22:%%22Read%%22}}]" --ntlm --fail --globoff -u : || GOTO Error0
curl "http://localhost:5000/rest/Common/MyClaim/?filters=[{%%22Filter%%22:%%22Claim%%22,%%22Value%%22:{%%22ClaimResource%%22:%%22Common.RolePermission%%22,%%22ClaimRight%%22:%%22Read%%22}}]" --ntlm --fail --globoff -u : || GOTO Error0

@REM Deactivatable:
@REM (THIS TEST REQUIRES Rhetos.CommonConceptsTest)
curl "http://localhost:5000/rest/TestDeactivatable/BasicEnt/" --ntlm --fail --globoff -u : || GOTO Error0
curl "http://localhost:5000/rest/TestDeactivatable/BasicEnt/?filters=[{%%22Filter%%22:%%22Rhetos.Dom.DefaultConcepts.ActiveItems,%%20Rhetos.Dom.DefaultConcepts.Interfaces%%22}]" --ntlm --fail --globoff -u : || GOTO Error0
curl "http://localhost:5000/rest/TestDeactivatable/BasicEnt/?filters=[{%%22Filter%%22:%%22Rhetos.Dom.DefaultConcepts.ActiveItems%%22}]" --ntlm --fail --globoff -u : || GOTO Error0
curl "http://localhost:5000/rest/TestDeactivatable/BasicEnt/?filters=[{%%22Filter%%22:%%22ActiveItems%%22}]" --ntlm --fail --globoff -u : || GOTO Error0

@REM DateTime (note that the "+" is escaped with "%2B"):
@REM (THIS TEST REQUIRES Rhetos.CommonConceptsTest)
curl "http://localhost:5000/rest/TestHistory/Standard/" --ntlm --fail --globoff -u : || GOTO Error0
curl "http://localhost:5000/rest/TestHistory/Standard/?filters=[{%%22Filter%%22:%%22System.DateTime%%22,%%22Value%%22:%%22/Date(1544195644420%%2B0100)/%%22}]" --ntlm --fail --globoff -u : || GOTO Error0

@REM ================================================

@ECHO.
@ECHO %~nx0 SUCCESSFULLY COMPLETED.
@EXIT /B 0

:Error0
@ECHO.
@ECHO %~nx0 FAILED.
@EXIT /B 1
