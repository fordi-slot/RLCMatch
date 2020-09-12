using Fordi.Common;
using Fordi.Core;
using Fordi.UI;
using Fordi.UI.MenuControl;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fordi.Networking
{
    [RequireComponent(typeof(PhotonTransformView))]
    public class OvrPlayerSync : MonoBehaviour, ISyncHelper, IPunObservable, IPointerClickHandler
    {

        public bool avatarSet = false;
        public int playerId;
        PhotonTransformView pView;
        public bool isRemotePlayer = false;
        private Transform fChild = null;
        private Transform camRig = null;

        private IUIEngine m_uiEngine = null;
        private IExperienceMachine m_experienceMachine = null;
        private PhotonView m_photonView;

        private void Awake()
        {
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
            m_experienceMachine = IOCCore.Resolve<IExperienceMachine>();
            m_photonView = GetComponent<PhotonView>();
        }

        public void Init(bool _avatarSet, bool _isRemotePlayer, int _playerId)
        {
            avatarSet = _avatarSet;
            isRemotePlayer = _isRemotePlayer;
            playerId = _playerId;
        }

        private void OnEnable()
        {
            //if (GetComponent<PhotonTransformView>() != null)
            //    pView = GetComponent<PhotonTransformView>();
            //else
            //    pView = gameObject.AddComponent<PhotonTransformView>();

            //this.GetComponent<PhotonView>().Synchronization = ViewSynchronization.UnreliableOnChange;
            //this.GetComponent<PhotonView>().ObservedComponents = new List<Component>();
            //if (this.GetComponent<PhotonView>().ObservedComponents.Count > 0)
            //    this.GetComponent<PhotonView>().ObservedComponents.Clear();
            //this.GetComponent<PhotonView>().ObservedComponents.Add(this.transform.GetComponent<PhotonTransformView>());
            //this.GetComponent<PhotonView>().ObservedComponents.Add(this);

            ////Debug.Log("______" + GetComponent<PhotonView>().ObservedComponents.Count);

            //pView.m_SynchronizePosition = true;
            //pView.m_SynchronizeRotation = true;

            //pView.m_PositionModel.TeleportEnabled = true;
        }

        //private void Update()
        //{
        //    if (!isRemotePlayer)
        //        return;

        //    if (fChild == null)
        //    {
        //        if (transform.childCount > 0)
        //            fChild = transform.GetChild(0);
        //        return;
        //    }

        //    fChild.localPosition = new Vector3(0, -1, 0);
        //    //fChild.rotation = Quaternion.identity;
        //}

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
          
        }

        public void PauseSync()
        {

        }

        public void ResumeSync()
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.LogError(PhotonNetwork.LocalPlayer.ActorNumber + " " + playerId);
            if (PhotonNetwork.LocalPlayer.ActorNumber != playerId)
            {
                m_uiEngine.OpenContextUI(new MenuArgs()
                {
                    Persist = false,
                    Position = new Vector2(eventData.position.x, eventData.position.y),
                    Items = CreateContextMenu()
                });
            }
        }

        MenuItemInfo[] CreateContextMenu()
        {
            MenuItemInfo[] items = new MenuItemInfo[2];

            items[0] = new MenuItemInfo()
            {
                CommandType = MenuCommandType.SEND_FRIEND_REQUEST,
                Command = "Send Friend Request",
                Text = "Send Friend Request",
                Action = new MenuItemEvent(),
                Data = playerId
            };

            items[0].Action.AddListener(OnContextItemClick);

            items[1] = new MenuItemInfo()
            {
                 CommandType = MenuCommandType.INVITE_FOR_SEX,
                 Command = "Invite For Sex",
                 Text = "Invite For Sex",
                 Action = new MenuItemEvent(),
                 Data = playerId
            };

            items[1].Action.AddListener(OnContextItemClick);


            return items;
        }

        private void OnContextItemClick(MenuClickArgs arg0)
        {
            Debug.LogError(playerId + " " + PhotonNetwork.LocalPlayer.ActorNumber + " " + arg0.CommandType.ToString());
            var targetPlayerId = (int)arg0.Data;
            m_uiEngine.CloseLastScreen();
            var targetPlayer = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == targetPlayerId);
            m_photonView.RPC("RPC_Request", targetPlayer, PhotonNetwork.LocalPlayer.ActorNumber, (int)arg0.CommandType);
        }

        [PunRPC]
        private void RPC_Request(int senderId, int commandType)
        {
            MenuCommandType menuCommand = (MenuCommandType)commandType;
            Debug.LogError(senderId + " " + menuCommand.ToString());


            m_uiEngine.Popup(new PopupInfo()
            {
                 Title = "REQUEST",
                 Content = (menuCommand == MenuCommandType.SEND_FRIEND_REQUEST ? "Friend Request" : "Invite For Sex") + " by " + senderId,
                 Action = (val) => 
                 {
                     if (menuCommand == MenuCommandType.INVITE_FOR_SEX && val == PopupInfo.PopupAction.ACCEPT)
                     {
                         Selection.Location = "PrivateMeeting";
                         Selection.ExperienceType = ExperienceType.MEETING;
                         m_experienceMachine.LoadExperience();
                     }
                 }
            });

        }
    }
}
