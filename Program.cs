using System;
using System.Threading.Tasks;

namespace AsyncTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var t = MainAsync();
            t.Wait();
        }

        static async Task<object> MainAsync() 
        {
            Console.WriteLine("World is created...");
            var world = new World();
            await world.BuildAsync();
            Console.WriteLine("Hello World!");
            var factory = new Factory();
            var cities = factory.CreateCities().GetEnumerator();
            for( int i = 0; i < 10; i++) 
            {
                cities.MoveNext();
                Console.WriteLine(cities.Current);
            }
            return new Task<object>(() => "");
        }
    }
}
