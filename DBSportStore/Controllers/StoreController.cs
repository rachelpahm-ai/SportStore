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


        public ActionResult Index(int? categoryId)
        {
            // Bắt đầu từ toàn bộ sản phẩm đang bán.
            var products = db.Product
                .Where(p => p.IsActive);

            // Nếu có categoryId, chỉ giữ các Product thuộc Category đó.
            if (categoryId.HasValue)
            {
                products = products.Where(
                    p => p.CategoryID == categoryId.Value);
            }

            var model = new StoreHomeViewModel
            {
                Categories = db.Category
                    .OrderBy(c => c.CategoryName)
                    .ToList(),

                Products = products
                    .Select(p => new ProductCardViewModel
                    {
                        ProductID = p.ProductID,
                        NamePro = p.ProductName,
                        CategoryName = p.Category.CategoryName,
                        Price = p.Price,
                        ImagePro = p.ImageUrl
                    })
                    .ToList(),

                // Ghi nhớ danh mục đang lọc.
                SelectedCategoryId = categoryId
            };

            return View(model);
        }
        public ActionResult AboutStore()
        {
            return View();
        }
    }
}