using System.Drawing;
using System.Windows.Forms;
using SharpDX.Windows;

namespace RenderTest
{
	public static class Core
	{
		public const int Height = 600;
		public const int Width = 800;
		public static Form RenderForm { get; private set; }

		private static Graphics graphics;
		private static Input input;
		private static bool Closed;

		public static bool Initialize()
		{
			InitializeWindow();

			graphics = new Graphics();
			if(!graphics.Initialize(Height, Width, RenderForm.Handle))
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
			RenderLoop.Run(RenderForm, Render);
		}

		private static void Render()
		{
			
		}

		private static bool Frame()
		{
			return !input[Keys.Escape] && graphics.Frame();
		}

		private static void InitializeWindow()
		{
			RenderForm = new RenderForm("RenderTest")
			{
				ClientSize = new Size(Width, Height)
			};
			RenderForm.Closed += (obj, args) => Closed = true;
			RenderForm.MouseEnter += (obj, args) => Cursor.Show();
			RenderForm.MouseLeave += (obj, args) => Cursor.Hide();
		}

		private static void ShutdownWindow()
		{
			RenderForm.Close();
			RenderForm.Dispose();
			RenderForm = null;
		}
	}
}
