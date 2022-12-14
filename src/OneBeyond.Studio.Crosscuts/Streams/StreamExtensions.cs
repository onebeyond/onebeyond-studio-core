using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;

namespace OneBeyond.Studio.Crosscuts.Streams;

/// <summary>
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="inputStream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<byte[]> ToByteArrayAsync(this Stream inputStream, CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNull(inputStream, nameof(inputStream));

        using (var memmoryStream = new MemoryStream())
        {
            await inputStream.CopyToAsync(memmoryStream, 81920, cancellationToken).ConfigureAwait(false);
            inputStream.Position = 0;
            return memmoryStream.ToArray();
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="inputStream"></param>
    /// <returns></returns>
    public static byte[] ToByteArray(this Stream inputStream)
    {
        EnsureArg.IsNotNull(inputStream, nameof(inputStream));

        using (var memmoryStream = new MemoryStream())
        {
            inputStream.CopyTo(memmoryStream, 81920);
            inputStream.Position = 0;
            return memmoryStream.ToArray();
        }
    }
}
