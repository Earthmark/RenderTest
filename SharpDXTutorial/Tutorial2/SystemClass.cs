﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Windows;
using System.Windows.Forms;
using System.Drawing;

namespace Tutorial2
{
	internal class SystemClass : ICloneable
	{
		#region Variables / Properties
		SystemClass ApplicationHandle;
		string ApplicationName;

		private bool Disposed { get; set; }
		private Form MainForm { get; set; }
		private FormWindowState CurrentFormWindowState;

		public SystemConfiguration Configuration { get; private set; }
		public InputClass Input { get; private set; }
		public GraphicsClass Graphics { get; private set; }
		#endregion

		#region Static
		#endregion

		#region Constructors
		internal SystemClass()
		{
		}
		#endregion

		#region Methods
		public virtual bool Initialize()
		{
			if(Configuration == null)
				Configuration = new SystemConfiguration();

			InitializeWindows();

			if (Input == null)
			{
				Input = new InputClass();
				Input.Initialize();
			}

			if (Graphics == null)
			{
				Graphics = new GraphicsClass();
				Graphics.Initialize(Configuration, MainForm.Handle);
			}

			return true;
		}

		private void InitializeWindows()
		{
			if (MainForm != null)
				return;

			ApplicationHandle = this;
			ApplicationName = "Engine";

			MainForm = new RenderForm(Configuration.Title)
			{
				ClientSize = new Size(Configuration.Width, Configuration.Height)
			};

			MainForm.Show();
		}

		public void Run()
		{
			Initialize();

			var isFormClosed = false;
			var formIsResizing = false;

			MainForm.KeyDown += HandleKeyDown;
			MainForm.KeyUp += HandleKeyUp;
			MainForm.Closed += (o, args) => { isFormClosed = true; };
			MainForm.MouseEnter += (o, args) => { Cursor.Hide(); };
			MainForm.MouseLeave += (o, args) => { Cursor.Show(); };
			MainForm.Resize += (o, args) =>
			{
				if (MainForm.WindowState != CurrentFormWindowState)
				{
					HandleResize(o, args);
				}

				CurrentFormWindowState = MainForm.WindowState;
			};

			MainForm.ResizeBegin += (o, args) => { formIsResizing = true; };
			MainForm.ResizeEnd += (o, args) =>
			{
				formIsResizing = false;
				HandleResize(o, args);
			};

			RenderLoop.Run(MainForm, () =>
			{
				if (isFormClosed)
				{
					return;
				}

				var result = Frame();
				if (!result)
					Exit();
			});
		}

		private void Exit()
		{
			MainForm.Close();
		}

		private bool Frame()
		{
			// Check if the user pressed escape and wants to exit the application.
			if (Input.IsKeyDown(Keys.Escape))
				return false;

			// Do the frame processing for the graphics object.
			return Graphics.Frame();
		}

		public void Shutdown()
		{
			if (Graphics != null)
			{
				Graphics.Shutdown();
				Graphics = null;
			}

			if (Input != null)
			{
				Input = null;
			}

			ShutdownWindows();
		}

		private void ShutdownWindows()
		{
			if (MainForm != null)
				MainForm.Dispose();

			MainForm = null;
			ApplicationHandle = null;
		}
		#endregion

		#region Interface Methods
		public object Clone()
		{
			return this.MemberwiseClone();
		}
		#endregion

		#region Virtual Methods
		#endregion

		#region Events
		/// <summary>
		///   Handles a key down event.
		/// </summary>
		/// <param name = "sender">The sender.</param>
		/// <param name = "e">The <see cref = "System.Windows.Forms.KeyEventArgs" /> instance containing the event data.</param>
		private void HandleKeyDown(object sender, KeyEventArgs e)
		{
			KeyDown(e);
		}

		/// <summary>
		///   Handles a key up event.
		/// </summary>
		/// <param name = "sender">The sender.</param>
		/// <param name = "e">The <see cref = "System.Windows.Forms.KeyEventArgs" /> instance containing the event data.</param>
		private void HandleKeyUp(object sender, KeyEventArgs e)
		{
			KeyUp(e);
		}

		private void HandleResize(object sender, EventArgs e)
		{
			if (MainForm.WindowState == FormWindowState.Minimized)
			{
				return;
			}
		}

		protected virtual void KeyDown(KeyEventArgs e)
		{
			Input.KeyDown(e.KeyCode);
		}

		protected virtual void KeyUp(KeyEventArgs e)
		{
			Input.KeyUp(e.KeyCode);
		}

		#endregion
	}
}
	