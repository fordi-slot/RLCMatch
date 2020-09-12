using Fordi.Common;
using Fordi.Core;
using Fordi.UI;
using Fordi.UI.MenuControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RLC.UI
{
    public class RLCMenu : MenuScreen
    {
        [Header("View Prefabs")]
        [SerializeField]
        private MenuScreen m_poseSelectionViewPrefab;

        private ICommonResource m_commonResource = null;

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

            MenuArgs poseMenu = new MenuArgs()
            {
                Items = Experience.GetCategoryMenu(m_commonResource.GetCategories(ResourceType.ANIMATION), ResourceType.ANIMATION),
            };
            SpawnPage(m_poseSelectionViewPrefab, poseMenu);
        }

        protected virtual IScreen SpawnPage(IScreen screenPrefab, MenuArgs args)
        {
            m_contentRoot.gameObject.SetActive(true);
            var menu = Instantiate(screenPrefab.Gameobject, m_contentRoot).GetComponent<IScreen>();

            if (menu is MenuScreen menuScreen)
                menuScreen.OpenMenu(m_userInterface, args);
            else if (menu is MessageScreen messageScreen && args is MessageArgs messageArgs)
            {
                messageScreen.Init(m_userInterface, messageArgs);
            }

            return menu;
        }
    }
}
