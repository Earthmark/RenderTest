using System;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace RenderTest
{
	public sealed class Input : IDisposable
	{
		private readonly ConcurrentDictionary<Keys, bool> keys; 

		private Form linkForm;

		public bool Alt
		{
			get { return this[Keys.Alt]; }
		}

		public bool Ctl
		{
			get { return this[Keys.ControlKey]; }
		}

		public bool this[Keys key]
		{
			get
			{
				bool var;
				return keys.TryGetValue(key, out var) && var;
			}
		}

		public Input()
		{
			keys = new ConcurrentDictionary<Keys, bool>(4, 512);
		}

		~Input()
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
			linkForm.KeyUp -= LinkFormOnKeyUp;
			linkForm.KeyDown -= LinkFormOnKeyDown;
		}

		public void Initialize(Form form)
		{
			linkForm = form;
			linkForm.KeyUp += LinkFormOnKeyUp;
			linkForm.KeyDown += LinkFormOnKeyDown;
		}

		private void LinkFormOnKeyDown(object sender, KeyEventArgs keyEventArgs)
		{
			keys[keyEventArgs.KeyCode] = true;
		}

		private void LinkFormOnKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			keys[keyEventArgs.KeyCode] = false;
		}
	}
}
