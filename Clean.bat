@REM Delete all "bin", "obj" and "TestResults" subfolders:
powershell "Get-ChildItem -Path '%~dp0' -Recurse -Directory -Filter 'bin' | ForEach-Object { echo \"Deleting $($_.FullName)\"; Remove-Item $_.FullName -Recurse -Force }"
powershell "Get-ChildItem -Path '%~dp0' -Recurse -Directory -Filter 'obj' | ForEach-Object { echo \"Deleting $($_.FullName)\"; Remove-Item $_.FullName -Recurse -Force }"
powershell "Get-ChildItem -Path '%~dp0' -Recurse -Directory -Filter 'TestResults' | ForEach-Object { echo \"Deleting $($_.FullName)\"; Remove-Item $_.FullName -Recurse -Force }"
