using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Doppler.CloverAPI.DopplerSecurity;

public partial class IsSuperUserAuthorizationHandler : AuthorizationHandler<DopplerAuthorizationRequirement>
{
    private readonly ILogger<IsSuperUserAuthorizationHandler> _logger;

    [LoggerMessage(0, LogLevel.Debug, "The token hasn't super user permissions.")]
    partial void LogDebugTokenHasNotSuperuserPermissions();

    [LoggerMessage(1, LogLevel.Debug, "The token super user permissions is false.")]
    partial void LogDebugTokenSuperuserPermissionsIsFalse();

    public IsSuperUserAuthorizationHandler(ILogger<IsSuperUserAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DopplerAuthorizationRequirement requirement)
    {
        if (requirement.AllowSuperUser && IsSuperUser(context))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private bool IsSuperUser(AuthorizationHandlerContext context)
    {
        if (!context.User.HasClaim(c => c.Type.Equals(DopplerSecurityDefaults.SuperuserJwtKey, StringComparison.Ordinal)))
        {
            LogDebugTokenHasNotSuperuserPermissions();
            return false;
        }

        var isSuperUser = bool.Parse(context.User.FindFirst(c => c.Type.Equals(DopplerSecurityDefaults.SuperuserJwtKey, StringComparison.Ordinal)).Value);
        if (isSuperUser)
        {
            return true;
        }

        LogDebugTokenSuperuserPermissionsIsFalse();
        return false;
    }
}
