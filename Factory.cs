using System.Collections.Generic;

namespace AsyncTest
{
    public class Factory
    {
        // Infinity content generator
        public IEnumerable<string> CreateCities()
        {
            int index = 0;
            while(true)
            {
                index++;
                yield return "City_" + index;
            }
        }
    }
}