using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    // Extension methods for user manager to include Address and to find by email from claims principal
    // Course item 175
    public static class UserManagerExtensions
    {
        public static async Task<AppUser> FindByUserByClaimsPrincipleWithAddressAsync
        (this UserManager<AppUser> input, ClaimsPrincipal user)
        {
            var email = user?.Claims?
                        .FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            
            return await input.Users.Include(x => x.Address).SingleOrDefaultAsync(x => x.Email == email);
        }

        public static async Task<AppUser> FindByEmailFromClaimsPrinciple(this UserManager<AppUser> input, 
        ClaimsPrincipal user)
        {
             var email = user?.Claims?
                        .FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                        
            //Same as the above method but without the address included
            return await input.Users.SingleOrDefaultAsync(x => x.Email == email);
        }


    }
}