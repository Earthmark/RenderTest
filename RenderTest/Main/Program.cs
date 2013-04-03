namespace RenderTest.Main
{
	internal sealed class Program
	{
		private static void Main(string[] args)
		{
			if(Core.Initialize())
				Core.Run();
			Core.Shutdown();
		}
	}
}
