using System;

namespace test
{
    public class test1
    {
        public static long timeStamp = Convert.ToInt64((DateTime.Now - DateTime.Parse("1/1/1970 0:0:0")).TotalMilliseconds);

        public main void test2()
        {
            Console.WriteLine(timeStamp);
        }
        
    }
}