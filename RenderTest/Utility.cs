using System;

namespace RenderTest
{
	public static class Utility
	{
		public static bool SafeDispose(this IDisposable disposable)
		{
			if(disposable != null)
			{
				disposable.Dispose();
				return true;
			}
			return false;
		}
	}
}
