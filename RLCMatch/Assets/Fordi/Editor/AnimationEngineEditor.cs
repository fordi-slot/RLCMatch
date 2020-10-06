using Fordi.Core;
using RLC.Animation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RLC.UnityEditor
{
    [CustomEditor(typeof(AnimationEngine))]
    public class AnimationEngineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AnimationEngine animationEngine = (AnimationEngine)target;

            if (GUILayout.Button("Compile Animations"))
            {
                animationEngine.AnimationPoses.Clear();

                // Find all Texture2Ds that have 'co' in their filename, that are labelled with 'architecture' and are placed in 'MyAwesomeProps' folder
                string[] femaleClipGuids = AssetDatabase.FindAssets("hf_ t:AnimationClip", null);
                string[] maleClipGuids = AssetDatabase.FindAssets("hm_ t:AnimationClip", null);

                var femaleClips = new List<AnimationClip>();
                var maleClips = new List<AnimationClip>();

                foreach (var item in femaleClipGuids)
                {
                    femaleClips.Add(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(item)));
                }

                foreach (var item in maleClipGuids)
                {
                    maleClips.Add(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(item)));
                }

                animationEngine.ValidFemaleClips = femaleClips;
                animationEngine.ValidMaleClips = maleClips;

                foreach (var item in maleClips)
                {
                    var delimitterIndex = item.name.LastIndexOf("_");
                    if (delimitterIndex == -1)
                        continue;

                    if (delimitterIndex == 2)
                        delimitterIndex = item.name.Length;

                    string groupName = string.Empty, femaleClipName, key;
                    AnimationClip femaleClip;

                    try
                    {
                        groupName = item.name.Substring(3, delimitterIndex - 3);
                        femaleClipName = "hf_" + item.name.Substring(3, item.name.Length - 3);
                        femaleClip = femaleClips.Find(clip => clip.name == femaleClipName);
                        if (delimitterIndex == item.name.Length)
                            key = item.name.Substring(3, item.name.Length - 3);
                        else
                            key = item.name.Substring(delimitterIndex + 1, item.name.Length - delimitterIndex - 1);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        Debug.LogError(item.name + " " + item.name.Length + " " + delimitterIndex + " " + e.Message);
                        continue;
                    }

                    if (femaleClip == null)
                            continue;

                    //Debug.LogError(groupName + " : " + femaleClipName + ":" + item.name);

                    try
                    {
                        var pose = new AnimationPose()
                        {
                            GroupName = groupName,
                            FemaleClip = femaleClip,
                            MaleClip = item,
                            Key = key
                        };
                        animationEngine.AnimationPoses.Add(pose);
                    }
                    catch(Exception e)
                    {
                        Debug.LogError(item.name + " " + e.Message);
                    }
                }

                var dbGuids = AssetDatabase.FindAssets("t: AssetDB", null);
                List<AssetDB> databases = new List<AssetDB>();
                foreach (var item in dbGuids)
                {
                    databases.Add(AssetDatabase.LoadAssetAtPath<AssetDB>(AssetDatabase.GUIDToAssetPath(item)));
                }

                var database = databases[0];

                Dictionary<string, List<AnimationResource>> animations = new Dictionary<string, List<AnimationResource>>();

                foreach (var item in animationEngine.AnimationPoses)
                {
                    if (!animations.ContainsKey(item.GroupName))
                        animations[item.GroupName] = new List<AnimationResource>();
                    animations[item.GroupName].Add(new AnimationResource()
                    {
                        MaleClip = item.MaleClip,
                        FemaleClip = item.FemaleClip,
                        Name = item.Key,
                        ResourceType = ResourceType.ANIMATION,
                    });
                }

                database.Animations = new AnimationGroup[animations.Count];

                int index = 0;
                foreach (var item in animations)
                {
                    database.Animations[index] = new AnimationGroup()
                    {
                         ResourceType = ResourceType.ANIMATION,
                         Name = item.Key,
                         Resources = item.Value.ToArray()
                    };
                    index++;
                }
            }
        }
    }
}