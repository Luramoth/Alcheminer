using System.Reflection;
using Alpacka;
using log4net;
using log4net.Config;

namespace Alpacker;

class Program
{
    internal static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
    internal static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    
    static int Main(string[] args)
    {
        using (var stream = Assembly.GetManifestResourceStream("Alpacker.log4net.config"))
        {
            if (stream != null)
            {
                XmlConfigurator.Configure(stream);
            }
            else
            {
                Logger.Warn("configuration doesn't exist. god is dead, continuing with default.");
            }
        }
        
        Logger.InfoFormat("Alpacker Version {0} | 2026 Luramoth", Assembly.GetName()!.Version!.ToString());

        if (args.Length < 2)
        {
            Console.WriteLine();
            Logger.Info("usage:");
            Logger.Info("  pack <input-dir> <output-dir>       Create .alpack archive");
            Logger.Info("  list <archive.alpack>               List contents");
            Logger.Info("  extract <archive.alpack> [out-dir]  Extract archive files");

            return 1;
        }

        var command = args[0].ToLower();

        try
        {
            switch (command)
            {
                case "pack":
                    Pack(args[1], args[2]);
                    break;
                case "list":
                    list(args[1]);
                    break;
                case "extract":
                    Extract(args[1], args.Length > 2 ? args[2] : ".");
                    break;
                default:
                    Logger.Error($"Unknown command: {command}");
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error: {ex.Message}");
            return 1;
        }

        return 0;
    }

    private static void Pack(string inputDir, string outputDir)
    {
        var fileName = $"{outputDir}pack.alpack";
        Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);

        using var writer = new Alpacka.AlpackWriter(fileName);

        var files = Directory.EnumerateFiles(inputDir, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(inputDir, file).Replace('\\', '/');
            
            writer.AddFileFromDisk(relativePath, file);
            Logger.Info($"  + {relativePath}");
        }
        
        writer.Finalise();
        Logger.Info($"Created: {fileName}");
        Logger.Info($"  Files: {Directory.EnumerateFiles(inputDir, "*", SearchOption.AllDirectories).Count()}");
    }

    private static void list(string archivePath)
    {
        using var reader = new AlpackReader(archivePath);
        
        Logger.Info($"Archive: {archivePath}");
        Logger.Info($"Files: {reader.EntryCount}");
        Console.WriteLine();

        foreach (var path in reader.ListFiles().OrderBy(p => p))
        {
            Logger.Info($"  {path}");
        }
    }

    private static void Extract(string archivePath, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        using var reader = new AlpackReader(archivePath);

        foreach (var path in reader.ListFiles())
        {
            var data = reader.ReadFile(path!);
            var outputPath = Path.Combine(outputDir, path!);

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            File.WriteAllBytes(outputPath, data);
            
            Logger.Info($"  -> {path}");
        }
        
        Logger.Info($"Extracted to: {outputDir}");
    }
}