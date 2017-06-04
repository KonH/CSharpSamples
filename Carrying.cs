using System;

namespace CSharpSamples {
	public class Carrying {
		static class Logger {
			public static void Log(string from, string what) {
				Console.WriteLine("{0} says: {1}", from, what);
			}
		}

		class Worker {
			Action<string> logMethod;

			public Worker() {
				logMethod = ApplyPartial<string, string>(Logger.Log, "Worker");
			}

			public void DoWork() {
				logMethod("I do some work");
			}
		}

		static Action<T2> ApplyPartial<T1, T2>(Action<T1, T2> method, T1 arg0) {
			return (arg1) => method(arg0, arg1);
		}

		public static void Test() {
			var w = new Worker();
			w.DoWork();
		}
	}
}