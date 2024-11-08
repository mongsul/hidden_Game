using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using ColorUtility = UnityEngine.ColorUtility;

#if UNITY_EDITOR
using UnityEditor;
#endif

// _SJ      코드 유틸 라이브러리
namespace Core.Library
{
    public class CodeUtilLibrary : MonoBehaviour
    {
        public struct RandomVector
        {
            public Vector3 Min;
            public Vector3 Max;

            public void SetValue(Vector3 min, Vector3 max)
            {
                Min = min;
                Max = max;
            }

            public Vector3 GetRandomValue()
            {
                Vector3 randomVector = new Vector3();
                randomVector.x = (Min.x != Max.x) ? Math.Min(Min.x, Max.x) : Min.x;
                randomVector.y = (Min.y != Max.y) ? Math.Min(Min.y, Max.y) : Min.y;
                randomVector.z = (Min.z != Max.z) ? Math.Min(Min.z, Max.z) : Min.z;
                return randomVector;
            }

            public void SetX(float x)
            {
                Min.x = x;
                Max.x = x;
            }

            public void SetX(float minX, float maxX)
            {
                Min.x = minX;
                Max.x = maxX;
            }

            public void SetY(float y)
            {
                Min.y = y;
                Max.y = y;
            }

            public void SetY(float minY, float maxY)
            {
                Min.y = minY;
                Max.y = maxY;
            }

            public void SetZ(float z)
            {
                Min.z = z;
                Max.z = z;
            }

            public void SetZ(float minZ, float maxZ)
            {
                Min.z = minZ;
                Max.z = maxZ;
            }

            public Vector3 RandomAndMultiplyValue(float multiply)
            {
                Vector3 randomVector = new Vector3();
                randomVector.x = (Min.x != Max.x) ? Math.Min(Min.x, Max.x) : (Min.x * multiply);
                randomVector.y = (Min.y != Max.y) ? Math.Min(Min.y, Max.y) : (Min.y * multiply);
                randomVector.z = (Min.z != Max.z) ? Math.Min(Min.z, Max.z) : (Min.z * multiply);
                return randomVector;
            }
        }
        
        #region UI
        public static void SetAlphaToText(TMP_Text text, float alpha)
        {
            if (text)
            {
                Color color = text.color;
                color.a = alpha;
                text.color = color;
            }
        }
        
        public static Sprite LoadSprite(string resourcePath, string resourceName)
        {
            return LoadSprite(new ResourcePathData(resourcePath), resourceName);
        }
        
        public static Sprite LoadSprite(ResourcePathData resourcePath, string resourceName)
        {
            return ResourceManager.Instance.LoadImage(resourcePath, resourceName);
        }
        
        public static Color HexColor(string hexCode)
        {
            Color color;
            if ( ColorUtility.TryParseHtmlString( $"#{hexCode}", out color ) )
            {
                return color;
            }
            
            //Debug.LogError( "[UnityExtension::HexColor]invalid hex code - " + hexCode );
            return Color.white;
        }

        /*
        public static Sprite LoadItemIconSprite(string iconPath)
        {
            return LoadSprite(ResourcePath.UI_Icon, $"{iconPath}");
        }*/

        public static void SetStrechRectTransform(RectTransform rectTransform)
        {
            if (rectTransform)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
        }
        #endregion
        
        #region Transform
        public static Dictionary<string, Transform> MakeTransformNameMap(GameObject baseObject)
        {
            Dictionary<string, Transform> transformMap = new Dictionary<string, Transform>();
            if (!baseObject)
            {
                return transformMap;
            }
            
            Transform[] transformList = baseObject.GetComponentsInChildren<Transform>();
            for (int i = 0; i < transformList.Length; i++)
            {
                string boneName = transformList[i].gameObject.name;
                if (!transformMap.ContainsKey(boneName))
                {
                    transformMap.Add(boneName, transformList[i]);
                }
            }

            return transformMap;
        }
        
