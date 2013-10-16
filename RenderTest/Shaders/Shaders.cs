using System.Collections.Concurrent;
using System.IO;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace RenderTest.Shaders
{
	/// <summary>
	/// A compiler and storage for loaded shaders. Also compiles shaders when loaded.
	/// </summary>
	public static class Shaders
	{
		private static readonly ConcurrentDictionary<string, Effect> effects = new ConcurrentDictionary<string, Effect>();

		/// <summary>
		/// Loads the specified <see cref="Effect"/> or retrieves the specified <see cref="Effect"/>.
		/// </summary>
		/// <param name="device">The <see cref="Device"/> to bind the <see cref="Effect"/> to.</param>
		/// <param name="fileInfo">The file path to search for, also the name of the <see cref="Effect"> to look for.</see></param>
		/// <returns>The <see cref="Effect"/>, or null if not found.</returns>
		public static Effect Get(Device device, string fileInfo)
		{
			Effect effect;
			if(!effects.TryGetValue(fileInfo, out effect))
			{
				var info = new FileInfo(fileInfo);
				if(info.Exists)
				{
					var str = ShaderBytecode.PreprocessFromFile(info.FullName);
					using(var result = ShaderBytecode.Compile(str, "fx_5_0", ShaderFlags.None, EffectFlags.None))
					{
						effect = new Effect(device, result);
						effects[fileInfo] = effect;
					}
				}
			}
			return effect;
		}
	}
}
