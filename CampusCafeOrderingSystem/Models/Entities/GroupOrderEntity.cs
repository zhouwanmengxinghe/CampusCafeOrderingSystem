using System;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.Entities
{
    public class GroupOrderEntity
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string RequesterEmail { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
    }
}