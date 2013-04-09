using System.Collections.Concurrent;
using System.IO;
using System.Windows.Forms;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace RenderTest.Shaders
{
	public static class ShaderCompiler
	{
		private static readonly ConcurrentDictionary<string, Effect> effects = new ConcurrentDictionary<string, Effect>();

		public static Effect GetEffect(string name, Device device, string fileInfo)
		{
			Effect effect;
			if(!effects.TryGetValue(name, out effect) && fileInfo != null)
			{
				var info = new FileInfo(fileInfo);
				if(info.Exists)
				{
					var text = File.ReadAllText(info.FullName);
					var result = ShaderBytecode.Compile(text, "fx_4_1", ShaderFlags.None, EffectFlags.None);
					if(!result.HasErrors)
					{
						effect = new Effect(device, result.Bytecode);
						effects[name] = effect;
					}
					else
					{
						MessageBox.Show("Shader compilation error: " + result.ResultCode + " " + result.Message);
					}
				}
			}
			return effect;
		}

		public static Effect GetEffect(string name)
		{
			Effect effect;
			effects.TryGetValue(name, out effect);
			return effect;
		}
	}
}
