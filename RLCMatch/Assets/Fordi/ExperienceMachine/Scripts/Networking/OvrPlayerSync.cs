using Fordi.Common;
using Fordi.Core;
using Fordi.UI;
using Fordi.UI.MenuControl;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
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
        private Transform fChild = null;
        private Transform camRig = null;

        private IUIEngine m_uiEngine = null;
        private IExperienceMachine m_experienceMachine = null;
        private INetwork m_network;
        private PhotonView m_photonView;

        public bool IsRemotePlayer { get; set; } = false;

        private void Awake()
        {
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
            m_experienceMachine = IOCCore.Resolve<IExperienceMachine>();
            m_network = IOCCore.Resolve<INetwork>();
            m_photonView = GetComponent<PhotonView>();

          
        }

        private IEnumerator Start()
        {
            yield return null;
            yield return null;

            Debug.LogError(Selection.Location);
            if (Selection.Location == "PrivateMeeting")
            {
                var renderers = GetComponentsInChildren<Renderer>();
                foreach (var item in renderers)
                    item.enabled = false;
            }
        }

        public void Init(bool _avatarSet, bool _isRemotePlayer, int _playerId)
        {
            avatarSet = _avatarSet;
            IsRemotePlayer = _isRemotePlayer;
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

        [PunRPC]
        void RPC_Ping(int senderId, int number)
        {
            Debug.LogError("RPC_Ping: " + senderId + " : " + number);
        }

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
            if (Selection.Location != "PrivateMeeting" && IsRemotePlayer && Input.GetMouseButtonUp(1))
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
            MenuItemInfo[] items = new MenuItemInfo[3];

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


            items[2] = new MenuItemInfo()
            {
                CommandType = MenuCommandType.OTHER,
                Command = "Cancel",
                Text = "Cancel",
                Action = new MenuItemEvent(),
                Data = playerId
            };

            items[2].Action.AddListener(OnContextItemClick);

            return items;
        }

        private void OnContextItemClick(MenuClickArgs arg0)
        {
            Debug.LogError(playerId + " " + PhotonNetwork.LocalPlayer.ActorNumber + " " + arg0.CommandType.ToString());
            var targetPlayerId = (int)arg0.Data;
            m_uiEngine.CloseLastScreen();

            if (arg0.CommandType == MenuCommandType.OTHER)
                return;

            var targetPlayer = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == targetPlayerId);
            m_photonView.RPC("RPC_Request", targetPlayer, PhotonNetwork.LocalPlayer.ActorNumber, (int)arg0.CommandType);
        }

        [PunRPC]
        private void RPC_Request(int senderId, int commandType)
        {
            MenuCommandType menuCommand = (MenuCommandType)commandType;
            Debug.LogError(senderId + " " + menuCommand.ToString());
            var targetPlayer = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == senderId);
            var inviterName = targetPlayer == null ? senderId.ToString() : targetPlayer.NickName;

            m_uiEngine.Popup(new PopupInfo()
            {
                 Title = "REQUEST",
                 Content = (menuCommand == MenuCommandType.SEND_FRIEND_REQUEST ? "Friend Request" : "Invite For Sex") + " by " + inviterName,
                 Action = (val) => 
                 {
                     if (menuCommand == MenuCommandType.INVITE_FOR_SEX && val == PopupInfo.PopupAction.ACCEPT)
                     {
                         m_uiEngine.CloseLastScreen();
                         var roomName = Guid.NewGuid().ToString().Substring(0, 4);
                         m_photonView.RPC("RPC_PrivateRoom", targetPlayer, PhotonNetwork.LocalPlayer.ActorNumber, roomName);
                         m_network.EnterPrivateRoom(roomName, true);
                     }
                 }
            });

        }

        [PunRPC]
        private void RPC_PrivateRoom(int senderId, string roomName)
        {
            Debug.LogError("RPC_PrivateRoom: " + senderId + " " + roomName);
            
            Observable.TimerFrame(500).Subscribe(_ =>
            {
                m_network.EnterPrivateRoom(roomName, false);
                m_uiEngine.DisplayProgress("Loading: " + Selection.Location, true);

                //if (Array.Find(Network.Rooms, item => item.Name == roomName) != null)
                //{
                //    Debug.LogError("Room exists: " + roomName);
                //    m_network.EnterPrivateRoom(roomName, false);
                //}
                //else
                //{
                //    Debug.LogError("Room doesn't exist: " + roomName);
                //}
            });
        }
    }
}
