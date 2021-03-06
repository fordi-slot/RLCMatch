﻿using Fordi.Core;
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
                string[] femaleClipGuids = AssetDatabase.FindAssets("hf_ t:AnimationClip", new string[] { "Assets/Artwork/Animations" });
                string[] maleClipGuids = AssetDatabase.FindAssets("hm_ t:AnimationClip", new string[] { "Assets/Artwork/Animations" });

                var femaleClips = new List<AnimationClip>();
                var maleClips = new List<AnimationClip>();

                foreach (var item in femaleClipGuids)
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(item));
                    if (clip.name.Length > 0 && clip.name.ToLower().Substring(0, 2) == "hf")
                    {
                        if (clip.name.Substring(0, 2) != "hf")
                        {
                            var message = AssetDatabase.RenameAsset(AssetDatabase.GUIDToAssetPath(item), "hf" + clip.name.Substring(2, clip.name.Length - 2));
                            if (string.IsNullOrEmpty(message))
                            {
                                Debug.LogError("Renamed to " + clip.name);
                            }
                        }

                        femaleClips.Add(clip);
                    }
                }

                foreach (var item in maleClipGuids)
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(item));

                    if (clip.name.Length > 0 && clip.name.ToLower().Substring(0, 2) == "hm")
                    {
                        if (clip.name.Substring(0, 2) != "hm")
                        {
                            var message = AssetDatabase.RenameAsset(AssetDatabase.GUIDToAssetPath(item), "hm" + clip.name.Substring(2, clip.name.Length - 2));
                            if (string.IsNullOrEmpty(message))
                            {
                                Debug.LogError("Renamed to " + clip.name);
                            }
                        }

                        maleClips.Add(clip);
                    }
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
                        key = item.name.Substring(3, item.name.Length - 3);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        Debug.LogError(item.name + " " + item.name.Length + " " + delimitterIndex + " " + e.Message);
                        continue;
                    }

                    if (femaleClip == null)
                    {
                        Debug.LogError("FemaleClip " + femaleClipName + " not found");
                        continue;
                    }

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