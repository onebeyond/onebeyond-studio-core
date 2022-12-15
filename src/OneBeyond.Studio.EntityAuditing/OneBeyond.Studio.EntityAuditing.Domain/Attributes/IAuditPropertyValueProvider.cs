namespace OneBeyond.Studio.EntityAuditing.Domain.Attributes;

public interface IAuditPropertyValueProvider
{
    object GetValue(object entity, string columnName, object currentValue);
}
