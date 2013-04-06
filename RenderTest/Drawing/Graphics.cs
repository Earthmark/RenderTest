using System;
using System.Windows.Forms;
using RenderTest.Main;
using SharpDX;

namespace RenderTest.Drawing
{
	public sealed class Graphics : IDisposable
	{
		private bool fullScreen;
		private D3D directxDevice;
		private Camera camera;
		private Model model;
		private ColorShader colorShader;

		public bool FullScreen
		{
			get { return fullScreen; }
			set { fullScreen = value; }
		}

		public float ScreenDepth { get; private set; }
		public float ScreenNear { get; private set; }
		public IntPtr Hwnd { get; private set; }

		public bool VSync { get; set; }

		public bool Initialize(int height, int width, IntPtr hwnd)
		{
			VSync = true;
			fullScreen = false;
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

			camera = new Camera {Position = new Vector3(0f, 0f, -10f)};

			model = new Model();
			model.Initialize(directxDevice.Device);

			colorShader = new ColorShader();
			colorShader.Initialize(directxDevice.Device);

			return true;
		}

		~Graphics()
		{
			Shutdown();
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Shutdown();
		}

		private void Shutdown()
		{
			camera = null;

			if(model != null)
			{
				model.Dispose();
				model = null;
			}

			if(colorShader != null)
			{
				colorShader.Dispose();
				colorShader = null;
			}

			if(directxDevice != null)
			{
				directxDevice.Shutdown();
				directxDevice = null;
			}
		}

		public bool Frame()
		{
			return Render();
		}

		private bool Render()
		{
			directxDevice.BeginScene(Color.Gray);

			camera.Render();
			model.Render(directxDevice.Context);
			var flag = colorShader.Render(directxDevice.Context, model.VertexCount, directxDevice.WorldMatrix, camera.ViewMatrix, directxDevice.ProjectionMatrix);

			if(!flag)
				return false;

			directxDevice.EndScene();

			return true;
		}
	}
}
