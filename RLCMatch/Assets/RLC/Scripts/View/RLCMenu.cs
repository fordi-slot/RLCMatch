using Cornea.Web;
using ExitGames.Client.Photon;
using Fordi.ChatEngine;
using Fordi.Common;
using Fordi.Core;
using Fordi.UI;
using Fordi.UI.MenuControl;
using Photon.Pun;
using Photon.Realtime;
using RLC.Animation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RLC.UI
{
    public class RLCMenu : MenuScreen, IInRoomCallbacks
    {
        [Header("View Prefabs")]
        [SerializeField]
        private MenuScreen m_poseSelectionViewPrefab;
        [SerializeField]
        private MenuScreen m_friendsListPrefab;
        [SerializeField]
        Chat m_chatPrefab;

        private ICommonResource m_commonResource = null;
        private IAnimationEngine m_animationEngine = null;

        private Stack<IScreen> m_screensStack = new Stack<IScreen>();

        private IScreen m_friendsList, m_animationView;
        private Chat m_chat;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_commonResource = IOCCore.Resolve<ICommonResource>();
            m_animationEngine = IOCCore.Resolve<IAnimationEngine>();
            PhotonNetwork.AddCallbackTarget(this);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override void OpenMenu(IUserInterface userInterface, MenuArgs args)
        {
            m_userInterface = userInterface;
            Blocked = args.Block;
            Persist = args.Persist;
            ToggleChat(true);

#if LOCAL_TEST
            OpenAnimationsList();
            return;
#endif
            if (SceneManager.GetActiveScene().name == Fordi.Networking.Network.PrivateMeetingLocation && PhotonNetwork.PlayerList.Length > 1)
                OpenAnimationsList();
        }

        public void ToggleFriendsListView()
        {
            try
            {
                if (m_friendsList != null && m_friendsList.Gameobject != null)
                {
                    m_friendsList.Close();
                    if (m_animationView != null)
                        m_animationView.Reopen();
                    return;
                }
                else
                {
                    if (m_animationView != null)
                        m_animationView.Deactivate();

                    MenuItemInfo[] items = Experience.ResourceToMenuItems(m_webInterface.Friends);

                    //Experience.ResourceToMenuItems(m_commonResource.GetResource(ResourceType.USER, ""));


                    MenuArgs args = new MenuArgs()
                    {
                        Items = items
                    };
                    m_friendsList = SpawnPage(m_friendsListPrefab, args);
                }
            }
            catch
            {
                if (m_animationView != null)
                    m_animationView.Deactivate();

                MenuItemInfo[] items = Experience.ResourceToMenuItems(m_webInterface.Friends);

                //Experience.ResourceToMenuItems(m_commonResource.GetResource(ResourceType.USER, ""));


                MenuArgs args = new MenuArgs()
                {
                    Items = items
                };
                m_friendsList = SpawnPage(m_friendsListPrefab, args);
            }
        }

        private void OpenAnimationsList()
        {
            //Temp code:
            MenuItemInfo[] items = new MenuItemInfo[] { };
            items = Experience.GetCategoryMenu(m_commonResource.GetCategories(ResourceType.ANIMATION), ResourceType.ANIMATION);

            MenuArgs poseMenu = new MenuArgs()
            {
                Items = items
            };
            m_animationView = SpawnPage(m_poseSelectionViewPrefab, poseMenu);
        }

        public void ToggleChat(bool val)
        {
            if (!val)
            {
                if (m_chat != null)
                    m_chat.gameObject.SetActive(false);
                return;
            }
            if (m_chat == null)
                m_chat = Instantiate(m_chatPrefab, transform);
            else
                m_chat.gameObject.SetActive(true);
        }

        protected virtual IScreen SpawnPage(IScreen screenPrefab, MenuArgs args)
        {
            try
            {
                if (m_screensStack.Count > 0 && m_screensStack.Peek().Persist)
                {
                    m_screensStack.Peek().Deactivate();
                }
            }
            catch
            {

            }

            m_contentRoot.gameObject.SetActive(true);
            var menu = Instantiate(screenPrefab.Gameobject, m_contentRoot).GetComponent<IScreen>();

            if (menu is MenuScreen menuScreen)
                menuScreen.OpenMenu(m_userInterface, args);
            else if (menu is MessageScreen messageScreen && args is MessageArgs messageArgs)
            {
                messageScreen.Init(m_userInterface, messageArgs);
            }

            m_screensStack.Push(menu);

            return menu;
        }

#region IN_ROOM_CALLBACK
        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogError("OnPlayerEnteredRoom " + newPlayer.NickName);
            if (SceneManager.GetActiveScene().name == Fordi.Networking.Network.PrivateMeetingLocation && m_animationView == null)
                OpenAnimationsList();
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            m_animationEngine.StopAll();
            if (m_animationView != null)
                m_animationView.Close();
        }

        public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
        }
#endregion
    }
}
