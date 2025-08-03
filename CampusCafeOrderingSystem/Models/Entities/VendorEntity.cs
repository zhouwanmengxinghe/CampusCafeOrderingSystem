using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.Entities
{
    public class VendorEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public virtual ICollection<VendorQualificationFileEntity> QualificationFiles { get; set; } = new List<VendorQualificationFileEntity>();
    }
}