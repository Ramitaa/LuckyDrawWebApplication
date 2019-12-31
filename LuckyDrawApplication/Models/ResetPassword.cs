using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LuckyDrawApplication.Models
{
    public class ResetPassword
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public String NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        [Display(Name = "Confirm Password")]
        public String ConfirmPassword { get; set; }

        [Display(Name = "Email Address")]
        public String EmailAddress { get; set; }

        [Display(Name = "Token")]
        public String Token { get; set; }
    }
}