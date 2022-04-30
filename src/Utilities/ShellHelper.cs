using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Utilities;

public static class ShellHelper
{
    public static Task<int> Bash(this string cmd)
    {
        var source = new TaskCompletionSource<int>();
        var escapedArgs = cmd.Replace("\"", "\\\"");
        var process = new Process {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    // RedirectStandardOutput = false,
                    // RedirectStandardError = true,
                    UseShellExecute = true
                },
                EnableRaisingEvents = true
        };
        process.Exited += (sender, args) =>
        {
            if (process.ExitCode == 0) {
                source.SetResult(0);
            } else {
                source.SetException(new Exception($"Command `{cmd}` failed with exit code `{process.ExitCode}`"));
            }
            process.Dispose();    
        };

        try {
            process.Start();
        } catch (Exception e) {
            source.SetException(e);
        }
        return source.Task;
    }
}