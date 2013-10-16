using System;
using System.Windows.Forms;
using RenderTest.Main;
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

		/// <summary>
		/// The child <see cref="D3D"/> used for rendering.
		/// </summary>
		private D3D directxDevice;
		
		/// <summary>
		/// The <see cref="Camera"/> used for perspective.
		/// </summary>
		private Camera camera;
		
		/// <summary>
		/// The current <see cref="Model"/> information buffer.
		/// </summary>
		private Model model;
		
		/// <summary>
		/// The current shader used to render the verts from <see cref="Model"/>
		/// </summary>
		private ColorShader colorShader;

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
		/// The pointer to the current <see cref="RenderForm"/>
		/// </summary>
		public IntPtr Hwnd { get; private set; }

		/// <summary>
		/// IF VSync is enabled.
		/// </summary>
		public bool VSync { get; set; }

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

			directxDevice = new D3D();
			if(directxDevice == null)
				return false;

			if(!directxDevice.Initialize(this, Hwnd))
			{
				MessageBox.Show("Could not initialize Direct3D");
				return false;
			}

			camera = new Camera {Position = new Vector3(0f, 0f, 10f)};

			model = new Model();
			if(!model.Initialize(directxDevice.Device))
			{
				MessageBox.Show("Could not initialize model buffers.");
				return false;
			}

			colorShader = new ColorShader();
			if(!colorShader.Initialize(directxDevice.Device))
			{
				MessageBox.Show("Could not initialize ColorShader");
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
			if(colorShader.SafeDispose()) colorShader = null;
			if(directxDevice.SafeDispose()) directxDevice = null;
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
			directxDevice.BeginScene(Color.Gray);

			model.Render(directxDevice.Context);
			var flag = colorShader.Render(directxDevice.Context, model.VertexCount, directxDevice.WorldMatrix, camera.ViewMatrix, directxDevice.ProjectionMatrix);

			if(!flag)
				return false;

			directxDevice.EndScene();

			return true;
		}

		#endregion
	}
}
