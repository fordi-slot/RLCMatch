using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fordi.Common;
using Fordi.UI.MenuControl;
using System;
using TMPro;
using Fordi.UI;
using Photon.Pun;

namespace Fordi.Core
{
    public abstract class Gameplay : Experience
    {
        public override void ExecuteMenuCommand(MenuClickArgs args)
        {
            if (args.Data != null && ((ResourceComponent)args.Data).ResourceType == ResourceType.ANIMATION)
                return;

            base.ExecuteMenuCommand(args);

            if (args.CommandType == MenuCommandType.LOGOUT)
            {
                Selection.Location = Home.HOME_SCENE;
                Selection.ExperienceType = ExperienceType.HOME;
                ToggleMenu();
                m_experienceMachine.LoadExperience();
            }

            if (args.CommandType == MenuCommandType.CATEGORY_SELECTION)
            {
                var resourceType = ((ResourceComponent)args.Data).ResourceType;
                Debug.LogError(resourceType.ToString());
                m_uiEngine.OpenGridMenu(new GridArgs()
                {
                    Items = ResourceToMenuItems(GetResource(resourceType, args.Command)),
                    Title = "SELECT " + resourceType.ToString().ToUpper(),
                });
                return;
            }

            if (args.CommandType == MenuCommandType.VO)
            {
                var categories = GetCategories(ResourceType.AUDIO);
                if (categories.Length == 0 || (categories.Length == 1 && string.IsNullOrEmpty(categories[0].Name)))
                    m_uiEngine.OpenGridMenu(new GridArgs()
                    {
                        AudioClip = m_commonResource.GetGuideClip(MenuCommandType.VO),
                        Items = ResourceToMenuItems(GetResource(ResourceType.AUDIO, "")),
                        Title = "SELECT AUDIO",
                    });
                else
                    m_uiEngine.OpenGridMenu(new GridArgs()
                    {
                        AudioClip = m_commonResource.GetGuideClip(MenuCommandType.VO),
                        Items = GetCategoryMenu(categories, ResourceType.AUDIO),
                        Title = "WHICH MEDITATION SUITS YOUR MOOD?",
                    });
            }

            else if (args.CommandType == MenuCommandType.SELECTION)
            {
                m_uiEngine.Close();
                AudioSelectionFlag = false;
            }
        }

        /// <summary>
        /// Gameplay resource selection happens here.
        /// </summary>
        /// <param name="args"></param>
        public override void UpdateResourceSelection(MenuClickArgs args)
        {
            base.UpdateResourceSelection(args);
            if (args.Data != null && args.Data is ExperienceResource)
            {
                ExperienceResource resource = (ExperienceResource)args.Data;

                if (resource.ResourceType == ResourceType.AUDIO)
                {
                    Selection.VoiceOver = ((AudioResource)resource).Clip;

                    AudioArgs voArgs = new AudioArgs(Selection.VoiceOver, AudioType.VO);
                    voArgs.FadeTime = 2;
                    m_audio.Play(voArgs);
                    AudioSelectionFlag = true;
                }
            }

            //Apply selection to gameplay.
            //Selection can be accessed from Selection object.

        }

        public override void OnLoad()
        {
            base.OnLoad();
            if (Selection.VoiceOver == null || ExperienceMachine.AppMode == AppMode.TRAINING)
                return;
            AudioArgs voArgs = new AudioArgs(Selection.VoiceOver, AudioType.VO);
            voArgs.FadeTime = 2;
            m_audio.Play(voArgs);
        }
    }
}
