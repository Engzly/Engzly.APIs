using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engzly.Domain.Entities.Identity;

namespace Engzly.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateJwtToken(User user);
        string GenerateRefreshToken();
    }
}
