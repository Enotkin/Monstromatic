using FluentAssertions;
using Monstromatic.Models;
using NUnit.Framework;

namespace Monstromatic.Tests.Models;

[TestFixture]
public class SkillTests
{
    [Test]
    public void CreateNew_Skill_ReturnSumOfLevelAndFeatureModificator()
    {
        //Arrange
        const int level = 2;
        const int featureModificator = 0;
        const int expectedValue = level + featureModificator;
        
        //Act
        var skill = new Skill("TestName", level, featureModificator);

        //Assert
        skill.Value.Should().Be(expectedValue);
    }

    [Test]
    public void CreateNew_SkillWithNegativeFeatureModificator_ReturnMinimumValue()
    {
        //Arrange
        const int level = 1;
        const int featureModificator = -1;
        
        //Act
        var skill = new Skill("TestName", level, featureModificator);
        
        //Assert
        skill.Value.Should().Be(1);
    }
    
    [Test]
    public void Increment_Skill_ReturnIncrementedValue()
    {
        //Arrange
        const int level = 2;
        const int featureModificator = 0;
        const int expectedValue = level + featureModificator + 1;
        
        var skill = new Skill("TestName", level, featureModificator);
        
        //Act
        skill.Increment();

        //Assert
        skill.Value.Should().Be(expectedValue);
    }
    
    [Test]
    public void Decrement_Skill_ReturnDecrementedValue()
    {
        //Arrange
        const int level = 2;
        const int featureModificator = 0;
        const int expectedValue = level + featureModificator - 1;
        
        var skill = new Skill("TestName", level, featureModificator);
        
        //Act
        skill.Decrement();

        //Assert
        skill.Value.Should().Be(expectedValue);
    }

    [Test]
    public void Decrement_Skill_ValueCantBeLess1()
    {
        //Arrange
        const int level = 1;
        const int featureModificator = 0;
        
        var skill = new Skill("TestName", level, featureModificator);
        
        //Act
        skill.Decrement();
        
        //Assert
        skill.Value.Should().Be(1);
    }
}