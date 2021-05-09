using System.Collections.Generic;
using System.Threading.Tasks;

namespace CliTest.Core
{
    public interface IProcessRunner
    {
        Task<ProcessResult> RunProcess(string program, IEnumerable<string> arguments);
    }

    public record ProcessResult(string Output, int ExitCode);
}
