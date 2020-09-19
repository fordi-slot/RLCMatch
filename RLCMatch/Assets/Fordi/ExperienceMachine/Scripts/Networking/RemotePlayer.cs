using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Networking
{
    public class RemotePlayer : MonoBehaviour
    {
        [SerializeField]
        private OvrPlayerSync m_playerSync = null;
        [SerializeField]
        private PhotonView m_playerPhotonView = null;
        [SerializeField]
        private PhotonView m_avatarPhotonView = null;
        [SerializeField]
        private TextMesh m_playerName;

        public int playerId { get; private set; }
        public Transform RightHand { get { return m_rightHand; } }
        public Transform LeftHand { get { return m_leftHand; } }
        public Transform Pen { get { return m_pen; } }

        private Transform m_pen;
        private Transform m_rightHand, m_leftHand = null;

        private Photon.Realtime.Player m_photonPlayer = null;

        public void Setup(int senderId, int playerViewId)
        {
            Debug.LogError(senderId + " " + playerViewId);
            name = "RemotePlayer: " + senderId; 
            m_playerSync.IsRemotePlayer = true;
            m_playerSync.playerId = senderId;
            m_playerPhotonView.ViewID = playerViewId;

            m_photonPlayer = Array.Find(PhotonNetwork.PlayerListOthers, item => item.ActorNumber == senderId);
            m_playerName.text = m_photonPlayer.NickName;
        }
    }
}
