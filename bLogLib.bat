@echo off
CALL setRelease.bat

dotnet build LogLib/LogLib.csproj -p:UseProjectReferences=false