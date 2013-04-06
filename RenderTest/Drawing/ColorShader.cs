using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.IO;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace RenderTest.Drawing
{
	public sealed class ColorShader : IDisposable
	{
		private struct MatrixSetBuffer
		{
			// ReSharper disable NotAccessedField.Local
			public Matrix World;
			public Matrix View;
			public Matrix Projection;
			// ReSharper restore NotAccessedField.Local
		};

		private EffectPass pass;
		private InputLayout layout;
		private Buffer matrixBuffer;

		public bool Initialize(Device device)
		{
			return InitializeShader(device, "");
		}

		~ColorShader()
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
			ShutdownShader();
		}

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

		private bool InitializeShader(Device device, string str)
		{
			try
			{
				var effect = new Effect(device, NativeFile.ReadAllBytes(str));
				pass = effect.GetTechniqueByName("ColorShader").GetPassByName("ColorPass");

				layout = new InputLayout(device, pass.Description.Signature, ColorDrawingVertex.VertexDeclaration);

				var matrixBufferInfo = new BufferDescription(Matrix.SizeInBytes, ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

				matrixBuffer = new Buffer(device, matrixBufferInfo);

				return true;
			}
			catch(Exception e)
			{
				MessageBox.Show("Shader error: " + e.Message);
				return false;
			}
		}

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

		private bool SetShaderParameters(DeviceContext context, Matrix world, Matrix view, Matrix projection)
		{
			try
			{
				world.Transpose();
				view.Transpose();
				projection.Transpose();

				DataStream stream;
				context.MapSubresource(matrixBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);

				var matrixSet = new MatrixSetBuffer
				{
					World = world,
					View = view,
					Projection = projection
				};

				stream.Write(matrixSet);

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

		private void RenderShader(DeviceContext context, int num)
		{
			context.InputAssembler.InputLayout = layout;

			pass.Apply(context);

			context.DrawIndexed(num, 0, 0);
		}
	}
}
