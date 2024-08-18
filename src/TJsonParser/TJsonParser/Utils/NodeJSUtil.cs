using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TJsonParser.Utils;

public static class NodeJSUtil
{
    public static async Task<string> GetKeyAsync(string enc)
    {
        var jsContent = $"console.log(666);";
        var filePath = Path.Combine("Tools", "decode.js");
        await File.WriteAllTextAsync(filePath, jsContent);
        return await RunJsAsync(filePath);
    }

    private static async Task<string> RunJsAsync(string jsPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = jsPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null) return "";
        var outputTask = process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (await outputTask).Trim();
    }
}