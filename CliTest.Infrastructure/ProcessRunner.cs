using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using CliTest.Core;

namespace CliTest.Infrastructure
{
    internal class ProcessRunner : IProcessRunner
    {
        public async Task<ProcessResult> RunProcess(string program, IEnumerable<string> arguments)
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
            process.OutputDataReceived += (_, e) => outputBuilder.AppendLine(e.Data);
            process.ErrorDataReceived += (_, e) => outputBuilder.AppendLine(e.Data);
            _ = process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            return new ProcessResult(outputBuilder.ToString(), process.ExitCode);
        }
    }

    public static class ProcessRunnerFactory
    {
        public static IProcessRunner Create() => new ProcessRunner();
    }
}