        /// <summary>
        /// Returns the rectangle transform. Will return null if a normal transform is used.
        /// </summary>
        /// <param name="component">The component of which to get the rectangle transform.</param>
        /// <returns>The rectangle transform instance.</returns>
        public static RectTransform GetRectTransform(UnityEngine.Component component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            
            return component.transform as RectTransform;
        }

        /// <summary>
        /// Returns the rectangle transform. Will return null if a normal transform is used.
        /// </summary>
        /// <param name="gameObject">The game object of which to get the rectangle transform.</param>
        /// <returns>The rectangle transform instance.</returns>
        public static RectTransform GetRectTransform(GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));
            
            return gameObject.transform as RectTransform;
        }
        #endregion

        #region Math
        public static Vector3 VectorMultiply(Vector3 vector1, Vector3 vector2)
        {
            Vector3 result;
            result.x = vector1.x * vector2.x;
            result.y = vector1.y * vector2.y;
            result.z = vector1.z * vector2.z;

            return result;
        }
        
        public static Vector3 VectorDivide(Vector3 vector1, Vector3 vector2)
        {
            Vector3 result;
            result.x = vector1.x / vector2.x;
            result.y = vector1.y / vector2.y;
            result.z = vector1.z / vector2.z;

            return result;
        }
        
        public static float CrossXZ(in Vector3 V1, in Vector3 V2)
        {
            return (V1.x * V2.z) - (V1.z * V2.x);
        }
        
        public static Vector2 VectorMultiply(Vector2 vector1, Vector2 vector2)
        {
            Vector2 result;
            result.x = vector1.x * vector2.x;
            result.y = vector1.y * vector2.y;

            return result;
        }
        
        public static float DistXZ(in Vector3 V1, in Vector3 V2)
        {
            return Mathf.Sqrt(Square(V2.x - V1.x) + Square(V2.z - V1.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistSquareXZ(in Vector3 V1, in Vector3 V2)
        {
            return Square(V2.x - V1.x) + Square(V2.z - V1.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Square(int Val)
        {
            return Val * Val;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Square(float Val)
        {
            return Val * Val;
        }
        
        /// <summary>
        /// V1 에 V2 값이 포함 되어 있는지 확인
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainByte(byte V1, byte V2)
        {
            return (V1 & V2) != 0;
        }
    
        /// <summary>
        /// V1 에 V2 를 더해준다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnByte(ref byte V1, byte V2)
        {
            V1 |= V2;
        }
    
        /// <summary>
        /// V1 에 V2 를 빼준다.s
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OffByte(ref byte V1, byte V2)
        {
            if (ContainByte(V1, V2))
                V1 ^= (byte)(V1 & V2);
        }

        public static RandomVector GetRandomVector()
        {
            RandomVector vector = new RandomVector();
            vector.SetValue(Vector3.zero, Vector3.zero);
            return vector;
        }

        public static RandomVector GetRandomVector(Vector3 min, Vector3 max)
        {
            RandomVector vector = new RandomVector();
            vector.SetValue(min, max);
            return vector;
        }

        public static Vector3 GetRandomVectorValue(RandomVector vector)
        {
            return vector.GetRandomValue();
        }

        public static Vector3 GetRandomVectorValue(Vector3 min, Vector3 max)
        {
            return GetRandomVectorValue(GetRandomVector(min, max));
        }

        public static Vector3 GetMinVector(Vector3 vec1, Vector3 vec2)
        {
            Vector3 min = new Vector3();
            min.x = Math.Min(vec1.x, vec2.x);
            min.y = Math.Min(vec1.y, vec2.y);
            min.z = Math.Min(vec1.z, vec2.z);
            return min;
        }

        public static Vector3 GetMaxVector(Vector3 vec1, Vector3 vec2)
        {
            Vector3 max = new Vector3();
            max.x = Math.Max(vec1.x, vec2.x);
            max.y = Math.Max(vec1.y, vec2.y);
            max.z = Math.Max(vec1.z, vec2.z);
            return max;
        }

        public static float GetAngleXZ(Vector3 start, Vector3 target)
        {
            Vector3 dt = target - start;
            float rad = Mathf.Atan2(dt.x, dt.z);
            float degree = rad * Mathf.Rad2Deg;
		
            if (degree < 0) 
            {
                degree += 360;
            }
		
            return degree;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRage(Vector3 standard, float range, Vector3 check)
        {
            return DistSquareXZ(standard, check) < range * range;
        }
        #endregion

        #region Engine

        public static bool IsPlaying()
        {
#if UNITY_EDITOR
            return Application.isPlaying;
#else
            return true;
#endif
        }

#if UNITY_EDITOR

        public static bool CanPause = true;
        
        public static void PrintErrorMessage(in object InMessage)
        {
            Debug.Log(string.Format("<color=red>{0}</color>", InMessage));
        }

        public static void SetColorLog(string log, string color = "white")
        {
            Debug.Log(string.Format("<color={0}>{1}</color>", color, log));
        }
#endif

        #endregion

        #region List
        public static float GetListValue(float[] list, int listIndex)
        {
            if (list == null)
            {
                return 0.0f;
            }

            if ((listIndex < 0) || (listIndex >= list.Length))
            {
                return 0.0f;
            }

            return list[listIndex];
        }
        
        public static T GetListValue<T>(List<T> list, int listIndex, T defaultValue)
        {
            if (list == null)
            {
                return defaultValue;
            }

            if ((listIndex < 0) || (listIndex >= list.Count))
            {
                return defaultValue;
            }

            return list[listIndex];
        }
        #endregion

        #region Particle
        public static void SetParticleSpeed(ParticleSystem[] particles, float speed)
        {
            if (particles == null)
            {
                return;
            }
            
            for (int i = 0; i < particles.Length; ++i)
            {
                SetParticleSpeed(particles[i], speed);
            }
        }
        
        public static void SetParticleSpeed(ParticleSystem particleSystem, float speed)
        {
            if (particleSystem == null)
            {
                return;
            }
            
            var main = particleSystem.main;
            main.simulationSpeed = speed;
        }
        #endregion

        #region Scene
        private static IEnumerable<T> GetComponentsEnumerableInActiveScene<T>(bool includeInactive = true)
        {
            // Scene내부에 있는 모든 컴포넌트를 리턴하는 함수
            
            // 활성화 되어있는 Scene의 루트에 있는 GameObject 목록을 가져온다.
            var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            // 비어있는 IEnumerable<T>
            IEnumerable<T> resultComponents = (T[])Enumerable.Empty<T>();
            foreach (var item in rootGameObjects)
            {
                // includeInactive = true 로 지정하면, GameObject가 비활성화 되어있어도 가져온다.
                var components = item.GetComponentsInChildren<T>(includeInactive);
                resultComponents = resultComponents.Concat(components);
            }

            return resultComponents;
        }
        
        public static T[] GetComponentsInActiveScene<T>(bool includeInactive = true)
        {
            return GetComponentsEnumerableInActiveScene<T>(includeInactive).ToArray();
        }
        
        public static List<T> GetComponentsListInActiveScene<T>(bool includeInactive = true)
        {
            return GetComponentsEnumerableInActiveScene<T>(includeInactive).ToList();
        }

        public static T GetComponentInActiveScene<T>(bool includeInactive = true)
        {
            // 1개의 컴포넌트만 얻어올 경우에는 이 함수를 사용한다.
            // GetComponentsInActiveScene을 기반으로 작성해서 조금 비효율적 (한번 수정을 했음)
            
            // 활성화 되어있는 Scene의 루트에 있는 GameObject 목록을 가져온다.
            var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            // 비어있는 IEnumerable<T>
            IEnumerable<T> resultComponents = (T[])Enumerable.Empty<T>();
            foreach (var item in rootGameObjects)
            {
                // includeInactive = true 로 지정하면, GameObject가 비활성화 되어있어도 가져온다.
                T component = item.GetComponentInChildren<T>(includeInactive);
                if (component != null)
                {
                    return component;
                }
            }

            return default;
        }
        #endregion
    }
}