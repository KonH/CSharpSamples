using System;
using System.IO;

namespace CSharpSamples {
	class FSWatcher {

		public static void Test() {
			var dir = Directory.GetCurrentDirectory();
			var watcher = new FileSystemWatcher(dir);
			watcher.Created += Watcher_Created;
			watcher.EnableRaisingEvents = true;
			Console.WriteLine($"Start watch to {dir}");
		}

		private static void Watcher_Created(object sender, FileSystemEventArgs e) {
			Console.WriteLine($"Created: '{e.FullPath}'");
		}
	}
}
