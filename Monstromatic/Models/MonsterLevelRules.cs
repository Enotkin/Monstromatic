using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Monstromatic.Models;

public static class MonsterLevelRules
{
    public static void ValidateBaseLevel(int level)
    {
        ValidateEvenLevel(level);
    }

    public static void ValidateEvenLevel(int level)
    {
        if (level % 2 != 0)
            throw new ValidationException("Monster level must be even.");
    }
}
