using System;
using System.Windows.Forms;
using RenderTest.Drawing;
using SharpDX;
using SharpDX.Direct3D11;

namespace RenderTest.Shaders
{
	/// <summary>
	/// Encapsulates a color <see cref="Effect"/> set of Vertex and Pixel shaders.
	/// </summary>
	public sealed class ColorShader : IDisposable
	{
		#region Fields

		private Effect effect;
		private EffectPass pass;

		private EffectMatrixVariable worldVar;
		private EffectMatrixVariable viewVar;
		private EffectMatrixVariable projectVar;

		private InputLayout layout;

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
				effect = Shaders.Get(device, "Shaders/Color.fx");
				pass = effect.GetTechniqueByName("ColorShader").GetPassByName("Pass1");

				layout = new InputLayout(device, pass.Description.Signature, ColorDrawingVertex.VertexDeclaration);

				worldVar = effect.GetVariableByName("World").AsMatrix();
				viewVar = effect.GetVariableByName("View").AsMatrix();
				projectVar = effect.GetVariableByName("Projection").AsMatrix();

				return true;
			}
			catch (Exception e)
			{
				MessageBox.Show("Shader error: " + e.Message);
				return false;
			}
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

		private void Shutdown()
		{
			if (effect.SafeDispose()) effect = null;
			if (pass.SafeDispose()) pass = null;

			if (worldVar.SafeDispose()) worldVar = null;
			if (viewVar.SafeDispose()) viewVar = null;
			if (projectVar.SafeDispose()) projectVar = null;

			if (layout.SafeDispose()) layout = null;
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

				worldVar.SetMatrix(world);
				viewVar.SetMatrix(view);
				projectVar.SetMatrix(projection);

				pass.Apply(context);

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
