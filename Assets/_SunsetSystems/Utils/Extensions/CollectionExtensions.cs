using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SunsetSystems.Combat;
using SunsetSystems.Entities;
using UnityEngine;

namespace SunsetSystems.Utils.Extensions
{
    public static class CollectionExtensions
    {
        public static T GetRandom<T>(this IEnumerable<T> enumerable)
        {
            int enumerableCount = enumerable.Count();
            if (enumerableCount <= 0)
            {
                Debug.LogWarning($"Tried to get random element of type {nameof(T)}, but collection {enumerable} is empty!");
                return default;
            }

            int itemIndex = Random.Range(0, enumerableCount);
            Debug.Log($"Got random item from collection {enumerable}! Item index: {itemIndex}");
            var elementAtIndex = enumerable.ElementAt(itemIndex);
            if (elementAtIndex == null)
                Debug.Break();
            return elementAtIndex;
        }

        public static T GetNearest<T>(this IEnumerable<T> enumerable, Vector3 position) where T : IContextProvider<ITargetableContext>
        {
            T nearest = default;
            float nearestDistanceSqr = float.MaxValue;
            foreach (var element in enumerable)
            {
                float distanceSqr = (element.GetContext().Transform.position - position).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearest = element;
                    nearestDistanceSqr = distanceSqr;
                }
            }
            return nearest;
        }
    }
}