﻿using Cornea.Web;
using Fordi.Common;
using Fordi.Core;
using Fordi.UI;
using Fordi.UI.MenuControl;
using Photon.Pun;
using RLC.Animation;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using LitJson;

namespace Fordi.Networking
{
    [Obsolete("Temp")]
    [Serializable]
    public class Friend : ExperienceResource
    {
        public int PlayerId;
        public string _id;
        public string displayName;
        public int primaryMemberID;
        public string email;
        public string dob;

        public static explicit operator Friend(UserFriend surrogate)
        {
            if (surrogate == null)
                return null;

            return new Friend()
            {
                Name = surrogate.displayName,
                displayName = surrogate.displayName,
                primaryMemberID = surrogate.primaryMemberID,
                email = surrogate.email,
                dob = surrogate.dob
            };

        }
    }


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
        private IAnimationEngine m_animationEngine = null;
        private IWebInterface m_webInterface = null;

        public bool IsRemotePlayer { get; set; } = false;

        private Renderer[] m_renderers = null;

        private Photon.Realtime.Player m_player;

        private void Awake()
        {
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
            m_experienceMachine = IOCCore.Resolve<IExperienceMachine>();
            m_network = IOCCore.Resolve<INetwork>();
            m_photonView = GetComponent<PhotonView>();
            m_animationEngine = IOCCore.Resolve<IAnimationEngine>();
            m_webInterface = IOCCore.Resolve<IWebInterface>();
            Debug.LogError("Subscribed: m_animationEngine.InteractionStateChange");
            m_animationEngine.InteractionStateChange += PlayerInteractionStateChange;
        }

        private void OnDestroy()
        {
            m_animationEngine.InteractionStateChange -= PlayerInteractionStateChange;
        }

        private IEnumerator Start()
        {
            yield return null;
            yield return null;

            if (Selection.Location == "PrivateMeeting")
            {
                Hide(true);
            }

            yield return new WaitForSeconds(2);
            m_player = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == playerId);
        }

        private void Hide(bool val)
        {
            if (m_renderers == null)
                m_renderers = GetComponentsInChildren<Renderer>();
            foreach (var item in m_renderers)
                item.enabled = !val;
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


        private void PlayerInteractionStateChange(object sender, AnimationPose pose)
        {
            if (pose != null)
                Hide(true);
            //else
            //    Hide(false);
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

        private bool IsAFriend()
        {
            foreach (var item in m_webInterface.Friends)
            {
                if (item.displayName == m_player.NickName)
                {
                    return true;
                }
            }
            return false;
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
                Data = playerId,
                IsValid = !IsAFriend()
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
            var targetPlayerId = (int)arg0.Data;
            var targetPlayer = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == targetPlayerId);
            object userId = null;

            if (targetPlayer != null)
            {
                var customProperties = targetPlayer.CustomProperties;
                if (customProperties != null)
                    customProperties.TryGetValue(Network.UserIdKey, out userId);
            }

            if (arg0.CommandType == MenuCommandType.SEND_FRIEND_REQUEST)
            {
                m_webInterface.SendFriendRequest((string)userId, (error, message) =>
                {
                    var jsonObject = JsonMapper.ToObject(message);
                    var success = !error && Convert.ToString(jsonObject["success"]) == "true";
                    if (success)
                    {
                        m_photonView.RPC("RPC_Request", targetPlayer, PhotonNetwork.LocalPlayer.ActorNumber, (int)arg0.CommandType);
                        m_uiEngine.CloseLastScreen();
                    }
                });
            }
            else
            {

                m_uiEngine.CloseLastScreen();

                if (arg0.CommandType == MenuCommandType.OTHER)
                    return;
                m_photonView.RPC("RPC_Request", targetPlayer, PhotonNetwork.LocalPlayer.ActorNumber, (int)arg0.CommandType);
            }
        }

        [PunRPC]
        private void RPC_Request(int senderId, int commandType)
        {
            MenuCommandType menuCommand = (MenuCommandType)commandType;
            Debug.LogError(senderId + " " + menuCommand.ToString());
            var targetPlayer = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == senderId);
            var inviterName = targetPlayer == null ? senderId.ToString() : targetPlayer.NickName;

            object userId = null;

            if (targetPlayer != null)
            {
                var customProperties = targetPlayer.CustomProperties;
                if (customProperties != null)
                    customProperties.TryGetValue(Network.UserIdKey, out userId);
            }

            m_uiEngine.Popup(new PopupInfo()
            {
                Title = "REQUEST",
                TimeInSeconds = 10,
                Content = (menuCommand == MenuCommandType.SEND_FRIEND_REQUEST ? "Friend Request" : "Invite For Sex") + " by " + inviterName,
                Action = (val) =>
                {
                    if (menuCommand == MenuCommandType.SEND_FRIEND_REQUEST)
                    {
                        if (val == PopupInfo.PopupAction.ACCEPT)
                        {

                            m_webInterface.AcceptFriendRequest((string)userId, (error, message) =>
                            {
                                var jsonObject = JsonMapper.ToObject(message);
                                var success = !error && Convert.ToString(jsonObject["success"]) == "true";
                                Debug.LogError(error + " " + success);
                                if (success)
                                {
                                    var friend = new Friend()
                                    {
                                        PlayerId = senderId,
                                        Name = targetPlayer.NickName
                                    };
                                    m_webInterface.AddFriend(friend);
                                    m_uiEngine.CloseLastScreen();
                                    m_photonView.RPC("RPC_InviteResponse", targetPlayer, PhotonNetwork.LocalPlayer.ActorNumber, val == PopupInfo.PopupAction.ACCEPT);
                                }
                            });
                        }
                        else
                        {
                            m_uiEngine.CloseLastScreen();
                            m_photonView.RPC("RPC_InviteResponse", targetPlayer, PhotonNetwork.LocalPlayer.ActorNumber, val == PopupInfo.PopupAction.ACCEPT);
                        }
                    }

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
        private void RPC_InviteResponse(int senderId, bool accepted)
        {
            Debug.LogError("RPC_InviteResponse: " + senderId + " " + accepted);

            var targetPlayer = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == senderId);

            if (accepted)
            {
                var friend = new Friend()
                {
                    PlayerId = senderId,
                    Name = targetPlayer.NickName
                };
                m_webInterface.AddFriend(friend);
            }

            m_uiEngine.DisplayMessage(new MessageArgs()
            {
                Persist = false,
                Block = true,
                Text = "Invite " + (accepted ? "accepted" : "rejected"),
                BackEnabled = false,
                OkEnabled = true,
            });
        }

        [PunRPC]
        private void RPC_PrivateRoom(int senderId, string roomName)
        {
            Debug.LogError("RPC_PrivateRoom: " + senderId + " " + roomName);
            m_uiEngine.DisplayMessage(new MessageArgs()
            {
                Persist = false,
                Block = true,
                Text = "Invited accepted.",
                BackEnabled = false,
                OkEnabled = true,
            });

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

