using AL.UI;
using Fordi.Core;
using Fordi.Standalone.UI;
using Fordi.UI;
using Fordi.UI.MenuControl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AL.UI
{
    public class TabbedScreen : MenuScreen
    {
        [SerializeField]
        protected ToggleGroup m_toggleGroup;
        [SerializeField]
        protected Transform m_pageRoot;
        [SerializeField]
        protected bool m_allowSwitchOff = true;

        protected List<IScreen> m_pages = new List<IScreen>();

        public override IMenuItem SpawnMenuItem(MenuItemInfo menuItemInfo, GameObject prefab, Transform parent)
        {
            if (menuItemInfo.Action == null)
                menuItemInfo.Action = new MenuItemEvent();

            menuItemInfo.Action.AddListener(OnTabClick);

            var menuItem = base.SpawnMenuItem(menuItemInfo, prefab, parent);
            TabInteraction tabInteraction = (TabInteraction)menuItem;
            var toggle = (Toggle)tabInteraction.Selectable;
            toggle.group = m_toggleGroup;
            if (m_menuItems.Count == 1 && !m_allowSwitchOff)
                toggle.isOn = true;

            InitializeTab((ITabItem)menuItem);
            return menuItem;
        }

        public override void OpenMenu(IUserInterface userInterface, MenuArgs args)
        {
            m_userInterface = userInterface;
            Blocked = args.Block;
            Persist = args.Persist;

            foreach (var item in m_pages)
                item.Close();
            m_toggleGroup.allowSwitchOff = true;
            base.OpenMenu(userInterface, args);
            m_toggleGroup.allowSwitchOff = m_allowSwitchOff;

            //var obj = Instantiate(m_menuItem, m_contentRoot, false);
        }

        protected virtual void InitializeTab(ITabItem tabItem)
        {

        }

        protected virtual IScreen SpawnPage(IScreen screenPrefab, MenuArgs args)
        {
            m_pageRoot.gameObject.SetActive(true);
            var menu = Instantiate(screenPrefab.Gameobject, m_pageRoot).GetComponent<IScreen>();

            if (menu is MenuScreen menuScreen)
                menuScreen.OpenMenu(m_userInterface, args);
            else if (menu is MessageScreen messageScreen && args is MessageArgs messageArgs)
            {
                messageScreen.Init(m_userInterface, messageArgs);
            }

            return menu;
        }


        protected virtual void OnTabClick(MenuClickArgs arg0)
        {
            //Debug.Log("OnTabClick " + arg0.Name);
        }
    }
}
