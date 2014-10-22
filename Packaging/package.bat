set var=0.15.7

c:\LocalNugetPackages\nuget pack Hermes.Core.nuspec -Version %var%
c:\LocalNugetPackages\nuget pack Hermes.Messaging.nuspec -Version %var%
c:\LocalNugetPackages\nuget pack Hermes.EntityFramework.nuspec -Version %var%
c:\LocalNugetPackages\nuget pack Hermes.ServiceHost.nuspec -Version %var%

move *.nupkg c:\\LocalNugetPackages

pause