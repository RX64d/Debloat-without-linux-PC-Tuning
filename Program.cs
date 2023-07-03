using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

class Program
{
    static void Main()
    {
        string rootDirectory = @"C:\";

        string[] additionalFilesToDelete =
        {
            @"C:\Windows\SystemApps\Microsoft.Windows.FileExplorer_cw5n1h2txyewy"
        };

        Console.WriteLine("Removing unwanted files and directories...");

        foreach (string fileToDelete in additionalFilesToDelete)
        {
            if (File.Exists(fileToDelete))
            {
                SafeDeleteFile(fileToDelete);
            }
            else
            {
                Console.WriteLine($"File '{fileToDelete}' not found.");
            }
        }

        string programFilesPath = Path.Combine(rootDirectory, "Program Files");
        string system32Path = Path.Combine(rootDirectory, "Windows/System32");

        if (!Directory.Exists(programFilesPath) || !Directory.Exists(system32Path))
        {
            Console.WriteLine("Error: Directory does not appear to be the root directory of a Windows installation.");
            Console.ReadLine();
            Environment.Exit(1);
        }

        string windowsAppsPath = Path.Combine(programFilesPath, "WindowsApps");

        GrantAccessToAdministrators(windowsAppsPath);

        if (Directory.Exists(windowsAppsPath))
        {
            ExecutePowerShellCommand($"Remove-Item -Path \"{windowsAppsPath}\" -Recurse -Force");
        }
        else
        {
            Console.WriteLine($"Location '{windowsAppsPath}' not found.");
        }

        string programDataPath = Path.Combine(rootDirectory, "ProgramData/Packages");

        GrantAccessToAdministrators(programDataPath);

        if (Directory.Exists(programDataPath))
        {
            ExecutePowerShellCommand($"Remove-Item -Path \"{programDataPath}\" -Recurse -Force");
        }
        else
        {
            Console.WriteLine($"Location '{programDataPath}' not found.");
        }

        string usersAppDataPath = Path.Combine(rootDirectory, "Users");
        string[] userDirectories = Directory.GetDirectories(usersAppDataPath, "*", SearchOption.TopDirectoryOnly);

        foreach (string userDirectory in userDirectories)
        {
            string windowsAppsUserPath = Path.Combine(userDirectory, "AppData/Local/Microsoft/WindowsApps");
            if (Directory.Exists(windowsAppsUserPath))
            {
                GrantAccessToAdministrators(windowsAppsUserPath);

                ExecutePowerShellCommand($"Remove-Item -Path \"{windowsAppsUserPath}\" -Recurse -Force");
            }
            else
            {
                Console.WriteLine($"Location '{windowsAppsUserPath}' not found.");
            }

            string packagesUserPath = Path.Combine(userDirectory, "AppData/Local/Packages");
            if (Directory.Exists(packagesUserPath))
            {
                GrantAccessToAdministrators(packagesUserPath);

                ExecutePowerShellCommand($"Remove-Item -Path \"{packagesUserPath}\" -Recurse -Force");
            }
            else
            {
                Console.WriteLine($"Location '{packagesUserPath}' not found.");
            }
        }

        string systemAppsPath = Path.Combine(rootDirectory, "Windows/SystemApps");

        GrantAccessToAdministrators(systemAppsPath);

        if (Directory.Exists(systemAppsPath))
        {
            ExecutePowerShellCommand($"Remove-Item -Path \"{systemAppsPath}\" -Recurse -Force");
        }
        else
        {
            Console.WriteLine($"Location '{systemAppsPath}' not found.");
        }

        string system32FilePath = Path.Combine(system32Path, "smartscreen.exe");

        if (File.Exists(system32FilePath))
        {
            SafeDeleteFile(system32FilePath);
        }
        else
        {
            Console.WriteLine($"File '{system32FilePath}' not found.");
        }

        Console.WriteLine("Cleanup complete.");
        Console.ReadLine();
    }

    static void GrantAccessToAdministrators(string path)
    {
        Process process = new Process();
        process.StartInfo.FileName = "icacls.exe";
        process.StartInfo.Arguments = $"\"{path}\" /grant administrators:F";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        process.WaitForExit();
    }

    static void SafeDeleteFile(string path)
    {
        try
        {
            File.SetAttributes(path, FileAttributes.Normal);
            File.Delete(path);
            Console.WriteLine($"Deleted file: {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file '{path}': {ex.Message}");
        }
    }

    static void ExecutePowerShellCommand(string command)
    {
        Process process = new Process();
        process.StartInfo.FileName = "powershell.exe";
        process.StartInfo.Arguments = $"-Command \"{command}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        process.WaitForExit();
    }
}
