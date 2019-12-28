using LuckyDrawApplication.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LuckyDrawApplication.Controllers
{
    public class HomeController : Controller
    {
        private int CODE_1 = 8888;
        private int CODE_2 = 500;
        private int CODE_3 = 200;
        private string response_message = "";

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CreateUserAndDraw()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateUserAndDraw(Models.User user)
        {
            if(user != null) 
            {
                if(DuplicateUserExists(user))
                {
                    return Json(new
                    {
                        success = false,
                        draw = -1,
                        message = user.Name.ToUpper() + " has been already been registered!"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    int results = CreateNewUser(user);
                 
                    return Json(new
                    {
                        success = true,
                        draw = results,
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
        public ActionResult CreateStaffAndDraw()
        {
            return View();
        }
 
        public ActionResult Users()
        {

            List<User> userList = GetUserList();

            return View(userList);
        }

        public ActionResult Winners()
        {

            List<User> winnerList = GetWinnerList();

            return View(winnerList);
        }

        public ActionResult ViewUser(int id)
        {
            Models.User user = GetUser(id);
            return View(user);
        }

        [HttpGet]
        public ActionResult ModifyUser(int id)
        {
            Models.User user = GetUser(id);
            return View(user);
        }

        [HttpPost]
        public ActionResult ModifyUser(Models.User user)
        {
            if (user != null)
            {
                ModifyExistingUser(user);
                return Json(new
                {
                    success = true,
                    url = Url.Action("Users", "Home"),
                    message = user.Name.ToUpper() + " has been successfully modified!"
                }, JsonRequestBehavior.AllowGet);
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
        public int CreateNewUser(Models.User user)
        {
            response_message = "";

            try
            {
                MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
                cn.Open();

                MySqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = String.Format("INSERT INTO user(Name, ICNumber, EmailAddress, ContactNumber, Project, Unit, SalesConsultant, SalesLocation, PrizeWon) VALUES (@name, @ic, @email, @contact, @project, @unit, @salesconsultant, @saleslocation, @prizewon)");
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.Name.ToUpper();
                cmd.Parameters.Add("@ic", MySqlDbType.VarChar).Value = user.ICNumber;
                cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = user.EmailAddress.ToLower();
                cmd.Parameters.Add("@contact", MySqlDbType.VarChar).Value = user.ContactNumber;
                cmd.Parameters.Add("@project", MySqlDbType.VarChar).Value = user.Project.ToUpper();
                cmd.Parameters.Add("@unit", MySqlDbType.VarChar).Value = user.Unit.ToUpper();
                cmd.Parameters.Add("@salesconsultant", MySqlDbType.VarChar).Value = user.SalesConsultant.ToUpper();
                cmd.Parameters.Add("@saleslocation", MySqlDbType.VarChar).Value = user.SalesLocation.ToUpper();
                cmd.Parameters.Add("@prizewon", MySqlDbType.Int16).Value = 0;

                MySqlDataReader rd = cmd.ExecuteReader();
                rd.Close();
                cmd.Dispose();
                cn.Close();
            }

            catch (Exception e)
            {
                response_message = e.Message;
            }
    
            return CheckForLuckyDraw(user);
        }

        [NonAction]
        public static int CheckForLuckyDraw(Models.User user)
        {
            int dayID = 0, bigPrize = 0, mediumPrize = 0, smallPrize = 0;

            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT dayID, bigPrize, mediumPrize, smallPrize FROM algorithm WHERE CURDATE() = date AND CURTIME() >= startTime AND curtime() <= endTime");

            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                dayID = Convert.ToInt32(rd["dayID"].ToString());
                bigPrize = Convert.ToInt32(rd["bigPrize"].ToString());
                mediumPrize = Convert.ToInt32(rd["mediumPrize"].ToString());
                smallPrize = Convert.ToInt32(rd["smallPrize"].ToString());
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            if (bigPrize > 0)
            {
                UpdateDatabaseWhenLuckyDrawIsWon(user.UserID, dayID, 1, bigPrize);
                return 1;
            }

            else if (mediumPrize > 0)
            {
                UpdateDatabaseWhenLuckyDrawIsWon(user.UserID, dayID, 2, mediumPrize);
                return 2;
            }

            else if (smallPrize > 0)
            {
                UpdateDatabaseWhenLuckyDrawIsWon(user.UserID, dayID, 3, smallPrize);
                return 3;
            }

            else
            {
                return 0;
            }
        }

        [NonAction]
        public static void UpdateDatabaseWhenLuckyDrawIsWon(int userID, int dayID, int code, int prizeNumber)
        {
            string type = "";

            switch (code)
            {
                case 1: 
                    type = "bigPrize"; 
                    break;

                case 2: 
                    type = "mediumPrize"; 
                    break;

                case 3: 
                    type = "smallPrize"; 
                    break;

                default: type = "bigPrize"; break;
            }
            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("UPDATE algorithm SET " + type + "= @prize WHERE dayID = @id");
            cmd.Parameters.Add("@id", MySqlDbType.Int16).Value = dayID;
            cmd.Parameters.Add("@prize", MySqlDbType.VarChar).Value = prizeNumber - 1;         
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            cn.Close();

            MySqlConnection cn2 = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn2.Open();

            MySqlCommand cmd2 = cn2.CreateCommand();
            cmd2.CommandType = CommandType.Text;
            cmd2.CommandText = String.Format("UPDATE user SET PrizeWon= @PrizeWon WHERE UserID = @UserID");
            cmd2.Parameters.Add("@UserID", MySqlDbType.Int16).Value = userID;
            cmd2.Parameters.Add("@PrizeWon", MySqlDbType.VarChar).Value = code;
            cmd2.ExecuteNonQuery();
            cmd2.Dispose();
            cn2.Close();

        }

        // Check if ic, project and unit clashes;
        [NonAction]
        public static Boolean DuplicateUserExists(Models.User user)
        {
            int count = 0;

            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();
            
            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT COUNT(UserID) AS userExists FROM user WHERE ICNumber = @ic AND Project = @project AND Unit = @unit");
            cmd.Parameters.Add("@ic", MySqlDbType.Int32).Value = user.ICNumber;
            cmd.Parameters.Add("@project", MySqlDbType.VarChar).Value = user.Project.ToUpper();
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
        public static List<Models.User> GetUserList()
        {
            List<Models.User> UserList = new List<Models.User>();
            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT * FROM user");
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Models.User user = new Models.User();
                user.UserID = Convert.ToInt32(rd["UserID"].ToString());
                user.Name = rd["Name"].ToString();
                user.ICNumber = rd["ICNumber"].ToString();
                user.EmailAddress = rd["EmailAddress"].ToString();
                user.ContactNumber = rd["ContactNumber"].ToString();
                user.Project = rd["Project"].ToString();
                user.Unit = rd["Unit"].ToString();
                user.SalesConsultant = rd["SalesConsultant"].ToString();
                user.SalesLocation = rd["SalesLocation"].ToString();
                user.PrizeWon = Convert.ToInt32(rd["PrizeWon"].ToString());
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
        public static List<Models.User> GetWinnerList()
        {
            List<Models.User> UserList = new List<Models.User>();
            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT * FROM user WHERE PrizeWon > 0");
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                Models.User user = new Models.User();
                user.UserID = Convert.ToInt32(rd["UserID"].ToString());
                user.Name = rd["Name"].ToString();
                user.ICNumber = rd["ICNumber"].ToString();
                user.EmailAddress = rd["EmailAddress"].ToString();
                user.ContactNumber = rd["ContactNumber"].ToString();
                user.Project = rd["Project"].ToString();
                user.Unit = rd["Unit"].ToString();
                user.SalesConsultant = rd["SalesConsultant"].ToString();
                user.SalesLocation = rd["SalesLocation"].ToString();
                user.PrizeWon = Convert.ToInt32(rd["PrizeWon"].ToString());
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
            MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
            cn.Open();

            MySqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = String.Format("SELECT * FROM user WHERE UserID = @id");
            cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = userID;
            MySqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                user.UserID = Convert.ToInt32(rd["UserID"].ToString());
                user.Name = rd["Name"].ToString();
                user.ICNumber = rd["ICNumber"].ToString();
                user.EmailAddress = rd["EmailAddress"].ToString();
                user.ContactNumber = rd["ContactNumber"].ToString();
                user.Project = rd["Project"].ToString();
                user.Unit = rd["Unit"].ToString();
                user.SalesConsultant = rd["SalesConsultant"].ToString();
                user.SalesLocation = rd["SalesLocation"].ToString();
                user.PrizeWon = Convert.ToInt32(rd["PrizeWon"].ToString());
                user.DateTime = rd["DateTime"].ToString();
            }

            rd.Close();
            cmd.Dispose();
            cn.Close();

            return user;
        }

        // Modify existing user;
        [NonAction]
        public void ModifyExistingUser(Models.User user)
        {
            try
            {
                MySqlConnection cn = new MySqlConnection(@"DataSource=localhost;Initial Catalog=luckydraw;User Id=root;Password=''");
                cn.Open();

                MySqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = String.Format("UPDATE user SET Name = @name, ICNumber = @ic, EmailAddress= @email, ContactNumber = @contact, Project = @project, Unit = @unit, SalesConsultant = @salesconsultant, SalesLocation = @saleslocation WHERE UserID = @id");
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = user.UserID;
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.Name.ToUpper();
                cmd.Parameters.Add("@ic", MySqlDbType.VarChar).Value = user.ICNumber;
                cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = user.EmailAddress.ToLower();
                cmd.Parameters.Add("@contact", MySqlDbType.VarChar).Value = user.ContactNumber;
                cmd.Parameters.Add("@project", MySqlDbType.VarChar).Value = user.Project.ToUpper();
                cmd.Parameters.Add("@unit", MySqlDbType.VarChar).Value = user.Unit.ToUpper();
                cmd.Parameters.Add("@salesconsultant", MySqlDbType.VarChar).Value = user.SalesConsultant.ToUpper();
                cmd.Parameters.Add("@saleslocation", MySqlDbType.VarChar).Value = user.SalesLocation.ToUpper();

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
    }
}