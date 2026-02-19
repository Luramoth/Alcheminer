using System.Diagnostics;

namespace AlchemyEngine.IO;

public class Fetcher
{
    public static string GetFileContents(string path)
    {
        using var stream = (Runtime.WasCalledFromEngineAssembly()) ? Runtime.AlchemyAssembly.GetManifestResourceStream(path) : Runtime.GameAssembly.GetManifestResourceStream(path);

        if (stream == null)
        {
            return "null";
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}