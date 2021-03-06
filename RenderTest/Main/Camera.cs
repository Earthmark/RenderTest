﻿using SharpDX;

namespace RenderTest.Main
{
	/// <summary>
	/// A set of positions and rotations used for drawing.
	/// </summary>
	public class Camera
	{
		#region Properties

		/// <summary>
		/// Sets the position of the camera.
		/// </summary>
		public Vector3 Position { get; set; }

		/// <summary>
		/// Sets the rotation of the camera.
		/// </summary>
		public Vector3 Rotation { get; set; }

		/// <summary>
		/// The viewmatrix, used after render is called.
		/// </summary>
		public Matrix ViewMatrix
		{
			get
			{
				var up = Vector3.UnitY;
				var position = Position;
				var lookAt = Vector3.UnitZ;

				var rotation = Rotation * 0.0174532925f;
				var rotationMatrix = Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);

				lookAt = Vector3.TransformCoordinate(lookAt, rotationMatrix);
				up = Vector3.TransformCoordinate(up, rotationMatrix);

				lookAt = position + lookAt;

				return Matrix.LookAtLH(position, lookAt, up);
			}
		}

		#endregion
	}
}
