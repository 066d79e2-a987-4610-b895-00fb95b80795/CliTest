using System.Collections.Generic;
using System.Linq;

namespace CliTest.Core
{
    public static class ListExtensions
    {
        public static T PopFirst<T>(this List<T> l)
        {
            var result = l.First();
            l.RemoveAt(0);
            return result;
        }
    }
}
