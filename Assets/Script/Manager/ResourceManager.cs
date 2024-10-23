using System.Collections.Generic;
using Core.Library;
using UnityEngine;

public partial class ResourceManager : SingletonTemplate<ResourceManager>
{
    Dictionary<string, Object> cachedResources = new Dictionary<string, Object>();

    string localPath = "Assets/Resources";

    public bool _isLoading = false;

    public void addCachedResource(string key, Object data)
    {
        this.cachedResources.Add(key, data);
    }

    /// <summary>
    /// 리소스 로드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pathData"></param>
    /// <param name="fileName"></param>
    /// <param name="isCache"></param>
    /// <param name="KeyCallback"></param>
    /// <returns></returns>
    public T Load<T>(ResourcePathData pathData, string fileName, bool isCache = true, System.Action<string> KeyCallback = null) where T : Object
    {
        T obj = default(T);
        string key = null;
        key = GetResourceKey(pathData, fileName);

        //일반 번들 캐싱 되어있음
        if (cachedResources.ContainsKey(key))
        {
            obj = cachedResources[key] as T;
        }
        else //일반 번들 캐싱 안되어있음
        {
            if (obj == null)
            {
                obj = LoadFile<T>(pathData.m_Path, fileName, key);
                #if UNITY_EDITOR
               // Debug.Log($"Load : Path[{pathData.m_Path}], fileName[{fileName}]");
                #endif
                if (obj != null)
                {
                    /* LogManager.Instance.ShowLog<ResourceLogData>(eLogType.Log, pathData.GetPath(), fileName, obj.GetType().ToString());
 */
                    cachedResources.Add(key, obj);
                }
                else
                {
#if UNITY_EDITOR
                    CodeUtilLibrary.SetColorLog(string.Format("{0} 경로에 {1}파일이 없습니다.", pathData.GetPath(), fileName), "magenta");
                    //Debug.LogError(string.Format("{0} 경로에 {1}파일이 없습니다.", pathData.GetPath(), fileName));
#endif
                }
            }
        }

        KeyCallback?.Invoke(key);
        return obj;
    }

    public T[] LoadFolder<T>(ResourcePathData pathData) where T : Object
    {
        return Resources.LoadAll<T>(pathData.m_Path);
    }

    private T LoadFile<T>(string path, string fileName, string key) where T : Object
    {
        T obj = default;
        obj = LoadBuiltInFile<T>(key);

        if (obj == null)
        {
            string assetKey = string.Format("{0}{1}", path, fileName);
            obj = LoadLocalFile<T>(assetKey);
        }


        return obj;
    }

    ///통빌드 변경이슈로 어드레서블 사용안함
/*    private T LoadFile<T>(string path, string fileName, string key) where T : Object
    {
        T obj = default;

#if USE_ADDRESSABLE
        obj = LoadBuiltInFile<T>(key);

        if (obj == null)
        {
            obj = LoadAddressFile<T>(path, fileName);
        }
#else
#if USE_BUNDLE
        obj = LoadBundleFile<T>(path, fileName);
#elif UNITY_EDITOR
        obj = LoadBuiltInFile<T>(key);

        if (obj == null)
        {
            obj = LoadAddressFile<T>(path, fileName);
        }
#endif
        if (obj == null)
        {
            obj = LoadBuiltInFile<T>(key);
        }
#endif

        return obj;
    }*/

    /// <summary>
    /// Resources폴더에서 파일을 불러온다
    /// </summary>
    private T LoadBuiltInFile<T>(string key) where T : Object
    {
        T obj = Resources.Load<T>(key);
        return obj;
    }

