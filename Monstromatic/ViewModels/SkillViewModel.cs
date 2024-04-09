using Monstromatic.Models;
using ReactiveUI;


namespace Monstromatic.ViewModels;

public class SkillViewModel : ViewModelBase
{
    private readonly Skill _skill;

    public SkillViewModel(string name, int level, int featuresModificator)
    {
        _skill = new Skill(name, level, featuresModificator);
    }

    public string Name => _skill.Name;

    public int SkillValue
    {
        get => _skill.Value;
        set
        {
            _skill.SetModificator(value);
            this.RaisePropertyChanged(nameof(SkillValue));
        }
    }
}