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


        public ActionResult Index(
            int? categoryId,
            string keyword)
        {
            // Bắt đầu từ sản phẩm đang bán.
            var products = db.Product
                .Where(p => p.IsActive);

            // Lọc theo danh mục nếu có categoryId.
            if (categoryId.HasValue)
            {
                products = products.Where(
                    p => p.CategoryID == categoryId.Value);
            }

            // Tìm kiếm nếu người dùng có nhập từ khóa.
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // Bỏ khoảng trắng thừa ở đầu/cuối.
                keyword = keyword.Trim();

                // Chỉ giữ Product có tên hoặc mô tả chứa từ khóa.
                products = products.Where(
                    p => p.ProductName.Contains(keyword)
                      || p.Description.Contains(keyword));
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

                SelectedCategoryId = categoryId,
                SearchTerm = keyword
            };

            return View(model);
        }
        public ActionResult AboutStore()
        {
            return View();
        }
    }
}