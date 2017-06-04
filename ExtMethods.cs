using System;
using System.Linq;

namespace CSharpSamples {
	static class StringEx {
		public static string Filter(this string s, Func<char, bool> filter) {
			return new string(s.Where(filter).ToArray());
		}
	}
	class ExtMethods {
		public static void Test() {
			var str = "123abc";
			var fstr = str.Filter(c => char.IsNumber(c));
			Console.WriteLine(fstr);
		}
	}
}
