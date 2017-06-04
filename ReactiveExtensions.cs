using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace CSharpSamples {
	class ReactiveExtensions {
		static ISubject<string> _field = new Subject<string>();

		public static void Test() {
			_field.Subscribe((newValue) => OnNewValue(newValue));
			_field.OnNext("test");
			_field.Where((line) => line.Contains("test")).Subscribe(OnFilteredChanged);
			_field.OnNext("test");
			_field.OnNext("123");
			_field.Subscribe((_) => {}, (ex) => Console.WriteLine($"Error: {ex.Message}"), () => Console.WriteLine("Completed"));
			_field.OnCompleted();
		}

		static void OnNewValue(string newValue) {
			Console.WriteLine($"New value: {newValue}");
		}

		static void OnFilteredChanged(string newValue) {
			Console.WriteLine($"Filtered: {newValue}");
		}
	}
}
