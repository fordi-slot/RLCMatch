using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System;
using Fordi.Core;
using Fordi.Common;
using UnityEngine.SceneManagement;
using Fordi.UI.MenuControl;
using System.Linq;
using ExitGames.Client.Photon;
using Cornea.Web;
using Fordi.UI;
using UniRx;


namespace Fordi.Networking
{
    public interface INetwork
    {
        [Obsolete("Temporary work for testing purpose")]
        void EnterMeeting();
        void EnterPrivateRoom(string roomName, bool host);
        void CreateRoom(string roomName);
        void JoinRoom(string roomName);
        void LeaveRoom();
        EventHandler RoomListUpdateEvent { get; set; }
        void ToggleScreenStreaming(bool val);
        RemotePlayer GetRemotePlayer(int actorNumber);
    }

    public class Network : MonoBehaviourPunCallbacks, INetwork, IOnEventCallback
    {
        [SerializeField]
        private RemotePlayer m_maleRemotePlayerPrefab = null;
        [SerializeField]
        private RemotePlayer m_femaleRemotePlayerPrefab = null;

        public const byte trailBegin = 137;
        public const byte trailFinish = 138;
        public const byte deletePreviousTrail = 139;
        public const byte whiteboardNoteBegan = 140;
        public const byte videoMuteToggle = 141;

        public const string MeetingRoom = "Meeting";
        public const string LobbyRoom = "Lobby";
        public const string ActorNumberString = "ActorNumber";
        public const string GenderKey = "Gender";
        public const string OculusIDString = "OculusID";
        public const string PrivateMeetingLocation = "PrivateMeeting";

        public enum RoomStatus
        {
            NONE,
            PUBLIC,
            PRIVATE
        }


        private IUIEngine m_uiEngine = null;
        private IExperienceMachine m_experienceMachine = null;
        private IWebInterface m_webInterface = null;

        private static List<RoomInfo> m_rooms = new List<RoomInfo>();
        public static RoomInfo[] Rooms { get { return m_rooms.ToArray(); } }

        public EventHandler RoomListUpdateEvent { get; set; }

        private Dictionary<int, RemotePlayer> m_remotePlayers = new Dictionary<int, RemotePlayer>();

        private bool m_debug = false;

        private static RoomStatus m_roomStatus = RoomStatus.NONE;

        #region INITIALIZATIONS
        private void Awake()
        {
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
            m_experienceMachine = IOCCore.Resolve<IExperienceMachine>();
            m_webInterface = IOCCore.Resolve<IWebInterface>();
            m_webInterface.OnUserDataUpdate += UserDataUpdate;

            if (!PhotonNetwork.IsConnectedAndReady)
                PhotonNetwork.ConnectUsingSettings();
        }

        private void OnDestroy()
        {
            m_webInterface.OnUserDataUpdate -= UserDataUpdate;
        }

        private void UserDataUpdate(object sender, EventArgs e)
        {
            ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable();
            playerCustomProperties.Add(ActorNumberString, PhotonNetwork.LocalPlayer.ActorNumber);
            playerCustomProperties.Add(GenderKey, m_webInterface.UserInfo.gender);
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerCustomProperties);
            PhotonNetwork.NickName = m_webInterface.UserInfo.name;
        }

        private static bool m_playersSpawned = false;
        private void OnLevelWasLoaded(int level)
        {
            if (PhotonNetwork.InRoom && !m_playersSpawned)
            {
                Log("Level: " + level + " In room: " + PhotonNetwork.InRoom);
                Observable.TimerFrame(20).Subscribe(_ => RaisePlayerSpawnEvent());
                //m_playersSpawned = true;
            }
            //    if (PhotonNetwork.InRoom)
            //        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Cube"), Vector3.one, Quaternion.identity, 0);
        }
        #endregion

