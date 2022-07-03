using System;
using System.IO;
using System.Reflection;

namespace Tests;

public static class ReflectionExtensions {
    public static T GetFieldValue<T>(this object obj, string name) {
        // Set the flags so that private and public fields from instances will be found
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var field = obj.GetType().GetField(name, bindingFlags);
        if (field?.GetValue(obj) is T myValue)
            return myValue;
       
        return default!;
    }
    
}

public static class PathUtil {

    public static string GetTestPath(string relativePath)
    {
        var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
        var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
        var dirPath = Path.GetDirectoryName(codeBasePath) ?? "";

        System.Console.WriteLine(dirPath);

        if(relativePath.Length > 0){
            return Path.Combine(dirPath, "Resources", relativePath);
        } else {
            return Path.Combine(dirPath, "Resources");
        }
    }
}
