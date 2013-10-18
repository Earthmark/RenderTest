using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace RenderTest.Textures
{
	class Texture : IDisposable
	{
		public ShaderResourceView Resource { get; private set; }

		~Texture()
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
			if(Resource.SafeDispose()) Resource = null;
		}

		public bool Initialize(Device device, string file)
		{
			try
			{
				Resource = ShaderResourceView.FromFile(device, file);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
