
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
                // Quy định số sản phẩm mỗi trang.
                const int pageSize = 6;

                // Không cho page nhỏ hơn 1.
                if (page < 1)
                {
                    page = 1;
                }

                var products = db.Product
                    .Where(p => p.IsActive);

                // Lọc theo Category.
                if (categoryId.HasValue)
                {
                    products = products.Where(
                        p => p.CategoryID == categoryId.Value);
                }

                // Tìm kiếm.
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.Trim();

                    products = products.Where(
                        p => p.ProductName.Contains(keyword)
                          || p.Description.Contains(keyword));
                }

                // Đếm số Product sau lọc/tìm kiếm.
                int totalItems = products.Count();

                // Tính số trang cần có.
                int totalPages =
                    (int)Math.Ceiling((double)totalItems / pageSize);

                // Khi không có kết quả vẫn để trang hiện tại = 1.
                if (totalPages == 0)
                {
                    totalPages = 1;
                }

                // Nếu URL truyền page quá lớn thì đưa về trang cuối.
                if (page > totalPages)
                {
                    page = totalPages;
                }

                var model = new StoreHomeViewModel
                {
                    Categories = db.Category
                        .OrderBy(c => c.CategoryName)
                        .ToList(),

                    Products = products
                        // Phải sắp xếp trước khi Skip/Take.
                        .OrderByDescending(p => p.ProductID)

                        // Bỏ qua sản phẩm của các trang trước.
                        .Skip((page - 1) * pageSize)

                        // Chỉ lấy đúng số sản phẩm của trang hiện tại.
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
