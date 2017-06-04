using System;
using System.Collections.Immutable;

namespace CSharpSamples {
	class Immutable {

		public static void Test() {
			var builder = ImmutableList.CreateBuilder<string>();
			builder.Add("123");
			var list = builder.ToImmutable();
			var list2 = list.Add("234");
			Console.WriteLine(ReferenceEquals(list, list2));
		}
	}
}
