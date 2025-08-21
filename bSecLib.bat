@echo off
CALL setRelease.bat

dotnet build SecLib/SecLib.csproj -p:UseProjectReferences=false