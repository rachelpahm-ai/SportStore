using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        // Kết nối CSDL DBSportStore bằng EF Database First.
        private DBSportStoreEntities db =
            new DBSportStoreEntities();


        // =====================================================
        // MỞ FORM SIGN UP
        // =====================================================

        // GET: chạy khi người dùng mở /Account/SignUp.
        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }


        // =====================================================
        // NHẬN DỮ LIỆU SIGN UP - BẢN TEST VALIDATION
        // =====================================================

        // POST: chạy khi người dùng bấm nút Đăng ký.
        [HttpPost]

        // Kiểm tra Anti-Forgery Token gửi từ form.
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(
            SignUpViewModel model)
        {
            // ModelState.IsValid = true khi dữ liệu
            // thỏa tất cả DataAnnotations trong ViewModel.
            if (!ModelState.IsValid)
            {
                // Dữ liệu sai -> trả lại View
                // để hiển thị thông báo Validation.
                return View(model);
            }


            // Chức năng 1 chỉ test Validation.
            // Chức năng 2 mới Hash + Salt và lưu CSDL.
            ViewBag.SignUpTest =
                "Thông tin đăng ký hợp lệ.";

            return View(model);
        }


        // Giải phóng DbContext khi Controller kết thúc.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

    }
}