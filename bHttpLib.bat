@echo off
CALL setRelease.bat

dotnet clean
dotnet build HttpLib/HttpLib.csproj -p:UseProjectReferences=false