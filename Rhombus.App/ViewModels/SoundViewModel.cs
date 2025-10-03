using System;
using System.Windows.Input;
using Rhombus.App.Models;
using Rhombus.App.Services;

namespace Rhombus.App.ViewModels;

public class SoundViewModel
{
    public Sound Model { get; }
    private readonly AudioPlaybackService _audio;
    private readonly GlobalHotkeyService _hotkeys;
    private readonly Action<SoundViewModel> _remove;

    public string DisplayName => Model.DisplayName;
    public string HotkeyLabel => string.IsNullOrWhiteSpace(Model.Hotkey) ? "(no hotkey)" : Model.Hotkey!;

    public ICommand PlayCommand { get; }
    public ICommand BindCommand { get; }
    public ICommand RemoveCommand { get; }

    public SoundViewModel(Sound model, AudioPlaybackService audio, GlobalHotkeyService hotkeys, Action<SoundViewModel> remove)
    {
        Model = model;
        _audio = audio;
        _hotkeys = hotkeys;
        _remove = remove;

        PlayCommand = new RelayCommand(() => _audio.Play(Model.Path));
        BindCommand = new RelayCommand(BindHotkey);
        RemoveCommand = new RelayCommand(() => _remove(this));
    }

    private void BindHotkey()
    {
        var input = Microsoft.VisualBasic.Interaction.InputBox("Enter hotkey (e.g. Ctrl+Alt+1)", "Bind Hotkey", Model.Hotkey ?? "Ctrl+Alt+1");
        if (string.IsNullOrWhiteSpace(input)) return;

        if (!string.IsNullOrWhiteSpace(Model.Hotkey))
            _hotkeys.Unbind(Model.Hotkey!);

        Model.Hotkey = input.Trim();
        _hotkeys.Bind(Model.Hotkey!, () => _audio.Play(Model.Path));
    }
}
