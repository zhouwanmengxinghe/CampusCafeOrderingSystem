using System;
using System.ComponentModel.DataAnnotations;

namespace CampusCafeOrderingSystem.Models;

public class SettingsViewModel
{
    [Required, Display(Name = "System Name")]
    [StringLength(100)]
    public string SystemName { get; set; } = "Campus Cafe Ordering System";

    [Required, Display(Name = "Default Currency")]
    [RegularExpression(@"^(NZD|USD|AUD|CNY|EUR)$")]
    public string Currency { get; set; } = "NZD";

    [Required, Range(5, 240, ErrorMessage = "Order timeout must be between 5 and 240 minutes.")]
    [Display(Name = "Order Timeout (minutes)")]
    public int OrderTimeoutMinutes { get; set; } = 30;

    [Display(Name = "Enable Email Notifications")]
    public bool EnableEmail { get; set; } = true;

    [Display(Name = "Enable SMS Notifications")]
    public bool EnableSms { get; set; } = false;

    [Display(Name = "Daily Report Time")]
    public TimeSpan? DailyReportTime { get; set; } = new TimeSpan(9, 0, 0);

    [Display(Name = "Default Time Zone")]
    [StringLength(80)]
    public string DefaultTimeZone { get; set; } = "Pacific/Auckland";

    [Display(Name = "Maintenance Mode")]
    public bool MaintenanceMode { get; set; } = false;
}
