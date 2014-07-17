set var=0.11.1

d:\LocalNugetPackages\nuget pack Hermes.Core.nuspec -Version %var%
d:\LocalNugetPackages\nuget pack Hermes.Messaging.nuspec -Version %var%
d:\LocalNugetPackages\nuget pack Hermes.EntityFramework.nuspec -Version %var%
d:\LocalNugetPackages\nuget pack Hermes.ServiceHost.nuspec -Version %var%

move *.nupkg D:\\LocalNugetPackages

pause