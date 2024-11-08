using System.Collections.Generic;
using Core.Library;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using Unity.VisualScripting;
using UnityEngine;

namespace Script.Library
{
    public class SpineUtilLibrary : MonoBehaviour
    {
        #region Spine
        public static TrackEntry PlaySpineAnim(SkeletonGraphic skeleton, string animName, bool isLoop)
        {
            TrackEntry trackEntry = new TrackEntry();
            if (!skeleton)
            {
                return null;
            }

            SkeletonData skeletonData = skeleton.skeletonDataAsset.GetSkeletonData(true);
            Spine.Animation anim = skeletonData.FindAnimation(animName);
            if (anim == null)
            {
                return null;
            }

            return skeleton.AnimationState.SetAnimation(0, anim, isLoop);
        }

        public static TrackEntry PlaySpineAnim(SkeletonAnimation skeleton, string animName, bool isLoop)
        {

            TrackEntry trackEntry = new TrackEntry();
            if (!skeleton)
            {
                return null;
            }

            SkeletonData skeletonData = skeleton.skeletonDataAsset.GetSkeletonData(true);
            Spine.Animation anim = skeletonData.FindAnimation(animName);
            if (anim == null)
            {
                return null;
            }

            return skeleton.AnimationState.SetAnimation(0, anim, isLoop);
        }

        /*
        public static TrackEntry PlaySpineAnim(SkeletonGraphic skeleton, List<string> animNameList, bool isLastAnimLoop)
        {
            TrackEntry trackEntry = new TrackEntry();
            if (!skeleton)
            {
                return null;
            }
        
            SkeletonData skeletonData = skeleton.skeletonDataAsset.GetSkeletonData(true);
            Spine.Animation prevAnim = null;
            float delayTime = 0.0f;

            for (int i = 0; i < animNameList.Count; i++)
            {
                Spine.Animation anim = skeletonData.FindAnimation(animNameList[i]);
                delayTime = (prevAnim == null) ? 0.0f : prevAnim.Duration;  
                if (anim != null)
                {
                    bool isLoop = (i == animNameList.Count - 1) ? isLastAnimLoop : false;
                    if (i == 0)
                    {
                        trackEntry = skeleton.AnimationState.SetAnimation(0, anim, isLoop);
                    }
                    else
                    {
                        trackEntry = skeleton.AnimationState.AddAnimation(0, anim, isLoop, delayTime);
                    }
                }

                prevAnim = anim;
            }

            return trackEntry;
        }*/

        public static void StopSpineAnim(SkeletonGraphic skeleton)
        {
            if (!skeleton)
            {
                return;
            }

            if (skeleton.AnimationState != null)
            {
                skeleton.AnimationState.ClearTracks();
            }
        }

        public static bool ChangeSkeletonToSpine(SkeletonGraphic skeleton, SkeletonDataAsset skeletonDataAsset)
        {
            if (!skeleton)
            {
                return false;
            }

            if (!skeletonDataAsset)
            {
                return false;
            }

            skeleton.skeletonDataAsset = skeletonDataAsset;
            skeleton.Initialize(true);
            return true;
        }

        /*
        public static void UnloadSkeleton()
        {
            Resources.UnloadAsset(skeletonDataAsset);
            skeletonDataAsset = null;

            Destroy(createdSkeletonObject);
      
            Resources.UnloadUnusedAssets();
        }*/

        public static void UpdateSpine(SkeletonGraphic skeleton)
        {
            if (skeleton && skeleton.gameObject.activeSelf)
            {
                skeleton.AnimationState.Update(Time.unscaledDeltaTime);
            }
        }

        public static void ClearSpineAnim(SkeletonAnimation skeleton, int trackIndex)
        {
            if (!skeleton)
            {
                return;
            }

            if (skeleton.state == null)
            {
                return;
            }

            skeleton.state.ClearTrack(trackIndex);
        }
        #endregion

        #region SpineBone
        private static void AttachIconsToChildren(Transform root)
        {
#if UNITY_EDITOR
            if (root != null)
            {
                SkeletonUtilityBone[] utilityBones = root.GetComponentsInChildren<SkeletonUtilityBone>();
                foreach (SkeletonUtilityBone utilBone in utilityBones)
                    SkeletonUtilityInspector.AttachIcon(utilBone);
            }
#endif
        }

        // 본 스폰 하려면 MultipleCanvasRenderer 설정을 킬것. 꺼져있으면 실패함
        public static SkeletonUtility SpawnHierarchySpineBone(SkeletonGraphic skeletonGraphic)
        {
            SkeletonUtility bone;
            if (skeletonGraphic)
            {
                bone = skeletonGraphic.GetOrAddComponent<SkeletonUtility>();
            }
            else
            {
                return null;
            }

            Skeleton skeleton = skeletonGraphic.Skeleton;
            if (bone && (skeleton != null))
            {
                if (bone.SkeletonComponent != null)
                {
                    bone.boneRoot = skeletonGraphic.transform;

                    //Transform transform = bone.GetBoneRoot();
                    GameObject hierarchy = bone.SpawnBoneRecursively(skeleton.RootBone, bone.boneRoot, SkeletonUtilityBone.Mode.Follow, true, true, true);
                    bone.CollectBones();

                    // GameObject hierarchy = bone.SpawnHierarchy(SkeletonUtilityBone.Mode.Follow, true, true, true);
                    Transform ht = hierarchy ? hierarchy.transform : null;
                    if (ht)
                    {
                        ht.localPosition = Vector3.zero;
                        ht.localScale = Vector3.one;
                    }

                    AttachIconsToChildren(bone.boneRoot);
                }
            }

            return bone;
        }

