
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;
using System.Data.Entity;

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


        // B07 - Bài tập 1: Xóa toàn bộ giỏ hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearCart()
        {
            // Lấy giỏ hàng từ Session và xóa tất cả sản phẩm
            GetCart().Clear();

            // Quay về trang giỏ hàng
            return RedirectToAction("ShowCart");
        }


        // ==================== HIỂN THỊ THÔNG TIN GIỎ TRÊN NAVBAR ====================

        // Action này chỉ được gọi từ View/Layout,
        // không cho người dùng truy cập trực tiếp bằng URL.
        [ChildActionOnly]
        public PartialViewResult BagCart()
        {
            // Lấy Cart hiện tại từ Session.
            // Nếu chưa có Cart thì biến cart sẽ bằng null.
            var cart = Session["Cart"] as Cart;


            // Nếu chưa có Cart -> số lượng = 0.
            // Nếu có Cart -> tính tổng số lượng sản phẩm.
            ViewBag.CartQty =
                cart == null ? 0 : cart.TotalQuantity();


            // Nếu chưa có Cart -> tổng tiền = 0.
            // Nếu có Cart -> tính tổng tiền của giỏ.
            ViewBag.CartTotal =
                cart == null ? 0 : cart.TotalMoney();


            // Gửi dữ liệu sang Partial View BagCart.cshtml.
            return PartialView();
        }
        // ==================== MỞ FORM CHECKOUT ====================

        // GET: hiển thị trang Checkout cho người dùng nhập thông tin.
        // =====================================================
        // MỞ FORM CHECKOUT
        // =====================================================

        public ActionResult Checkout()
        {
            // Lấy Cart hiện tại từ Session.
            var cart = GetCart();


            // Không cho Checkout nếu giỏ trống.
            if (!cart.Items.Any())
            {
                return RedirectToAction("ShowCart");
            }


            // Tạo ViewModel cho trang Checkout.
            var model = new CheckoutViewModel
            {
                Cart = cart
            };


            // =================================================
            // NẾU CUSTOMER ĐÃ LOGIN -> ĐIỀN SẴN HỒ SƠ
            // =================================================

            // CustomerID chỉ có khi Login bằng tài khoản
            // đã gắn với một Customer.
            if (Session["CustomerID"] != null)
            {
                // Session lưu object nên ép về int.
                int customerId =
                    (int)Session["CustomerID"];


                var customer =
                    db.Customer.Find(customerId);


                if (customer != null)
                {
                    model.FullName =
                        customer.FullName;

                    model.Phone =
                        customer.Phone;

                    model.Email =
                        customer.Email;

                    model.Address =
                        customer.Address;


                    // Mặc định địa chỉ giao hàng
                    // bằng địa chỉ hồ sơ Customer.
                    model.ShippingAddress =
                        customer.Address;
                }
            }


            return View(model);
        }


        // ==================== NHẬN DỮ LIỆU CHECKOUT VÀ LƯU ĐƠN HÀNG ====================

        // Action này nhận dữ liệu từ form Checkout bằng phương thức POST.
        [HttpPost]

        // Kiểm tra Anti-Forgery Token gửi từ View.
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(
            CheckoutViewModel model)
        {
            // Lấy Cart hiện tại đang lưu trong Session.
            var cart = GetCart();

            // Cart không được gửi từ form lên,
            // nên phải gắn lại vào ViewModel để View vẫn hiển thị tóm tắt đơn hàng.
            model.Cart = cart;


            // ==================== KIỂM TRA GIỎ HÀNG ====================

            // Nếu giỏ không còn sản phẩm thì không cho đặt hàng.
            // Any() kiểm tra danh sách có ít nhất một phần tử hay không.
            if (!cart.Items.Any())
            {
                return RedirectToAction("ShowCart");
            }


            // ==================== KIỂM TRA DỮ LIỆU KHÁCH HÀNG ====================

            // ModelState.IsValid = true khi dữ liệu người dùng nhập
            // thỏa các Validation trong CheckoutViewModel.
            if (!ModelState.IsValid)
            {
                // Nếu dữ liệu chưa hợp lệ thì trả lại form
                // để hiển thị thông báo lỗi.
                return View(model);
            }


            // ==================== KIỂM TRA LẠI TỒN KHO ====================

            // Phải kiểm tra lại ngay trước khi tạo đơn
            // vì tồn kho có thể đã thay đổi sau lúc Product được thêm vào Cart.
            foreach (var item in cart.Items)
            {
                // Đọc Product mới nhất từ CSDL theo ProductID.
                var product = db.Product.Find(item.ProductID);

                // Không cho đặt nếu:
                // - Product không còn tồn tại
                // - Product đã ngừng bán
                // - tồn kho hiện tại nhỏ hơn số lượng khách đặt
                if (product == null
                    || !product.IsActive
                    || product.StockQuantity < item.Quantity)
                {
                    // Thêm lỗi chung vào ModelState để hiển thị trên Checkout View.
                    ModelState.AddModelError(
                        "",
                        "Sản phẩm " + item.ProductName
                        + " không còn đủ tồn kho.");

                    return View(model);
                }
            }


            // ==================== BẮT ĐẦU TRANSACTION ====================

            // Transaction bảo đảm các thao tác:
            // Customer + OrderPro + OrderDetail + cập nhật tồn kho
            // phải cùng thành công.
            // Nếu một bước lỗi thì toàn bộ thay đổi sẽ rollback.
            using (var transaction =
                db.Database.BeginTransaction())
            {
                try
                {
                    // =====================================================
                    // 1. TẠO CUSTOMER MỚI CHO KHÁCH VÃNG LAI
                    // =====================================================

                    // B07 chưa có Login.
                    // Mỗi lần Guest Checkout sẽ tạo một Customer mới.
                    var customer = new Customer
                    {
                        // UserID = null nghĩa là Customer chưa có tài khoản.
                        UserID = null,

                        FullName = model.FullName,
                        Phone = model.Phone,
                        Email = model.Email,
                        Address = model.Address
                    };

                    // Đánh dấu Customer mới cần được thêm vào CSDL.
                    db.Customer.Add(customer);

                    // Ghi Customer xuống CSDL.
                    db.SaveChanges();

                    // Sau SaveChanges(), SQL Server đã sinh CustomerID
                    // và EF tự cập nhật giá trị vào customer.CustomerID.


                    // =====================================================
                    // 2. TẠO ORDERPRO
                    // =====================================================

                    var order = new OrderPro
                    {
                        // Ghi nhận thời điểm đặt hàng.
                        OrderDate = System.DateTime.Now,

                        // Gắn đơn hàng với Customer vừa được tạo.
                        CustomerID = customer.CustomerID,

                        // Lưu snapshot thông tin giao hàng trên chính đơn hàng.
                        // Sau này Customer thay đổi thông tin,
                        // đơn cũ vẫn giữ đúng dữ liệu lúc đặt.
                        RecipientName = model.FullName,
                        ShippingPhone = model.Phone,
                        ShippingAddress = model.ShippingAddress,

                        // Tổng tiền lấy từ Cart hiện tại.
                        TotalAmount = cart.TotalMoney(),

                        // Trạng thái ban đầu của đơn.
                        Status = "Chờ xử lý"
                    };

                    // Đánh dấu OrderPro mới cần thêm vào CSDL.
                    db.OrderPro.Add(order);

                    // Ghi OrderPro xuống CSDL.
                    db.SaveChanges();

                    // Sau SaveChanges(), SQL Server đã sinh OrderID
                    // và EF cập nhật vào order.OrderID.


                    // =====================================================
                    // 3. TẠO CÁC ORDERDETAIL
                    // =====================================================

                    // Mỗi CartItem sẽ tạo thành một dòng OrderDetail.
                    foreach (var item in cart.Items)
                    {
                        // Đọc lại Product để cập nhật tồn kho.
                        var product = db.Product.Find(
                            item.ProductID);


                        // Tạo chi tiết đơn hàng cho Product hiện tại.
                        var detail = new OrderDetail
                        {
                            // Gắn chi tiết vào OrderPro vừa tạo.
                            OrderID = order.OrderID,

                            ProductID = item.ProductID,

                            // Số lượng khách đặt.
                            Quantity = item.Quantity,

                            // Lưu đơn giá tại thời điểm đặt hàng.
                            // Không lấy lại Product.Price sau này.
                            UnitPrice = item.UnitPrice
                        };


                        // Thêm OrderDetail vào CSDL.
                        db.OrderDetail.Add(detail);


                        // Trừ tồn kho theo số lượng vừa đặt.
                        product.StockQuantity -= item.Quantity;
                    }


                    // Ghi toàn bộ OrderDetail và tồn kho mới xuống CSDL.
                    db.SaveChanges();


                    // ==================== COMMIT TRANSACTION ====================

                    // Tất cả thao tác phía trên đều thành công
                    // thì xác nhận lưu toàn bộ thay đổi.
                    transaction.Commit();


                    // ==================== XÓA GIỎ HÀNG ====================

                    // Chỉ xóa Cart SAU KHI transaction đã commit thành công.
                    // Nếu lưu đơn bị lỗi thì giỏ vẫn còn để khách thử lại.
                    cart.Clear();


                    // ==================== CHUYỂN SANG TRANG THÀNH CÔNG ====================

                    // Chuyển sang OrderSuccess và gửi OrderID vừa tạo.
                    return RedirectToAction(
                        "OrderSuccess",
                        new { id = order.OrderID });
                }
                catch
                {
                    // ==================== XỬ LÝ KHI CÓ LỖI ====================

                    // Nếu có lỗi trong quá trình lưu,
                    // hủy toàn bộ thay đổi của transaction.
                    transaction.Rollback();


                    // Thêm thông báo lỗi để hiển thị lại trên Checkout View.
                    ModelState.AddModelError(
                        "",
                        "Có lỗi khi lưu đơn hàng. "
                        + "Vui lòng thử lại.");

                    // Giữ nguyên Cart và dữ liệu người dùng đã nhập.
                    return View(model);
                }
            }
        }
        // ==================== HIỂN THỊ TRANG ĐẶT HÀNG THÀNH CÔNG ====================

        // Nhận OrderID từ URL sau khi Checkout thành công.
        // ==================== HIỂN THỊ TRANG ĐẶT HÀNG THÀNH CÔNG ====================

        // Nhận OrderID từ URL sau khi Checkout thành công.     

        public ActionResult OrderSuccess(int id)
        {
            // Lấy OrderPro theo OrderID.
            // Include(Customer) để lấy thêm thông tin khách hàng.
            // Include(OrderDetail.Product) để lấy:
            // - danh sách chi tiết đơn hàng
            // - thông tin Product của từng dòng chi tiết.
            var order = db.OrderPro
                .Include(o => o.Customer)

                // Với mỗi OrderDetail, lấy thêm Product tương ứng
                // để View có thể hiển thị tên và ảnh sản phẩm.
                .Include(o => o.OrderDetail.Select(
                    d => d.Product))

                // Chỉ lấy đúng đơn hàng có OrderID bằng id truyền vào.
                // FirstOrDefault() trả về null nếu không tìm thấy.
                .FirstOrDefault(o => o.OrderID == id);


            // Nếu không tìm thấy đơn hàng thì trả về lỗi 404.
            if (order == null)
            {
                return HttpNotFound();
            }


            // B07 - Bài tập 3: Tính tổng số lượng sản phẩm
            ViewBag.TotalQuantity = db.OrderDetail
                .Where(d => d.OrderID == id)
                .Sum(d => (int?)d.Quantity) ?? 0;

            // Gửi OrderPro cùng Customer, OrderDetail và Product
            // sang View OrderSuccess.cshtml để hiển thị đầy đủ đơn hàng.
            return View(order);
        }


    }
}