    /*
    public T LoadAssetSynchronously<T>(string assetKey) where T : Object
    {
        // 먼저, 키에 해당하는 리소스 위치들이 존재하는지 확인합니다.
        var locationHandle = Addressables.LoadResourceLocationsAsync(assetKey, typeof(T));
        locationHandle.WaitForCompletion();
        if (locationHandle.Result.Count <= 0)
        {
            // 주어진 키에 해당하는 리소스 위치가 없다면 로드를 시도하지 않습니다.
           // Debug.LogError($"Invalid key: {assetKey}. No resource locations found.");
            return null;
        }

        // 리소스 위치가 있다면, 에셋을 로드합니다.
        var assetHandle = Addressables.LoadAssetAsync<T>(assetKey);
        assetHandle.WaitForCompletion();
        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            // 성공적으로 로드된 에셋을 반환합니다.
            return assetHandle.Result;
        }
        else
        {
            // 에셋 로드에 실패했을 경우 에러를 로그합니다.
            Debug.LogError($"Failed to load asset with key {assetKey}: {assetHandle.OperationException}");
            return null;
        }
    }

    public T LoadAddressFile<T>(string path, string fileName) where T : UnityEngine.Object
    {
        string assetKey = string.Format("{0}{1}.prefab", path, fileName);
        T obj = LoadAssetSynchronously<T>(assetKey);


        if(obj == null)
        {
            assetKey = string.Format("{0}{1}.mat", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }
        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.bytes", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }

        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.spriteatlas", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }

        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.mixer", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }

        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.ogg", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }
        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.wav", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }
        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.mp3", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }
        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.mp4", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }
        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.png", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }
        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.txt", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }
        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.otf", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }
        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.json", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }
        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.atlas", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }

        if (obj == null)
        {
            assetKey = string.Format("{0}{1}.asset", path, fileName);
            obj = LoadAssetSynchronously<T>(assetKey);
        }

        if(obj == null)
        {
            Debug.LogErrorFormat(" 해당 리소스를 찾을수없습니다. {0}", assetKey);
        }
        return obj;
    }
    */

    /// <summary>
    /// 지역변수 localPath에 해당하는 경로에서 파일을 불러온다(에디터 전용)
    /// </summary>
    private T LoadLocalFile<T>(string key) where T : Object
    {

        T obj = default;

        //아래 코드 바꾸고싶은데 급함... 

        //프리팹 탐색
        obj = Resources.Load<T>(string.Format("{0}/{1}.prefab", localPath, key));

        //Bytes 탐색.                                                                     
        if (null == obj)
            obj = Resources.Load<T>(string.Format(string.Format("{0}/{1}.mat", localPath, key)));

        //Bytes 탐색.                                                                     
        if (null == obj)
            obj = Resources.Load<T>(string.Format(string.Format("{0}/{1}.bytes", localPath, key)));

        if (null == obj)
            obj = Resources.Load<T>(string.Format(string.Format("{0}/{1}.spriteatlas", localPath, key)));

        //사운드 탐색.
        if (null == obj)
            obj = Resources.Load<T>(string.Format(string.Format("{0}/{1}.ogg", localPath, key)));
        if (null == obj)
            obj = Resources.Load<T>(string.Format(string.Format("{0}/{1}.wav", localPath, key)));
        //영상 탐색
        if (null == obj)
            obj = Resources.Load<T>(string.Format(string.Format("{0}/{1}.mp4", localPath, key)));

        //이미지 탐색.                                                                     
        if (null == obj)
            obj = Resources.Load<T>(string.Format(string.Format("{0}/{1}.png", localPath, key)));

        //텍스트 탐색.                                                                     
        if (null == obj)
            obj = Resources.Load<T>(string.Format(string.Format("{0}/{1}.txt", localPath, key)));

        //폰트 탐색.                                                                 
        if (null == obj)
            obj = Resources.Load<T>(string.Format(string.Format("{0}/{1}.otf", localPath, key)));

        //json 테이블 탐색                                                                   
        if (null == obj)
            obj = Resources.Load<T>(string.Format(string.Format("{0}/{1}.json", localPath, key)));

        return obj;
    }

    /// <summary>
    /// 조건에 맞게 리소스의 키를 리턴
    /// </summary>
    /// <param name="pathData"></param>
    /// <param name="fileName"></param>
    /// <param name="isAdult"></param>
    /// <returns></returns>
    public string GetResourceKey(ResourcePathData pathData, string fileName, bool isAdult = false)
    {

        return GetResourceKey(pathData.m_Path, fileName);
    }
    /// <returns>
    /// 조건에 맞는 리소스의 키</returns>
    public string GetResourceKey(string path, string fileName)
    {
        return string.Format("{0}{1}", path, fileName);
    }
}

public enum BundleGroupingType
{
    Default,
    Count, // 특정 갯수만큼 묶기
    Folder,
}

public class ResourcePathData
{
    public string m_Path;
    public string m_adultPath;
    public BundleGroupingType m_bundleGroupingType;
    public int m_groupingCount;
    public bool m_isResourceAssets;

    public ResourcePathData(string local, string adultPath = null, bool isBundle = true, BundleGroupingType bundleGroupingType = BundleGroupingType.Default, int groupingCount = 0)
    {
        this.m_Path = local;
        m_adultPath = adultPath;
        m_isResourceAssets = isBundle;
        m_bundleGroupingType = bundleGroupingType;
        m_groupingCount = groupingCount;
    }

