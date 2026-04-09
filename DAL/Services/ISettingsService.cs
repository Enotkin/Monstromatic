using DAL.Models.Settings;

namespace DAL.Services;

public interface ISettingsService
{
    AppSettings Settings { get; }
}
