
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBSportStore.Models
{
    public class StoreHomeViewModel
    {
        // Danh sách danh mục lấy từ bảng Category
        public List<Category> Categories { get; set; }

        // Danh sách sản phẩm hiển thị trên Trang Chủ
        public List<ProductCardViewModel> Products { get; set; }

        // Chức năng lọc danh mục
        public int? SelectedCategoryId { get; set; }

        // Chức năng tìm kiếm
        public string SearchTerm { get; set; }

        // Chức năng phân trang - Bước 5.1
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Tổng số sản phẩm sau khi lọc và tìm kiếm
        public int TotalItems { get; set; }
        //6. Chức năng 5 - Bổ sung lọc sản phẩm theo khoảng giá
        // Giá tối thiểu người dùng nhập.
        // decimal? cho phép để trống ô Giá từ.
        public decimal? MinPrice { get; set; }

        // Giá tối đa người dùng nhập.
        // decimal? cho phép để trống ô Giá đến.
        public decimal? MaxPrice { get; set; }


    }
}
