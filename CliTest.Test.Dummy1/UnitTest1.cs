using System;
using System.Threading.Tasks;
using Xunit;

namespace CliTest.Test.Dummy1
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            await Task.Delay(TimeSpan.FromSeconds(1.5));
        }
    }
}
