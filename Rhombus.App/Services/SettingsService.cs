using System.IO;
using System.Text.Json;
using Rhombus.App.Models;
using Rhombus.App.ViewModels;

namespace Rhombus.App.Services;

public class SettingsService
{
    private readonly string _dir;
    private readonly string _file;

    public string DefaultDownloadDirectory { get; }

    public SettingsService()
    {
        _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "rhombus");
        Directory.CreateDirectory(_dir);
        _file = Path.Combine(_dir, "settings.json");
        DefaultDownloadDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "rhombus");
    }

    public void Save(IEnumerable<SoundViewModel> vms)
    {
        var list = vms.Select(v => v.Model).ToList();
        File.WriteAllText(_file, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
    }

    public List<Sound> Load()
    {
        if (!File.Exists(_file)) return new();
        return JsonSerializer.Deserialize<List<Sound>>(File.ReadAllText(_file)) ?? new();
    }
}
