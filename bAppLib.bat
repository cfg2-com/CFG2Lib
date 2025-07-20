@echo off
CALL setRelease.bat

dotnet clean
dotnet build AppLib/AppLib.csproj -p:UseProjectReferences=false