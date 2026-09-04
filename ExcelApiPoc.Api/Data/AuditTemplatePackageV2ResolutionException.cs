namespace ExcelApiPoc.Api.Data;

public enum AuditTemplatePackageV2ResolutionFailure
{
    TemplateNotFound,
    FrameworkNotFound,
    TemplateNotApplicable,
    AssociationNotFound,
    MultipleAssociations,
    InconsistentFrameworkVersionReferences,
    InconsistentConfiguration
}

public sealed class AuditTemplatePackageV2ResolutionException : Exception
{
    public AuditTemplatePackageV2ResolutionException(AuditTemplatePackageV2ResolutionFailure failure, string message)
        : base(message)
    {
        Failure = failure;
    }

    public AuditTemplatePackageV2ResolutionFailure Failure { get; }
}
