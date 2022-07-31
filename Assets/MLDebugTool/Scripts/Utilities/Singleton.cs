using UnityEngine;

namespace MLDebugTool.Scripts.Utilities
{
    /// <summary>
    /// Creates a singleton reference, but does not create new instance when called.
    /// </summary>
    /// <typeparam name="T">The type of the instance reference</typeparam>
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T instance = null;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType(typeof(T)) as T;
                }
                
                return instance;
            }
        }

        protected virtual void Awake()
        {
            instance = Instance;
        }
    }
}