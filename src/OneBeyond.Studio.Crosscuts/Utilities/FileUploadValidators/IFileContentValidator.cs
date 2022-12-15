using System.IO;

namespace OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;

public interface IFileContentValidator
{
    /// <summary>
    /// Validates if the file provided corresponds to the content type stated.
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="contentType">File content type</param>
    /// <param name="content">File stream</param>
    /// <returns></returns>
    void ValidateFileContent(string fileName, string contentType, Stream content);

    /// <summary>
    /// Validates if the file provided corresponds to the content type stated.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <param name="content"></param>
    void ValidateFileContent(string fileName, string contentType, byte[] content);

    /// <summary>
    /// </summary>
    string ContentType { get; }
}
