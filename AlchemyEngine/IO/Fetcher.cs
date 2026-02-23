using System.Diagnostics;
using System.Reflection;

namespace AlchemyEngine.IO;

/// <summary>
/// Class in which to retrieve Resources from
/// </summary>
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

    /// <summary>
    /// Will grab the contents of a file stored inside the executor's assembly Through EmbeddedResource.
    /// To use it's recommended you put Include="Assets\**\*" in a EmbeddedResource tag inside the ItemGroup tag inside your game's .csproj file.
    /// </summary>
    /// <param name="path">A string specifying the path to your resource. ex: "Assets/Shader/shader.slang" would be "{GameNamespace}.Assets.Shader.shader.slang"</param>
    /// <param name="assembly">The assembly to get the EmbeddedResource from</param>
    /// <returns>A string with the file's contents</returns>
    public static string GetFileContents(string path, Assembly assembly)
    {
        using var stream = assembly.GetManifestResourceStream(path);

        if (stream == null)
        {
            return "null";
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}