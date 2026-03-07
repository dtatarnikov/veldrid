using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLSwapchainTexture : MTLTexture
    {
        private MetalBindings.MTLTexture _deviceTexture;
        private uint _width;
        private uint _height;
        private MTLPixelFormat _mtlPixelFormat;

        public override MetalBindings.MTLTexture DeviceTexture => _deviceTexture;

        public override MTLPixelFormat MTLPixelFormat => _mtlPixelFormat;

        public override MTLTextureType MTLTextureType => MTLTextureType.Type2D;

        public override uint Width => _width;

        public override uint Height => _height;

        public override uint Depth => 1;

        public override uint ArrayLayers => 1;

        public override uint MipLevels => 1;

        public override TextureUsage Usage => TextureUsage.RenderTarget;

        public override TextureType Type => TextureType.Texture2D;

        public override TextureSampleCount SampleCount => TextureSampleCount.Count1;
        
        public void SetDrawable(CAMetalDrawable drawable, CGSize size, PixelFormat format)
        {
            _deviceTexture = drawable.texture;
            _width = (uint)size.width;
            _height = (uint)size.height;
            _mtlPixelFormat = MTLFormats.VdToMTLPixelFormat(Format, false);
        }
    }
}
