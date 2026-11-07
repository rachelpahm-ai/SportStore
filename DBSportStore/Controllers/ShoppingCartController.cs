
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class ShoppingCartController : Controller
    {
        // Kết nối CSDL để đọc Product.
        private DBSportStoreEntities db =
            new DBSportStoreEntities();

        // Lấy Cart hiện tại từ Session.
        // Nếu chưa có thì tạo Cart mới.
        private Cart GetCart()
        {
            var cart = Session["Cart"] as Cart;

            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }

            return cart;
        }

        // Hiển thị giỏ hàng.
        public ActionResult ShowCart()
        {
            return View(GetCart());
        }

        // Nhận ProductID từ nút Mua.
        public ActionResult AddToCart(int id)
        {
            // Chỉ thêm Product đang tồn tại và đang bán.
            var product = db.Product
                .FirstOrDefault(
                    p => p.ProductID == id
                    && p.IsActive);

            if (product == null)
            {
                TempData["CartError"] =
                    "Sản phẩm không tồn tại hoặc đã ngừng bán.";

                return RedirectToAction("Index", "Store");
            }

            // Cart.Add tự kiểm tra tồn kho.
            bool added = GetCart().Add(product);

            if (!added)
            {
                TempData["CartError"] =
                    "Không thể thêm sản phẩm. "
                    + "Số lượng đã đạt giới hạn tồn kho.";
            }

            // Chuyển sang trang giỏ hàng.
            return RedirectToAction("ShowCart");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        // GET: ShoppingCart
        public ActionResult Index()
        {
            return View();
        }

        // ==================== CẬP NHẬT SỐ LƯỢNG SẢN PHẨM ====================

        // Action này chỉ nhận request theo phương thức POST.
        // Dùng POST vì thao tác này làm thay đổi dữ liệu trong giỏ hàng.
        [HttpPost]

        // Kiểm tra Anti-Forgery Token gửi từ View.
        // Giúp hạn chế request giả mạo từ bên ngoài.
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(
            int productId,   // Mã sản phẩm cần cập nhật
            int quantity)    // Số lượng mới người dùng nhập
        {
            // Đọc lại Product từ CSDL theo ProductID.
            // Mục đích: lấy StockQuantity mới nhất trong database,
            // không chỉ dựa vào số tồn kho đã lưu trong Session.
            var product = db.Product.Find(productId);


            // Nếu Product không còn tồn tại trong CSDL
            // thì không thể cập nhật giỏ hàng.
            if (product == null)
            {
                // TempData dùng để gửi thông báo sang request kế tiếp.
                TempData["CartError"] =
                    "Không tìm thấy sản phẩm.";

                // Quay lại trang Giỏ hàng.
                return RedirectToAction("ShowCart");
            }


            // Lấy Cart hiện tại từ Session rồi gọi hàm Update().
            // Update() sẽ kiểm tra:
            // - quantity phải > 0
            // - quantity không được vượt quá tồn kho hiện tại.
            bool updated = GetCart().Update(
                productId,
                quantity,
                product.StockQuantity);


            // Nếu Update() trả về false
            // nghĩa là số lượng người dùng nhập không hợp lệ.
            if (!updated)
            {
                TempData["CartError"] =
                    "Số lượng phải lớn hơn 0 "
                    + "và không vượt tồn kho.";
            }


            // Sau khi cập nhật xong,
            // quay lại trang Giỏ hàng để hiển thị kết quả mới.
            return RedirectToAction("ShowCart");
        }



        // ==================== XÓA SẢN PHẨM KHỎI GIỎ ====================

        // Dùng POST vì thao tác xóa làm thay đổi dữ liệu Cart.
        [HttpPost]

        // Kiểm tra Anti-Forgery Token từ form trong View.
        [ValidateAntiForgeryToken]
        public ActionResult Remove(
            int productId)   // Mã Product cần xóa
        {
            // Lấy Cart hiện tại từ Session
            // và xóa CartItem có ProductID tương ứng.
            GetCart().Remove(productId);

            // Quay lại trang Giỏ hàng sau khi xóa.
            return RedirectToAction("ShowCart");
        }
    }
}
