using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunsetSystems.Spellbook
{
    public interface IMagicUser
    {
        void UsePower(DisciplinePower power, IMagicUser castingActor);
        void UsePowerAfterTargetSelection(DisciplinePower power);
    }
}
