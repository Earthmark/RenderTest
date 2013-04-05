using System;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace RenderTest.Drawing
{
	public class Model : IDisposable
	{
		private Buffer vertexBuffer;
		private Buffer indexBuffer;

		public int IndexCount { get; private set; }
		public int VertexCount { get; private set; }

		public bool Initialize(Device device)
		{
			return InitializeBuffers(device);
		}

		~Model()
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
			ShutdownBuffers();
		}

		public void Render(DeviceContext context)
		{
			RenderBuffers(context);
		}

		private bool InitializeBuffers(Device device)
		{
			try
			{
				VertexCount = IndexCount = 3;

				var vertices = new ColorDrawingVertex[3];

				vertices[0].Position = new Vector4(-1f, -1f, 0f, 0f); // Bottom left.
				vertices[0].Color = new Color4(0f, 1f, 0f, 1f);

				vertices[1].Position = new Vector4(0f, 1f, 0f, 0f); // Top middle.
				vertices[1].Color = new Color4(0f, 1f, 0f, 1.0f);

				vertices[2].Position = new Vector4(1f, -1f, 0f, 0f); // Bottom right.
				vertices[2].Color = new Color4(0f, 1f, 0f, 1f);

				vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);

				var indices = new uint[3];

				indices[0] = 0;
				indices[1] = 1;
				indices[2] = 2;

				indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);

				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		private void ShutdownBuffers()
		{
			if(indexBuffer != null)
			{
				indexBuffer.Dispose();
				indexBuffer = null;
			}
			if(vertexBuffer != null)
			{
				vertexBuffer.Dispose();
				vertexBuffer = null;
			}
		}

		private void RenderBuffers(DeviceContext context1)
		{
			context1.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, ColorDrawingVertex.SizeInBytes, 0));
			context1.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
			context1.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
		}
	}
}
