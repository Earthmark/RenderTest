using System;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace RenderTest.Drawing
{
	/// <summary>
	/// A buffer used for storing verticies.
	/// </summary>
	public class Model : IDisposable
	{
		#region Field

		/// <summary>
		/// The <see cref="Buffer"/> storing the vertex positions.
		/// </summary>
		private Buffer vertexBuffer;

		/// <summary>
		/// The <see cref="Buffer"/> storing the indicies of the vertex positions.
		/// </summary>
		private Buffer indexBuffer;

		#endregion

		#region Properties

		/// <summary>
		/// The number of indicies in the IndexBuffer.
		/// </summary>
		public int IndexCount { get; private set; }

		/// <summary>
		/// The number of verticies in the VertexBuffer.
		/// </summary>
		public int VertexCount { get; private set; }

		#endregion

		#region Initialize and shutdown

		/// <summary>
		/// Initializes the <see cref="Model"/>, binding the vertex and index <see cref="Buffer"/>.
		/// </summary>
		/// <param name="device">The <see cref="Device"/> to bind to.</param>
		/// <returns>If the buffer binding was successful.</returns>
		public bool Initialize(Device device)
		{
			return InitializeBuffers(device);
		}

		~Model()
		{
			Shutdown();
		}

		/// <summary>
		/// Shuts down the current buffers.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Shutdown();
		}

		/// <summary>
		/// Does the actual shutdown
		/// </summary>
		private void Shutdown()
		{
			ShutdownBuffers();
		}
		
		#endregion

		#region Methods

		/// <summary>
		/// Renders the buffers, and binds them.
		/// </summary>
		/// <param name="context">The context to draw from.</param>
		public void Render(DeviceContext context)
		{
			RenderBuffers(context);
		}

		/// <summary>
		/// Initializes the buffers for drawing.
		/// </summary>
		/// <param name="device">The device to bind to.</param>
		/// <returns>If the binding was successful.</returns>
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

		/// <summary>
		/// Closes the used buffers.
		/// </summary>
		private void ShutdownBuffers()
		{
			if(indexBuffer.SafeDispose()) indexBuffer = null;
			if(vertexBuffer.SafeDispose()) vertexBuffer = null;
		}

		/// <summary>
		/// Draws the buffers, and binds them to the context.
		/// </summary>
		/// <param name="context1">The context to bind and use for drawing.</param>
		private void RenderBuffers(DeviceContext context1)
		{
			context1.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, ColorDrawingVertex.SizeInBytes, 0));
			context1.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
			context1.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
		}

		#endregion
	}
}
