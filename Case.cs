using System;

namespace CSharpSamples
{
    public class Case
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public Action Callback { get; private set; }

        public Case(string name, string description, Action callback)
        {
            Name = name;
            Description = description;
            Callback = callback;
        }
    }
}