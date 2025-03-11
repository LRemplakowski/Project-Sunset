using System.Linq;
using SunsetSystems.Core;
using UnityEngine;

public static class UniqueUtility
{
    public static T FindFirstWithID<T>(string withID, FindObjectsInactive includeInactive = FindObjectsInactive.Exclude) where T : MonoBehaviour, IUnique
    {
        return Object.FindObjectsByType<T>(includeInactive, FindObjectsSortMode.None).FirstOrDefault(mb => mb is IUnique unique && unique.GetID() == withID);
    }

    public static bool TryFindFirstWithID<T>(string withID, out T found, FindObjectsInactive includeInactive = FindObjectsInactive.Exclude) where T : MonoBehaviour, IUnique
    {
        found = FindFirstWithID<T>(withID, includeInactive);
        return found != null;
    }
}
