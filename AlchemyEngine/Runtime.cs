using System.Diagnostics;
using log4net;
using log4net.Config;

namespace AlchemyEngine;
using System.Reflection;


public class Runtime
{
    internal static readonly Assembly AlchemyAssembly = Assembly.GetExecutingAssembly();
    internal static readonly Assembly GameAssembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException();
    
    internal static readonly ILog Logger = LogManager.GetLogger(typeof(Runtime));
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
        Logger.DebugFormat("{0}", WasCalledFromEngineAssembly());
    }
    
    public static bool WasCalledFromEngineAssembly()
    {
        var stackTrace = new StackTrace();
        
        return stackTrace.GetFrame(1)!.GetMethod()?.DeclaringType?.Assembly == AlchemyAssembly;
    }
}