        public static bool AttachToSpineBone(Dictionary<string, Transform> transformMap, string boneName, GameObject gameObject)
        {
            return AttachToSpineBone(transformMap, boneName, gameObject, Vector3.zero, Vector3.one);
        }

        public static bool AttachToSpineBone(Dictionary<string, Transform> transformMap, string boneName, GameObject gameObject, Vector3 pos, Vector3 scale)
        {
            if (!gameObject)
            {
                return false;
            }

            if (transformMap.ContainsKey(boneName))
            {
                Transform boneTransform;
                if (transformMap.TryGetValue(boneName, out boneTransform))
                {
                    Transform transform = gameObject.transform;
                    transform.SetParent(boneTransform);
                    transform.localPosition = pos;
                    transform.localScale = scale;
                    return true;
                }
            }

            return false;
        }

        public static bool AttachToSpineBone(SkeletonGraphic skeletonGraphic, string boneName, GameObject gameObject)
        {
            return AttachToSpineBone(skeletonGraphic, boneName, gameObject, Vector3.zero, Vector3.one);
        }

        public static bool AttachToSpineBone(SkeletonGraphic skeletonGraphic, string boneName, GameObject gameObject, Vector3 pos, Vector3 scale)
        {
            SkeletonUtility bone = SpawnHierarchySpineBone(skeletonGraphic);
            if (!bone)
            {
                return false;
            }

            Dictionary<string, Transform> transformMap = CodeUtilLibrary.MakeTransformNameMap(bone.gameObject);
            return AttachToSpineBone(transformMap, boneName, gameObject, pos, scale);
        }
        #endregion

        #region SpineSkin
        public static Dictionary<string, Skin> MakeSkinMap(SkeletonGraphic skeletonGraphic)
        {
            Dictionary<string, Skin> skinMap = new Dictionary<string, Skin>();

            if (!skeletonGraphic)
            {
                return skinMap;
            }

            foreach (Skin skin in skeletonGraphic.SkeletonData.Skins)
            {
                skinMap.Add(skin.Name, skin);
            }

            return skinMap;
        }

        public static void SetSpineSkin(SkeletonGraphic skeletonGraphic, string skinName)
        {
            if (skeletonGraphic)
            {
                skeletonGraphic.Skeleton.SetSkin(skinName);
                skeletonGraphic.Skeleton.SetToSetupPose();
            }
        }
        #endregion

        #region SpineEvent
        public static List<float> GetSpineExecuteEventTime(SkeletonGraphic skeletonGraphic, string animName, EventDataReferenceAsset checkEvent)
        {
            List<float> eventTimeList = new List<float>();
            if (!skeletonGraphic)
            {
                return eventTimeList;
            }

            SkeletonData skeletonData = skeletonGraphic.skeletonDataAsset.GetSkeletonData(true);
            Spine.Animation anim = skeletonData.FindAnimation(animName);
            if (anim == null)
            {
                return eventTimeList;
            }

            int timelineCount = anim.Timelines.Count;
            Timeline[] timelines = anim.Timelines.Items;
            int i, j;

            for (i = 0; i < timelineCount; i++)
            {
                Timeline timeline = timelines[i];
                if (timeline is EventTimeline)
                {
                    EventTimeline eventTimeLine = timeline as EventTimeline;
                    Spine.Event[] eventList = eventTimeLine.Events;
                    for (j = 0; j < eventList.Length; j++)
                    {
                        if (eventList[j].Data == checkEvent.EventData)
                        {
                            eventTimeList.Add(eventList[j].Time);
                        }
                    }
                }
            }

            return eventTimeList;
        }

        public static List<Spine.Event> GetSpineExecuteEvent(SkeletonGraphic skeletonGraphic, string animName, EventDataReferenceAsset checkEvent)
        {
            List<Spine.Event> spineEventList = new List<Spine.Event>();
            if (!skeletonGraphic)
            {
                return spineEventList;
            }

            SkeletonData skeletonData = skeletonGraphic.skeletonDataAsset.GetSkeletonData(true);
            Spine.Animation anim = skeletonData.FindAnimation(animName);
            if (anim == null)
            {
                return spineEventList;
            }

            int timelineCount = anim.Timelines.Count;
            Timeline[] timelines = anim.Timelines.Items;
            int i, j;

            for (i = 0; i < timelineCount; i++)
            {
                Timeline timeline = timelines[i];
                if (timeline is EventTimeline)
                {
                    EventTimeline eventTimeLine = timeline as EventTimeline;
                    Spine.Event[] eventList = eventTimeLine.Events;
                    for (j = 0; j < eventList.Length; j++)
                    {
                        if (eventList[j].Data == checkEvent.EventData)
                        {
                            spineEventList.Add(eventList[j]);
                        }
                    }
                }
            }

            return spineEventList;
        }
        #endregion
    }
}