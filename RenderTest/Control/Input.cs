using System;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace RenderTest.Control
{
	/// <summary>
	/// Handles the input from the renderframe control.
	/// </summary>
	public sealed class Input : IDisposable
	{
		#region Fields
		
		private readonly ConcurrentDictionary<Keys, bool> keys;
		private readonly ConcurrentDictionary<Keys, KeyEventHandler> keyPressEvents;
		private Form linkForm;

		#endregion

		#region Properties

		/// <summary>
		/// Returns the state of the current key.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <returns>True if pressed, false otherwise.</returns>
		public bool this[Keys key]
		{
			get { return keys[key]; }
		}

		#endregion

		#region Initialize and Dispose

		/// <summary>
		/// Creates a new Input construct, does not initialize.
		/// </summary>
		public Input()
		{
			keys = new ConcurrentDictionary<Keys, bool>(4, 200);
			keyPressEvents = new ConcurrentDictionary<Keys, KeyEventHandler>(4, 200);
		}

		/// <summary>
		/// Initializes the current construct, this should only be called once.
		/// </summary>
		/// <param name="form">The form to link to.</param>
		public void Initialize(Form form)
		{
			foreach (var key in Enum.GetValues(typeof(Keys)))
			{
				keys[(Keys)key] = false;
				keyPressEvents[(Keys)key] = null;
			}

			linkForm = form;
			linkForm.KeyUp += LinkFormOnKeyUp;
			linkForm.KeyDown += LinkFormOnKeyDown;
		}

		/// <summary>
		/// Disposes the current object.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Shutdown();
		}

		~Input()
		{
			Shutdown();
		}

		private void Shutdown()
		{
			if(linkForm != null)
			{
				linkForm.KeyUp -= LinkFormOnKeyUp;
				linkForm.KeyDown -= LinkFormOnKeyDown;
			}
		}

		private void LinkFormOnKeyDown(object sender, KeyEventArgs keyEventArgs)
		{
			keys[keyEventArgs.KeyCode] = true;
			var invoke = keyPressEvents[keyEventArgs.KeyData];
			if(invoke != null) invoke(linkForm, keyEventArgs);
		}

		private void LinkFormOnKeyUp(object sender, KeyEventArgs keyEventArgs)
		{
			keys[keyEventArgs.KeyCode] = false;
			var invoke = keyPressEvents[keyEventArgs.KeyData];
			if (invoke != null) invoke(linkForm, keyEventArgs);
		}

		#endregion

		#region Subscription methods

		/// <summary>
		/// Subscribes an event for when the key is pressed.
		/// </summary>
		/// <param name="key">The key to bind to.</param>
		/// <param name="keyEvent">The event to subscribe.</param>
		public void Subscribe(Keys key, KeyEventHandler keyEvent)
		{
			if(keyEvent != null)
			lock(keyPressEvents)
			{
				keyPressEvents[key] += keyEvent;
			}
		}

		/// <summary>
		/// Unsubscribes an event from a key press.
		/// </summary>
		/// <param name="key">The key to bind.</param>
		/// <param name="keyEvent">The event to subscribe.</param>
		public void Unsubscribe(Keys key, KeyEventHandler keyEvent)
		{
			if(keyEvent != null)
			lock (keyPressEvents)
			{
				keyPressEvents[key] -= keyEvent;
			}
		}

		#endregion
	}
}
