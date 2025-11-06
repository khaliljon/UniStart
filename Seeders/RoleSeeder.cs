using Microsoft.AspNetCore.Identity;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Seeders
{
    /// <summary>
    /// Инициализация ролей и администратора
    /// </summary>
    public static class RoleSeeder
    {
        public static async Task SeedRolesAndAdminAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            // Создание ролей, если их нет
            string[] roles = { UserRoles.Admin, UserRoles.Teacher, UserRoles.Student };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"✅ Роль '{role}' создана");
                }
            }

            // Создание администратора по умолчанию
            var adminEmail = "admin@unistart.kz";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Администратор",
                    LastName = "Системы",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                    Console.WriteLine($"✅ Администратор создан: {adminEmail} / Admin123!");
                }
                else
                {
                    Console.WriteLine($"❌ Ошибка создания администратора: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Убедимся, что у существующего админа есть роль
                if (!await userManager.IsInRoleAsync(adminUser, UserRoles.Admin))
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                    Console.WriteLine($"✅ Роль Admin добавлена пользователю {adminEmail}");
                }
            }

            // Назначаем роль Student тестовому пользователю
            var testUser = await userManager.FindByEmailAsync("test@unistart.kz");
            if (testUser != null && !await userManager.IsInRoleAsync(testUser, UserRoles.Student))
            {
                await userManager.AddToRoleAsync(testUser, UserRoles.Student);
                Console.WriteLine($"✅ Роль Student добавлена тестовому пользователю");
            }
        }
    }
}
