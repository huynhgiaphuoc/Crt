using Jewelly.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Mvc;

namespace Jewelly.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        JwelleyEntities db = new JwelleyEntities();
        public ActionResult Cart()
        {
            if (Session["username"] != null)
            {
                if (TempData["result"] != null)
                {
                    ViewBag.SuccessMg = TempData["result"];
                }
                if (TempData["error"] != null)
                {
                    ViewBag.ErrorMg = TempData["error"];
                }
                if (Session["Cart"] == null)
                {
                    return RedirectToAction("Shoes", "Product");
                }
                ShoppingCart cart = Session["Cart"] as ShoppingCart;

                return View(cart);
            }
            else
            {
                RedirectToAction("Jewelry", "Product");
            }
            return View();
        }

        public ActionResult Jewelry(decimal? MinPrice, decimal? MaxPrice, int? Brandtype, int? jewelry, int? Gold, int? Categorytype, int? stoneq, string prices)
        {
            var model = new Join().SelectProduct(MinPrice, MaxPrice, Brandtype, Gold, jewelry, Categorytype, stoneq, prices).ToList();
            List<BrandMst> brand = db.BrandMsts.ToList();
            List<JewelTypeMst> jewe = db.JewelTypeMsts.ToList();
            List<CatMst> gold = db.CatMsts.ToList();
            List<GoldKrtMst> gold_t = db.GoldKrtMsts.ToList();
            List<StoneQltyMst> stone = db.StoneQltyMsts.ToList();
            dynamic models1 = new ExpandoObject();
            models1.Brand = brand;
            models1.Producter = model;
            models1.Jewe = jewe;
            models1.Cate = gold;
            models1.GoldType = gold_t;
            models1.Stone = stone;
            return View(models1);

        }

        public ActionResult Details(int id) 
        {
            var model = new JoinDetails().SelectDetails(id).ToList();
            dynamic models = new ExpandoObject();
            models.Details = model;
            return View(models);
        }

 


        [HttpGet]
        public ActionResult AddtoCart(int Id, string Img,string Path,string Name,decimal? Price,string Prod,int quantity=1)
        {
            if (Session["Username"] != null)
            {
                int user = (int)Session["userID"];
                Cocart cocart = new Cocart();
                cocart.Add(Id,user,Img, Path,Name,Price,Prod,quantity);
                return RedirectToAction("Details/"+Id,"Product");
            }
            else
            {
                return RedirectToAction("Payment","Product");
            }
           

        }
        public ActionResult ShowOrder()
        {
             if (Session["Username"] == null)
            {
                return Content("You are not logged in, Please log in!");

            }
            int username = (int)Session["userID"];
           var users = db.ShoppingCarts.Where(s => s.User_id==username).ToList();
            var money = db.ShoppingCarts.Sum(s => s.Total);
            dynamic model= new ExpandoObject();
            model.Details = users;
            model.Money = money;

            return View(model);
        }

        public PartialViewResult BagCartTotal()
        {
            int total_item = 0;
            var cart = db.ShoppingCarts.ToList();
            total_item=(int)cart.Sum(s => s.Quantity);
            ViewBag.QuantityCart = total_item;
            return PartialView("BagCartTotal");
        }


        public ActionResult Delete(int id)
        {
            if (Session["Username"] != null)
            {
                var cart = db.ShoppingCarts.FirstOrDefault(s => s.ID == id);
                if (cart != null)
                {
                    db.ShoppingCarts.Remove(cart);
                    TempData["result"] = "Delete Product in cart successfully!";
                    return RedirectToAction("Cart", "Product");
                }
                else
                {
                    return RedirectToAction("Cart", "Product");

                }


            }  
            else
                {
                    TempData["error"] = "You need to log in, please log in to continue!";
                    return RedirectToAction("Login", "Account");
                }
        }
        //public decimal? Total_money()
        //{
           
        //    return (decimal)money;
        //}

        //public ActionResult CheckOut()
        //{
        //    if (TempData["result"] != null)
        //    {
        //        ViewBag.SuccessMg = TempData["result"];
        //    }
        //    if (TempData["error"] != null)
        //    {
        //        ViewBag.ErrorMg = TempData["error"];
        //    }
        //    if (Session["username"] != null)
        //    {
        //        if (Session["Cart"] == null)
        //        {
        //            return RedirectToAction("Order", "Product");
        //        }
        //        Cart cart = Session["Cart"] as Cart;

        //        return View(cart);
        //    }
        //    else
        //    {
        //        TempData["error"] = "You need to log in, please log in to continue!";
        //        return RedirectToAction("Login", "Account");
        //    }
        //}

        //public ActionResult ShoppingSuccess()
        //{
        //    return View();
        //}

        //public ActionResult Payment()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult Checkedout(FormCollection form)
        //{
        //    if (TempData["result"] != null)
        //    {
        //        ViewBag.SuccessMg = TempData["result"];
        //    }
        //    if (TempData["error"] != null)
        //    {
        //        ViewBag.ErrorMg = TempData["error"];
        //    }
        //    if (Session["username"] != null)
        //    {
        //        try
        //        {
        //            Cart cart = Session["Cart"] as Cart;
        //            CartList cartList = new CartList();
        //            cartList.ShipCode = form["payment"];
        //            if (cartList.ShipCode == "Visa")
        //            {
        //                Payment payments = new Payment();
        //                payments.numbercard = form["numbercard"];
        //                payments.cgv = int.Parse(form["cgv"]);
        //                payments.expiration_date = DateTime.Parse(form["dateend"]);
        //                payments.type = "Visa";
        //                db.Payments.Add(payments);
        //            }
        //            else
        //            {
        //                cartList.payment_ID = null;
        //            }
        //            cartList.payment_ID = 1;
        //            cartList.userID = null;
        //            cartList.userName = "an1203";
        //            cartList.ShipCode = "SC123";
        //            cartList.OrderDate = DateTime.Now.ToString();
        //            cartList.MRP = decimal.Parse(form["mrp"]);
        //            cartList.ShipName = form["name"];
        //            cartList.Email_ID = form["email"];
        //            cartList.Phone = form["phone"];
        //            cartList.ShipAddress = form["address"];
        //            cartList.ShipCity = form["city"];
        //            cartList.ShipCountry = form["country"];
        //            cartList.Status = "Pending";
        //            cartList.Note = form["note"];
        //            cartList.Product_Name = form["product"];

        //            db.CartLists.Add(cartList);
        //            foreach (var item in cart.Items)
        //            {
        //                Orderdetail details = new Orderdetail();
        //                details.ID = cartList.ID;
        //                details.Style_Code = item.ItemMst.Style_Code;
        //                details.Quantity = (int?)item.quantity;
        //                details.UnitPrice = item.ItemMst.MRP;
        //                db.Orderdetails.Add(details);
        //            }
        //            db.SaveChanges();
        //            cart.ClearCart();
        //            return RedirectToAction("ShoppingSuccess", "Product");
        //        }
        //        catch
        //        {
        //            return Content("Error Checkout. Please information of Customer...");
        //        }
        //    }
        //    else
        //    {
        //        TempData["error"] = "You need to log in, please log in to continue!";
        //        return RedirectToAction("Login", "Account");
        //    }
        //}
    }
}