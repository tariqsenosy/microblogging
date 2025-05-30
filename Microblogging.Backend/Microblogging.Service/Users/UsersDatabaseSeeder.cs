using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microblogging.Service.Users
{
    public class UsersDatabaseSeeder
    {
        private readonly UserManager<MongoUser> _userManager;

        public UsersDatabaseSeeder(UserManager<MongoUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task SeedAsync()
        {
            const string defaultUsername = "abjjad";
            const string defaultPassword = "123456";

            var existingUser = await _userManager.FindByNameAsync(defaultUsername);
            if (existingUser == null)
            {
                var user = new MongoUser
                {
                    UserName = defaultUsername,
                    Email = "abjjad@example.com", // optional
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, defaultPassword);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to seed default user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}