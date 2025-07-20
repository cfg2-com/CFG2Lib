@echo off
CALL setRelease.bat

dotnet clean
dotnet build SQLiteLib/SQLiteLib.csproj -p:UseProjectReferences=false