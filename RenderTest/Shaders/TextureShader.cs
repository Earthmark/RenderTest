using System;
using System.Windows.Forms;
using RenderTest.Drawing;
using RenderTest.Textures;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace RenderTest.Shaders
{
	public class TextureShader : IDisposable
	{
		#region Fields

		private SamplerState pixelSampler;
		private PixelShader pixelShader;
		private VertexShader vertexShader;

		private Buffer matrixBuffer;
		private InputLayout layout;

		private Texture texture;

		#endregion

		#region Initialize and Shutdown

		/// <summary>
		/// Binds the effect shader to the specified <see cref="Device"/>.
		/// </summary>
		/// <param name="device">The device to bind the shader to.</param>
		/// <returns>If the binding was successful.</returns>
		public bool Initialize(Device device)
		{
			try
			{
				matrixBuffer = new Buffer(device, Matrix.SizeInBytes * 3, ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0) {DebugName = "Matrix buffer"};
				using (var bytecode = ShaderBytecode.CompileFromFile("Shaders/Texture.vs", "TextureVertexShader", "vs_4_0"))
				{
					layout = new InputLayout(device, ShaderSignature.GetInputSignature(bytecode), TextureDrawingVertex.VertexDeclaration) { DebugName = "Color vertex layout" };
					vertexShader = new VertexShader(device, bytecode) { DebugName = "Texture vertex shader" };
				}

				using (var bytecode = ShaderBytecode.CompileFromFile("Shaders/Texture.ps", "TexturePixelShader", "ps_4_0"))
				{

					pixelShader = new PixelShader(device, bytecode) { DebugName = "Texture pixel shader" };
				}

				var samplerDesc = new SamplerStateDescription
				{
					AddressU = TextureAddressMode.Wrap,
					AddressV = TextureAddressMode.Wrap,
					AddressW = TextureAddressMode.Wrap,
					Filter = Filter.ComparisonMinMagMipLinear,
					MaximumAnisotropy = 1,
					MipLodBias = 0f,
					MinimumLod = 0,
					MaximumLod = float.MaxValue,
					BorderColor = Color.LimeGreen,
					ComparisonFunction = Comparison.Always
				};
				pixelSampler = new SamplerState(device, samplerDesc);
				
				texture = new Texture();
				return texture.Initialize(device, "Textures/dirt.dds");
			}
			catch (Exception e)
			{
				MessageBox.Show("Shader error: " + e.Message);
				return false;
			}
		}

		~TextureShader()
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

		private void Shutdown()
		{
			if(pixelShader.SafeDispose()) pixelShader = null;
			if(vertexShader.SafeDispose()) vertexShader = null;
			if(texture.SafeDispose()) texture = null;
			if(matrixBuffer.SafeDispose()) matrixBuffer = null;
			if(layout.SafeDispose()) layout = null;
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
				
				context.VertexShader.Set(vertexShader);
				context.PixelShader.Set(pixelShader);

				DataStream mappedResource;
				context.MapSubresource(matrixBuffer, MapMode.WriteDiscard, MapFlags.None, out mappedResource);
				
				mappedResource.Write(world);
				mappedResource.Write(view);
				mappedResource.Write(projection);
				
				context.UnmapSubresource(matrixBuffer, 0);

				context.VertexShader.SetConstantBuffer(0, matrixBuffer);
				context.PixelShader.SetSampler(0, pixelSampler);
				context.PixelShader.SetShaderResource(0, texture.Resource);

				context.InputAssembler.InputLayout = layout;
				
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
			context.DrawIndexed(num, 0, 0);
		}

		#endregion
	}
}
