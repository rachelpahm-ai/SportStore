using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;
using System.Data.Entity;

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
        // =====================================================
        // SIGN UP - TẠO USERACCOUNT VÀ CUSTOMER
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(
            SignUpViewModel model)
        {
            // Dữ liệu form phải hợp lệ trước khi xử lý CSDL.
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =================================================
            // 1. KIỂM TRA USERNAME
            // =================================================

            // Any() trả true nếu đã có UserAccount
            // sử dụng đúng UserName người dùng đang nhập.
            bool userNameExists =
                db.UserAccount.Any(
                    u => u.UserName == model.UserName);

            if (userNameExists)
            {
                // Gắn lỗi trực tiếp vào ô UserName.
                ModelState.AddModelError(
                    "UserName",
                    "Tên đăng nhập đã tồn tại.");

                return View(model);
            }


            // =================================================
            // 2. LẤY ROLE CUSTOMER
            // =================================================

            // Tài khoản đăng ký công khai luôn là Customer.
            // Người dùng KHÔNG được chọn Role từ form.
            var customerRole =
                db.Role.FirstOrDefault(
                    r => r.RoleName == "Customer");

            if (customerRole == null)
            {
                ModelState.AddModelError(
                    "",
                    "Không tìm thấy Role Customer.");

                return View(model);
            }


            // =================================================
            // 3. TẠO SALT + PASSWORD HASH
            // =================================================

            // Mỗi tài khoản tạo một Salt mới.
            string salt =
                PasswordHelper.CreateSalt();

            // PasswordHash được tạo từ Password + Salt.
            string passwordHash =
                PasswordHelper.Hash(
                    model.Password,
                    salt);


            // =================================================
            // 4. BẮT ĐẦU TRANSACTION
            // =================================================

            // UserAccount và Customer phải cùng được tạo thành công.
            // Nếu một bước lỗi -> rollback toàn bộ.
            using (var transaction =
                db.Database.BeginTransaction())
            {
                try
                {
                    // =============================================
                    // 4.1. TẠO USERACCOUNT
                    // =============================================

                    var account = new UserAccount
                    {
                        UserName = model.UserName,

                        // Không lưu Password dạng rõ.
                        PasswordHash = passwordHash,
                        PasswordSalt = salt,

                        // Gán Role Customer lấy từ CSDL.
                        RoleID = customerRole.RoleID,

                        IsActive = true,

                        // Ghi thời điểm tạo tài khoản.
                        CreatedAt = DateTime.Now
                    };


                    // Đánh dấu entity cần INSERT.
                    db.UserAccount.Add(account);

                    // Ghi UserAccount xuống CSDL.
                    db.SaveChanges();

                    // Sau SaveChanges(), SQL Server sinh UserID.
                    // EF tự cập nhật giá trị vào account.UserID.


                    // =============================================
                    // 4.2. TẠO CUSTOMER GẮN VỚI USERID VỪA SINH
                    // =============================================

                    var customer = new Customer
                    {
                        // Đây là UserID do SQL Server sinh.
                        // KHÔNG lấy UserID từ dropdown hoặc từ form.
                        UserID = account.UserID,

                        FullName = model.FullName,
                        Phone = model.Phone,
                        Email = model.Email,
                        Address = model.Address
                    };


                    db.Customer.Add(customer);
                    db.SaveChanges();


                    // Cả UserAccount và Customer đều thành công.
                    transaction.Commit();


                    TempData["SignUpSuccess"] =
                        "Đăng ký thành công. "
                        + "Vui lòng đăng nhập.";


                    // Chức năng 3 sẽ tạo trang Login hoàn chỉnh.
                    return RedirectToAction("Login");
                }
                catch
                {
                    // Có lỗi -> hủy toàn bộ thay đổi
                    // trong transaction.
                    transaction.Rollback();

                    ModelState.AddModelError(
                        "",
                        "Có lỗi khi tạo tài khoản. "
                        + "Vui lòng thử lại.");

                    return View(model);
                }
            }
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

        // GET: mở trang Login.
        // Chức năng 3 sẽ bổ sung Login POST.
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // =====================================================
        // LOGIN
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(
            LoginViewModel model)
        {
            // Validation form phải hợp lệ.
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =================================================
            // 1. TÌM USERACCOUNT
            // =================================================

            // Include(Role) để lấy thêm RoleName
            // của tài khoản trong cùng truy vấn.
            var account = db.UserAccount
                .Include(u => u.Role)
                .FirstOrDefault(
                    u => u.UserName == model.UserName
                      && u.IsActive);


            // Không tìm thấy User hoặc tài khoản bị khóa.
            if (account == null)
            {
                ModelState.AddModelError(
                    "",
                    "Tên đăng nhập hoặc mật khẩu không đúng.");

                return View(model);
            }


            // =================================================
            // 2. KIỂM TRA PASSWORD
            // =================================================

            // Verify():
            // - lấy Password người dùng vừa nhập
            // - Hash lại bằng PasswordSalt trong CSDL
            // - so sánh với PasswordHash đã lưu.
            bool passwordCorrect =
                PasswordHelper.Verify(
                    model.Password,
                    account.PasswordSalt,
                    account.PasswordHash);

            if (!passwordCorrect)
            {
                ModelState.AddModelError(
                    "",
                    "Tên đăng nhập hoặc mật khẩu không đúng.");

                return View(model);
            }


            // =================================================
            // 3. LƯU TRẠNG THÁI LOGIN VÀO SESSION
            // =================================================

            // Session thuộc riêng phiên làm việc hiện tại.
            // Các trang khác có thể đọc lại những giá trị này.
            Session["UserID"] =
                account.UserID;

            Session["UserName"] =
                account.UserName;

            // RoleName có giá trị Admin hoặc Customer.
            Session["Role"] =
                account.Role.RoleName;


            // =================================================
            // 4. NẾU LÀ CUSTOMER -> LẤY HỒ SƠ CUSTOMER
            // =================================================

            var customer = db.Customer
                .FirstOrDefault(
                    c => c.UserID == account.UserID);

            if (customer != null)
            {
                Session["CustomerID"] =
                    customer.CustomerID;

                Session["FullName"] =
                    customer.FullName;
            }
            else
            {
                // Admin có thể không có Customer.
                Session.Remove("CustomerID");
                Session.Remove("FullName");
            }


            // =================================================
            // 5. CHUYỂN TRANG SAU LOGIN
            // =================================================

            // Admin vào trang quản trị.
            if (account.Role.RoleName == "Admin")
            {
                return RedirectToAction(
                    "Index",
                    "Admin");
            }


            // Customer quay về Trang Chủ.
            return RedirectToAction(
                "Index",
                "Store");
        }

        // =====================================================
        // LOGOUT
        // =====================================================

        // Dùng POST vì Logout làm thay đổi trạng thái phiên đăng nhập.
        [HttpPost]

        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Chỉ xóa các Session liên quan tài khoản.
            Session.Remove("UserID");
            Session.Remove("UserName");
            Session.Remove("Role");
            Session.Remove("CustomerID");
            Session.Remove("FullName");


            // KHÔNG gọi Session.Clear().
            // Session.Clear() sẽ xóa luôn Session["Cart"] của B07.


            return RedirectToAction(
                "Index",
                "Store");
        }

    }
}