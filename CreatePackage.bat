@SET Config=%1%
@IF [%1] == [] SET Config=Debug

@REM Kopiranje pluginova za potrebe izrade Package-a
@IF NOT EXIST Plugins\ForDeployment\ MD Plugins\ForDeployment\
@DEL /F /S /Q Plugins\ForDeployment\* || EXIT /B 1
CALL CopyPlugins.bat Plugins\ForDeployment\ %Config%

..\..\Rhetos\Source\CreatePackage\bin\%Config%\CreatePackage.exe .

@RD /S /Q Plugins\ForDeployment\
