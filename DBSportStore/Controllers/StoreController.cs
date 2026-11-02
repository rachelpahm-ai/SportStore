// Dùng ProductCardViewModel trong thư mục Models
using DBSportStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


// Namespace chứa các Controller
namespace DBSportStore.Controllers
{
    // Controller xử lý các request của trang Store
    public class StoreController : Controller
    {
        // tạo đối tượng kết nối database
        private DBSportStoreEntities db = new DBSportStoreEntities();

        // GET: Store
        // Action Index xử lý URL /Store/Index


        public ActionResult Index()
        {
            var model = new StoreHomeViewModel
            {
                // Lấy toàn bộ Category từ CSDL và sắp xếp theo tên.
                Categories = db.Category
            .OrderBy(c => c.CategoryName)
            .ToList(),

                // Giữ nguyên cách lấy sản phẩm đang bán của B05.
                Products = db.Product
            .Where(p => p.IsActive)
            .Select(p => new ProductCardViewModel
            {
                ProductID = p.ProductID,
                NamePro = p.ProductName,
                CategoryName = p.Category.CategoryName,
                Price = p.Price,
                ImagePro = p.ImageUrl
            })
            .ToList()
            };

            // Gửi StoreHomeViewModel thay cho một List<ProductCardViewModel>.
            return View(model);

        }
        public ActionResult AboutStore()
        {
            return View();
        }
    }
}