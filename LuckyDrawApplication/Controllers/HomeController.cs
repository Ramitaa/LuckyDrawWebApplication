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
    public class HomeController : Controller
    {
        private string response_message = "";

        public ActionResult Index()
        {
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (luckydrawevent == null)
                return RedirectToAction("Index", "Login");

            ViewBag.Name = "Harry Potter";
            ViewBag.PurchasersList = AdminController.GetPurchasersCount(luckydrawevent.EventID);
            ViewBag.WinnersList = AdminController.GetWinnersCount(luckydrawevent.EventID);

            return View();
        }

        [HttpGet]
        public ActionResult CreateUserAndDraw()
        {
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (luckydrawevent == null)
                return RedirectToAction("Index", "Login");

            var list = new List<SelectListItem>();
            for (var i = 1; i < 41; i++)
                list.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });

            ViewBag.FloorUnitList = list;
            ViewBag.ProjectList = GetProjectList(luckydrawevent.EventID);
            ViewBag.SalesLocation = luckydrawevent.EventLocation;
            ViewBag.Name = "Harry Potter";

            return View();
        }

        [HttpPost]
        public ActionResult CreateUserAndDraw(Models.User user)
        {
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (luckydrawevent == null)
                return RedirectToAction("Index", "Login");

            if (user != null) 
            {
                if(DuplicateUserExists(user))
                {
                    return Json(new
                    {
                        success = false,
                        draw = -1,
                        message = "This unit has already been purchased by another buyer!"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    int results = CreateNewUser(user, luckydrawevent.EventID);
                 
                    return Json(new
                    {
                        success = true,
                        draw = results,
                        urllink = Url.Action("LuckyDrawAnimation", "Home", new { name = user.Name.ToUpper(), prize = results }, "https"),
                        message = user.Name.ToUpper() + " has been registered successfully!"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                response_message = "User is null!";

                return Json(new
                {
                    success = false,
                    draw = -1,
                    message = user.Name.ToUpper() + " cannot be registered! Error: " + response_message
                }, JsonRequestBehavior.AllowGet);
            }
           
        }

        [HttpGet]
        public ActionResult LuckyDrawAnimation(string name, int prize)
        {
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (luckydrawevent == null)
                return RedirectToAction("Index", "Login");

            ViewBag.Name = "Harry Potter";
            ViewBag.WinnerName = name;
            ViewBag.WinnerPrize = prize;

            return View();
        }

        public ActionResult Users()
        {
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (luckydrawevent == null)
                return RedirectToAction("Index", "Login");
            
            List<User> userList = GetUserList(luckydrawevent.EventID);
            ViewBag.Name = "Harry Potter";

            return View(userList);
        }

        public ActionResult ViewUser(int id)
        {
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (luckydrawevent == null)
                return RedirectToAction("Index", "Login");

            Models.User user = GetUser(id);
            ViewBag.Name = "Harry Potter";
            return View(user);
        }

        [HttpGet]
        public ActionResult ModifyUser(int id)
        {
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (luckydrawevent == null)
                return RedirectToAction("Index", "Login");

            Models.User user = GetUser(id);
            string[] tokens = user.Unit.Split('-');
            ViewBag.Block = tokens[0];
            ViewBag.Level = tokens[1];
            ViewBag.Unit = tokens[2];

            var list = new List<SelectListItem>();
            for (var i = 1; i < 41; i++)
                list.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });

            ViewBag.FloorUnitList = list;
            ViewBag.ProjectList = GetProjectList(luckydrawevent.EventID);
            ViewBag.SalesLocation = luckydrawevent.EventLocation;
            ViewBag.Name = "Harry Potter";

            return View(user);
        }

        [HttpPost]
        public ActionResult ModifyUser(Models.User user)
        {
            Models.Event luckydrawevent = (Models.Event)Session["event"];

            if (luckydrawevent == null)
                return RedirectToAction("Index", "Login");

            if (user != null)
            {
                if (DuplicateUserExistsForModification(user))
                {
                    return Json(new
                    {
                        success = false,
                        message = "This unit has already been purchased by another buyer!"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModifyExistingUser(user, luckydrawevent.EventID);
                    return Json(new
                    {
                        success = true,
                        url = Url.Action("Users", "Home"),
                        message = user.Name.ToUpper() + " has been successfully modified!"
                    }, JsonRequestBehavior.AllowGet);
                }   
            }
            else
            {
                response_message = "User is null!";

                return Json(new
                {
                    success = false,
                    draw = -1,
                    message = user.Name.ToUpper() + " cannot be modified! Error: " + response_message
                }, JsonRequestBehavior.AllowGet);
            }

        }

        // Register new user;
        [NonAction]
        public int CreateNewUser(Models.User user, int eventCode)
        {
            int last_inserted_id = 0;
            response_message = "";

            try
            {
                MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
                cn.Open();

                MySqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = String.Format("INSERT INTO user(Name, ICNumber, EmailAddress, ContactNumber, EventID, ProjectID, Unit, SalesConsultant, PrizeWon) VALUES (@name, @ic, @email, @contact, @eventID, @projectID, @unit, @salesconsultant, @prizewon); SELECT LAST_INSERT_ID() AS id;");
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.Name.ToUpper();
                cmd.Parameters.Add("@ic", MySqlDbType.VarChar).Value = user.ICNumber;
                cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = user.EmailAddress.ToLower();
                cmd.Parameters.Add("@contact", MySqlDbType.VarChar).Value = user.ContactNumber;
                cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventCode;
                cmd.Parameters.Add("@projectID", MySqlDbType.Int32).Value = user.ProjectID;
                cmd.Parameters.Add("@unit", MySqlDbType.VarChar).Value = user.Unit.ToUpper();
                cmd.Parameters.Add("@salesconsultant", MySqlDbType.VarChar).Value = user.SalesConsultant.ToUpper();
                cmd.Parameters.Add("@prizewon", MySqlDbType.Int16).Value = 0;

                MySqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    last_inserted_id = Convert.ToInt32(rd["id"]);
                }
                rd.Close();
                cmd.Dispose();
                cn.Close();
            }

            catch (Exception e)
            {
                response_message = e.Message;
            }

            try
            {
                MySqlConnection cn1 = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
                cn1.Open();

                MySqlCommand cmd1 = cn1.CreateCommand();
                cmd1.CommandType = CommandType.Text;
                cmd1.CommandText = String.Format("UPDATE project SET NoOfProject= NoOfProject + 1 WHERE ProjectID = @projectID");
                cmd1.Parameters.Add("@projectID", MySqlDbType.Int32).Value = user.ProjectID;
              
                MySqlDataReader rd1 = cmd1.ExecuteReader();
                rd1.Close();
                cmd1.Dispose();
                cn1.Close();
            }

            catch (Exception e)
            {
                response_message = e.Message;
            }

            return CheckForLuckyDraw(user, last_inserted_id);
        }

        [NonAction]
        public static int CheckForLuckyDraw(Models.User user, int last_inserted_id)
        {
            string prizeCategory = "";
            int prizeCode = 0;
            int prizeAmount = 0;

            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT project.PrizeCategory AS prizeCategory, luckydraw.Prize AS prize FROM project INNER JOIN luckydraw ON project.ProjectID = luckydraw.ProjectID WHERE luckydraw.ProjectID = @projectID AND luckydraw.OrderNo = project.NoOfProject");
            cmd.Parameters.Add("@projectID", MySqlDbType.Int16).Value = user.ProjectID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                prizeCategory = rd["prizeCategory"].ToString();
                prizeCode = Convert.ToInt32(rd["prize"]);
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            Debug.WriteLine("Prize Category: " + prizeCategory + ", PrizeCode: " + prizeCode);

            if (prizeCode != 0)
            {
                string[] prizes = prizeCategory.Split(',');
                prizeAmount = Convert.ToInt32(prizes[prizeCode - 1]);
                UpdateDatabaseWhenLuckyDrawIsWon(last_inserted_id, prizeCode);
                return prizeAmount;
            }

            else
            {
                return 0;
            }
        }

        [NonAction]
        public static void UpdateDatabaseWhenLuckyDrawIsWon(int userID, int prizeCode)
        {
            Debug.WriteLine("Updating Database with PrizeCode: " + prizeCode);

            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("UPDATE user SET PrizeWon= @PrizeWon WHERE PurchaserID = @PurchaserID");
            cmd.Parameters.Add("@PurchaserID", MySqlDbType.Int16).Value = userID;
            cmd.Parameters.Add("@PrizeWon", MySqlDbType.VarChar).Value = prizeCode;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            cn.Close();

        }

        // Check if ic, project and unit clashes;
        [NonAction]
        public static Boolean DuplicateUserExists(Models.User user)
        {
            int count = 0;

            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();
            
            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT COUNT(PurchaserID) AS userExists FROM user WHERE ProjectID = @project AND Unit = @unit");
            cmd.Parameters.Add("@project", MySqlDbType.Int32).Value = user.ProjectID;
            cmd.Parameters.Add("@unit", MySqlDbType.VarChar).Value = user.Unit.ToUpper();

            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                count = Convert.ToInt32(rd["userExists"].ToString());
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return count > 0;

        }

        // Get users list
        [NonAction]
        public static List<Models.User> GetUserList(int eventID)
        {
            List<Models.User> UserList = new List<Models.User>();
            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT user.*, project.ProjectName, project.PrizeCategory, event.EventLocation FROM user INNER JOIN project on project.ProjectID = user.ProjectID INNER JOIN event ON event.EventID = user.EventID WHERE user.EventID = @eventID");
            cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Models.User user = new Models.User();
                user.PurchaserID = Convert.ToInt32(rd["PurchaserID"].ToString());
                user.Name = rd["Name"].ToString();
                user.ICNumber = rd["ICNumber"].ToString();
                user.EmailAddress = rd["EmailAddress"].ToString();
                user.ContactNumber = rd["ContactNumber"].ToString();
                user.EventID = Convert.ToInt32(rd["EventID"].ToString());
                user.ProjectID = Convert.ToInt32(rd["ProjectID"].ToString());
                user.ProjectName = rd["ProjectName"].ToString();
                user.SalesLocation = rd["EventLocation"].ToString();
                user.Unit = rd["Unit"].ToString();

                if (Convert.ToInt32(rd["PrizeWon"]) > 0)
                {
                    string[] prizes = rd["PrizeCategory"].ToString().Split(',');
                    user.PrizeWon = Convert.ToInt32(prizes[Convert.ToInt32(rd["PrizeWon"]) - 1]);
                }
                else
                {
                    user.PrizeWon = 0;
                }

                user.SalesConsultant = rd["SalesConsultant"].ToString();
                user.DateTime = rd["DateTime"].ToString();
                UserList.Add(user);
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return UserList;
        }

        // Get users list
        [NonAction]
        public static List<Models.User> GetWinnerList(int eventID)
        {
            List<Models.User> UserList = new List<Models.User>();
            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT user.*, project.ProjectName, project.PrizeCategory, event.EventLocation FROM user INNER JOIN project on project.ProjectID = user.ProjectID INNER JOIN event ON event.EventID = user.EventID WHERE PrizeWon > 0 AND user.EventID = @eventID");
            cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Models.User user = new Models.User();
                user.PurchaserID = Convert.ToInt32(rd["PurchaserID"].ToString());
                user.Name = rd["Name"].ToString();
                user.ICNumber = rd["ICNumber"].ToString();
                user.EmailAddress = rd["EmailAddress"].ToString();
                user.ContactNumber = rd["ContactNumber"].ToString();
                user.EventID = Convert.ToInt32(rd["EventID"].ToString());
                user.ProjectID = Convert.ToInt32(rd["ProjectID"].ToString());
                user.ProjectName = rd["ProjectName"].ToString();
                user.SalesLocation = rd["EventLocation"].ToString();
                user.Unit = rd["Unit"].ToString();
                user.SalesConsultant = rd["SalesConsultant"].ToString();

                if (Convert.ToInt32(rd["PrizeWon"]) > 0)
                {
                    string[] prizes = rd["PrizeCategory"].ToString().Split(',');
                    user.PrizeWon = Convert.ToInt32(prizes[Convert.ToInt32(rd["PrizeWon"]) - 1]);
                }
                else
                {
                    user.PrizeWon = 0;
                }

                user.DateTime = rd["DateTime"].ToString();
                UserList.Add(user);
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return UserList;
        }

        // Get users list
        [NonAction]
        public static Models.User GetUser(int userID)
        {
            Models.User user = new Models.User();
            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT project.ProjectName, project.PrizeCategory, event.EventLocation, user.* FROM user INNER JOIN project on project.ProjectID = user.ProjectID INNER JOIN event ON event.EventID = user.EventID WHERE PurchaserID = @id");
            cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = userID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                user.PurchaserID = Convert.ToInt32(rd["PurchaserID"].ToString());
                user.Name = rd["Name"].ToString();
                user.ICNumber = rd["ICNumber"].ToString();
                user.EmailAddress = rd["EmailAddress"].ToString();
                user.ContactNumber = rd["ContactNumber"].ToString();
                user.EventID = Convert.ToInt32(rd["EventID"].ToString());
                user.ProjectID = Convert.ToInt32(rd["ProjectID"].ToString());
                user.ProjectName = rd["ProjectName"].ToString();
                user.SalesLocation = rd["EventLocation"].ToString();
                user.Unit = rd["Unit"].ToString();
                user.SalesConsultant = rd["SalesConsultant"].ToString();

                if (Convert.ToInt32(rd["PrizeWon"]) > 0)
                {
                    string[] prizes = rd["PrizeCategory"].ToString().Split(',');
                    user.PrizeWon = Convert.ToInt32(prizes[Convert.ToInt32(rd["PrizeWon"]) - 1]);
                }
                else
                {
                    user.PrizeWon = 0;
                }

                user.DateTime = rd["DateTime"].ToString();
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return user;
        }

        // Modify existing user;
        [NonAction]
        public void ModifyExistingUser(Models.User user, int eventCode)
        {
            try
            {
                MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
                cn.Open();

                MySqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = String.Format("UPDATE user SET Name = @name, ICNumber = @ic, EmailAddress= @email, ContactNumber = @contact, EventID = @eventID, ProjectID = @projectID, Unit = @unit, SalesConsultant = @salesconsultant WHERE PurchaserID = @id");
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = user.PurchaserID;
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.Name.ToUpper();
                cmd.Parameters.Add("@ic", MySqlDbType.VarChar).Value = user.ICNumber;
                cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = user.EmailAddress.ToLower();
                cmd.Parameters.Add("@contact", MySqlDbType.VarChar).Value = user.ContactNumber;
                cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventCode;
                cmd.Parameters.Add("@projectID", MySqlDbType.Int32).Value = user.ProjectID;
                cmd.Parameters.Add("@unit", MySqlDbType.VarChar).Value = user.Unit.ToUpper();
                cmd.Parameters.Add("@salesconsultant", MySqlDbType.VarChar).Value = user.SalesConsultant.ToUpper();

                MySqlDataReader rd = cmd.ExecuteReader();
                rd.Close();
                cmd.Dispose();
                cn.Close();
            }

            catch (Exception e)
            {
                response_message = e.Message;
            }
        }

        // Check if project and unit clashes;
        [NonAction]
        public static Boolean DuplicateUserExistsForModification(Models.User user)
        {
            Debug.WriteLine("Checking for duplicate" + user.PurchaserID + " projectID: " + user.ProjectID + "unit: " + user.Unit.ToUpper());
            int count = 0;

            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT COUNT(PurchaserID) AS userExists FROM user WHERE ProjectID = @projectID AND Unit = @unit AND PurchaserID != @id");
            cmd.Parameters.Add("@id", MySqlDbType.VarChar).Value = user.PurchaserID;
            cmd.Parameters.Add("@projectID", MySqlDbType.Int32).Value = user.ProjectID;
            cmd.Parameters.Add("@unit", MySqlDbType.VarChar).Value = user.Unit.ToUpper();

            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                count = Convert.ToInt32(rd["userExists"].ToString());
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return count > 0;

        }

        [NonAction]
        public static List<SelectListItem> GetProjectList(int eventID)
        {
            List<SelectListItem> Projects = new List<SelectListItem>();

            MySqlConnection cn = new MySqlConnection(@"DataSource=103.6.199.135:3306;Initial Catalog=com12348_;User Id=luckywheel;Password=luckywheelrocks123@");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT * FROM project WHERE EventID = @eventID");
            cmd.Parameters.Add("@eventID", MySqlDbType.Int32).Value = eventID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Projects.Add(new SelectListItem() { Text = rd["ProjectName"].ToString(), Value = Convert.ToInt32(rd["ProjectID"]).ToString()});
            }
            rd.Close();
            cmd.Dispose();

            cn.Close();

            return Projects;
        }
    }
}