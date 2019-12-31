using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LuckyDrawApplication.Models
{
    public class Admin
    {
        [Display(Name = "ID")]
        public int ID { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [MaxLength(50, ErrorMessage = "Maximum length of 50")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Minumum length of 8")]
        [MaxLength(15, ErrorMessage = "Maximum length of 15")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Password Hash")]
        public string PasswordHash { get; set; }

        [Display(Name = "Salt")]
        public String Salt { get; set; }

        [Required]
        [Display(Name = "Event ID")]
        public int EventID { get; set; }


    }
}