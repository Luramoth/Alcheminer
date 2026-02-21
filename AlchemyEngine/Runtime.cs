using System.Diagnostics;
using AlchemyEngine.Core.Ecs;
using log4net;
using log4net.Config;

namespace AlchemyEngine;
using System.Reflection;

/// <summary>
/// The alchemy engine itself
/// </summary>
public class Runtime
{
    internal static readonly Assembly AlchemyAssembly = Assembly.GetExecutingAssembly();
    internal static readonly Assembly GameAssembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException();
    
    internal static readonly ILog Logger = LogManager.GetLogger(typeof(Runtime));

    /// <summary>
    /// Runtime initialised <see cref="EntityManager"/>, refer to <see cref="EntityManager"/> documentation.
    /// </summary>
    public static EntityManager EntityManager { get; private set; } = null!;
    
    /// <summary>
    /// The function that starts up the Alchemy Engine runtime. Required to run before any use of the engine.
    /// </summary>
    public static void Init()
    {
        BasicConfigurator.Configure();
        Logger.InfoFormat("Initializing Alchemy Engine for {0}", GameAssembly?.GetName().Name);

        using (var stream = AlchemyAssembly.GetManifestResourceStream("AlchemyEngine.log4net.config"))
        {
            if (stream != null)
            {
                XmlConfigurator.Configure(stream);
            }
            else
            {
                Logger.Warn("AlchemyEngine Logger configuration doesn't exist. god is dead, continuing with default.");
            }
        }

        Logger.Info("Logger Initialised.");

        EntityManager = new EntityManager();
        Logger.Info("Entity Manager Initialised");
    }
    
    /// <summary>
    /// Will check to see if the function being called (whatever function runs this check) is being run by the game or the engine
    /// </summary>
    /// <returns>True if last function is being called by AlchemyEngine.dll</returns>
    public static bool WasCalledFromEngineAssembly()
    {
        var stackTrace = new StackTrace();
        
        return stackTrace.GetFrame(1)!.GetMethod()?.DeclaringType?.Assembly == AlchemyAssembly;
    }
}