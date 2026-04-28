using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

namespace ImageProcessingSystem
{
    public interface IImageService
    {
        Task<byte[]> ProcessAndEnrichAsync(Stream inputStream, string fileName);
    }

    public class ImageService : IImageService
    {
        public async Task<byte[]> ProcessAndEnrichAsync(Stream inputStream, string fileName)
        {
            using var image = await Image.LoadAsync(inputStream);

            // Metadata Enrichment: Trích xuất thông tin (Làm dày logic code)
            var width = image.Width;
            var height = image.Height;

            // Xử lý Resize
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(400, 0),
                Mode = ResizeMode.Max
            }));

            // Chèn Watermark
            if (SystemFonts.Collection.Families.Any())
            {
                var family = SystemFonts.Collection.Families.First();
                var font = family.CreateFont(20, FontStyle.Bold);
                image.Mutate(x => x.DrawText($"Tuan - {width}x{height}", font, Color.WhiteSmoke, new PointF(10, 10)));
            }

            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            return ms.ToArray();
        }
    }
}