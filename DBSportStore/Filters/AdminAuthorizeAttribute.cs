using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DBSportStore.Filters
{
    // =========================================================
    // ATTRIBUTE KIỂM TRA QUYỀN ADMIN
    // =========================================================

    // Kế thừa AuthorizeAttribute để có thể gắn
    // [AdminAuthorize] lên Controller hoặc Action.
    public class AdminAuthorizeAttribute
        : AuthorizeAttribute
    {
        // =====================================================
        // KIỂM TRA REQUEST CÓ PHẢI ADMIN HAY KHÔNG
        // =====================================================

        protected override bool AuthorizeCore(
            HttpContextBase httpContext)
        {
            // Chưa có UserID trong Session
            // nghĩa là chưa Login.
            if (httpContext.Session["UserID"] == null)
            {
                return false;
            }


            // Convert.ToString() tránh lỗi nếu Session["Role"] null.
            string role =
                Convert.ToString(
                    httpContext.Session["Role"]);


            // Chỉ Role Admin mới trả true.
            return role == "Admin";
        }


        // =====================================================
        // XỬ LÝ KHI KHÔNG CÓ QUYỀN
        // =====================================================

        protected override void HandleUnauthorizedRequest(
            AuthorizationContext filterContext)
        {
            // Chưa Login -> chuyển về trang Login.
            if (filterContext.HttpContext
                .Session["UserID"] == null)
            {
                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "Account",
                                action = "Login"
                            }));

                return;
            }


            // Đã Login nhưng không phải Admin
            // -> trả về HTTP 403 Forbidden.
            filterContext.Result =
                new HttpStatusCodeResult(
                    403,
                    "Bạn không có quyền truy cập trang quản trị.");
        }
    }

}