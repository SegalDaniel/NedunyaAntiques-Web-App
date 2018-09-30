﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using NedunyaAntiquesWebApp.Models;
using WebGrease.Css.Extensions;

namespace NedunyaAntiquesWebApp.Controllers
{
    public class CustomersController : Controller
    {
        private ApplicationContext db = new ApplicationContext();

        private void MigrateShoppingCart(string Email)
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            cart.MigrateCart(Email);
            Session[ShoppingCart.CartSessionKey] = Email;
        }

        // GET: Customers
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        public ActionResult Index()
        {
            return View(db.Customers.ToList());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult LogIn([Bind(Include = "Email,Password,RememberMe")] Customer customer)
        {

            Customer cust = db.Customers.Find(customer.Email);
            string message = string.Empty;
            if (cust != null)
            {
                if (cust.Password != customer.Password)
                    message = "הסיסמא אינה תקינה";

                MigrateShoppingCart(customer.Email);
                FormsAuthentication.SetAuthCookie(customer.Email, customer.RememberMe);

                ViewBag.Message = message;
                return RedirectToAction("Index");
            }
            message = "האימייל שהזנת אינו נמצא במערכת";
            ViewBag.Message = message;
            return View("CustomerLog", customer);

        }

        // POST: /Customers/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.emptyCart();
            return RedirectToAction("Index");
        }

        // GET: Customers/Details/5
        // Using filter to allow access only to login users.
        //[Authorize] - TODO: uncomment before you go live
        public ActionResult Details(string Email)
        {
            if (Email == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(Email);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
        }

        // GET: Customers/Save
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        
        public ActionResult Save()
        {
            return View();
        }

        // POST: Customers/Save
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveClient([Bind(Exclude = "RememberMe,Transactions")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                Customer cust = db.Customers.Find(customer.Email);
          
                if (cust == null)
                {
                    db.Customers.Add(customer);
                    db.SaveChanges();
                    MigrateShoppingCart(customer.Email);
                    return RedirectToAction("Index");
                }
                string message = string.Empty;
                message = "כתובת האימייל שהזנת כבר קיימת במערכת";
                ViewBag.Message = message;
            }
            
            return View("CustomerForm", customer);
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(Customer customer)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(customer.OldPassword, customer.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "הסיסמא הישנה או החדשה שבחרת אינם חוקיים");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(customer);
        }

        // GET: Customers/Edit/5
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        public ActionResult Edit(string Email)
        {
            if (Email == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(Email);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Email,Password")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        public ActionResult Delete(string Email)
        {
            if (Email == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(Email);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Delete/5
        // Using filter to allow access only to admin users.
        //[Authorize (Roles ="administor")] - TODO: uncomment before you go live
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string Email)
        {
            Customer customer = db.Customers.Find(Email);
            db.Customers.Remove(customer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult CustomerForm()
        {
            return View();
        }

        public ActionResult CustomerLog()
        {
            return View();
        }


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
