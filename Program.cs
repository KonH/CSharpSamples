using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharpSamples
{
    public class Program
    {
        static List<Case> Cases = new List<Case>();

        public static void Main(string[] args)
        {
            InitCases();
            ProcessInput();
        }

        static void InitCases() 
        {
            AddCase("async", "some async features", () => AsyncTest());
            AddCase("carrying", "one functional feature", () => Carrying.Test());
            AddCase("lazy", "lazy init", () => LazyTest.Process());
        }

        static void AddCase(string name, string description, Action callback) {
            Cases.Add(new Case(name, description, callback));
        }

        static void ProcessInput() {
            Console.WriteLine("Select Test:");
            for(int i = 0; i < Cases.Count; i++)
            {
                var c = Cases[i];
                Console.WriteLine(string.Format("{0}. {1} ({2})", i, c.Name, c.Description));
            }
            var input = Console.ReadLine();
            var number = int.Parse(input);
            if( number >= 0 && number < Cases.Count ) 
            {
                var c = Cases[number];
                c.Callback();
            }
            Console.ReadKey();
        }

        static void AsyncTest() {
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
