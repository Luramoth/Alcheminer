using System.Diagnostics;

namespace AlchemyEngine.IO;

public class Fetcher
{
    /// <summary>
    /// Will grab the contents of a file stored inside the executor's assembly Through EmbeddedResource.
    /// To use it's recommended you put Include="Assets\**\*" in a EmbeddedResource tag inside the ItemGroup tag inside your game's .csproj file.
    /// </summary>
    /// <param name="path">A string specifying the path to your resource. ex: "Assets/Shader/shader.slang" would be "{GameNamespace}.Assets.Shader.shader.slang"</param>
    /// <returns>A string with the file's contents</returns>
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