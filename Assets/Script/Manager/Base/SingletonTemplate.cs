using UnityEngine;
using UnityEngine.Serialization;

public abstract class SingletonTemplate<T> : MonoBehaviour where T : class
{
    protected static T MyInstance;

    [FormerlySerializedAs("_dontDestroy")] [SerializeField]
    private bool dontDestroy = true;

    public SingletonTemplate()
    {
        MyInstance = this as T;
    }

    public SingletonTemplate(bool bDontDestroy)
    {
        MyInstance = this as T;
        dontDestroy = bDontDestroy;
    }

    public static T Instance
    {
        get
        {
            if(MyInstance == null)
            {
                MyInstance = new GameObject("@" + typeof(T).Name.ToString(), typeof(T)).GetComponent<T>();
            }
            return MyInstance;
        }
    }
    private void Awake()
    {
        if (dontDestroy)
        {
            DontDestroyOnLoad(FindTransformRoot(gameObject.transform));
        }
        AwakeSetting();
    }

    protected virtual void AwakeSetting() { }

    // 최상위 루트 찾기
    protected Transform FindTransformRoot(Transform target)
    {
        Transform result = target;
        if (result.parent != null)
        {
            result = FindTransformRoot(result.parent);
        }
        return result;
    }
}