    public string GetPath()
    {
        return m_Path;
    }
}

public class ResourcePath
{
    public static ResourcePathData SpriteTest = new ResourcePathData("stage1-5/anim/");
    /*
    public static ResourcePathData Effect_Material = new ResourcePathData("Effects/Effect_Material/");
    public static ResourcePathData Effect_UI = new ResourcePathData("Effects/Effect_Material/");
    public static ResourcePathData Effect_Character = new ResourcePathData("Effects/Prefab/Effect_Prefab_Character/", bundleGroupingType: BundleGroupingType.Count, groupingCount: 500);

    //UI 프리팹
    public static ResourcePathData Prefab_UI = new ResourcePathData("Prefabs/UI/");
    public static ResourcePathData Prefab_UI_Recruit = new ResourcePathData("Prefabs/UI/Recruit/");

    public static ResourcePathData Prefab_UI_Tween = new ResourcePathData("Prefabs/UI/Tween/");
    public static ResourcePathData Prefab_UI_View = new ResourcePathData("Prefabs/UI/View/");
    public static ResourcePathData Prefab_UI_Effect = new ResourcePathData("Prefabs/UI/Effects/");
    public static ResourcePathData Prefab_Challenges_Prefab = new ResourcePathData("Prefabs/UI/Popup/Challenges/Prefab/");
    public static ResourcePathData Prefab_Shop_Prefab = new ResourcePathData("Prefabs/UI/Popup/Shop/Prefab/");
    public static ResourcePathData Prefab_Unit_Buff = new ResourcePathData("Prefabs/Unit/Buff/");
    public static ResourcePathData Prefab_UI_View_SelectStage = new ResourcePathData("Prefabs/UI/View/SelectStage/");


    //팝업
    public static ResourcePathData Prefab_Popup_Common = new ResourcePathData("Prefabs/UI/Popup/Common/");
    public static ResourcePathData Prefab_Popup_Attendence = new ResourcePathData("Prefabs/UI/Popup/Attendence/");
    public static ResourcePathData Prefab_Popup_Attendence_Prefab = new ResourcePathData("Prefabs/UI/Popup/Attendence/Prefab");
    public static ResourcePathData Prefab_Popup_Bag = new ResourcePathData("Prefabs/UI/Popup/Bag/");
    public static ResourcePathData Prefab_Popup_Bag_Prefab = new ResourcePathData("Prefabs/UI/Popup/Bag/Prefab/");
    public static ResourcePathData Prefab_Popup_Login = new ResourcePathData("Prefabs/UI/Popup/Login/");
    public static ResourcePathData Prefab_Popup_Party = new ResourcePathData("Prefabs/UI/Popup/PartyPreset/");
    public static ResourcePathData Prefab_Popup_Party_Prefab = new ResourcePathData("Prefabs/UI/Popup/PartyPreset/Prefab/");
    public static ResourcePathData Prefab_Popup_Profile = new ResourcePathData("Prefabs/UI/Popup/Profile/");
    public static ResourcePathData Prefab_Popup_Shop = new ResourcePathData("Prefabs/UI/Popup/Shop/");
    public static ResourcePathData Prefab_Popup_Storage = new ResourcePathData("Prefabs/UI/Popup/Storage/");
    public static ResourcePathData Prefab_Popup_SimpleInventory = new ResourcePathData("Prefabs/UI/Popup/SimpleInventory/");
    public static ResourcePathData Prefab_Popup_Stronger = new ResourcePathData("Prefabs/UI/Popup/Stronger/");
    public static ResourcePathData Prefab_Popup_Task = new ResourcePathData("Prefabs/UI/Popup/Challenges/");
    public static ResourcePathData Prefab_Popup_Story = new ResourcePathData("Prefabs/UI/Popup/Story/");
    public static ResourcePathData Prefab_Popup_Mercenary = new ResourcePathData("Prefabs/UI/Popup/Mercenary/");
    public static ResourcePathData Prefab_Popup_Mercenary_Prefab = new ResourcePathData("Prefabs/UI/Popup/Mercenary/Prefab/");
    public static ResourcePathData Prefab_Tutorial = new ResourcePathData("Prefabs/UI/Tutorial/");
    public static ResourcePathData Prefab_Popup_Loading = new ResourcePathData("Prefabs/UI/Popup/Loading/");
    public static ResourcePathData Prefab_Popup_Recruit = new ResourcePathData("Prefabs/UI/Recruit/");
    public static ResourcePathData Prefab_Popup_InGame = new ResourcePathData("Prefabs/UI/InGame/");
    public static ResourcePathData Prefab_Popup_InGameSkill = new ResourcePathData("Prefabs/UI/InGame/Skill/");
    public static ResourcePathData Prefab_Popup_Adventure = new ResourcePathData("Prefabs/UI/Adventure/");
    public static ResourcePathData Prefab_Popup_DailyDungeon = new ResourcePathData("Prefabs/UI/DailyDungeon/");
    public static ResourcePathData Prefab_Popup_InfinityTower = new ResourcePathData("Prefabs/UI/InfinityTower/");
    public static ResourcePathData Prefab_Popup_Mail = new ResourcePathData("Prefabs/UI/Mail/");
	public static ResourcePathData Prefab_Popup_Explore = new ResourcePathData("Prefabs/UI/Popup/Explore/");
    public static ResourcePathData Prefab_Popup_MainTopMenu = new ResourcePathData("Prefabs/UI/Popup/MainTopMenu/");
    public static ResourcePathData Prefab_Popup_BattlePass = new ResourcePathData("Prefabs/UI/Popup/BattlePass/");
    public static ResourcePathData Prefab_Popup_Event = new ResourcePathData("Prefabs/UI/Popup/Event/");
    public static ResourcePathData Prefab_Popup_Common_Info = new ResourcePathData("Prefabs/UI/Common/Info/");
    public static ResourcePathData Prefab_Popup_Common_Setting = new ResourcePathData("Prefabs/UI/Common/Setting/");
    public static ResourcePathData Prefab_Popup_Event_Banner = new ResourcePathData("Prefabs/UI/Popup/Event/Banner");
    public static ResourcePathData Prefab_Popup_Help = new ResourcePathData("Prefabs/UI/Popup/Help/");
    public static ResourcePathData Prefab_Popup_Roulette = new ResourcePathData("Prefabs/UI/Popup/Roulette/");
    public static ResourcePathData Prefab_Popup_PowerRanking = new ResourcePathData("Prefabs/UI/PowerRanking/");
    
    // 테스트 및 디버그
    public static ResourcePathData Prefab_Popup_InGameTest = new ResourcePathData("Prefabs/UI/Popup/DebugAndTest/");

    //유닛
    public static ResourcePathData Prefab_Unit = new ResourcePathData("Prefabs/Unit/");
    public static ResourcePathData Prefab_Unit_Hero = new ResourcePathData("Prefabs/Unit/Hero/");
    public static ResourcePathData Prefab_Unit_Monster = new ResourcePathData("Prefabs/Unit/Monster/");
    public static ResourcePathData Prefab_Unit_FieldObject = new ResourcePathData("Prefabs/Unit/FieldObject/");
    public static ResourcePathData Prefab_Test = new ResourcePathData("Prefabs/Test/");

    //스킬 관련
    public static ResourcePathData Skill = new ResourcePathData("Skill/Prefab/");
    public static ResourcePathData SkillArea = new ResourcePathData("NewSkill/"); // Skill Area
    public static ResourcePathData HitEffect = new ResourcePathData("Skill/Prefab/");//히트 이펙트
    public static ResourcePathData Muzzle = new ResourcePathData("Skill/Prefab/");//히트 이펙트
    public static ResourcePathData Test = new ResourcePathData("Skill/Prefab/Test/");

    public static ResourcePathData NewSkill = new ResourcePathData("NewSkill/");
    public static ResourcePathData SkillEffect = new ResourcePathData("Skill/Vfx/");
    
    //사운드
    public static ResourcePathData DefaultSound = new ResourcePathData("DefaultSound/");


    public static ResourcePathData Sound_FX = new ResourcePathData("Sounds/FX/");
    public static ResourcePathData Sound_BGM = new ResourcePathData("Sounds/BGM/");
    public static ResourcePathData Sound_UI_FX = new ResourcePathData("Sounds/UI_FX/");

    public static ResourcePathData Sound_Story = new ResourcePathData("Table/Story/BGM/");
    public static ResourcePathData Img_Story = new ResourcePathData("Table/Story/IMG/");

    //아이템
    public static ResourcePathData UI_Icon = new ResourcePathData("Items/Icons/");
    public static ResourcePathData UI_Sprite = new ResourcePathData("Items/Sprite/");
    public static ResourcePathData UI_SPINE = new ResourcePathData("Items/Spine/");

    //스테이지
    public static ResourcePathData STAGE_BG = new ResourcePathData("UIAsset/As_stage/");
    //이미지
    public static ResourcePathData CHARACTER = new ResourcePathData("UIAsset/As_character/");
    public static ResourcePathData CHAR_READY = new ResourcePathData("UIAsset/As_character_ready/");
    public static ResourcePathData PARTY_MANAGE = new ResourcePathData("UIAsset/As_party_manage/");
    public static ResourcePathData PARTY_MANAGE_NEW = new ResourcePathData("UIAsset/As_party_manage/new/");
    public static ResourcePathData STORAGE_MANAGE = new ResourcePathData("UIAsset/As_storage_Ui/");
    public static ResourcePathData MISSION_MAGAGE = new ResourcePathData("UIAsset/As_mission_Ui/");
    public static ResourcePathData BAG_MANAGE = new ResourcePathData("UIAsset/As_bag_Ui/");
    public static ResourcePathData BUFF_ICON = new ResourcePathData("UIAsset/As_buff/");
    public static ResourcePathData UI_LOADING = new ResourcePathData("UIAsset/As_loading/");
    public static ResourcePathData UI_SHOP = new ResourcePathData("UIAsset/As_shop/"); // 상점의 개별 상점 이미지들. 
    public static ResourcePathData UI_SHOP2 = new ResourcePathData("UIAsset/As_shop2/"); // 상점의 개별 상점 이미지들. 
    public static ResourcePathData UI_BAG = new ResourcePathData("UIAsset/As_bag_Ui/"); // 가방 이미지 
    public static ResourcePathData UI_BAG_CHAR = new ResourcePathData("UIAsset/As_bag_Ui/character/"); // 가방 이미지 
    public static ResourcePathData UI_BAG_SKILL_ICON = new ResourcePathData("UIAsset/As_bag_Ui/skill_icon/"); // 희귀도 스킬 아이콘 
    public static ResourcePathData UI_NICK = new ResourcePathData("UIAsset/As_nickname/");
    public static ResourcePathData UI_Adventure = new ResourcePathData("UIAsset/As_adventure_Ui/image/");
    public static ResourcePathData UI_DailyDungeon = new ResourcePathData("UIAsset/As_daily_dungeon/new/");
    public static ResourcePathData UI_LevelUp = new ResourcePathData("UIAsset/As_party_levelup_Ui/");
    public static ResourcePathData UI_LevelUp_TEMP = new ResourcePathData("UIAsset/As_ingame_Ui/Tem/");
    public static ResourcePathData UI_LevelUp_Remake = new ResourcePathData("UIAsset/As_ingame_Ui/Levelup/");
    public static ResourcePathData UI_Monster_Icon = new ResourcePathData("UIAsset/As_ingame_Ui/icon/");
    public static ResourcePathData UI_BattlePass = new ResourcePathData("UIAsset/As_battlePass/");
     public static ResourcePathData UI_EndlessTop = new ResourcePathData("UIAsset/As_EndlessTop/season_reward_popup/");
    public static ResourcePathData UI_Event = new ResourcePathData("UIAsset/As_event/");
    public static ResourcePathData UI_Event_Banner = new ResourcePathData("UIAsset/As_home/eventbanner/");

    // 이미지를 찾지 못하면 예외처리용 이미지. 
    public static ResourcePathData UI_None = new ResourcePathData("UIAsset/As_party_levelup_Ui/Ingame_partylv_none_characterskill");

    public static ResourcePathData Spine_Lobby_Hero = new ResourcePathData("SpineAsset/hero/img/"); // 임시 미니 캐릭터 

    //Slot관련
    public static ResourcePathData UI_SlotDetail = new ResourcePathData("UIAsset/As_ingame_Ui/Mission_done/detail/");

    public static ResourcePathData CHAR_SPRITE = new ResourcePathData("Sprites/Character/");

    public static ResourcePathData CHAR_SPRITE_LD = new ResourcePathData("Sprites/LD/");
    public static ResourcePathData CHAR_ANIMATION = new ResourcePathData("Sprites/Character/Animations/");

    public static ResourcePathData HelpGuide = new ResourcePathData("images/HelpGuide/");
    
    // 클라 테이블
    public static ResourcePathData ClientTable = new ResourcePathData("ClientTable/");

    // boss 벽
    public static ResourcePathData BossWall = new ResourcePathData("LevelPrefabs/");

    #region TEST
    public static ResourcePathData Test_Stage = new ResourcePathData("Prefabs/Stage/");
    public static ResourcePathData Test_Loading = new ResourcePathData("Prefabs/Test/");
    #endregion
    */
}