using System.Drawing;
using System.Windows.Forms;
using RenderTest.Control;
using SharpDX;
using SharpDX.Windows;
using Graphics = RenderTest.Drawing.Graphics;

namespace RenderTest.Main
{
	/// <summary>
	/// The container for the primary system classes.
	/// </summary>
	public static class Core
	{
		#region Fields
		
		/// <summary>
		/// True of the window was closed, all systems should be shut down.
		/// </summary>
		private static bool closed;

		#endregion

		#region Constants

		/// <summary>
		/// The height of the viewport.
		/// TODO: Convert to varible, be able to reset the D3D driver.
		/// </summary>
		public const int Height = 600;

		/// <summary>
		/// The width of the viewport.
		/// TODO: Convert to varible, be able to reset the D3D driver.
		/// </summary>
		public const int Width = 800;

		#endregion

		#region Properties

		/// <summary>
		/// The <see cref="Graphics"/> instance used as the current manager.
		/// </summary>
		public static Graphics Graphics { get; private set; }

		/// <summary>
		/// The <see cref="Input"/> instance used as the current manager.
		/// </summary>
		public static Input Input { get; private set; }

		/// <summary>
		/// The <see cref="RenderForm"/> used for drawing.
		/// </summary>
		public static Form RenderForm { get; private set; }

		/// <summary>
		/// The current timer for the game time.
		/// </summary>
		public static TimerTick Timer { get; private set; }

		/// <summary>
		/// Gets if the <see cref="RenderForm"/> was closed, if set to true the is closed.
		/// </summary>
		public static bool Closed
		{
			get { return closed; }
			set
			{
				closed = value;
				if(!closed && value)
					RenderForm.Close();
			}
		}

		#endregion

		#region Initialize and Shutdown

		/// <summary>
		/// Starts the used resources.
		/// </summary>
		/// <returns>If everything initialized properly.</returns>
		public static bool Initialize()
		{
			Timer = new TimerTick();

			InitializeWindow();

			Graphics = new Graphics();
			if(Graphics != null)
				if(!Graphics.Initialize(Height, Width, RenderForm.Handle))
					return false;

			Input = new Input();
			if(Input != null)
				Input.Initialize(RenderForm);

			return true;
		}

		/// <summary>
		/// Shuts down the used resources.
		/// </summary>
		public static void Shutdown()
		{
			if(Graphics != null)
			{
				Graphics.Dispose();
				Graphics = null;
			}

			if(Input != null)
			{
				Input.Dispose();
				Input = null;
			}

			ShutdownWindow();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Starts the current <see cref="RenderLoop"/>.
		/// </summary>
		public static void Run()
		{
			RenderLoop.Run(RenderForm, Render);
		}

		/// <summary>
		/// The loop for drawing the <see cref="Graphics"/> device.
		/// Also contains tick and update loops.
		/// </summary>
		private static void Render()
		{
			if (Closed)
				return;

			Timer.Tick();

			if(Input[Keys.Escape] || !Frame())
			{
				RenderForm.Close();
			}
		}

		/// <summary>
		/// Draws the different render stages.
		/// </summary>
		/// <returns>If drawing was successful, false if there was errors.</returns>
		private static bool Frame()
		{
			return Graphics.Frame();
		}

		/// <summary>
		/// Starts the <see cref="RenderForm"/>.
		/// TODO: Handle manual resize.
		/// </summary>
		private static void InitializeWindow()
		{
			RenderForm = new RenderForm("RenderTest")
			{
				ClientSize = new Size(Width, Height),
				StartPosition = FormStartPosition.CenterScreen
			};
			RenderForm.Closed += (obj, args) => Closed = true;
			RenderForm.MouseEnter += (obj, args) => Cursor.Show();
			RenderForm.MouseLeave += (obj, args) => Cursor.Hide();
		}

		/// <summary>
		/// Closes, disposes, and removes the <see cref="RenderForm"/>.
		/// </summary>
		private static void ShutdownWindow()
		{
			RenderForm.Close();
			RenderForm.Dispose();
			RenderForm = null;
		}

		#endregion
	}
}
