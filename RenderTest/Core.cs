using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RenderTest
{
	public static class Core
	{
		public static Form RenderForm { get; private set; }

		private static Graphics graphics;
		private static Input input;

		public static bool Initialize()
		{
			int height, width;
			InitializeWindow(out height, out width);

			graphics = new Graphics();
			if(!graphics.Initialize(height, width, new IntPtr()))
				return false;

			input = new Input();
			input.Initialize(RenderForm);
			return true;
		}

		public static void Shutdown()
		{
			graphics.Dispose();
			graphics = null;

			input.Dispose();
			input = null;

			ShutdownWindow();
		}

		public static void Run()
		{
			
		}

		private static bool Frame()
		{
			
		}

		private static void InitializeWindow(out int num1, out int num2)
		{
			
		}

		private static void ShutdownWindow()
		{
			
		}
	}
}
