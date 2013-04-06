using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace RenderTest.Main
{
	public class Camera
	{
		public Vector3 Position { get; set; }
		public Vector3 Rotation { get; set; }
		public Matrix ViewMatrix { get; private set; }

		public void Render()
		{
			var up = Vector3.UnitY;
			var position = Position;
			var lookAt = Vector3.UnitZ;

			var rotation = Rotation * 0.0174532925f;
			var rotationMatrix = Matrix.RotationYawPitchRoll(rotation.X, rotation.Y, rotation.Z);

			Vector3.TransformCoordinate(ref lookAt, ref rotationMatrix, out lookAt);
			Vector3.TransformCoordinate(ref up, ref rotationMatrix, out up);

			ViewMatrix = Matrix.LookAtLH(position, lookAt, up);
		}
	}
}
