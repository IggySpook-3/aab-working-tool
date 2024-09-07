using System;
using System.Diagnostics;
using System.IO;

class AabToApkConverter
{
    static void Main(string[] args)
    {
         Console.WriteLine("============================================");
        Console.WriteLine("      Welcome to the AAB to APK Converter    ");
        Console.WriteLine("============================================\n");
        Console.WriteLine("Main functionalities:");
        Console.WriteLine("  1. Convert .aab files to .apks");
        Console.WriteLine("  2. Install .apks on a connected device");
        Console.WriteLine("  3. Extract and display AndroidManifest.xml\n");
        Console.WriteLine("If you encounter any errors, please message me via Slack!\n");

        Console.WriteLine("--------------------------------------------");
        Console.WriteLine("Please provide the required file paths:");
        Console.WriteLine("--------------------------------------------");

        // User input for files
        Console.WriteLine("Enter the path to the AAB file:");
        string aabFilePath = Console.ReadLine();
        Console.WriteLine("--------------------------------------------");

        Console.WriteLine("Enter the path to the bundletool.jar:");
        string bundletoolPath = Console.ReadLine();
        Console.WriteLine("--------------------------------------------");

        Console.WriteLine("Enter the output directory for the APKS:");
        string outputDirectory = Console.ReadLine();
        Console.WriteLine("--------------------------------------------\n");

        // Ensure files exist
        if (!File.Exists(aabFilePath))
        {
            Console.WriteLine("The specified AAB file does not exist.");
            return;
        }

        if (!File.Exists(bundletoolPath))
        {
            Console.WriteLine("The specified bundletool.jar file does not exist.");
            return;
        }

        if (!Directory.Exists(outputDirectory))
        {
            Console.WriteLine("The specified output directory does not exist. Creating directory...");
            Directory.CreateDirectory(outputDirectory);
        }

        string apkOutputPath = Path.Combine(outputDirectory, "output.apks");

        // Check if there is enough available space
        DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(outputDirectory));
        long availableSpace = driveInfo.AvailableFreeSpace;
        const long minimumRequiredSpace = 1L * 1024 * 1024 * 1024; // 1 GB

        if (availableSpace < minimumRequiredSpace)
        {
            Console.WriteLine("ERROR: Not enough disk space on the drive. Please free up space and try again.\n");
            return;
        }

        if (availableSpace < minimumRequiredSpace)
        {
            Console.WriteLine("Not enough disk space on the drive. Please free up space and try again.");
            return;
        }

        Console.WriteLine("Would you like to (1) Convert AAB to APKs or (2) Read AndroidManifest.xml? (Enter 1 or 2):");
        string userChoice = Console.ReadLine();

        try
        {
            if (userChoice == "1")
            {
                ConvertAabToApk(aabFilePath, bundletoolPath, apkOutputPath);
                Console.WriteLine("Conversion completed successfully. APK saved at: " + apkOutputPath);

                // Offer to install the APKS on a device
                Console.WriteLine("Would you like to install the APK on a connected device? (yes/no):");
                string installResponse = Console.ReadLine().ToLower();

                if (installResponse == "yes")
                {
                    InstallApksToDevice(bundletoolPath, apkOutputPath);
                }
            }
            else if (userChoice == "2")
            {
                ReadAndroidManifest(aabFilePath, bundletoolPath);
            }
            else
            {
                Console.WriteLine("Invalid choice. Please run the script again.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred during the process: " + ex.Message);
        }
    }

    static void ConvertAabToApk(string aabFilePath, string bundletoolPath, string apkOutputPath)
    {
        // Bundletool command for conversion
        string command = $"-jar \"{bundletoolPath}\" build-apks --bundle=\"{aabFilePath}\" --output=\"{apkOutputPath}\" --mode=universal";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "java",
            Arguments = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Bundletool error: {error}");
            }

            Console.WriteLine(output);
        }
    }

    static void InstallApksToDevice(string bundletoolPath, string apkOutputPath)
    {
        string command = $"-jar \"{bundletoolPath}\" install-apks --apks=\"{apkOutputPath}\"";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "java",
            Arguments = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Bundletool error during installation: {error}");
            }

            Console.WriteLine("APK installation completed successfully.");
            Console.WriteLine(output);
        }
    }

    static void ReadAndroidManifest(string aabFilePath, string bundletoolPath)
    {
        // Command to dump AndroidManifest.xml
        string command = $"-jar \"{bundletoolPath}\" dump manifest --bundle=\"{aabFilePath}\"";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "java",
            Arguments = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Bundletool error: {error}");
            }

            // Display the AndroidManifest.xml content
            Console.WriteLine("AndroidManifest.xml content:");
            Console.WriteLine(output);
        }
    }
}
