
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

namespace ECom_App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =Roles.ADMIN)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _service;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CompanyController(IUnitOfWork service, IWebHostEnvironment webHostEnvironment)
        {
            _service = service;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        public IActionResult Upsert(int? companyId)
        {
            try
            {
                Company company=new Company();
                if (companyId != null)
                {
                    company = _service.Company.FirstOrDefault(x => x.CompanyId == companyId);
                }
                return View(company);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company Company)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string rootPath = _webHostEnvironment.WebRootPath;
                    if (Company.CompanyId == 0)
                    {
                        _service.Company.Add(Company);
                        TempData["msg"] = "Company " + Alerts.SUCCESS;
                    }
                    else
                    {
                        _service.Company.Update(Company);
                        TempData["msg"] = "Company " + Alerts.UPDATE;

                    }
                    _service.Save();
                    return RedirectToAction("Index");
                }
                else
                {

                    TempData["msgError"] = "Company " + Alerts.FAILSUCCESS;

                }
                return View(Company);
            }
            catch (Exception)
            {
                return View("Error");
            }

        }

        #region APISCALL
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var companies = _service.Company.GetAll().ToList();

                return Json(new { data = companies});
            }
            catch (Exception)
            {
                return Json(new { sucess = false, message = "Error while Get all Company" });
            }
        }
        [HttpDelete]
        public IActionResult DeleteCompany(int? Id)
        {
            try
            {
               var CompanyToBeDeleted=_service.Company.FirstOrDefault(u=>u.CompanyId == Id);
                if(CompanyToBeDeleted == null)
                {
                    return Json(new {sucess=false,message="Error while deleting Company"});
                }
                _service.Company.Remove(CompanyToBeDeleted); 
                _service.Save();
                return Json(new { sucess = true, message = "Company " + Alerts.DELETE });

            }
            catch (Exception)
            {
                return Json(new { sucess = false, message = "Error while deleting Company" });
            }
        }
        #endregion

    }
}
