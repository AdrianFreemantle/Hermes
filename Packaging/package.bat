rd /S /Q Packages
mkdir Packages

nuget pack Hermes.Core.nuspec
nuget pack Hermes.EntityFramework.nuspec
nuget pack Hermes.SeriviceStack.nuspec
nuget pack Hermes.ServiceHost.nuspec

move *.nupkg Packages\

pause