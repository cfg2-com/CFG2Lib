@echo off
CALL setRelease.bat

dotnet build SysLib/SysLib.csproj -p:UseProjectReferences=false