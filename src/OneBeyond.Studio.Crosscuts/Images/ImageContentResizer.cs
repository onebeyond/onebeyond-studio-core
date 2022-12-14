using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace OneBeyond.Studio.Crosscuts.Images;

public static class ImageContentResizer
{
    /// <summary>
    /// Resizes an image proportionally to its new width.
    /// </summary>
    public static Stream Resize(Stream imageContentStream, int newWidth)
    {
        var resizedContent = new MemoryStream();

        using (imageContentStream)
        using (var image = Image.FromStream(imageContentStream))
        {
            var imageResizeFactor = image.Width * 1.0 / newWidth;

            using (var resizedImage = image.Resize(
                Convert.ToInt32(image.Width / imageResizeFactor),
                Convert.ToInt32(image.Height / imageResizeFactor)))
            {
                resizedImage.Save(resizedContent, ImageFormat.Png);
                resizedContent.Position = 0;
            }
        }

        return resizedContent;
    }
}
