using Ecom.Utility;
using ECom.DataAccess.Data;
using ECom.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECom.DataAccess.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DBInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception)
            {
            }

            if (!_roleManager.RoleExistsAsync(Roles.ADMIN).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(Roles.ADMIN)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Roles.CUSTOMER)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Roles.EMPLOYEE)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Roles.COMPANY)).GetAwaiter().GetResult();


                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "Admin@gmail.com",
                    Email = "Admin@gmail.com",
                    Name = "Mahesh Maheshwari",
                    PhoneNumber = "789456123",
                    StreetAddress = "Malad",
                    State = "Maharashtra",
                    PostalCode = "400097",
                    City = "Mumbai"
                }, "Admin@123").GetAwaiter().GetResult();
                ApplicationUser user = _db.ApplicationUser.FirstOrDefault(x => x.Email == "Admin@gmail.com");
                _userManager.AddToRoleAsync(user, Roles.ADMIN).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
