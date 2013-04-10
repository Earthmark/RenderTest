using System;
using System.Windows.Forms;
using RenderTest.Shaders;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace RenderTest.Drawing
{
	/// <summary>
	/// Encapsulates a color <see cref="Effect"/> set of Vertex and Pixel shaders.
	/// </summary>
	public sealed class ColorShader : IDisposable
	{
		#region Fields

		/// <summary>
		/// The current effect pass, representing a color vertex and pixel shader.
		/// </summary>
		private EffectPass pass;

		/// <summary>
		/// The layout of data to bind.
		/// </summary>
		private InputLayout layout;

		/// <summary>
		/// The buffer of matricies to bind.
		/// </summary>
		private Buffer matrixBuffer;

		#endregion

		#region Initialize and Shutdown

		/// <summary>
		/// Binds the effect shader to the specified <see cref="Device"/>.
		/// </summary>
		/// <param name="device">The device to bind the shader to.</param>
		/// <returns>If the binding was successful.</returns>
		public bool Initialize(Device device)
		{
			return InitializeShader(device, "Shaders/Color.fx");
		}

		~ColorShader()
		{
			Shutdown();
		}

		/// <summary>
		/// Disposes the current color shader, as well as frees the specified resources.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Shutdown();
		}

		/// <summary>
		/// Does the actual shutdown.
		/// </summary>
		private void Shutdown()
		{
			ShutdownShader();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Draws the devices to the specified <see cref="DeviceContext"/>.
		/// </summary>
		/// <param name="context">The context to draw from.</param>
		/// <param name="indexCount">The number of indicies to draw.</param>
		/// <param name="world">The world <see cref="Matrix"/> to use for transforms.</param>
		/// <param name="view">The view <see cref="Matrix"/> to use for transforms.</param>
		/// <param name="projection">The projection <see cref="Matrix"/> to use for transforms.</param>
		/// <returns>If the drawing works correctly.</returns>
		public bool Render(DeviceContext context, int indexCount, Matrix world, Matrix view, Matrix projection)
		{
			var result = SetShaderParameters(context, world, view, projection);
			if(result)
			{
				RenderShader(context, indexCount);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Initializes the shaders to the specified <see cref="Device"/>.
		/// </summary>
		/// <param name="device">The device to bind to.</param>
		/// <param name="str">The name of the shader to load.</param>
		/// <returns>If the binding was successful.</returns>
		private bool InitializeShader(Device device, string str)
		{
			try
			{
				var effect = ShaderCompiler.GetEffect("ColorShader", device, str);
				pass = effect.GetTechniqueByName("ColorShader").GetPassByName("ColorPass");

				layout = new InputLayout(device, pass.Description.Signature, ColorDrawingVertex.VertexDeclaration);

				var matrixBufferInfo = new BufferDescription(Matrix.SizeInBytes * 3, ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

				matrixBuffer = new Buffer(device, matrixBufferInfo);

				return true;
			}
			catch(Exception e)
			{
				MessageBox.Show("Shader error: " + e.Message);
				return false;
			}
		}
		
		/// <summary>
		/// Shuts down the current resources.
		/// </summary>
		private void ShutdownShader()
		{
			if(layout != null)
			{
				layout.Dispose();
				layout = null;
			}
			if(matrixBuffer != null)
			{
				matrixBuffer.Dispose();
				matrixBuffer = null;
			}
			if(pass != null)
			{
				pass.Dispose();
				pass = null;
			}
		}

		/// <summary>
		/// Binds the shader and world matricies.
		/// </summary>
		/// <param name="context">The context to draw from.</param>
		/// <param name="world">The world <see cref="Matrix"/> to draw from.</param>
		/// <param name="view">The view <see cref="Matrix"/> to use for transforms.</param>
		/// <param name="projection">The projection <see cref="Matrix"/> to use for transforms.</param>
		/// <returns></returns>
		private bool SetShaderParameters(DeviceContext context, Matrix world, Matrix view, Matrix projection)
		{
			try
			{
				world.Transpose();
				view.Transpose();
				projection.Transpose();

				DataStream stream;
				context.MapSubresource(matrixBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);

				stream.Write(world);
				stream.Write(view);
				stream.Write(projection);

				context.UnmapSubresource(matrixBuffer, 0);

				const int bufferNum = 0;

				context.VertexShader.SetConstantBuffer(bufferNum, matrixBuffer);

				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Draws using the current <see cref="DeviceContext"/>, after the data has been bound.
		/// </summary>
		/// <param name="context">The context to draw from.</param>
		/// <param name="num">The number of indicies to draw.</param>
		private void RenderShader(DeviceContext context, int num)
		{
			context.InputAssembler.InputLayout = layout;

			pass.Apply(context);

			context.DrawIndexed(num, 0, 0);
		}

		#endregion
	}
}
