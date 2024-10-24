using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloadManager// : SimpleManagerBase<PreloadManager>
{
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public static void ExecutePreload()
    {
        IPreloader[] preloaderList = GetComponentsInActiveScene<IPreloader>();
        if (preloaderList != null)
        {
            for (int j = 0; j < preloaderList.Length; j++)
            {
                preloaderList[j].OnExecutePreload();
            }
        }
    }
    
    // 通常trueしか指定しないのでデフォルト引数をtrueにしてます
    public static T[] GetComponentsInActiveScene<T>(bool includeInactive = true)
    {
        // ActiveなSceneのRootにあるGameObject[]を取得する
        var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        // 空の IEnumerable<T>
        IEnumerable<T> resultComponents = (T[])Enumerable.Empty<T>();
        foreach (var item in rootGameObjects)
        {
            // includeInactive = true を指定するとGameObjectが非活性なものからも取得する
            var components = item.GetComponentsInChildren<T>(includeInactive);
            resultComponents = resultComponents.Concat(components);
        }
        return resultComponents.ToArray();
    }

    // 1つだけ取得したい場合はこちら（GetComponentsInActiveSceneを元にして書いたので少し非効率です）
    public static T GetComponentInActiveScene<T>(bool includeInactive = true)
    {
        // ActiveなSceneのRootにあるGameObject[]を取得する
        var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        // 空の IEnumerable<T>
        IEnumerable<T> resultComponents = (T[])Enumerable.Empty<T>();
        foreach (var item in rootGameObjects)
        {
            // includeInactive = true を指定するとGameObjectが非活性なものからも取得する
            var components = item.GetComponentsInChildren<T>(includeInactive);
            resultComponents = resultComponents.Concat(components);
        }
        return resultComponents.First();
    }
}