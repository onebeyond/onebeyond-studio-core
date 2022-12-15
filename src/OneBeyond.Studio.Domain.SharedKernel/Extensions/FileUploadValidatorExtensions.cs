using EnsureThat;
using OneBeyond.Studio.Crosscuts.Utilities.FileUploadValidators;
using OneBeyond.Studio.Domain.SharedKernel.Entities.Dto;

namespace OneBeyond.Studio.Domain.SharedKernel.Extensions;

/// <summary>
/// </summary>
public static class FileUploadValidatorExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public static void ValidateFile(this FileValidatorBuilder validator, FileContentDto dto)
    {
        EnsureArg.IsNotNull(dto, nameof(dto));
        validator.ValidateFile(dto.Name, dto.ContentType, dto.Content);
    }

    /// <summary>
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public static void ValidateFile(this IFileContentValidator validator, FileContentDto dto)
    {
        EnsureArg.IsNotNull(dto, nameof(dto));
        validator.ValidateFileContent(dto.Name, dto.ContentType, dto.Content);
    }

}
