using UnityEngine;

namespace Core
{
    public abstract class SimpleManagerBase<T> : MonoBehaviour where T : class
    {
        protected static T MyInstance;

        public SimpleManagerBase()
        {
            MyInstance = this as T;
        }

        public static T Instance
        {
            get
            {
                if (MyInstance == null)
                {
                    MyInstance = new GameObject("@" + typeof(T).Name, typeof(T)).GetComponent<T>();
                }
                return MyInstance;
            }
        }
        
        private void Awake()
        {
            AwakeSetting();
        }
        
        private void OnDestroy()
        {
            //if(_instance != (this as T)) return;
            OnDestroyProcess();
            Dispose();
        }
        
        protected virtual void OnDestroyProcess(){
        }
        
        public virtual void Dispose()
        {
            MyInstance = null;
        }

        protected virtual void AwakeSetting() { }

        public static bool IsValidInstance()
        {
            return (MyInstance != null);
        }
    }
}