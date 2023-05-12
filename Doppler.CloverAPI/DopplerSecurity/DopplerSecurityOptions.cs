using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace Doppler.CloverAPI.DopplerSecurity;

public class DopplerSecurityOptions
{
    public IEnumerable<SecurityKey> SigningKeys { get; set; } = System.Array.Empty<SecurityKey>();
}
