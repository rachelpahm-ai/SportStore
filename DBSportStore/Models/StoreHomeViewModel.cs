using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBSportStore.Models
{
    public class StoreHomeViewModel
    {
        // Danh sách danh mục lấy trực tiếp từ bảng Category.
        public List<Category> Categories { get; set; }

        // Danh sách sản phẩm đang bán để Trang Chủ tiếp tục hiển thị.
        public List<ProductCardViewModel> Products { get; set; }
        //StoreHomeViewModel gom hai loại dữ liệu mà Trang Chủ cần cùng lúc: Categories để tạo danh sách danh mục và Products để hiển thị các card sản phẩm.
        //Nếu vẫn dùng IEnumerable<ProductCardViewModel> như B05 thì View không có chỗ nhận thêm danh sách Category.

        // Mã danh mục người dùng đang chọn.
        // int? cho phép null = không lọc danh mục.
        public int? SelectedCategoryId { get; set; }

<<<<<<< HEAD
        // Từ khóa người dùng đang tìm kiếm.
        public string SearchTerm { get; set; }
=======
        // Trang hiện tại.
        public int CurrentPage { get; set; }

        // Tổng số trang.
        public int TotalPages { get; set; }
>>>>>>> 5761ff9 (5.1. Bổ sung thông tin phân trang vào StoreHomeViewModel)
    }
}
