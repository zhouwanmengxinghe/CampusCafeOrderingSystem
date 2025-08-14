// /Services/AppSettingsStore.cs
using CampusCafeOrderingSystem.Models;

namespace CampusCafeOrderingSystem.Services
{
    /// <summary>
    /// 演示用的内存“持久化”，以后可替换为数据库或 IOptions 配置
    /// </summary>
    public static class AppSettingsStore
    {
        private static SettingsViewModel _current = new();

        public static SettingsViewModel Load() => new SettingsViewModel
        {
            SystemName = _current.SystemName,
            Currency = _current.Currency,
            OrderTimeoutMinutes = _current.OrderTimeoutMinutes,
            EnableEmail = _current.EnableEmail,
            EnableSms = _current.EnableSms,
            DailyReportTime = _current.DailyReportTime,
            DefaultTimeZone = _current.DefaultTimeZone,
            MaintenanceMode = _current.MaintenanceMode
        };

        public static void Save(SettingsViewModel m) => _current = m;

        public static void Reset() => _current = new SettingsViewModel();
    }
}
