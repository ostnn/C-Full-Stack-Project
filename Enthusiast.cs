using System;
using System.ComponentModel.DataAnnotations;

namespace HobbyHub.Models
{
    public class Enthusiast
    {
        [Key]
        public int EnthusiastId {get;set;}
        public int UserId {get;set;}
        public User Creator {get;set;}
        public int HobbyId {get;set;}
        public Hobby Hobby {get;set;}
    }
}