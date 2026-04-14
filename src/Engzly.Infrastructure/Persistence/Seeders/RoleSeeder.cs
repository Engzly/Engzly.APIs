using Engzly.Domain.Entities.Identity;
using Engzly.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Engzly.Infrastructure.Persistence.Seeders
{
    public static class RoleSeeder
    {
        public const string AdminRole = "Admin";
        public const string ClientRole = "Client";
        public const string HelperRole = "Helper";

        private static readonly string[] Roles = { AdminRole, ClientRole, HelperRole };

        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager,
            IConfiguration configuration)
        {
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var email = Environment.GetEnvironmentVariable("ENGZLY_ADMIN_EMAIL")
                        ?? configuration["Admin:Email"];
            var password = Environment.GetEnvironmentVariable("ENGZLY_ADMIN_PASSWORD")
                           ?? configuration["Admin:Password"];
            var firstName = Environment.GetEnvironmentVariable("ENGZLY_ADMIN_FIRSTNAME")
                            ?? configuration["Admin:FirstName"]
                            ?? "Engzly";
            var lastName = Environment.GetEnvironmentVariable("ENGZLY_ADMIN_LASTNAME")
                           ?? configuration["Admin:LastName"]
                           ?? "Admin";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                if (!await userManager.IsInRoleAsync(existing, AdminRole))
                    await userManager.AddToRoleAsync(existing, AdminRole);
                return;
            }

            var admin = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                City = "N/A",
                AccountType = AccountType.Client,
                Status = UserStatus.Active,
                IsIdentityVerified = true
            };

            var created = await userManager.CreateAsync(admin, password);
            if (!created.Succeeded)
                return;

            await userManager.AddToRoleAsync(admin, AdminRole);
        }
    }
}
