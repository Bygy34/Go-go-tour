namespace GoGoTour.Infrastructure.Security;

public class AdminAuthOptions
{
    public const string SectionName = "AdminAuth";
    public string Token { get; set; } = "super-admin-token";
}
