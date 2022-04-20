using System;

namespace X1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Dojo dojo = new Dojo();
            dojo.run().Wait();
            Console.WriteLine("program ended");
        }
    }
}
