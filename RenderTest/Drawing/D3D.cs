using System;
using RenderTest.Main;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Device1 = SharpDX.Direct3D11.Device1;
using Resource = SharpDX.Direct3D11.Resource;

namespace RenderTest.Drawing
{
	public sealed class D3D
	{
		private SwapChain1 swapChain;
		private RenderTargetView renderTargetView;
		private Texture2D depthStencilBuffer;
		private DepthStencilState depthStencilState;
		private DepthStencilView depthStencilView;
		private RasterizerState rasterState;
		private Adapter adapter;

		public int VideoCardMemory { get; private set; }
		public string VideoCardDescription { get; private set; }
		public Device1 Device { get; private set; }
		public DeviceContext1 Context { get; private set; }

		public Matrix ProjectionMatrix { get; private set; }
		public Matrix WorldMatrix { get; private set; }
		public Matrix OrthoMatrix { get; private set; }

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
					VideoCardMemory = adapter.Description.DedicatedVideoMemory << 10 << 10;

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

				var rasterDesc = new RasterizerStateDescription
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

				rasterState = new RasterizerState(Device, rasterDesc);

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

		public void Shutdown()
		{
			if(swapChain != null)
			{
				swapChain.SetFullscreenState(false, null);
			}

			if(rasterState != null)
			{
				rasterState.Dispose();
				rasterState = null;
			}

			if(depthStencilView != null)
			{
				depthStencilView.Dispose();
				depthStencilView = null;
			}

			if(depthStencilState != null)
			{
				depthStencilState.Dispose();
				depthStencilState = null;
			}

			if(depthStencilBuffer != null)
			{
				depthStencilBuffer.Dispose();
				depthStencilBuffer = null;
			}

			if(renderTargetView != null)
			{
				renderTargetView.Dispose();
				renderTargetView = null;
			}

			if(Device != null)
			{
				Device.Dispose();
				Device = null;
			}

			if(swapChain != null)
			{
				swapChain.Dispose();
				swapChain = null;
			}
		}

		public void BeginScene(Color4 color)
		{
			Context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1, 0);

			Context.ClearRenderTargetView(renderTargetView, color);
		}

		public void EndScene()
		{
			swapChain.Present(Core.Graphics.VSync ? 1 : 0, 0);
		}
	}
}
