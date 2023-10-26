
using ECom.Models.ViewModels;
using ECom.DataAccess.Data;
using ECom.DataAccess.Repository.IRepository;
using ECom.Models;
using ECom.Models.ViewModels;
using ECom.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Ecom.Utility;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ECom_App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.ADMIN)]
    public class UserController : Controller
    {
        //private readonly IUnitOfWork _service;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(ApplicationDbContext dbContext,UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult RoleManagement(string userId)
        {
            var roleId = _dbContext.UserRoles.FirstOrDefault(x => x.UserId == userId).RoleId;
            var RoleManagementVM = new RoleManagementVM
            {
                ApplicationUser = _dbContext.ApplicationUser.Include(x => x.company).FirstOrDefault(x => x.Id == userId),
                RoleList = _dbContext.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _dbContext.Companies.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.CompanyId.ToString()
                })

            };
            RoleManagementVM.ApplicationUser.Role = _dbContext.Roles.FirstOrDefault(x => x.Id == roleId).Name;

            return View(RoleManagementVM);
        }
        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagement)
        {
            var roleId = _dbContext.UserRoles.FirstOrDefault(x => x.UserId == roleManagement.ApplicationUser.Id).RoleId;
            var oldRole = _dbContext.Roles.FirstOrDefault(x => x.Id == roleId).Name;
            if (!(roleManagement.ApplicationUser.Role==oldRole))
            {
                var applicationUser = _dbContext.ApplicationUser.FirstOrDefault(x => x.Id == roleManagement.ApplicationUser.Id);
                if (roleManagement.ApplicationUser.Role != Roles.COMPANY)
                {
                    applicationUser.CompanyId=roleManagement.ApplicationUser.CompanyId;
                }
                if (oldRole==Roles.COMPANY)
                { 
                    applicationUser.CompanyId = null;
                }
                _dbContext.SaveChanges();
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagement.ApplicationUser.Role).GetAwaiter().GetResult();
            
            }
            return RedirectToAction(nameof(Index));
        }

        #region APISCALL
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var users = _dbContext.ApplicationUser.Include(x => x.company).ToList();
                var usersRoles = _dbContext.UserRoles.ToList();
                var roles = _dbContext.Roles.ToList();
                foreach (var user in users)
                {
                    if (user.company == null)
                    {
                        var roleId = usersRoles.FirstOrDefault(x => x.UserId == user.Id).RoleId;
                        user.Role = roles.FirstOrDefault(x => x.Id == roleId).Name;
                        user.company = new Company
                        {
                            Name = ""
                        };
                    }
                }

                return Json(new { data = users });
            }
            catch (Exception)
            {
                return Json(new { sucess = false, message = "Error while Get all Company" });
            }
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string? Id)
        {
            try
            {
                var dbUser = _dbContext.ApplicationUser.FirstOrDefault(x => x.Id == Id);
                if (dbUser.LockoutEnd != null && dbUser.LockoutEnd > DateTime.Now)
                {
                    dbUser.LockoutEnd = DateTime.Now;
                }
                else
                {
                    dbUser.LockoutEnd = DateTime.Now.AddMonths(6);
                }
                _dbContext.SaveChanges();
                return Json(new { sucess = false, message = "User Lock Sucessfully " });
            }
            catch (Exception)
            {
                return Json(new { sucess = false, message = "Error while deleting Company" });
            }
        }
        #endregion

    }
}
