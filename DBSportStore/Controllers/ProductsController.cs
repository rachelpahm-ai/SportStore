using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;
// Thư viện xử lý đường dẫn, tên file và phần mở rộng file
using System.IO;

// Thư viện Web, dùng HttpPostedFileBase để nhận file upload
using System.Web;

namespace DBSportStore.Controllers
{
    public class ProductsController : Controller
    {
        // Hàm dùng chung để lưu file ảnh vào thư mục Content/images
        // Giữ tên file gốc để dễ đối chiếu với dữ liệu trong CSDL.
        // Nếu đã có file trùng tên, tự thêm -1, -2, ... để tránh ghi đè.
        private string SaveImage(HttpPostedFileBase imageFile)
        {
            // Chỉ lấy tên file, không lấy đường dẫn thư mục trên máy người dùng.
            // Ví dụ file được chọn ở D:\...\gym-01.jpg thì chỉ lấy gym-01.jpg.
            string originalFileName = Path.GetFileName(imageFile.FileName);

            // Tách tên file và phần mở rộng.
            // gym-01.jpg -> fileNameWithoutExtension = gym-01; extension = .jpg
            string fileNameWithoutExtension =
                Path.GetFileNameWithoutExtension(originalFileName);

            string extension =
                Path.GetExtension(originalFileName).ToLower();

            // Thư mục vật lý dùng để lưu ảnh trong project.
            string folder = Server.MapPath("~/Content/images");

            // Nếu thư mục images chưa tồn tại thì tạo mới.
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // Ban đầu giữ nguyên tên file gốc.
            string fileName =
                fileNameWithoutExtension + extension;

            string path =
                Path.Combine(folder, fileName);

            // Nếu Content/images đã có file cùng tên,
            // tự thêm -1, -2, ... để không ghi đè file cũ.
            int count = 1;

            while (System.IO.File.Exists(path))
            {
                fileName =
                    fileNameWithoutExtension
                    + "-"
                    + count
                    + extension;

                path =
                    Path.Combine(folder, fileName);

                count++;
            }

            // Copy file được upload vào Content/images của project.
            imageFile.SaveAs(path);

            // Database chỉ lưu tên file, không lưu đường dẫn đầy đủ.
            return fileName;
        }
        private DBSportStoreEntities db = new DBSportStoreEntities();

        // GET: Products
        public ActionResult Index()
        {
            var product = db.Product.Include(p => p.Category);
            return View(product.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        // Action Create nhận dữ liệu khi form gửi bằng POST
        [HttpPost]
        // Kiểm tra Anti-Forgery Token
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            // Chỉ nhận các thuộc tính được phép từ form
            [Bind(Include = "ProductName,Description,CategoryID,Price,StockQuantity,IsActive")]
Product product,

            // Nhận file từ <input type="file" name="imageFile">
            HttpPostedFileBase imageFile)
        {
            // Chỉ xử lý khi người dùng có chọn file
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                // Lấy phần mở rộng để kiểm tra loại file
                string extension = Path.GetExtension(imageFile.FileName).ToLower();

                // Các định dạng ảnh được phép
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

                // Kiểm tra định dạng file
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "ImageUrl",
                        "Chỉ chấp nhận file JPG, JPEG hoặc PNG.");
                }
                // Giới hạn dung lượng tối đa 2 MB
                else if (imageFile.ContentLength > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "ImageUrl",
                        "Dung lượng ảnh tối đa là 2 MB.");
                }
                else
                {
                    // Lưu ảnh và gán tên file vào Product.ImageUrl
                    product.ImageUrl = SaveImage(imageFile);
                }
            }

            // Chỉ lưu dữ liệu khi Model hợp lệ
            if (ModelState.IsValid)
            {
                // Thêm sản phẩm mới vào DbSet Products
                db.Product.Add(product);

                // Ghi thay đổi xuống database
                db.SaveChanges();

                // Quay về trang danh sách sản phẩm
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu lỗi, tạo lại danh sách Category cho DropDownList
            ViewBag.CategoryID = new SelectList(
                db.Category,
                "CategoryID",
                "CategoryName",
                product.CategoryID);

            // Trả lại View Create cùng dữ liệu đã nhập
            return View(product);
        }
        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,Description,CategoryID,Price,ImageUrl,StockQuantity,IsActive")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Product.Find(id);
            db.Product.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

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
