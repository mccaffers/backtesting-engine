using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Utilities;

public static class ShellHelper
{

    public static string RunCommandWithBash(this string command)
    {
        var escapedArgs = command.Replace("\"", "\\\"");
        var psi = new ProcessStartInfo();
        psi.FileName = "/bin/bash";
        psi.Arguments =  $"-c \"{escapedArgs}\"";
        psi.RedirectStandardOutput = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        using var process = Process.Start(psi);

        process?.WaitForExit();
        return process?.StandardOutput.ReadToEnd() ?? "";
    }

    public static Task<int> Bash(this string cmd)
    {
        var source = new TaskCompletionSource<int>();
        var escapedArgs = cmd.Replace("\"", "\\\"");
        var process = new Process {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
        };

        process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>{
            ConsoleLogger.SystemLog(e.Data ?? "");
        };

        process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>{
            if(!string.IsNullOrEmpty(e.Data)){
                ConsoleLogger.SystemLog(e.Data);
            }
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
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

        } catch (Exception e) {
            source.SetException(e);
        }
        return source.Task;
    }
}