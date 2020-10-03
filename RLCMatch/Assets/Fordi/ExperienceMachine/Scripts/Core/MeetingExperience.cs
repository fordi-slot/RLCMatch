using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fordi.Common;
using Fordi.UI.MenuControl;
using Fordi.UI;

namespace Fordi.Core
{
    public class MeetingExperience : Gameplay
    {
        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_music = m_commonResource.AssetDb.MeetingMusic;
        }

        public override ExperienceResource[] GetResource(ResourceType resourceType, string category)
        {
            ExperienceResource[] resources = base.GetResource(resourceType, category);
            if (resources != null)
                return resources;

            if (resourceType == ResourceType.MUSIC)
                return Array.Find(m_music, item => item.Name.Equals(category)).Resources;

            return null;
        }

        public override ResourceComponent[] GetCategories(ResourceType resourceType)
        {
            ResourceComponent[] categories = base.GetCategories(resourceType);
            if (categories != null)
                return categories;

            if (resourceType == ResourceType.MUSIC)
                return m_music;

            return null;
        }

        public override void OnLoad()
        {
            base.OnLoad();
            Debug.LogError("OnLoad");
            StartCoroutine(TakeASeat());
        }

        private IEnumerator TakeASeat()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;

#if LOCAL_TEST
            OpenMenu();
            yield break;
#endif

            //Debug.LogError(PhotonNetwork.LocalPlayer.ActorNumber);
            if (PhotonNetwork.LocalPlayer.ActorNumber < 1)
            {
                if (m_teleportAnchors.Count > 0)
                    m_experienceMachine.Player.DoWaypointTeleport(m_teleportAnchors[0]);
                yield break;
            }
            if (m_teleportAnchors.Count > PhotonNetwork.LocalPlayer.ActorNumber - 1)
                m_experienceMachine.Player.DoWaypointTeleport(m_teleportAnchors[PhotonNetwork.LocalPlayer.ActorNumber - 1]);

            yield return new WaitForSeconds(.2f);

            OpenMenu();
        }
    }
}
