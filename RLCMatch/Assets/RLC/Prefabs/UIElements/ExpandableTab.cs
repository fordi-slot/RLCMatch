using Fordi.Common;
using Fordi.Core;
using Fordi.Standalone.UI;
using RLC.Animation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fordi.UI.MenuControl
{
    public class ExpandableTab : TabInteraction
    {
        [SerializeField]
        private MenuItemInfo[] m_subTabs;
        [SerializeField]
        private TabInteraction m_menuItem;
        [SerializeField]
        private Transform m_contentRoot;
        [SerializeField]
        private ToggleGroup m_toggleGroup;
        [SerializeField]
        private GameObject m_subItemPagePrefab;
        [SerializeField]
        private bool m_allowSwitchOff = false;

        protected List<IMenuItem> m_menuItems = new List<IMenuItem>();

        protected IUIEngine m_uiEngine;
        protected IUserInterface m_userInterface;
        protected Toggle m_toggle;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            if (m_contentRoot == null)
                m_contentRoot = transform.parent;
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
            m_userInterface = m_uiEngine.GetInterface();
        }

        public override void DataBind(IUserInterface userInterface, MenuItemInfo item)
        {
            base.DataBind(userInterface, item);

            if (item.Data is AnimationGroup animationGroup)
            {
                m_subTabs = Experience.ResourceToMenuItems(((AnimationGroup)item.Data).Resources);
            }
        }

        protected override void OnValueChange(bool val)
        {
            base.OnValueChange(val);

            if (val)
            {
                OpenMenu(m_userInterface, new MenuArgs()
                {
                    Items = m_subTabs == null ? new MenuItemInfo[] { } : m_subTabs,
                    Data = Page == null ? null: Page.transform.parent
                });
            }
            else
                Clear();
        }

        public virtual void OpenMenu(IUserInterface userInterface, MenuArgs args)
        {
            if (m_contentRoot == null)
                m_contentRoot = transform.parent;

            gameObject.SetActive(true);
            Clear();
            m_menuItems.Clear();
            gameObject.SetActive(true);
            foreach (var item in args.Items)
                SpawnMenuItem(item, m_menuItem, m_contentRoot);
        }

        public virtual IMenuItem SpawnMenuItem(MenuItemInfo menuItemInfo, TabInteraction prefab, Transform parent)
        {
            if (menuItemInfo.Action == null)
                menuItemInfo.Action = new MenuItemEvent();

            menuItemInfo.Action.AddListener(OnTabClick);

            var item = Instantiate(prefab, parent, false);
            IMenuItem menuItem = item;

            if (parent == transform.parent)
                item.transform.SetSiblingIndex(transform.GetSiblingIndex() + m_menuItems.Count + 1);

            menuItem.DataBind(m_userInterface, menuItemInfo);
            m_menuItems.Add(menuItem);

            TabInteraction tabInteraction = (TabInteraction)menuItem;
            var toggle = (Toggle)tabInteraction.Selectable;
            toggle.group = m_toggleGroup;
            if (m_menuItems.Count == 1 && !m_allowSwitchOff)
                toggle.isOn = true;

            InitializeTab((ITabItem)menuItem);

            return menuItem;
        }

        private IAnimationEngine m_animationEngine;
        private void OnTabClick(MenuClickArgs arg0)
        {
            if (m_clearingInitiated)
                return;

            //Debug.LogError(arg0.Name);
            if (m_animationEngine == null)
                m_animationEngine = IOCCore.Resolve<IAnimationEngine>();
            if (m_toggleGroup.AnyTogglesOn())
                m_animationEngine.SwitchToState(arg0.Name);
        }

        private void InitializeTab(ITabItem menuItem)
        {
            if (m_subItemPagePrefab == null)
                return;

            var args = new MessageArgs()
            {
                Title = menuItem.Item.Text,
                Data = menuItem.Item
            };
            menuItem.Page = SpawnPage(m_subItemPagePrefab, args).Gameobject;
        }

        protected virtual IScreen SpawnPage(GameObject screenPrefab, MenuArgs args)
        {
            Transform pageRoot = Page.transform.parent;
            pageRoot.gameObject.SetActive(true);
            var menu = Instantiate(screenPrefab, pageRoot).GetComponent<IScreen>();

            if (menu is MenuScreen menuScreen)
                menuScreen.OpenMenu(m_userInterface, args);
            else if (menu is MessageScreen messageScreen && args is MessageArgs messageArgs)
            {
                messageScreen.Init(m_userInterface, messageArgs);
            }

            return menu;
        }

        private bool m_clearingInitiated = false;
        public virtual void Clear()
        {
            m_clearingInitiated = true;
            for (int i = 0; i < m_menuItems.Count; i++)
            {
                Destroy(m_menuItems[i].Gameobject);
            }
            m_menuItems.Clear();
            m_clearingInitiated = false;
        }

    }
}
