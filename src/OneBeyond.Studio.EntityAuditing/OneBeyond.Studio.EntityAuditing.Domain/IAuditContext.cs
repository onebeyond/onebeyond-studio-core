namespace OneBeyond.Studio.EntityAuditing.Domain;

public interface IAuditContext
{
    string AuthorId { get; }
    string Context { get; }
}
