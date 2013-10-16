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
		
		private SwapChain1 swapChain;

		private RenderTargetView renderTargetView;

		private Texture2D depthStencilBuffer;

		private DepthStencilState depthStencilState;

		private DepthStencilView depthStencilView;

		private RasterizerState rasterState;

		private Adapter adapter;

		#endregion

		#region Properties

		public int VideoCardMemory { get; private set; }
		
		public string VideoCardDescription { get; private set; }

		public Device1 Device { get; private set; }

		public DeviceContext1 Context { get; private set; }

		public Matrix ProjectionMatrix { get; private set; }

		public Matrix WorldMatrix { get; private set; }
		
		public Matrix OrthoMatrix { get; private set; }

		public bool FullScreen
		{
			get { return swapChain.IsFullScreen; }
			set { swapChain.IsFullScreen = value; }
		}

		#endregion

		#region Initialize and shutdown

		/// <summary>
		/// Starts the current instance as linked from a <see cref="Graphics"/> manager.
		/// </summary>
		/// <param name="graphics">The object to get information from.</param>
		/// <param name="hwnd">The pointer to the current <see cref="RenderForm"/>.</param>
		/// <returns>If the device binding was successful.</returns>
		public bool Initialize(Graphics graphics, IntPtr hwnd)
		{
			try
			{
				var swapChainDescription = new SwapChainDescription1
				{
					BufferCount = 1,
					Usage = Usage.RenderTargetOutput,
					Width = Core.Width,
					Height = Core.Height,
					Format = Format.R8G8B8A8_UNorm,
					SampleDescription = new SampleDescription(1, 0),
					Flags = SwapChainFlags.None,
					SwapEffect = SwapEffect.Discard
				};
				var swapFullScreenDescription = new SwapChainFullScreenDescription
				{
					RefreshRate = new Rational(60, 1),
					Windowed = true
				};

				using(var defaultDevice = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug))
					//using (var defaultDevice = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport))
				{
					Device = defaultDevice.QueryInterface<Device1>();
				}

				using(var dxgiDevice2 = Device.QueryInterface<Device2>())
				{
					adapter = dxgiDevice2.Adapter;
					VideoCardDescription = adapter.Description.Description;
					VideoCardMemory = adapter.Description.DedicatedVideoMemory >> 10 >> 10;
					
					using(var dxgiFactory2 = adapter.GetParent<Factory2>())
					{
						dxgiFactory2.MakeWindowAssociation(hwnd, WindowAssociationFlags.IgnoreAltEnter);
						swapChain = dxgiFactory2.CreateSwapChainForHwnd(dxgiDevice2, hwnd, ref swapChainDescription, swapFullScreenDescription, null);
					}
				}

				Context = Device.ImmediateContext1;

				var backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0);
				renderTargetView = new RenderTargetView(Device, backBuffer);

				var textDes = new Texture2DDescription
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

				depthStencilBuffer = new Texture2D(Device, textDes);

				var depthStencilDesc = new DepthStencilStateDescription
				{
					IsDepthEnabled = true,
					DepthWriteMask = DepthWriteMask.All,
					DepthComparison = Comparison.Less,
					IsStencilEnabled = true,
					StencilReadMask = 0xFF,
					StencilWriteMask = 0xFF,

					FrontFace = new DepthStencilOperationDescription
					{
						FailOperation = StencilOperation.Keep,
						DepthFailOperation = StencilOperation.Increment,
						PassOperation = StencilOperation.Keep,
						Comparison = Comparison.Always
					},

					BackFace = new DepthStencilOperationDescription
					{
						FailOperation = StencilOperation.Keep,
						DepthFailOperation = StencilOperation.Decrement,
						PassOperation = StencilOperation.Keep,
						Comparison = Comparison.Always
					}
				};

				depthStencilState = new DepthStencilState(Device, depthStencilDesc);

				Context.OutputMerger.SetDepthStencilState(depthStencilState, 1);

				var depthStencilViewDesc = new DepthStencilViewDescription
				{
					Format = Format.D24_UNorm_S8_UInt,
					Dimension = DepthStencilViewDimension.Texture2D,
					Texture2D = new DepthStencilViewDescription.Texture2DResource
					{
						MipSlice = 0
					}
				};

				depthStencilView = new DepthStencilView(Device, depthStencilBuffer, depthStencilViewDesc);

				Context.OutputMerger.SetTargets(depthStencilView, renderTargetView);
				
				var rasterDesc = new RasterizerStateDescription
				{
					IsAntialiasedLineEnabled = false,
					CullMode = CullMode.Back,
					DepthBias = 0,
					DepthBiasClamp = 0.0f,
					IsDepthClipEnabled = true,
					FillMode = FillMode.Solid,
					IsFrontCounterClockwise = false,
					IsMultisampleEnabled = false,
					IsScissorEnabled = false,
					SlopeScaledDepthBias = .0f
				};

				rasterState = new RasterizerState(Device, rasterDesc);

				Context.Rasterizer.State = rasterState;
				Context.Rasterizer.SetViewport(0, 0, Core.Width, Core.Height);

				ProjectionMatrix = Matrix.PerspectiveFovLH((float) (Math.PI / 4), ((float) Core.Width) / Core.Height, graphics.ScreenNear, graphics.ScreenDepth);
				WorldMatrix = Matrix.Identity;
				OrthoMatrix = Matrix.OrthoLH(Core.Width, Core.Height, graphics.ScreenNear, graphics.ScreenDepth);
				
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
			if(swapChain != null) swapChain.SetFullscreenState(false, null);

			if(rasterState.SafeDispose()) rasterState = null;

			if(depthStencilView.SafeDispose()) depthStencilView = null;
			if(depthStencilState.SafeDispose()) depthStencilState = null;
			if(depthStencilBuffer.SafeDispose()) depthStencilBuffer = null;

			if(renderTargetView.SafeDispose()) renderTargetView = null;

			if(Device.SafeDispose()) Device = null;
			if(swapChain.SafeDispose()) swapChain = null;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Clears the rendertarget with the specified color.
		/// </summary>
		/// <param name="color">The color to clear with.</param>
		public void BeginScene(Color4 color)
		{
			Context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1, 0);

			Context.ClearRenderTargetView(renderTargetView, color);
		}

		/// <summary>
		/// Ends the current presentation information.
		/// </summary>
		public void EndScene()
		{
			swapChain.Present(Core.Graphics.VSync ? 1 : 0, 0);
		}

		#endregion
	}
}
