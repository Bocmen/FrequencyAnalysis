using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace FrequencyAnalysisApp.Extension
{
    public static class UIElementExtension
    {
        public static async Task SavePngAsync(this UIElement uiElement, StorageFile storageFile)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(uiElement);
            var pixelBuffer = await rtb.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();
            var displayInformation = DisplayInformation.GetForCurrentView();
            using (var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                     BitmapAlphaMode.Premultiplied,
                                     (uint)rtb.PixelWidth,
                                     (uint)rtb.PixelHeight,
                                     displayInformation.RawDpiX,
                                     displayInformation.RawDpiY,
                                     pixels);
                await encoder.FlushAsync();
            }
        }
    }
}
