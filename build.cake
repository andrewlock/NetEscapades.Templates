var target = Argument("Target", "Default");

var buildNumber = HasArgument("BuildNumber") ?
    Argument<int>("BuildNumber") :
    AppVeyor.IsRunningOnAppVeyor ? AppVeyor.Environment.Build.Number :
    EnvironmentVariable("BuildNumber") != null ? int.Parse(EnvironmentVariable("BuildNumber")) :
    0;
var isTag = EnvironmentVariable("APPVEYOR_REPO_TAG") != null && EnvironmentVariable("APPVEYOR_REPO_TAG") == "true" ;
var preReleaseSuffix = isTag ? null : "beta";

var artifactsDirectory = Directory("./artifacts");
var packagesDirectory = Directory("./packages");
var nuspecFile = GetFiles("./**/*.nuspec").First().ToString();
var nuspecContent = string.Empty;

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDirectory);
        DeleteDirectories(GetDirectories("**/bin"), true);
        DeleteDirectories(GetDirectories("**/obj"), true);
    });


Task("Version")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        string versionSuffix = string.Empty;
        if (!string.IsNullOrEmpty(preReleaseSuffix))
        {
            versionSuffix = "-" + preReleaseSuffix + "-" + buildNumber.ToString("D4");
        }

        nuspecContent = System.IO.File.ReadAllText(nuspecFile);
        System.IO.File.WriteAllText(nuspecFile, nuspecContent.Replace("-*", versionSuffix));
        Information("VersionSuffix set to " + versionSuffix);
    });


Task("Pack")
    .IsDependentOn("Version")
    .Does(() =>
    {
        NuGetPack(
            nuspecFile,
            new NuGetPackSettings()
            {
                OutputDirectory = artifactsDirectory
            });
        System.IO.File.WriteAllText(nuspecFile, nuspecContent);
    });

Task("Default")
    .IsDependentOn("Pack");

RunTarget(target);
