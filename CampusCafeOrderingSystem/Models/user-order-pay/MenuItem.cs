using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CafeApp.Models
{
    public class MenuItem
    {
        public int Id {get; set;}

        [Required]
        public string Name { get; set; } = string.Empty;
    
        public string Description {get; set;}=string.Empty;

        [Required]
        public decimal Price {get;set;}

        public string ImageUrl {get;set;}= string.Empty; 
    }
   
}
