using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern bool DeleteFile(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern bool RemoveDirectory(string lpPathName);

    static void Main()
    {
        string rootDirectory = @"C:\";

        string[] wildcardNames = { "onedrive", "edge" };

        string programFilesPath = Path.Combine(rootDirectory, "Program Files");
        string system32Path = Path.Combine(rootDirectory, "Windows/System32");

        if (!Directory.Exists(programFilesPath) || !Directory.Exists(system32Path))
        {
            Console.WriteLine("error: directory does not appear to be the root directory of a Windows installation");
            Console.ReadLine();
            Environment.Exit(1);
        }

        Console.WriteLine("Removing unwanted files and directories...");

        string windowsAppsPath = Path.Combine(programFilesPath, "WindowsApps");

        GrantAccessToAdministrators(windowsAppsPath);

        if (Directory.Exists(windowsAppsPath))
        {
            SafeDeleteDirectory(windowsAppsPath);
        }
        else
        {
            Console.WriteLine($"Location '{windowsAppsPath}' not found.");
        }

        string programDataPath = Path.Combine(rootDirectory, "ProgramData/Packages");

        GrantAccessToAdministrators(programDataPath);

        if (Directory.Exists(programDataPath))
        {
            SafeDeleteDirectory(programDataPath);
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

                SafeDeleteDirectory(windowsAppsUserPath);
            }
            else
            {
                Console.WriteLine($"Location '{windowsAppsUserPath}' not found.");
            }

            string packagesUserPath = Path.Combine(userDirectory, "AppData/Local/Packages");
            if (Directory.Exists(packagesUserPath))
            {
                GrantAccessToAdministrators(packagesUserPath);

                SafeDeleteDirectory(packagesUserPath);
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
            SafeDeleteDirectory(systemAppsPath);
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

        string windowsFilePath = Path.Combine(system32Path, "mobsync.exe");

        if (File.Exists(windowsFilePath))
        {
            SafeDeleteFile(windowsFilePath);
        }
        else
        {
            Console.WriteLine($"File '{windowsFilePath}' not found.");
        }

        foreach (string wildcardName in wildcardNames)
        {
            string[] foundDirectories = Directory.GetDirectories(rootDirectory, $"*{wildcardName}*", SearchOption.AllDirectories);

            foreach (string foundDirectory in foundDirectories)
            {
                if (!foundDirectory.Contains("bin"))
                {
                    SafeDeleteDirectory(foundDirectory);
                }
            }
        }

        // Additional deletion commands
        string[] additionalDirectoriesToDelete =
        {
            @"ProgramData/Packages",
            @"Users/*/AppData/Local/Microsoft/WindowsApps",
            @"Users/*/AppData/Local/Packages",
            @"Windows/SystemApps"
        };

        foreach (string directoryToDelete in additionalDirectoriesToDelete)
        {
            string[] foundDirectories = Directory.GetDirectories(rootDirectory, directoryToDelete, SearchOption.AllDirectories);

            foreach (string foundDirectory in foundDirectories)
            {
                if (!foundDirectory.Contains("bin"))
                {
                    SafeDeleteDirectory(foundDirectory);
                }
            }
        }

        string[] additionalFilesToDelete =
        {
            @"Windows/System32/smartscreen.exe",
            @"Windows/System32/mobsync.exe"
        };

        foreach (string fileToDelete in additionalFilesToDelete)
        {
            string[] foundFiles = Directory.GetFiles(rootDirectory, fileToDelete, SearchOption.AllDirectories);

            foreach (string foundFile in foundFiles)
            {
                if (!foundFile.Contains("bin"))
                {
                    SafeDeleteFile(foundFile);
                }
            }
        }

        Console.WriteLine("Cleanup completed successfully.");
        Console.ReadLine();
        Environment.Exit(0);
    }

    static void SafeDeleteDirectory(string directoryPath)
    {
        try
        {
            RemoveDirectory(directoryPath);
            Console.WriteLine($"Location '{directoryPath}' found and deleted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting location '{directoryPath}': {ex.Message}");
        }
    }

    static void SafeDeleteFile(string filePath)
    {
        try
        {
            DeleteFile(filePath);
            Console.WriteLine($"File '{filePath}' found and deleted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file '{filePath}': {ex.Message}");
        }
    }

    static void GrantAccessToAdministrators(string path)
    {
        Process process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;

        process.Start();

        string takeOwnCommand = string.Format("TAKEOWN /F \"{0}\" /A", path);
        process.StandardInput.WriteLine(takeOwnCommand);
        string icaclsCommand = string.Format("ICACLS \"{0}\" /GRANT Administrators:(F)", path);
        process.StandardInput.WriteLine(icaclsCommand);
        process.StandardInput.Close();

        process.WaitForExit();
    }
}
