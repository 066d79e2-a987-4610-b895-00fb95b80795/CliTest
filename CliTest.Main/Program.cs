using System.Threading.Tasks;
using CliTest.Core;
using CliTest.Infrastructure;

namespace CliTest.Main
{
    internal static class Program
    {
        internal static async Task<int> Main(string[] args)
        {
            var options = CliParser.Parse(args);

            if (options == null)
            {
                return 1;
            }

            var testRunner = TestRunnerFactory.Create(
                AnsiTerminalFactory.Create(),
                ProcessRunnerFactory.Create(),
                LocalTestHistoryFactory.Create(),
                options);
            await testRunner.MainLoop();
            return 0;
        }
    }
}
