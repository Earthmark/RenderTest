using System;
using RenderTest.Main;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;
using Device1 = SharpDX.Direct3D11.Device1;
using Resource = SharpDX.Direct3D11.Resource;

namespace RenderTest.Drawing
{
	/// <summary>
	/// The base class used for Direct3D usage.
	/// </summary>
	public sealed class D3D : IDisposable
	{
		#region Field
		
		private SwapChain SwapChain;

		private RenderTargetView RenderTargetView;

		private Texture2D DepthStencilBuffer;

		private DepthStencilState DepthStencilState;

		private DepthStencilView DepthStencilView;

		private RasterizerState RasterState;

		#endregion

		#region Properties

		public bool VSync { get; private set; }

		public int VideoCardMemory { get; private set; }
		
		public string VideoCardDescription { get; private set; }

		public Device Device { get; private set; }

		public DeviceContext Context { get; private set; }

		public Matrix ProjectionMatrix { get; private set; }

		public Matrix WorldMatrix { get; private set; }
		
		public Matrix OrthoMatrix { get; private set; }

		public bool FullScreen
		{
			get { return SwapChain.IsFullScreen; }
			set { SwapChain.IsFullScreen = value; }
		}

		#endregion

		#region Initialize and shutdown

		/// <summary>
		/// Starts the current instance as linked from a <see cref="Graphics"/> manager.
		/// </summary>
		/// <param name="graphics">The object to get information from.</param>
		/// <param name="hwnd">The pointer to the current <see cref="RenderForm"/>.</param>
		/// <returns>If the device binding was successful.</returns>
		public bool Initialize(int height, int width, float screenDepth, float screenNear, IntPtr hwnd)
		{
			try
			{
				// Store the vsync setting.
				VSync = true;
				
				// Create a DirectX graphics interface factory.
				var factory = new Factory();
				// Use the factory to create an adapter for the primary graphics interface (video card).
				var adapter = factory.GetAdapter(0);
				// Get the primary adapter output (monitor).
				var monitor = adapter.Outputs[0];
				// Get modes that fit the DXGI_FORMAT_R8G8B8A8_UNORM display format for the adapter output (monitor).
				var modes = monitor.GetDisplayModeList(Format.R8G8B8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);
				// Now go through all the display modes and find the one that matches the screen width and height.
				// When a match is found store the the refresh rate for that monitor, if vertical sync is enabled. 
				// Otherwise we use maximum refresh rate.
				var rational = new Rational(0, 1);
				if (VSync)
				{
					foreach (var mode in modes)
					{
						if (mode.Width == width && mode.Height == height)
						{
							rational = new Rational(mode.RefreshRate.Numerator, mode.RefreshRate.Denominator);
							break;
						}
					}
				}

				// Get the adapter (video card) description.
				var adapterDescription = adapter.Description;

				// Store the dedicated video card memory in megabytes.
				VideoCardMemory = adapterDescription.DedicatedVideoMemory >> 10 >> 10;

				// Convert the name of the video card to a character array and store it.
				VideoCardDescription = adapterDescription.Description;

				// Release the adapter output.
				monitor.Dispose();

				// Release the adapter.
				adapter.Dispose();

				// Release the factory.
				factory.Dispose();

				// Initialize the swap chain description.
				var swapChainDesc = new SwapChainDescription()
				{
					// Set to a single back buffer.
					BufferCount = 1,
					// Set the width and height of the back buffer.
					ModeDescription = new ModeDescription(width, height, rational, Format.R8G8B8A8_UNorm),
					// Set the usage of the back buffer.
					Usage = Usage.RenderTargetOutput,
					// Set the handle for the window to render to.
					OutputHandle = hwnd,
					// Turn multisampling off.
					SampleDescription = new SampleDescription(1, 0),
					// Set to full screen or windowed mode.
					IsWindowed = true,
					// Don't set the advanced flags.
					Flags = SwapChainFlags.None,
					// Discard the back buffer content after presenting.
					SwapEffect = SwapEffect.Discard
				};

				// Create the swap chain, Direct3D device, and Direct3D device context.
				Device device;
				SwapChain swapChain;
				Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, swapChainDesc, out device, out swapChain);

				Device = device;
				SwapChain = swapChain;
				Context = device.ImmediateContext;

				// Get the pointer to the back buffer.
				var backBuffer = Resource.FromSwapChain<Texture2D>(SwapChain, 0);

				// Create the render target view with the back buffer pointer.
				RenderTargetView = new RenderTargetView(device, backBuffer);

				// Release pointer to the back buffer as we no longer need it.
				backBuffer.Dispose();

				// Initialize and set up the description of the depth buffer.
				var depthBufferDesc = new Texture2DDescription()
				{
					Width = Core.Width,
					Height = Core.Height,
					MipLevels = 1,
					ArraySize = 1,
					Format = Format.D24_UNorm_S8_UInt,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default,
					BindFlags = BindFlags.DepthStencil,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None
				};

				// Create the texture for the depth buffer using the filled out description.
				DepthStencilBuffer = new Texture2D(device, depthBufferDesc);

				// Initialize and set up the description of the stencil state.
				var depthStencilDesc = new DepthStencilStateDescription()
				{
					IsDepthEnabled = true,
					DepthWriteMask = DepthWriteMask.All,
					DepthComparison = Comparison.Less,
					IsStencilEnabled = true,
					StencilReadMask = 0xFF,
					StencilWriteMask = 0xFF,
					// Stencil operation if pixel front-facing.
					FrontFace = new DepthStencilOperationDescription()
					{
						FailOperation = StencilOperation.Keep,
						DepthFailOperation = StencilOperation.Increment,
						PassOperation = StencilOperation.Keep,
						Comparison = Comparison.Always
					},
					// Stencil operation if pixel is back-facing.
					BackFace = new DepthStencilOperationDescription()
					{
						FailOperation = StencilOperation.Keep,
						DepthFailOperation = StencilOperation.Decrement,
						PassOperation = StencilOperation.Keep,
						Comparison = Comparison.Always
					}
				};

				// Create the depth stencil state.
				DepthStencilState = new DepthStencilState(Device, depthStencilDesc);

				// Set the depth stencil state.
				Context.OutputMerger.SetDepthStencilState(DepthStencilState, 1);

				// Initialize and set up the depth stencil view.
				var depthStencilViewDesc = new DepthStencilViewDescription()
				{
					Format = Format.D24_UNorm_S8_UInt,
					Dimension = DepthStencilViewDimension.Texture2D,
					Texture2D = new DepthStencilViewDescription.Texture2DResource()
					{
						MipSlice = 0
					}
				};

				// Create the depth stencil view.
				DepthStencilView = new DepthStencilView(Device, DepthStencilBuffer, depthStencilViewDesc);

				// Bind the render target view and depth stencil buffer to the output render pipeline.
				Context.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);

