using GoGoTour.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GoGoTour.Api.Controllers;

[ApiController]
public abstract class BaseAdminController : ControllerBase
{
    private readonly AdminAuthOptions _adminAuthOptions;

    protected BaseAdminController(IOptions<AdminAuthOptions> adminAuthOptions)
    {
        _adminAuthOptions = adminAuthOptions.Value;
    }

    protected bool IsAuthorizedAdmin()
    {
        if (!Request.Headers.TryGetValue("X-Admin-Token", out var token))
        {
            return false;
        }

        return token.ToString() == _adminAuthOptions.Token;
    }
}
