using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace RenderTest.Drawing
{
	/// <summary>
	/// The vertex type used for drawing color points.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ColorDrawingVertex
	{
		#region Static components

		/// <summary>
		/// The vertex declerations used for binding.
		/// </summary>
		public static readonly InputElement[] VertexDeclaration = new[]
		{
			new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0),
			new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 0)
		};

		/// <summary>
		/// The size of the object in bytes.
		/// </summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(ColorDrawingVertex));

		#endregion

		#region Fields

		/// <summary>
		/// The position vector, pretend it is a <see cref="Vector3"/>.
		/// </summary>
		public Vector4 Position;
		
		/// <summary>
		/// The color vector.
		/// </summary>
		public Color4 Color;

		#endregion
	}
}
