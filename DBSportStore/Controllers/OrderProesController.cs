using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;
using DBSportStore.Filters;

namespace DBSportStore.Controllers
{
    [AdminAuthorize]
    public class OrderProesController : Controller
    {
        private DBSportStoreEntities db = new DBSportStoreEntities();

        // GET: OrderProes
        public ActionResult Index()
        {
            var orderPro = db.OrderPro.Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate);
            return View(orderPro.ToList());
        }

        // B08 - Bài 2: Chỉ Admin được đổi trạng thái đơn hàng.
        // Không nhận cả OrderPro từ form, tránh sửa các cột không liên quan.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            string[] allowedStatuses =
            {
                "Chờ xử lý",
                "Đã xác nhận",
                "Đang giao",
                "Hoàn thành",
                "Đã hủy"
            };

            if (string.IsNullOrWhiteSpace(status) ||
                !allowedStatuses.Contains(status))
            {
                TempData["OrderError"] = "Trạng thái đơn hàng không hợp lệ.";
                return RedirectToAction("Index");
            }

            var order = db.OrderPro.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            order.Status = status;
            db.SaveChanges();

            TempData["OrderSuccess"] = "Đã cập nhật trạng thái đơn hàng #" + id + ".";
            return RedirectToAction("Index");
        }

        // GET: OrderProes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderPro orderPro = db.OrderPro.Find(id);
            if (orderPro == null)
            {
                return HttpNotFound();
            }
            return View(orderPro);
        }

        // GET: OrderProes/Create
        public ActionResult Create()
        {
            ViewBag.CustomerID = new SelectList(db.Customer, "CustomerID", "FullName");
            return View();
        }

        // POST: OrderProes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderID,OrderDate,CustomerID,ShippingAddress,TotalAmount,Status")] OrderPro orderPro)
        {
            if (ModelState.IsValid)
            {
                db.OrderPro.Add(orderPro);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerID = new SelectList(db.Customer, "CustomerID", "FullName", orderPro.CustomerID);
            return View(orderPro);
        }

        // GET: OrderProes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderPro orderPro = db.OrderPro.Find(id);
            if (orderPro == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerID = new SelectList(db.Customer, "CustomerID", "FullName", orderPro.CustomerID);
            return View(orderPro);
        }

        // POST: OrderProes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderID,OrderDate,CustomerID,ShippingAddress,TotalAmount,Status")] OrderPro orderPro)
        {
            if (ModelState.IsValid)
            {
                db.Entry(orderPro).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerID = new SelectList(db.Customer, "CustomerID", "FullName", orderPro.CustomerID);
            return View(orderPro);
        }

        // GET: OrderProes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderPro orderPro = db.OrderPro.Find(id);
            if (orderPro == null)
            {
                return HttpNotFound();
            }
            return View(orderPro);
        }

        // POST: OrderProes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OrderPro orderPro = db.OrderPro.Find(id);
            db.OrderPro.Remove(orderPro);
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
