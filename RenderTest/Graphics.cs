using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RenderTest
{
	public sealed class Graphics : IDisposable
	{
		private bool fullScreen;

		public bool FullScreen
		{
			get { return fullScreen; }
			set { fullScreen = value; }
		}

		public bool VSync { get; private set; }

		public float ScreenDepth { get; private set; }
		public float ScreenNear { get; private set; }

		public bool Initialize(int height, int width, IntPtr hwnd)
		{
			VSync = true;
			fullScreen = false;
			ScreenDepth = 1000.1f;
			ScreenNear = 0.1f;

			return true;
		}

		public void Shutdown()
		{
			
		}

		public bool Frame()
		{
			return true;
		}

		private bool Render()
		{
			return true;
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
