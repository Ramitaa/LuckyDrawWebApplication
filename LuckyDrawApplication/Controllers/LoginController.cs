using LuckyDrawApplication.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
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


        //----------------------------------------------------------------------------------------------------------------------------------------------
        //                                                              ADMIN CODE
        //----------------------------------------------------------------------------------------------------------------------------------------------
        
            // GET: Login
        [HttpGet]
        public ActionResult AdminIndex()
        {
            ViewBag.Events = GetEventList();
            return View();
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(Models.ForgotPassword forgotPassword)
        {
            if (ModelState.IsValid)
            {
                Tuple<bool, String> updateToken = SetForgotPasswordToken(forgotPassword.EmailAddress);
                if (updateToken.Item1)
                {
                    var lnkHref = Url.Action("ResetPassword", "Login", new { email = forgotPassword.EmailAddress, token = updateToken.Item2 }, "https");
                    String body = "Hi there!\nYour reset password link is " + lnkHref + ".\nClick on this link to reset your LuckyWheel admin account's password.\n\nBest regards,\nLuckyWheel Team";
                    SendEmail("LuckyWheel | Reset Password Link (Admin Account)", body, forgotPassword.EmailAddress);
                    return RedirectToAction("AdminIndex", "Login");
                }
                else
                {
                    ViewBag.ErrorMessage = "No such email address is registered under LuckyWheel.";
                    return View();
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult ResetPassword(String email, String token)
        {
            Models.ResetPassword rp = new Models.ResetPassword();
            rp.EmailAddress = email;
            rp.Token = token;

            return View(rp);
        }

        [HttpPost]
        public ActionResult ResetPassword(Models.ResetPassword resetPassword)
        {
            if (ModelState.IsValid)
            {
                if (ResetPasswordInDB(resetPassword))
                {
                    return RedirectToAction("AdminIndex", "Login");
                }
                else
                {
                    ViewBag.ErrorMessage = "The token is invalid. Please reset your password again!";
                    return View();
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Please ensure all fields are valid.";
                return View();
            }
        }

        // POST: LoginAdmin
        [HttpPost]
        public ActionResult AdminIndex(Models.Admin admin)
        {
            Debug.WriteLine("Password: '" + admin.Password);

            if (ModelState.IsValid)
            {
                Tuple<bool, int, string> result = DecryptPasswordForAdmin(admin.Email, admin.Password);
                admin.ID = result.Item2;
                admin.Name = result.Item3;

                if (result.Item1)
                {
                    Models.Event luckydrawevent = GetEventDetails(admin.EventID);
                   
                    Session["admin"] = admin;
                    Session["event"] = luckydrawevent;

                    return RedirectToAction("Index", "Admin");
                }
                ViewBag.ErrorMessage = "Authentication failed!";
                return View();
            }

            ViewBag.ErrorMessage = "Authentication failed!";
            return View();

        }

        [NonAction]
        public static Tuple<bool, int, string> DecryptPasswordForAdmin(string emailAddress, string password)
        {
            bool isPasswordMatch = false;
            int UserID = 0;
            string name = "";

            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();
            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT * FROM adminlogin WHERE emailAddress = @emailAddress");
            cmd.Parameters.Add("@emailAddress", MySqlDbType.VarChar).Value = emailAddress;

            MySqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                var hash = createPasswordHash(rd["salt"].ToString(), password);

                if (hash.Equals(rd["passwordHash"].ToString()))
                {
                    UserID = Convert.ToInt32(rd["userID"]);
                    name = rd["name"].ToString();
                    isPasswordMatch = true;
                    break;
                }
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return new Tuple<bool, int, string>(isPasswordMatch, UserID, name);

        }

        [NonAction]
        public static Models.Event GetEventDetails(int eventID)
        {
            Models.Event luckydrawevent = new Models.Event();
 
            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();
            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT * FROM event WHERE EventID = @id");
            cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = eventID;

            MySqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                luckydrawevent.EventID = Convert.ToInt32(rd["EventID"]);
                luckydrawevent.EventCode = (rd["EventCode"]).ToString();
                luckydrawevent.EventLocation = rd["EventLocation"].ToString();
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return luckydrawevent;
        }

        [NonAction]
        public static void SendEmail(string Subject, string Body, string To)
        {
            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
            mail.To.Add(To);
            mail.From = new MailAddress("ramitaa.loganathan98@gmail.com");
            mail.Subject = Subject;
            mail.Body = Body;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.Credentials = new NetworkCredential("ramitaa.loganathan98@gmail.com", "RDJ123Forever@");
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }

        [NonAction]
        public static Tuple<bool, String> SetForgotPasswordToken(String emailAddress)
        {
            bool userExists = false, emailExists = false;
            String token = getSalt();

            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT COUNT(emailAddress) AS count FROM adminlogin WHERE emailAddress = @emailAddress");
            cmd.Parameters.Add("@emailAddress", MySqlDbType.VarChar).Value = emailAddress;

            MySqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                if (Convert.ToInt32(rd["count"].ToString()) == 0)
                    userExists = false;
                else
                    userExists = true;
            }

            rd.Close();
            cmd.Dispose();

            if (userExists)
            {
                MySqlCommand cmd1 = cn.CreateCommand();
                cmd1.CommandType = CommandType.Text;
                cmd1.CommandText = String.Format("SELECT COUNT(emailAddress) AS count FROM adminforgotpassword WHERE emailAddress = @emailAddress");
                cmd1.Parameters.Add("@emailAddress", MySqlDbType.VarChar).Value = emailAddress;

                MySqlDataReader rd1 = cmd1.ExecuteReader();

                while (rd1.Read())
                {
                    if (Convert.ToInt32(rd1["count"].ToString()) == 0)
                        emailExists = false;
                    else
                        emailExists = true;
                }

                rd1.Close();
                cmd1.Dispose();

                if (emailExists)
                {
                    token = getSalt(); ;

                    MySqlCommand cmd2 = cn.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = String.Format("UPDATE adminforgotpassword SET token = @token WHERE emailAddress = @emailAddress");
                    cmd2.Parameters.Add("@emailAddress", MySqlDbType.VarChar).Value = emailAddress;
                    cmd2.Parameters.Add("@token", MySqlDbType.VarChar).Value = token;
                    MySqlDataReader rd2 = cmd2.ExecuteReader();
                    rd2.Close();
                    cmd2.Dispose();
                }
                else
                {
                    token = getSalt(); ;

                    MySqlCommand cmd3 = cn.CreateCommand();
                    cmd3.CommandType = CommandType.Text;
                    cmd3.CommandText = String.Format("INSERT INTO adminforgotpassword(emailAddress, token) VALUES(@emailAddress, @token)");
                    cmd3.Parameters.Add("@emailAddress", MySqlDbType.VarChar).Value = emailAddress;
                    cmd3.Parameters.Add("@token", MySqlDbType.VarChar).Value = token;
                    MySqlDataReader rd3 = cmd3.ExecuteReader();
                    rd3.Close();
                    cmd3.Dispose();
                }

                cn.Close();

                return new Tuple<bool, String>(true, token);
            }
            else
            {
                return new Tuple<bool, String>(false, "");
            }
        }

        [NonAction]
        public static bool ResetPasswordInDB(Models.ResetPassword resetPassword)
        {
            bool tokenMatches = false;

            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT COUNT(emailAddress) AS count FROM adminforgotpassword WHERE token = @token AND emailAddress = @emailAddress");
            cmd.Parameters.Add("@emailAddress", MySqlDbType.VarChar).Value = resetPassword.EmailAddress;
            cmd.Parameters.Add("@token", MySqlDbType.VarChar).Value = resetPassword.Token;

            MySqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                if (Convert.ToInt32(rd["count"].ToString()) == 0)
                    tokenMatches = false;
                else
                    tokenMatches = true;
            }

            rd.Close();
            rd.Dispose();

            if (tokenMatches)
            {
                string salt = getSalt();
                string passwordHash = createPasswordHash(salt, resetPassword.NewPassword);

                MySqlCommand cmd1 = cn.CreateCommand();
                cmd1.CommandType = CommandType.Text;
                cmd1.CommandText = String.Format("UPDATE adminlogin SET passwordHash = @passwordHash, salt = @salt WHERE emailAddress = @emailAddress");
                cmd1.Parameters.Add("@emailAddress", MySqlDbType.VarChar).Value = resetPassword.EmailAddress;
                cmd1.Parameters.Add("@passwordHash", MySqlDbType.VarChar).Value = passwordHash;
                cmd1.Parameters.Add("@salt", MySqlDbType.Blob).Value = salt;

                MySqlDataReader rd1 = cmd1.ExecuteReader();

                rd1.Close();
                cmd1.Dispose();
                cn.Close();

                return true;

            }
            else
                return false;
        }

        [NonAction]
        public static List<SelectListItem> GetEventList()
        {
            List<SelectListItem> Events = new List<SelectListItem>();

            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT * FROM event");
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Events.Add(new SelectListItem() { Text = Convert.ToInt32(rd["EventID"]).ToString() + "- " + rd["EventLocation"].ToString(), Value = Convert.ToInt32(rd["EventID"]).ToString() });
            }
            rd.Close();
            cmd.Dispose();

            cn.Close();

            return Events;
        }
    }
}

