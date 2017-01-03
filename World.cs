using System;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpSamples
{
    public class World
    {
        public Task<World> BuildAsync() 
        {
            return Task<World>.Factory.StartNew(Build);
        }

       // Long operation
        World Build() 
        {
            for( int i = 0; i < 10; i++ ) 
            {
                Thread.Sleep(500);
                var percent = (i + 1) * 10;
                Console.WriteLine(string.Format("{0}%", percent));
            }
            return this;
        }
    }
}