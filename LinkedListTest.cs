using System;
using System.Collections.Generic;

namespace CSharpSamples
{
    public static class LinkedListTest
    {
        public static void Test() 
		{
			LinkedList<string> list = new LinkedList<string>();
			var node = list.AddFirst("root");
			list.AddAfter(node, "after");
			list.AddBefore(node, "before");
			Console.WriteLine("List:");
			foreach( var item in list ) {
				Console.WriteLine(item);
			}
			Console.WriteLine($"First: {list.First.Value}, last: {list.Last.Value}");
		}
    }
}