using System.Collections.Generic;
using System.Threading.Tasks;

namespace CliTest.Core
{
    public interface ITestHistory
    {
        bool HasPreviouslyFailed(IDotnetTest test);
        Task LoadFailedTests();
        Task SaveFailedTests(IEnumerable<IDotnetTest> tests);
    }
}
