using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Solution] readonly Solution Solution;
    
    // If the solution name and the project (plugin) name are different, then indicate the project (plugin) name here
    string PluginName => Solution.Name;

    Target Compile => _ => _
        .Executes(() =>
        {
            var project = Solution.GetProject(PluginName);
            if (project == null)
                throw new FileNotFoundException("Not found!");
            
            var postBuild = Environment.GetEnvironmentVariable("ModPlusPostBuild");
            var build = new List<string>();
            foreach (var (_, c) in project.Configurations)
            {
                var configuration = c.Split("|")[0];
                var platform = c.Split("|")[1];

                if (configuration == "Debug" || build.Contains(configuration))
                    continue;

                Logger.Success($"Configuration: {configuration}");

                build.Add(configuration);

                MSBuild(_ => _
                    .SetProjectFile(project.Path)
                    .SetConfiguration(configuration)
                    .SetTargetPlatform(MSBuildTargetPlatform.x64)
                    .SetTargets("Restore"));
                MSBuild(_ => _
                    .SetProjectFile(project.Path)
                    .SetConfiguration(configuration)
                    .SetTargetPlatform(MSBuildTargetPlatform.x64)
                    .SetTargets("Rebuild"));

                if (File.Exists(postBuild))
                {
                    var targetPath = Path.Combine(
                        project.Directory, 
                        "bin", 
                        platform,
                        configuration, 
                        $"{project.Name}_{configuration.Replace("R", string.Empty)}.dll");
                    Logger.Success($"TargetPath: {targetPath}");
                    Process.Start(postBuild, $"Revit \"{targetPath}\" s");
                }
                else
                    Logger.Warn("ModPlus PostBuild application not found");
            }
        });
}
