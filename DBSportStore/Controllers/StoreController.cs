
using DBSportStore.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DBSportStore.Controllers
{
    public class StoreController : Controller
    {
        // Kết nối database
        private DBSportStoreEntities db = new DBSportStoreEntities();

        // GET: Store
        // Lọc danh mục + Tìm kiếm + Phân trang
        public ActionResult Index(
            int? categoryId,
            string keyword,
            int page = 1)
        {
            // Quy định số sản phẩm mỗi trang
            const int pageSize = 6;

            // Không cho page nhỏ hơn 1
            if (page < 1)
            {
                page = 1;
            }

            // Lấy các sản phẩm đang bán
            var products = db.Product
                .Where(p => p.IsActive);

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                products = products.Where(
                    p => p.CategoryID == categoryId.Value);
            }

            // Tìm kiếm theo tên hoặc mô tả sản phẩm
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                products = products.Where(
                    p => p.ProductName.Contains(keyword)
                      || p.Description.Contains(keyword));
            }

            // Đếm sản phẩm sau khi lọc và tìm kiếm
            int totalItems = products.Count();

            // Tính tổng số trang
            int totalPages =
                (int)Math.Ceiling((double)totalItems / pageSize);

            // Nếu không có sản phẩm vẫn giữ trang hiện tại = 1
            if (totalPages == 0)
            {
                totalPages = 1;
            }

            // Không cho page vượt quá tổng số trang
            if (page > totalPages)
            {
                page = totalPages;
            }

            // Tạo ViewModel truyền dữ liệu sang View
            var model = new StoreHomeViewModel
            {
                Categories = db.Category
                    .OrderBy(c => c.CategoryName)
                    .ToList(),

                Products = products
                    .OrderByDescending(p => p.ProductID)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
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
                SearchTerm = keyword,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(model);
        }

        public ActionResult AboutStore()
        {
            return View();
        }
    }
}
