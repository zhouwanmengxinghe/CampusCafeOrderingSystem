using System;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models.Entities
{
    public class VendorQualificationFileEntity
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public DateTime UploadedAt { get; set; }
        public virtual VendorEntity Vendor { get; set; }
    }
}