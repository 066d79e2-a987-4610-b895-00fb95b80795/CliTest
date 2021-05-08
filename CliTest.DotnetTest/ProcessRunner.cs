using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace CliTest.DotnetTest
{
    // XXX This should be internal
    public static class ProcessRunner
    {
        public static async Task<ProcessResult> RunProcess(string program, params string[] arguments)
        {
            var outputBuilder = new StringBuilder();
            var process = new Process();
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = program;
            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }
            // XXX I could color stdout red
            process.OutputDataReceived += (_, e) => outputBuilder.AppendLine(e.Data);
            process.ErrorDataReceived += (_, e) => outputBuilder.AppendLine(e.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            return new ProcessResult(outputBuilder.ToString(), process.ExitCode);
        }
    }

    public record ProcessResult(string Output, int ExitCode);
}
