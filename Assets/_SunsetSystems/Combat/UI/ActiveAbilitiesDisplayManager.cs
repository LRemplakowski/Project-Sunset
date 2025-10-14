using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SunsetSystems.Combat.UI
{
    //public class ActiveAbilitiesDisplayManager : SerializedMonoBehaviour
    //{
    //    [Title("References")]
    //    [SerializeField]
    //    private Transform buttonsParent;

    //    private List<GameObject> buttonInstances = new();

    //    public void ShowAbilities(ICombatant currentActor)
    //    {
    //        //buttonInstances.ForEach(b => Addressables.ReleaseInstance(b));
    //        //buttonInstances.Clear();
    //        //var knownPowers = currentActor.References.AbilityUser.GetNonCoreAbilities();
    //        //foreach (var ability in knownPowers)
    //        //{
    //        //    Task<GameObject> buttonTask = Addressables.InstantiateAsync(ability.GetAbilityUIData().GetAbilityGUIButtonAsset(), buttonsParent).Task;
    //        //    _ = Task.Run(async () =>
    //        //    {
    //        //        await buttonTask;
    //        //        buttonInstances.Add(buttonTask.Result);
    //        //    });
    //        //}
    //    }
    //}
}
