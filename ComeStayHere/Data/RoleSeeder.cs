using Microsoft.AspNetCore.Identity;

namespace ComeStayHere.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Guest", "Host", "Admin", "SuperAdmin" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var superAdminEmail = "superadmin@comestayhere.co.za";
            var superAdminPassword = "Admin@123";

            var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdmin == null)
            {
                superAdmin = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    superAdmin,
                    superAdminPassword
                );

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                }
            }
        }
    }
}