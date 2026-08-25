using System.Collections.Generic;
using System.Collections.ObjectModel;
using Monstromatic.Models;
using ReactiveUI;

namespace Monstromatic.ViewModels;

public class ProfileSelectionViewModel : ViewModelBase
{
    private Profile? _selectedProfile;
    private bool _rememberChoice;

    public ProfileSelectionViewModel(IEnumerable<Profile> profiles, string? rememberedProfileId)
    {
        Profiles = new ObservableCollection<Profile>(profiles);

        _selectedProfile = Profiles.Count > 0 ? Profiles[0] : null;
        foreach (var profile in Profiles)
        {
            if (profile.Id == rememberedProfileId)
            {
                _selectedProfile = profile;
                _rememberChoice = true;
            }
        }
    }

    public ObservableCollection<Profile> Profiles { get; }

    public Profile? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedProfile, value);
            this.RaisePropertyChanged(nameof(CanOpen));
        }
    }

    public bool RememberChoice
    {
        get => _rememberChoice;
        set => this.RaiseAndSetIfChanged(ref _rememberChoice, value);
    }

    public bool CanOpen => SelectedProfile is not null;

    public void Add(Profile profile)
    {
        Profiles.Add(profile);
        SelectedProfile = profile;
    }
}
