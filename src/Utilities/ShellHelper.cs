using System.Diagnostics;
using Microsoft.Extensions.Logging;

public static class ShellHelper
{
    public static Task<int> Bash(this string cmd)
    {
        var source = new TaskCompletionSource<int>();
        var escapedArgs = cmd.Replace("\"", "\\\"");
        var process = new Process {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
        };
        process.Exited += (sender, args) =>
        {
            System.Console.WriteLine(process.StandardError.ReadToEnd());
            System.Console.WriteLine(process.StandardOutput.ReadToEnd());
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
            System.Console.WriteLine(e);
            source.SetException(e);
        }
        return source.Task;
    }
}