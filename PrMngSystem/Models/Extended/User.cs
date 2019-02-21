using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PrMngSystem.Models
{
    [MetadataType(typeof(UserMetaData))] //apply valdiation model 
    public partial class User
    {
        //for adding validations 
        public string confirmPassword { get; set; }

    }

    public class UserMetaData
    {
        [Display(Name = "Username")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Username is required")]
        public string username { get; set; }

        [Display(Name = "First name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name is required")]
        public string name { get; set; }

        [Display(Name = "Last name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name is required")]
        public string surname { get; set; }

        [Display(Name = "Email")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }

        [Display(Name = "Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        [DataType(DataType.Password)] //this is a code otherwise it will show plain text I want to shoe password like *******
        [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
        public string password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)] //this is a code otherwise it will show plain text I want to shoe password like *******
        [Compare("password", ErrorMessage = "Confirm password and password do not match")]
        public string confirmPassword { get; set; }

    }
}