﻿using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace RenderTest.Drawing
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ColorDrawingVertex
	{
		public static readonly InputElement[] VertexDeclaration = new[]
		{
			new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0),
			new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 0)
		};

		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(ColorDrawingVertex));

		public Vector4 Position;
		public Color4 Color;
	}
}