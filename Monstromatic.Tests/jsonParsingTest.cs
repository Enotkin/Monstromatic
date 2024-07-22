using System.Collections.Generic;
using Monstromatic.Data;
using Monstromatic.Data.Storages;
using Monstromatic.Models;
using NUnit.Framework;

namespace Monstromatic.Tests;

public class jsonParsingTest
{
    [Test]
    public void Foo()
    {
        var fileName = @"C:\Users\User\AppData\Roaming\Monstromatic\features.json";
        
        var ss = new FileBaseDataStorage<List<MonsterFeature>>(fileName);

        var result = ss.Read();
    }
}