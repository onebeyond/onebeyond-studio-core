namespace OneBeyond.Studio.Domain.SharedKernel.Specifications;

/// <summary>
/// </summary>
public class Paging
{
    /// <summary>
    /// </summary>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    public Paging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    /// <summary>
    /// </summary>
    public int Skip { get; }

    /// <summary>
    /// </summary>
    public int Take { get; }
}