        #region CORE_NETWORKING
        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Log("OnConnectedToMaster");
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.JoinLobby();
        }

        private static bool m_autoJoined = true;

        private Action m_joinedLobby = null;

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Log("OnJoinedLobby");


            //PhotonNetwork.LocalPlayer.NickName = m_webInterface.UserInfo.userName;



            m_joinedLobby?.Invoke();
            //EnterMeeting();
        }

        public void EnterMeeting()
        {
            m_roomStatus = RoomStatus.PRIVATE;
            if (PhotonNetwork.CountOfRooms > 0)
            {
                JoinRoom("Lobby");
            }
            else
                CreateRoom("Lobby");
        }

        public void EnterPrivateRoom(string roomName, bool host)
        {
            if (PhotonNetwork.InRoom)
            {
                m_joinedLobby = () =>
                {
                    m_roomStatus = RoomStatus.PRIVATE;

                    if (host)
                        CreateRoom(roomName);
                    else
                        JoinRoom(roomName);
                };
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                m_roomStatus = RoomStatus.PRIVATE;

                if (host)
                    CreateRoom(roomName);
                else
                    JoinRoom(roomName);
            }
        }

        public void CreateRoom(string roomName)
        {
            Debug.LogError("CreateRoom: " + PhotonNetwork.IsConnectedAndReady);
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                m_uiEngine.DisplayResult(new Error()
                {
                    ErrorCode = Error.E_InvalidOperation,
                    ErrorText = "Not connected to multiplayer server yet."
                });
                return;
            }

            m_uiEngine.DisplayProgress("Creating room: " + roomName);
            RoomOptions options = new RoomOptions
            {
                IsVisible = true,
                IsOpen = true,
                MaxPlayers = 10
            };

            PhotonNetwork.CreateRoom(roomName, options);
        }

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            Log("Created");
            m_rooms.Add(PhotonNetwork.CurrentRoom);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            Debug.LogError("OnJoinedRoom");
            m_autoJoined = true;

            if (m_roomStatus == RoomStatus.PUBLIC)
            {
                Selection.Location = MeetingRoom;
                Selection.ExperienceType = ExperienceType.MEETING;
                if (PhotonNetwork.IsMasterClient)
                    m_experienceMachine.LoadExperience();
            }
            else
            {
                Selection.Location = PrivateMeetingLocation;
                Selection.ExperienceType = ExperienceType.MEETING;
                if (PhotonNetwork.IsMasterClient)
                    m_experienceMachine.LoadExperience();
            }
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            m_roomStatus = RoomStatus.NONE;
            base.OnCreateRoomFailed(returnCode, message);
            Error error = new Error(Error.E_Exception);
            error.ErrorText = message;
            m_uiEngine.DisplayResult(error);
            Log(message);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Error error = new Error(Error.E_Exception);
            error.ErrorText = message;
            m_uiEngine.DisplayResult(error);
            base.OnJoinRoomFailed(returnCode, message);
        }

        public void JoinRoom(string roomName)
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                m_uiEngine.DisplayResult(new Error()
                {
                    ErrorCode = Error.E_InvalidOperation,
                    ErrorText = "Not connected to multiplayer server yet."
                });
                return;
            }

            m_uiEngine.DisplayProgress("Joining room: " + roomName);
            PhotonNetwork.JoinRoom(roomName);
        }

        public void LeaveRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            m_rooms = roomList.Where(room => !room.RemovedFromList).ToList();
            RoomListUpdateEvent?.Invoke(this, EventArgs.Empty);
            //if (m_rooms.Count > 0)
            //    JoinRoom(m_rooms[0].Name);
            //Debug.LogError("Recieved room udate: " + m_rooms.Count);
            //foreach (var item in m_rooms)
            //{
            //    Log(item.Name);
            //}
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            Destroy(m_remotePlayers[otherPlayer.ActorNumber].gameObject);
            m_remotePlayers.Remove(otherPlayer.ActorNumber);
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            
            //Selection.Location = LobbyRoom;
            //Selection.ExperienceType = ExperienceType.LOBBY;
            //m_uiEngine.Close();
            //m_experienceMachine.LoadExperience();
        }


        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
               
            }
        }

        private void Log(string message)
        {
            if (m_debug)
                Debug.LogError(message);
        }
        #endregion

        #region SPAWN
        [PunRPC]
        private void RPC_SpawnPlayer(int senderId, int playerViewId, bool firstHand)
        {
            Debug.LogError("RPC_SpawnPlayer: " + senderId + " " + firstHand);

            var player = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == senderId);

            object gender = Gender.MALE;

            if (player != null)
            {
                var customProperties = player.CustomProperties;
                if (customProperties != null)
                    customProperties.TryGetValue(GenderKey, out gender);
            }

            var playerPrefab = (Gender)gender == Gender.MALE ? m_maleRemotePlayerPrefab : m_femaleRemotePlayerPrefab;

            var remotePlayer = Instantiate(playerPrefab);


            remotePlayer.Setup(senderId, playerViewId);
            m_remotePlayers[senderId] = remotePlayer;
            if (firstHand)
                RaiseSecondHandPlayerSpawnEvent(senderId);

        }

        public void RaiseSecondHandPlayerSpawnEvent(int targetPlayerId)
        {
            Debug.LogError("RaisePlayerSpawnEvent: " + SceneManager.GetActiveScene().name + " " + m_experienceMachine.Player.PlayerViewId);
            var targetPlayer = Array.Find(PhotonNetwork.PlayerList, item => item.ActorNumber == targetPlayerId);
            photonView.RPC("RPC_SpawnPlayer", targetPlayer, PhotonNetwork.LocalPlayer.ActorNumber, m_experienceMachine.Player.PlayerViewId, false);
        }

        public void RaisePlayerSpawnEvent()
        {
            Debug.LogError("RaisePlayerSpawnEvent: " + SceneManager.GetActiveScene().name + " " + m_experienceMachine.Player.PlayerViewId);

            try
            {
                var playerSync = m_experienceMachine.Player.PlayerController.GetComponent<OvrPlayerSync>();
                playerSync.Init(true, false, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            catch (NullReferenceException)
            {
                Debug.LogError("Networking scripts not steup properly on local player");
                return;
            }

            //RaiseEventOptions options = new RaiseEventOptions
            //{
            //    CachingOption = EventCaching.AddToRoomCache,
            //    Receivers = ReceiverGroup.All
            //};
            photonView.RPC("RPC_SpawnPlayer", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, m_experienceMachine.Player.PlayerViewId, true);
        }

        public void ToggleScreenStreaming(bool val)
        {
            photonView.RPC("RPC_RemoteScreenShareNotification", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber, val);
        }

        public RemotePlayer GetRemotePlayer(int actorNumber)
        {
            if (m_remotePlayers.ContainsKey(actorNumber))
                return m_remotePlayers[actorNumber];
            return null;
        }
        #endregion
    }
}
