@echo off
@setlocal

dotnet restore src\Veldrid.sln

set UseStableVersions=true

dotnet pack -c Release src\Veldrid.OpenGLBindings\Veldrid.OpenGLBindings.csproj
dotnet pack -c Release src\Veldrid.MetalBindings\Veldrid.MetalBindings.csproj
dotnet pack -c Release src\Veldrid\Veldrid.csproj
