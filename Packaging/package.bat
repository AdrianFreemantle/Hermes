rd /S /Q Packages
mkdir Packages

c:\LocalNugetPackages\nuget pack Hermes.Core.nuspec
c:\LocalNugetPackages\nuget pack Hermes.Messaging.nuspec
c:\LocalNugetPackages\nuget pack Hermes.EntityFramework.nuspec
c:\LocalNugetPackages\nuget pack Hermes.SeriviceStack.nuspec
c:\LocalNugetPackages\nuget pack Hermes.ServiceHost.nuspec

move *.nupkg c:\LocalNugetPackages\

pause