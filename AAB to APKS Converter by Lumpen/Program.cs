using System;
using System.Diagnostics;
using System.IO;

class AabToApkConverter
{
    static void Main(string[] args)
    {
        Console.WriteLine("Greetings!Thanks for using this script!");
        Console.WriteLine("Main functional: converting .aab files to .apks and installing them on connected device!");
        Console.WriteLine("If you catch an error, message me via Slack!");
        // юзер инпут для файлов 
        Console.WriteLine("Enter the path to the AAB file:");
        string aabFilePath = Console.ReadLine();

        Console.WriteLine("Enter the path to the bundletool.jar:");
        string bundletoolPath = Console.ReadLine();

        Console.WriteLine("Enter the output directory for the APKS:");
        string outputDirectory = Console.ReadLine();

        // убедиться, что файлы действительно существуют 
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

        // прооверка на наявность доступного места 
        DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(outputDirectory));
        long availableSpace = driveInfo.AvailableFreeSpace;
        const long minimumRequiredSpace = 5L * 1024 * 1024 * 1024; // 5 GB

        if (availableSpace < minimumRequiredSpace)
        {
            Console.WriteLine("Not enough disk space on the drive. Please free up space and try again.");
            return;
        }

        try
        {
            ConvertAabToApk(aabFilePath, bundletoolPath, apkOutputPath);
            Console.WriteLine("Conversion completed successfully. APK saved at: " + apkOutputPath);

            // предложить установить файл APKS на устройство 
            Console.WriteLine("Would you like to install the APK on a connected device? (yes/no):");
            string installResponse = Console.ReadLine().ToLower();

            if (installResponse == "yes")
            {
                InstallApksToDevice(bundletoolPath, apkOutputPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred during the process: " + ex.Message);
        }
    }

    static void ConvertAabToApk(string aabFilePath, string bundletoolPath, string apkOutputPath)
    {
        // команда бандлтула 
        string command = $"-jar \"{bundletoolPath}\" build-apks --bundle=\"{aabFilePath}\" --output=\"{apkOutputPath}\" --mode=universal";

        // информация про старт процесса 
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

            // дебаг
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            // ошибочки 
            if (process.ExitCode != 0)
            {
                throw new Exception($"Bundletool error: {error}");
            }

            Console.WriteLine(output);
        }
    }

    static void InstallApksToDevice(string bundletoolPath, string apkOutputPath)
    {
        // команда установки APKS 
        string command = $"-jar \"{bundletoolPath}\" install-apks --apks=\"{apkOutputPath}\"";

        // информация про старт процесса 
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

            // дебаг
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            // ошибочки 
            if (process.ExitCode != 0)
            {
                throw new Exception($"Bundletool error during installation: {error}");
            }

            Console.WriteLine("APK installation completed successfully.");
            Console.WriteLine(output);
        }
    }
}
