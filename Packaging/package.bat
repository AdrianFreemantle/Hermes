set var=0.5.0-test

d:\LocalNugetPackages\nuget pack Hermes.Core.nuspec -Version %var%
d:\LocalNugetPackages\nuget pack Hermes.Messaging.nuspec -Version %var%
d:\LocalNugetPackages\nuget pack Hermes.EntityFramework.nuspec -Version %var%
d:\LocalNugetPackages\nuget pack Hermes.OrmLite.nuspec -Version %var%
d:\LocalNugetPackages\nuget pack Hermes.ServiceHost.nuspec -Version %var%

move *.nupkg %LOCALAPPDATA%\NuGet\Cache

pause