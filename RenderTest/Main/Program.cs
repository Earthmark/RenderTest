using System;
using System.IO;

namespace RenderTest.Main
{
	/// <summary>
	/// The launching point of the program.
	/// </summary>
	internal static class Program
	{
		/// <summary>
		/// The launching point of the program.
		/// </summary>
		/// <param name="args">Any command line arguemnts.</param>
		[Obsolete("Do not launch the program internally.", true)]
		private static void Main(string[] args)
		{
			var newPath = Path.Combine(Environment.CurrentDirectory, Environment.Is64BitProcess ? "x64" : "x86") + ";" + Environment.GetEnvironmentVariable("PATH");
			Environment.SetEnvironmentVariable("PATH", newPath);

			if(Core.Initialize())
				Core.Run();
			Core.Shutdown();
		}
	}
}
