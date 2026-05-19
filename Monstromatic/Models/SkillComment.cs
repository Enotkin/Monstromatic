namespace Monstromatic.Models;

public class SkillComment
{
    public SkillComment(string featureName, string text)
    {
        FeatureName = featureName;
        Text = text;
    }

    public string FeatureName { get; }

    public string Text { get; }
}