				// Setup the raster description which will determine how and what polygon will be drawn.
				var rasterDesc = new RasterizerStateDescription()
				{
					IsAntialiasedLineEnabled = false,
					CullMode = CullMode.Back,
					DepthBias = 0,
					DepthBiasClamp = .0f,
					IsDepthClipEnabled = true,
					FillMode = FillMode.Solid,
					IsFrontCounterClockwise = false,
					IsMultisampleEnabled = false,
					IsScissorEnabled = false,
					SlopeScaledDepthBias = .0f
				};

				// Create the rasterizer state from the description we just filled out.
				RasterState = new RasterizerState(Device, rasterDesc);

				// Now set the rasterizer state.
				Context.Rasterizer.State = RasterState;

				// Setup and create the viewport for rendering.
				Context.Rasterizer.SetViewport(0, 0, width, height, 0, 1);

				// Setup and create the projection matrix.
				ProjectionMatrix = Matrix.PerspectiveFovLH((float)(Math.PI / 4), ((float)width) / height, screenNear, screenDepth);

				// Initialize the world matrix to the identity matrix.
				WorldMatrix = Matrix.Identity;

				// Create an orthographic projection matrix for 2D rendering.
				OrthoMatrix = Matrix.OrthoLH(width, height, screenNear, screenDepth);

				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Finalize, used for emergency disposing.
		/// </summary>
		~D3D()
		{
			Shutdown();
		}

		/// <summary>
		/// Disposes the current instance.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Shutdown();
		}

		/// <summary>
		/// Does actual cleanup.
		/// </summary>
		private void Shutdown()
		{
			if(SwapChain != null) SwapChain.SetFullscreenState(false, null);

			if(RasterState.SafeDispose()) RasterState = null;

			if(DepthStencilView.SafeDispose()) DepthStencilView = null;
			if(DepthStencilState.SafeDispose()) DepthStencilState = null;
			if(DepthStencilBuffer.SafeDispose()) DepthStencilBuffer = null;

			if(RenderTargetView.SafeDispose()) RenderTargetView = null;

			if(Device.SafeDispose()) Device = null;
			if(SwapChain.SafeDispose()) SwapChain = null;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Clears the rendertarget with the specified color.
		/// </summary>
		/// <param name="color">The color to clear with.</param>
		public void BeginScene(Color4 color)
		{
			Context.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1, 0);

			Context.ClearRenderTargetView(RenderTargetView, color);
		}

		/// <summary>
		/// Ends the current presentation information.
		/// </summary>
		public void EndScene()
		{
			SwapChain.Present(VSync ? 1 : 0, PresentFlags.None);
		}

		#endregion
	}
}
