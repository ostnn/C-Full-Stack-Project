using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HobbyHub.Models
{
    public class LoginUser
    {
        [Required]
        public string LoginUserName {get;set;}
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string LoginPassword {get;set;}
    }
}