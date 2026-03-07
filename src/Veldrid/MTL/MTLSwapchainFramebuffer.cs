using System.Collections.Generic;
using System.Diagnostics;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLSwapchainFramebuffer : MTLFramebuffer
    {
        private readonly MTLGraphicsDevice _gd;
        private readonly MTLSwapchain _parentSwapchain;
        private readonly MTLSwapchainTexture _colorTexture;
        private MTLTexture _depthTexture;
        
        private readonly FramebufferAttachment[] _colorTargets;
        private FramebufferAttachment? _depthTarget;
        
        private readonly PixelFormat _colorFormat;
        private readonly PixelFormat? _depthFormat;

        public override uint Width => _colorTexture.Width;
        public override uint Height => _colorTexture.Height;

        public override OutputDescription OutputDescription { get; }

        public override IReadOnlyList<FramebufferAttachment> ColorTargets => _colorTargets;
        public override FramebufferAttachment? DepthTarget => _depthTarget;

        public MTLSwapchainFramebuffer(
            MTLGraphicsDevice gd,
            MTLSwapchain parent,
            PixelFormat? depthFormat,
            PixelFormat colorFormat)
        {
            _gd = gd;
            _parentSwapchain = parent;
            _colorFormat = colorFormat;
            
            OutputAttachmentDescription? depthAttachment = null;
            if (depthFormat != null)
            {
                _depthFormat = depthFormat;
                depthAttachment = new OutputAttachmentDescription(depthFormat.Value);
            }
            
            _colorTexture = new MTLSwapchainTexture();
            OutputAttachmentDescription colorAttachment = new OutputAttachmentDescription(colorFormat);
            _colorTargets = new [] { new FramebufferAttachment(_colorTexture, 0) };

            OutputDescription = new OutputDescription(depthAttachment, colorAttachment);
        }

        private void RecreateDepthTexture(uint width, uint height)
        {
            Debug.Assert(_depthFormat.HasValue);
            _depthTexture?.Dispose();

            _depthTexture = Util.AssertSubtype<Texture, MTLTexture>(
                _gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                    width, height, 1, 1, _depthFormat.Value, TextureUsage.DepthStencil)));
            _depthTarget = new FramebufferAttachment(_depthTexture, 0);
        }

        public void UpdateTextures(CAMetalDrawable drawable, CGSize size)
        {
            _colorTexture.SetDrawable(drawable, size, _colorFormat);

            if (_depthFormat.HasValue && (size.width != _depthTexture?.Width || size.height != _depthTexture?.Height))
                RecreateDepthTexture((uint)size.width, (uint)size.height);
        }

        public bool EnsureDrawableAvailable() => _parentSwapchain.EnsureDrawableAvailable();

        public override void Dispose()
        {
            _depthTexture?.Dispose();
            base.Dispose();
        }
    }
}
