using DBSportStore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DBSportStore.Controllers
{
    // Gắn trên class:
    // mọi action trong AdminController
    // đều phải qua kiểm tra AdminAuthorize.
    [AdminAuthorize]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }

}