using System;

namespace CSharpSamples {
	public static class StringFormat {
		public static void Test() {
			var testVar = 42;
			var testVar2 = 32;
			Console.WriteLine($"Test: {testVar}, {{0}}", testVar2);
		}
	}
}