using SunsetSystems.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Extensions
{
    public static float GetPathLength(this NavMeshPath path)
    {
        // Calculate path length
        float pathLength = 0;
        for (int i = 1; i < path.corners.Length; i++)
        {
            pathLength += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return pathLength;
    }

    public static bool IsNullOrEmpty<T>(this T[] array)
    {
        if (array == null || array.Length == 0)
            return true;
        else
            return Array.Exists(array, element => element != null);
    }

    public static void DestroyChildren(this Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.Equals(transform))
                continue;
            else
                UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    public static void DestroyChildren(this Transform transform, ICollection<Transform> except)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.Equals(transform) || except.Contains(child))
                continue;
            else
                UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    public static void DestroyChildren(this Transform transform, Transform except)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.Equals(transform) || except.Equals(child))
                continue;
            else
                UnityEngine.Object.Destroy(child.gameObject);
        }
    }

    public static void DestroyChildrenImmediate(this Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.Equals(transform))
                continue;
            else
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }
}