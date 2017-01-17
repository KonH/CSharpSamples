using System;

namespace CSharpSamples
{
    public class LazyTest
    {
        class Singleton {
            public static Lazy<Singleton> Instance = new Lazy<Singleton>(() => new Singleton());

            static Singleton() {
                Console.WriteLine("Static constructor");
            }

            public Singleton() 
            {
                Console.WriteLine("Instance consructor");
            }

            public static void StaticMethod() 
            {
                Console.WriteLine("Some logics without instance");
            }

            public void InstanceMethod() {
                Console.WriteLine("Some logics with instance");
            }
        }

        public static void Process() 
        {
            Singleton.StaticMethod();
            var instance = Singleton.Instance;
            Console.WriteLine("Does not call any instance methods");
            instance.Value.InstanceMethod();
        }
    }
}