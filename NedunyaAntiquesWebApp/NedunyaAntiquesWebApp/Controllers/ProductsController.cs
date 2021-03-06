﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using NedunyaAntiquesWebApp.Models;

namespace NedunyaAntiquesWebApp.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationContext db = new ApplicationContext();
        string AdminId = "954da09c-478f-4012-bd0e-76180a40d039";

        // GET: Products
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        public async Task<ActionResult> Index(string selectCat, string free, string priceMax, string priceMin,
            string hightMax, string hightMin, string onSale, string canRent)
        {
            var userID = Session["userID"];
            if(userID != null && userID.ToString() == AdminId)
            {
                IQueryable<string> catQuery = from p in db.Products
                                              orderby p.Category
                                              select p.Category;

                IQueryable<string> subCatQuery = from p in db.Products
                                                 orderby p.SubCategory
                                                 select p.SubCategory;

                List<SelectListItem> items = new List<SelectListItem>();
                items.Add(new SelectListItem { Text = "הכל", Value = "", Selected = true });
                foreach (var cat in catQuery.Distinct())
                {
                    items.Add(new SelectListItem { Text = cat, Value = cat });
                }
                foreach (var subCat in subCatQuery.Distinct())
                {
                    items.Add(new SelectListItem { Text = subCat, Value = subCat });
                }
                ViewBag.selectCat = items;

                var products = from p in db.Products
                               select p;

                if (!String.IsNullOrEmpty(free))
                {
                    products = products.Where(s => s.Description.Contains(free));
                }

                if (!String.IsNullOrEmpty(selectCat))
                {
                    products = products.Where(x => x.Category == selectCat || x.SubCategory == selectCat);
                }

                if (!String.IsNullOrEmpty(priceMin))
                {
                    var p = Convert.ToDecimal(priceMin);
                    products = products.Where(x => x.Price >= p);
                }

                if (!String.IsNullOrEmpty(priceMax))
                {
                    var p = Convert.ToDecimal(priceMax);
                    products = products.Where(x => x.Price <= p);
                }

                if (!String.IsNullOrEmpty(hightMin))
                {
                    var h = Convert.ToDouble(hightMin);
                    products = products.Where(x => x.Height >= h);
                }

                if (!String.IsNullOrEmpty(hightMax))
                {
                    var h = Convert.ToDouble(hightMax);
                    products = products.Where(x => x.Height <= h);
                }

                if (!String.IsNullOrEmpty(onSale))
                {
                    products = products.Where(x => x.Sale == true);
                }

                if (!String.IsNullOrEmpty(canRent))
                {
                    products = products.Where(x => x.Rented == true);
                }

                return View(await products.ToListAsync());
            }
            return RedirectToAction("CustomerLog", "Customers");
            
        }

        private class categoryCount
        {
            public string catName { get; }
            public int count { get; set; }
            public categoryCount(string catName, int count)
            {
                this.catName = catName;
                this.count = count;
            }
        }

        public ActionResult Api()
        {
            IQueryable<string> subCatQuery = from p in db.Products
                                             orderby p.SubCategory
                                             select p.SubCategory;

            List<categoryCount> subCat = new List<categoryCount>();

            foreach (var s in subCatQuery.Distinct())
            {
                subCat.Add(new categoryCount(s, 0));
            }
            foreach (var s in subCatQuery)
            {
                foreach (categoryCount c in subCat)
                {
                    if (c.catName.Equals(s))
                        c.count++;
                }
            }

            return Json(subCat, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Charts()
        {
           return View();
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        //GET : Products/Show
        public ActionResult ShowCategory(string category,string subcategory)
        {

            // List<Product> _productList;
        //    var productList = from p in db.Products where p.Category.Equals(category) select p;
          /*  if (!productList.Any())
            {
                return RedirectToAction("Index", "Home");
            }*/

            ViewBag.Category=category;
            ViewBag.SubCategory = subcategory;
            return View();
         }

        public ActionResult _ProductList(string category, string subcategory)
        {
            IQueryable<Product> productList;

            if (category != "null") { 
               productList = from p in db.Products where p.Category.Equals(category) select p;
            }
            else
            {
               productList = from p in db.Products where p.SubCategory.Equals(subcategory) select p;
            }
            ViewBag.Category = category;
            return PartialView(productList.ToList());
        }




        public ActionResult ShowProdOnSale()
        {
            var productList = from p in db.Products where p.Sale.Equals(true) select p;
            if (!productList.Any())
             {
                return RedirectToAction("Index","Home");
             }
            return View(productList.ToList());
        }



        // GET: Products/Save
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        public ActionResult Create()
        {
            var userID = Session["userID"];
            if(userID != null && userID.ToString() == AdminId)
            {
                ViewBag.Message = "Your application description page.";

                return View();
            }
            return RedirectToAction("CustomerLog", "Customers");
            
        }

        // POST: Products/Save
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "Name,Price,Substance,Category,SubCategory,Height,Width,Depth,Sale,DiscountPercentage,Rented,RentalPriceForDay,Description")] Product product, IEnumerable<HttpPostedFileBase> Images)
        {
            if (ModelState.IsValid)
            {
                if (Images.ElementAt(0)!= null)
                {
                    var imageList = new List<Image>();
                
                    foreach (var image in Images)
                    {
                        string imageName = System.IO.Path.GetFileName(image.FileName);
                        string physicalPath = Server.MapPath("~/Images/" + imageName);
                        image.SaveAs(physicalPath);
                        WebImage photo = new WebImage(physicalPath);
                        photo.Resize(640, 480);
                        photo.Save("~/Images/Thumbs/" + imageName);
                        var img = new Image { ProductId = product.ProductId};                        
                        img.Name = imageName;
                        img.Product = product;
                        imageList.Add(img);
                     }
                   
                    product.Images = imageList;
                }
                else
                {
                    return RedirectToAction("Create");
                }
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // GET: Products/Edit/5
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: Products/Edit/5
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit([Bind(Include = "ProductId,Name,Price,Substance,Category,SubCategory,Height,Width,Depth,Sale,DiscountPercentage,Rented,RentalPriceForDay,Description")] Product product, IEnumerable<HttpPostedFileBase> Images)
        {
            var userID = Session["userID"];
            if(userID != null && userID.ToString() == AdminId)
            {
                if (db.Products.Find(product.ProductId) != null)
                {
                    if (ModelState.IsValid)
                    {
                        if (Images.ElementAt(0) != null)
                        {
                            var productImages = db.Images.Where(n => n.ProductId == product.ProductId).ToList();
                            foreach (var image in productImages)
                            {
                                db.Images.Remove(image);
                            }
                            db.SaveChanges();

                            var imageList = new List<Image>();
                            foreach (var image in Images)
                            {
                                string imageName = System.IO.Path.GetFileName(image.FileName);
                                string physicalPath = Server.MapPath("~/Images/" + imageName);
                                image.SaveAs(physicalPath);
                                WebImage photo = new WebImage(physicalPath);
                                photo.Resize(640, 480);
                                photo.Save("~/Images/Thumbs/" + imageName);
                                var img = new Image { ProductId = product.ProductId };
                                img.Name = imageName;
                                img.Product = product;
                                imageList.Add(img);
                                db.Images.Add(img);
                            }

                            product.Images = imageList;
                        }

                        db.Products.AddOrUpdate(product);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }



                }
                else
                {
                    Response.Write(("<script>alert('Product was not found, please try another product');</script>"));
                }


                return View(product);
            }
            return RedirectToAction("CustomerLog", "Customers");
           
        
        }

        // GET: Products/Delete/5
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        public ActionResult Delete(int? id)
        {
            var userID = Session["userID"];
            if(userID != null && userID.ToString() == AdminId)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Product product = db.Products.Find(id);
                if (product == null)
                {
                    return HttpNotFound();
                }
                return View(product);
            }
            return RedirectToAction("CustomerLog", "Customers");
            
            
        }

        // POST: Products/Delete/5
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /*
        [HttpGet]
        public ActionResult DeleteAll()
        {
            foreach (Product p in db.Products)
            {
                db.Products.Remove(p);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        */

        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
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
