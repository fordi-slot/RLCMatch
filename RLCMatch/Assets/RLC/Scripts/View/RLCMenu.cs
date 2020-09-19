using Cornea.Web;
using Fordi.Common;
using Fordi.Core;
using Fordi.UI;
using Fordi.UI.MenuControl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RLC.UI
{
    public class RLCMenu : MenuScreen
    {
        [Header("View Prefabs")]
        [SerializeField]
        private MenuScreen m_poseSelectionViewPrefab;
        [SerializeField]
        private MenuScreen m_friendsListPrefab;

        private ICommonResource m_commonResource = null;

        private Stack<IScreen> m_screensStack = new Stack<IScreen>();

        private IScreen m_friendsList, m_animationView;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_commonResource = IOCCore.Resolve<ICommonResource>();
        }


        public override void OpenMenu(IUserInterface userInterface, MenuArgs args)
        {
            m_userInterface = userInterface;
            Blocked = args.Block;
            Persist = args.Persist;

            if (Selection.Location == Fordi.Networking.Network.PrivateMeetingLocation)
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

                    MenuItemInfo[] items = Experience.ResourceToMenuItems(WebInterface.s_friends.ToArray());

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

                MenuItemInfo[] items = Experience.ResourceToMenuItems(WebInterface.s_friends.ToArray());

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
    }
}
