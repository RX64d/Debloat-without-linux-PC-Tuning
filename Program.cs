using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main()
    {
        if (IsWindows11())
        {
            Console.WriteLine("Windows 11 detected. Skipping file deletion.");
            Console.ReadLine();
            return;
        }

        string rootDirectory = @"C:\";

        Console.WriteLine("Removing unwanted files and directories...");

        string programFilesPath = Path.Combine(rootDirectory, "Program Files");
        string system32Path = Path.Combine(rootDirectory, "Windows", "System32");

        if (!Directory.Exists(programFilesPath) || !Directory.Exists(system32Path))
        {
            Console.WriteLine("Error: Directory does not appear to be the root directory of a Windows installation.");
            Console.ReadLine();
            Environment.Exit(1);
        }

        string windowsAppsPath = Path.Combine(programFilesPath, "WindowsApps");
        RemoveDirectory(windowsAppsPath);

        string programDataPath = Path.Combine(rootDirectory, "ProgramData", "Packages");
        RemoveDirectory(programDataPath);

        string usersAppDataPath = Path.Combine(rootDirectory, "Users");
        DirectoryInfo usersDirectory = new DirectoryInfo(usersAppDataPath);
        DirectoryInfo[] userDirectories = usersDirectory.GetDirectories();

        Parallel.ForEach(userDirectories, userDirectory =>
        {
            string windowsAppsUserPath = Path.Combine(userDirectory.FullName, "AppData", "Local", "Microsoft", "WindowsApps");
            string packagesUserPath = Path.Combine(userDirectory.FullName, "AppData", "Local", "Packages");

            RemoveDirectory(windowsAppsUserPath);
            RemoveDirectory(packagesUserPath);
        });

        string systemAppsPath = Path.Combine(rootDirectory, "Windows", "SystemApps");
        RemoveDirectory(systemAppsPath);

        string system32FilePath = Path.Combine(system32Path, "smartscreen.exe");
        ExecutePowerShellCommand($"Remove-Item -Path \"{system32FilePath}\" -Force");

        Console.WriteLine("Cleanup complete.");
        Console.ReadLine();
    }

    static void RemoveDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            GrantAccessToAdministrators(directoryPath);
            ExecutePowerShellCommand($"Remove-Item -Path \"{directoryPath}\" -Recurse -Force");
        }
        else
        {
            Console.WriteLine($"Location '{directoryPath}' not found.");
        }
    }

    static void GrantAccessToAdministrators(string path)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "icacls.exe";
        startInfo.Arguments = $"\"{path}\" /grant administrators:F";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.CreateNoWindow = true;

        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }

    static void ExecutePowerShellCommand(string command)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "powershell.exe";
        startInfo.Arguments = $"-Command \"{command}\"";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.CreateNoWindow = true;

        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }

    static bool IsWindows11()
    {
        var osVersion = Environment.OSVersion;
        if (osVersion.Platform == PlatformID.Win32NT && osVersion.Version.Major == 10 && osVersion.Version.Minor >= 0 && osVersion.Version.Build >= 22000)
        {
            return true;
        }

        return false;
    }
}
