using Fordi.Common;
using Fordi.Core;
using Fordi.UI;
using Fordi.UI.MenuControl;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fordi.Standalone.UI
{

    public interface ITabItem : IMenuItem
    {
        GameObject Page { get; set; }
    }

    public class TabInteraction : ToggleInteraction, ITabItem
    {
        [SerializeField]
        private GameObject m_page;
        [SerializeField]
        private TextMeshProUGUI m_description;

        protected MenuItemInfo m_item;
        public MenuItemInfo Item
        {
            get { return m_item; }
        }

        public GameObject Gameobject { get { return gameObject; } }

        public GameObject Page
        {
            get
            {
                return m_page;
            }
            set
            {
                m_page = value;
                if (m_page != null)
                    m_page.gameObject.SetActive(((Toggle)selectable).isOn);
            }
        }

        private IUserInterface m_userInterface;

        public static EventHandler TabChangeInitiated = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            var toggle = (Toggle)selectable;
            if (toggle != null)
                toggle.onValueChanged.AddListener(OnValueChange);
            m_experienceMachine = IOCCore.Resolve<IExperienceMachine>();
        }

        private bool m_destroyInitiated = false;
        protected override void OnDestroyOverride()
        {
            m_destroyInitiated = true;
            base.OnDestroyOverride();
            var toggle = (Toggle)selectable;
            if (toggle != null)
                toggle.onValueChanged.RemoveAllListeners();
            if (m_page != null && m_page.gameObject != null)
                Destroy(m_page.gameObject);
        }

        protected override void OnValueChange(bool val)
        {
            base.OnValueChange(val);
            if (m_page != null)
                m_page.SetActive(val);
        }

        public virtual void DataBind(IUserInterface userInterface, MenuItemInfo item)
        {
            m_item = item;
            m_userInterface = userInterface;

            if (m_item != null)
            {
                if (m_icon != null)
                {
                    m_icon.sprite = m_item.Icon;
                    m_icon.gameObject.SetActive(m_item.Icon != null || (m_item.Data != null && m_item.Data is ColorResource));
                }
                m_text.text = m_item.Text;
            }
            else
            {
                if (m_icon != null)
                {
                    m_icon.sprite = null;
                    m_icon.gameObject.SetActive(false);
                }
                m_text.text = string.Empty;
            }

            if (m_item.Validate == null)
                m_item.Validate = new MenuItemValidationEvent();

            if (m_experienceMachine == null)
                m_experienceMachine = IOCCore.Resolve<IExperienceMachine>();
            if (m_appTheme == null)
                m_appTheme = IOCCore.Resolve<IAppTheme>();

            m_item.Validate.AddListener(m_experienceMachine.CanExecuteMenuCommand);
            m_item.Validate.AddListener((args) => args.IsValid = m_item.IsValid);

            var validationResult = IsValid();
            if (validationResult.IsVisible)
            {
                var toggle = (Toggle)selectable;

                if (m_item.IsValid)
                {
                    if (toggle.isOn)
                        m_text.color = overrideColor ? overriddenHighlight : m_appTheme.GetSelectedTheme(m_platform).buttonHighlightTextColor;
                    else
                        m_text.color = overrideColor ? overriddenHighlight : m_appTheme.GetSelectedTheme(m_platform).buttonNormalTextColor;
                }
                else
                {
                    m_text.color = m_appTheme.GetSelectedTheme(m_platform).buttonDisabledTextColor;

                }

                if (m_item.Action == null)
                    m_item.Action = new MenuItemEvent();
                m_item.Action.AddListener(m_experienceMachine.ExecuteMenuCommand);
                toggle.onValueChanged.AddListener((val) =>
                {
                    if (m_destroyInitiated)
                        return;

                    if (val)
                        m_item.Action.Invoke(new MenuClickArgs(m_item.Path, m_item.Text, m_item.Command, m_item.CommandType, m_item.Data));
                    else if (toggle.group != null && !toggle.group.AnyTogglesOn())
                        m_item.Action.Invoke(new MenuClickArgs(m_item.Path, m_item.Text, m_item.Command, m_item.CommandType, null));
                });
            }

            if (m_root != null)
                m_root.gameObject.SetActive(validationResult.IsVisible);
            else
                gameObject.SetActive(validationResult.IsVisible);
            selectable.interactable = validationResult.IsValid;

            //if (m_allowTextScroll)
            //    StartCoroutine(InitializeTextScroll());

            if (m_description != null)
                m_description.text = ((ResourceComponent)item.Data).Description;
        }

        protected MenuItemValidationArgs IsValid()
        {
            if (m_item == null)
            {
                return new MenuItemValidationArgs(m_item.Command) { IsValid = false, IsVisible = false };
            }

            if (m_item.Validate == null)
            {
                return new MenuItemValidationArgs(m_item.Command) { IsValid = true, IsVisible = true };
            }

            MenuItemValidationArgs args = new MenuItemValidationArgs(m_item.Command);
            m_item.Validate.Invoke(args);
            return args;
        }
    }
}
