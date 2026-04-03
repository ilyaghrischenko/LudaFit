using DotNetEnv;

namespace LudaFit.Core.Extensions;

internal static class EnvExtensions
{
    public static IEnumerable<KeyValuePair<string, string>> LoadOrThrow(string pathToEnv = ".env")
    {
        if (!File.Exists(pathToEnv))
        {
            throw new EnvVariableNotFoundException(".env file not found", pathToEnv);
        }
        
        LoadOptions loadOptions = new(onlyExactPath: true);
        
        return Env.Load(options: loadOptions);
    }
}
