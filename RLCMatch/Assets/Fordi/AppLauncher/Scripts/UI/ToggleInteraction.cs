using Fordi.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fordi.UI
{
    public class ToggleInteraction : VRUIInteraction
    {
        [SerializeField]
        protected Image m_icon;
        [SerializeField]
        protected Theme m_skin;

        [SerializeField]
        protected GameObject m_onImage, m_offImage;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            var toggle = (Toggle)selectable;
            toggle.onValueChanged.AddListener(OnValueChange);

            if (m_onImage)
                m_onImage.SetActive(toggle.isOn);
            if (m_offImage)
                m_offImage.SetActive(!toggle.isOn);
        }

        public override void Init()
        {
            base.Init();
            var toggle = (Toggle)selectable;
            if (toggle.isOn)
            {
                ToggleBackgroundHighlight(true);
                ToggleOutlineHighlight(true);
                Pop(true);
            }
            if (m_image != null)
                m_image.gameObject.SetActive(toggle.isOn);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            var toggle = (Toggle)selectable;
            toggle.onValueChanged.RemoveAllListeners();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            var toggle = (Toggle)selectable;
            if (toggle.interactable && !toggle.isOn)
                base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            var toggle = (Toggle)selectable;
            if (toggle.interactable && !toggle.isOn)
                base.OnPointerExit(eventData);
        }

        public override void ToggleBackgroundHighlight(bool val)
        {
            base.ToggleBackgroundHighlight(val);

            Theme skin = m_skin;
            if (skin == null)
                skin = m_appTheme.GetSelectedTheme(m_platform);

            if (selection)
                selection.color = val ? skin.toggleSelectionColor : skin.toggleNormalColor;
            if (m_icon)
                m_icon.color = val ? skin.ToggleCheckHighlight : skin.ToggleCheckNormal;

        }

        public override void ToggleOutlineHighlight(bool val)
        {

            Theme skin = m_skin;
            if (skin == null)
                skin = m_appTheme.GetSelectedTheme(m_platform);


            if (m_text)
            {
                if (val && selectable.interactable)
                    m_text.color = skin.buttonHighlightTextColor;
                else
                    m_text.color = overrideColor ? overriddenHighlight : skin.buttonNormalTextColor;
            }

            if (m_image != null)
            {
                if (val && selectable.interactable)
                    m_image.color = skin.buttonHighlightTextColor;
                else
                    m_image.color = skin.buttonNormalTextColor;
            }

            if (m_additionalImage != null)
            {
                if (val && selectable.interactable)
                    m_additionalImage.color = skin.buttonHighlightTextColor;
                else
                    m_additionalImage.color = skin.buttonNormalTextColor;
            }

            if (m_toggleImage != null)
            {
                if (val && selectable.interactable)
                    m_toggleImage.color = skin.buttonHighlightTextColor;
                else
                    m_toggleImage.color = skin.buttonNormalTextColor;
            }

            //if (overrideColor)
            //{
            //    selection.color = val ? overriddenHighlight : Color.white;
            //}
        }

        protected virtual void OnValueChange(bool val)
        {
            ToggleBackgroundHighlight(val);
            ToggleOutlineHighlight(val);
            Pop(val);

            if (m_image != null)
                m_image.gameObject.SetActive(val);

            try
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            catch (System.Exception)
            {


            }

            if (m_onImage)
                m_onImage.SetActive(val);
            if (m_offImage)
                m_offImage.SetActive(!val);
        }

        protected override void OnDisableOverride()
        {
            Toggle toggle = (Toggle)selectable;

            if (toggle != null && !toggle.isOn)
            {
                ToggleOutlineHighlight(false);
                ToggleBackgroundHighlight(false);
                Pop(false);
            }
            try
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            catch (System.Exception)
            {


            }
        }
    }
}