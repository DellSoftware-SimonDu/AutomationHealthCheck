using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace test
{
    [TestFixture]
    public class Class1
    {
        public static long timeStamp = Convert.ToInt64((DateTime.Now - DateTime.Parse("1/1/1970 0:0:0")).TotalMilliseconds);
        [TestCase]
        

        public void test2()
        {
            //ISO8601TimestampUTC": "2016-08-10T01:00:00.000Z",
            Console.WriteLine(timeStamp);
            
            var  isoTimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:fffZ");
            Console.WriteLine(isoTimeStamp);
            Console.WriteLine(utcTimestamp);
        }
    }
}

