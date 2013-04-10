using System.Collections.Concurrent;
using System.IO;
using System.Windows.Forms;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace RenderTest.Shaders
{
	/// <summary>
	/// A compiler and storage for loaded shaders. Also compiles shaders when loaded.
	/// </summary>
	public static class ShaderCompiler
	{
		#region Fields

		/// <summary>
		/// A dictonary of used effects.
		/// </summary>
		private static readonly ConcurrentDictionary<string, Effect> effects = new ConcurrentDictionary<string, Effect>();

		#endregion

		#region Methods

		/// <summary>
		/// Loads the specified <see cref="Effect"/> or retrieves the specified <see cref="Effect"/>.
		/// </summary>
		/// <param name="name">The name of the <see cref="Effect"/> to save to search for.</param>
		/// <param name="device">The <see cref="Device"/> to bind the <see cref="Effect"/> to.</param>
		/// <param name="fileInfo">The file path to search for.</param>
		/// <returns>The <see cref="Effect"/>, or null if not found.</returns>
		public static Effect GetEffect(string name, Device device, string fileInfo)
		{
			Effect effect;
			if(!effects.TryGetValue(name, out effect) && fileInfo != null)
			{
				var info = new FileInfo(fileInfo);
				if(info.Exists)
				{
					var str = ShaderBytecode.PreprocessFromFile(info.FullName);
					using(var result = ShaderBytecode.Compile(str, "fx_4_0", ShaderFlags.None, EffectFlags.None))
					{
						effect = new Effect(device, result);
						effects[name] = effect;
					}
				}
			}
			return effect;
		}

		/// <summary>
		/// Returns a caches <see cref="Effect"/>.
		/// </summary>
		/// <param name="name">The name of the shader to load.</param>
		/// <returns>Either the shader of found, or null.</returns>
		public static Effect GetEffect(string name)
		{
			Effect effect;
			effects.TryGetValue(name, out effect);
			return effect;
		}

		#endregion
	}
}
