using System;
using System.IO;
using System.Reflection;

namespace RenderTest.Main
{
	internal sealed class Program
	{
		private static void Main(string[] args)
		{
			var assemblyUri = new Uri(Assembly.GetEntryAssembly().CodeBase);
			var assemblyPath = Path.GetDirectoryName(assemblyUri.LocalPath);
			var newPath = Path.GetFullPath(Path.Combine(assemblyPath, (IntPtr.Size == 4 ? "x86" : "x64"))) + ";" + System.Environment.GetEnvironmentVariable("PATH");

			Environment.SetEnvironmentVariable("PATH", newPath);

			if(Core.Initialize())
				Core.Run();
			Core.Shutdown();
		}
	}
}
