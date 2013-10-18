using System;
using System.Windows.Forms;
using RenderTest.Main;
using RenderTest.Shaders;
using SharpDX;
using SharpDX.Windows;

namespace RenderTest.Drawing
{
	/// <summary>
	/// The interface for linking with a Direct3D graphics device.
	/// </summary>
	public sealed class Graphics : IDisposable
	{
		#region Fields

		private D3D directXDevice;
		
		private Camera camera;
		
		private Model model;

		private TextureShader shader;

		#endregion

		#region Properties

		/// <summary>
		/// The current state of the graphics device.
		/// </summary>
		public bool FullScreen { get; set; }

		/// <summary>
		/// The screen depth of the render target.
		/// </summary>
		public float ScreenDepth { get; private set; }

		/// <summary>
		/// The screen near of the render target.
		/// </summary>
		public float ScreenNear { get; private set; }

		/// <summary>
		/// IF VSync is enabled.
		/// </summary>
		public bool VSync { get; set; }

		/// <summary>
		/// The pointer to the current <see cref="RenderForm"/>
		/// </summary>
		public IntPtr Hwnd { get; private set; }

		#endregion

		#region Initialize and Shutdown

		/// <summary>
		/// Initializes the current <see cref="Graphics"/> object.
		/// </summary>
		/// <param name="height">The height of the window.</param>
		/// <param name="width">The width of the window.</param>
		/// <param name="hwnd">The pointer to the current <see cref="RenderForm"/>.</param>
		/// <returns>True if initialization is successful.</returns>
		public bool Initialize(int height, int width, IntPtr hwnd)
		{
			VSync = true;
			FullScreen = false;
			ScreenDepth = 1000.1f;
			ScreenNear = 0.1f;
			Hwnd = hwnd;

			directXDevice = new D3D();
			if(directXDevice == null)
				return false;

			if(!directXDevice.Initialize(height, width, 1000f, 0.1f, Hwnd))
			{
				MessageBox.Show("Could not initialize Direct3D");
				return false;
			}

			camera = new Camera {Position = new Vector3(0f, 0f, -2.5f)};

			model = new Model();
			if(!model.Initialize(directXDevice.Device))
			{
				MessageBox.Show("Could not initialize model buffers.");
				return false;
			}

			shader = new TextureShader();
			if(!shader.Initialize(directXDevice.Device))
			{
				MessageBox.Show("Could not initialize shader");
				return false;
			}

			return true;
		}

		~Graphics()
		{
			Shutdown();
		}

		/// <summary>
		/// Disposes of the used resources and cleans up any usages.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Shutdown();
		}

		/// <summary>
		/// The actual cleanup method for <see cref="Graphics"/>.
		/// </summary>
		private void Shutdown()
		{
			camera = null;

			if(model.SafeDispose()) model = null;
			if(shader.SafeDispose()) shader = null;
			if(directXDevice.SafeDispose()) directXDevice = null;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Draws the current graphics buffers.
		/// </summary>
		/// <returns>If the drawing was successful.</returns>
		public bool Frame()
		{
			return Render();
		}

		/// <summary>
		/// Clears then draws the current renderframe.
		/// </summary>
		/// <returns>If the drawing was successful.</returns>
		private bool Render()
		{
			directXDevice.BeginScene(Color.Gray);

			model.Render(directXDevice.Context);
			var flag = shader.Render(directXDevice.Context, model.IndexCount, directXDevice.WorldMatrix, camera.ViewMatrix, directXDevice.ProjectionMatrix);

			if(!flag)
				return false;

			directXDevice.EndScene();

			return true;
		}

		#endregion
	}
}
