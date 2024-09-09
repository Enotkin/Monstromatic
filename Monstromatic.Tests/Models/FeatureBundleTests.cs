using System.Collections.Generic;
using FluentAssertions;
using Monstromatic.Models;
using NUnit.Framework;

namespace Monstromatic.Tests.Models;

public class FeatureBundleTests
{
    [Test]
    public void AttackModificator_ReturnValueEqualAttackModifierFromSingleMonsterFeature()
    {
        //Arrage
        var features = new List<MonsterFeature>
        {
            new()
            {
                Key = "TestKey",
                DisplayName = "TestName",
                AttackModifier = 1
            }
        };
        var expectedValue = 1;
        
        var bundle = new FeaturesBundle(features);
        //Act
        var featureModificator = bundle.AttackModificator;
        //Assert
        featureModificator.Should().Be(expectedValue);
    }
    
    [Test]
    public void AttackModificator_ReturnSumAttackModifiersFromFeatures()
    {
        //Arrage
        var features = new List<MonsterFeature>
        {
            new()
            {
                Key = "TestKey",
                DisplayName = "TestName",
                AttackModifier = 1
            },
            new()
            {
                Key = "TestKey2",
                DisplayName = "TestName2",
                AttackModifier = 1
            }
        };
        const int expectedValue = 2;
        
        var bundle = new FeaturesBundle(features);
        //Act
        var featureModificator = bundle.AttackModificator;
        //Assert
        featureModificator.Should().Be(expectedValue);
    }
}