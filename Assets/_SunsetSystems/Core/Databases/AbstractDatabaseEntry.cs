using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace SunsetSystems.Core.Database
{
    public abstract class AbstractDatabaseEntry<T> : SerializedScriptableObject, IDatabaseEntry<T>
    {
        public abstract string DatabaseID { get; protected set; }
        [LabelText("Data Script Name")]
        public abstract string ReadableID { get; protected set; }

        protected abstract void RegisterToDatabase();
        protected abstract void UnregisterFromDatabase();


        protected virtual void Awake()
        {
#if UNITY_EDITOR
            if (string.IsNullOrWhiteSpace(DatabaseID))
            {
                AssignNewID();
            }
            if (string.IsNullOrEmpty(ReadableID))
                ReadableID = name;
            if (string.IsNullOrWhiteSpace(DatabaseID) == false)
                RegisterToDatabase();
#endif
        }

        [Button("Force Validate")]
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (string.IsNullOrWhiteSpace(DatabaseID))
            {
                AssignNewID();
            }
            if (string.IsNullOrWhiteSpace(DatabaseID) == false)
            {
                try
                {
                    RegisterToDatabase();
                }
                catch (NullReferenceException)
                {
                    UnityEngine.Debug.LogError($"DatabaseEntry >>> Could not register {this} to Database!", this);
                }
            }
#endif
        }

        protected virtual void Reset()
        {
#if UNITY_EDITOR
            AssignNewID();
#endif
        }

        protected virtual void OnDestroy()
        {
#if UNITY_EDITOR
            UnregisterFromDatabase();
#endif
        }

        [Button("Randomize ID")]
        private void AssignNewID()
        {
#if UNITY_EDITOR
            DatabaseID = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
