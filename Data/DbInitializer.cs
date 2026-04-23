using Microsoft.AspNetCore.Identity;
using Pets_friends.Models;

namespace Pets_friends.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // Get the RoleManager and UserManager from the DI container
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<UserAccount>>();

            // 1. Define the roles your application needs
            string[] roleNames = { "Admin", "Client", "Vet", "Merchant", "Shelter" };

            // 2. Loop through and create them if they don't exist
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 3. Create a default Admin account if one doesn't exist
            string adminEmail = "admin@petfriends.jo";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);


            // ... (Your existing code for creating the Admin account is right above this)

            // 4. Create a Dummy Vet so we can test the UI!
            string vetEmail = "drsarah@petfriends.jo";
            var vetUser = await userManager.FindByEmailAsync(vetEmail);

            if (vetUser == null)
            {
                // Create the Identity Account first
                var newVet = new UserAccount
                {
                    UserName = vetEmail,
                    Email = vetEmail,
                    FullName = "Dr. Sarah Al-Khalidi",
                    PhoneNumber = "+962 79 123 4567",
                    EmailConfirmed = true,
                    IsProfileComplete = true
                };

                var createVet = await userManager.CreateAsync(newVet, "Vet123!");
                if (createVet.Succeeded)
                {
                    await userManager.AddToRoleAsync(newVet, "Vet");

                    // Now link a VetProfile to that Identity Account
                    var context = serviceProvider.GetRequiredService<AppDbContext>();

                    context.VetProfiles.Add(new VetProfile
                    {
                        UserAccountId = newVet.Id, // Link to the account we just made!
                        Specialization = "Small Animal & Exotic Pet Specialist",
                        ClinicName = "PetPals Veterinary Center",
                        ClinicAddress = "12 Al-Madeena St., Amman, Jordan",
                        YearsOfExperience = 9,
                        Description = "Dr. Sarah is a compassionate veterinarian with over 9 years of experience caring for dogs, cats, rabbits, and exotic companions.",
                        ImageUrl = "https://placehold.co/160x160/C8A882/white?text=Dr.Sarah",
                        Services = "General Check-ups & Vaccinations, Dental Cleaning & Oral Health, Spay & Neuter Surgery, Emergency & Critical Care"
                    });

                    await context.SaveChangesAsync();
                }
            }
            if (adminUser == null)
            {
                var newAdmin = new UserAccount
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Master Admin",
                    EmailConfirmed = true,
                    IsProfileComplete = true
                };

                // Create the user with a default password
                var createPowerUser = await userManager.CreateAsync(newAdmin, "Admin123!");
                if (createPowerUser.Succeeded)
                {
                    // Assign the "Admin" role to this user
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}