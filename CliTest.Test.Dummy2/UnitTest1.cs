using System;
using System.Threading.Tasks;
using Xunit;

namespace CliTest.Test.Dummy2
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            await Task.Delay(TimeSpan.FromSeconds(3.5));
            Assert.True(false);
        }
    }
}
