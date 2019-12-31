using LuckyDrawApplication.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LuckyDrawApplication.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [HttpGet]
        public ActionResult Index()
        {
            string salt = getSalt();
            ViewBag.Hash = createPasswordHash("jjFeKvpPvLDN5Za7LZvdwKpWAA9i4tR67YD3s4nR5Fd7feSKZp66xsjY4seKwGUB", "101luckydraw");
            ViewBag.Salt = salt;

            return View();
        }

        // POST: Login
        [HttpPost]
        public ActionResult Index(Models.Event luckydrawevent)
        {
            Debug.WriteLine("Event code" + luckydrawevent.EventCode + "Event Password: " + luckydrawevent.EventPassword);

            if (ModelState.IsValid)
            {
                Tuple<bool, int, string> result = DecryptPassword(luckydrawevent.EventCode, luckydrawevent.EventPassword);
                luckydrawevent.EventID = result.Item2;
                luckydrawevent.EventLocation = result.Item3;

                if (result.Item1)
                {
                    Session["event"] = luckydrawevent;
                    return RedirectToAction("Index", "Home");

                }
                ViewBag.ErrorMessage = "Authentication failed!";
                return View();
            }

            ViewBag.ErrorMessage = "Authentication failed!";
            return View();

        }

        public ActionResult LogOut()
        {
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();

            return RedirectToAction("Index", "Login");
        }


        [NonAction]
        public static Tuple<bool, int, string> DecryptPassword(string code, string password)
        {
            Debug.WriteLine("IM IN HERE!");
            bool isPasswordMatch = false;
            int eventID = 0;
            string eventLocation = "";

            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();
            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT * FROM event WHERE EventCode = @code");
            cmd.Parameters.Add("@code", MySqlDbType.VarChar).Value = code;

            Debug.WriteLine("Code: " + code);

            MySqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                
                var hash = createPasswordHash(rd["EventSalt"].ToString(), password);

                if (hash.Equals(rd["EventPassword"].ToString()))
                {
                    eventID = Convert.ToInt32(rd["EventID"]);
                    eventLocation = rd["EventLocation"].ToString();
                    isPasswordMatch = true;
                    break;
                }
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return new Tuple<bool, int, string>(isPasswordMatch, eventID, eventLocation);
        }

        [NonAction]
        public static string createPasswordHash(string salt_c, string password)
        {

            int PASSWORD_BCRYPT_COST = 13;
            string PASSWORD_SALT = salt_c;
            string salt = "$2a$" + PASSWORD_BCRYPT_COST + "$" + PASSWORD_SALT;
            var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);

            Debug.WriteLine("Salt_c: " + salt_c, "Hash: " + hash);
            return hash;
        }

        [NonAction]
        public static string getSalt()
        {
            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 64).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
