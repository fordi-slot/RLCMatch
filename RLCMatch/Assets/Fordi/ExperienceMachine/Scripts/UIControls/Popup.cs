﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Fordi.Common;
using Fordi.UI.MenuControl;
using Fordi.Sync;
using Fordi.Core;
using UniRx;

namespace Fordi.UI
{
    public class PopupInfo : MenuArgs
    {
        public Sprite Preview;
        public string Content;
        public Action<PopupAction> Action;
        public int? TimeInSeconds = null;
        public enum PopupAction
        {
            ACCEPT,
            REJECT,
            CANCEL
        }
    }

    public class Popup : MonoBehaviour, IScreen
    {
        [SerializeField]
        private TextMeshProUGUI m_title, m_text;
        [SerializeField]
        private Image m_icon;
        [SerializeField]
        private Button m_okButton, m_closeButton;
        [SerializeField]
        private GameObject m_loader = null;

        public bool Blocked { get; private set; }
        public bool Persist { get; private set; }

        private Action m_closed = null;
        private Action<PopupInfo.PopupAction> m_action = null;

        public GameObject Gameobject { get { return gameObject; } }

        private IScreen m_pair = null;
        public IScreen Pair { get { return m_pair; } set { m_pair = value; } }

        private Vector3 m_localScale = Vector3.zero;

        [SerializeField]
        private List<SyncView> m_synchronizedElements = new List<SyncView>();

        private Action<PopupInfo.PopupAction> m_onPopupClck;

        private IUIEngine m_uiEngine;

        private PopupInfo m_info;

        private PopupInfo.PopupAction m_selectedAction = PopupInfo.PopupAction.ACCEPT; 

        private void Awake()
        {
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
            if (m_localScale == Vector3.zero)
                m_localScale = transform.localScale;

            foreach (var item in m_synchronizedElements)
            {
                FordiNetwork.RegisterPhotonView(item);
            }
        }

        public void Show(PopupInfo popupInfo)
        {
            m_info = popupInfo;
            m_onPopupClck = popupInfo.Action;
            gameObject.SetActive(true);
            m_action = popupInfo.Action;

            Blocked = popupInfo.Block;
            Persist = popupInfo.Persist;

            if (m_title != null && !string.IsNullOrEmpty(popupInfo.Title))
                m_title.text = popupInfo.Title;
            else if(m_title != null)
                m_title.text = "";

            if (!string.IsNullOrEmpty((string)popupInfo.Content))
                m_text.text = (string)popupInfo.Content;
            else
                m_text.text = "";
            if (popupInfo.Preview != null)
            {
                m_icon.sprite = popupInfo.Preview;
                m_icon.transform.parent.gameObject.SetActive(true);
            }
            else if(m_icon != null)
                m_icon.transform.parent.gameObject.SetActive(false);
            if (m_okButton != null)
                m_okButton.onClick.AddListener(() =>
                {
                    m_selectedAction = PopupInfo.PopupAction.ACCEPT;
                    m_uiEngine.CloseLastScreen();
                });
            if (m_closeButton != null)
                m_closeButton.onClick.AddListener(() =>
                {
                    m_selectedAction = PopupInfo.PopupAction.CANCEL;
                    m_uiEngine.CloseLastScreen();
                });
            if (m_info.TimeInSeconds.HasValue)
            {
                Observable.Timer(new TimeSpan(0, 0, m_info.TimeInSeconds.Value)).Subscribe(_ =>
                {
                    if (this != null && this.gameObject != null)
                        m_uiEngine.CloseLastScreen();
                });
            }
        }

        public void Close()
        {
            m_action?.Invoke(m_selectedAction);
            m_closed?.Invoke();
            Destroy(gameObject);
        }

        public void Reopen()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            m_loader.SetActive(false);
            gameObject.SetActive(false);
        }

        public void ShowPreview(Sprite sprite)
        {
            
        }

        public void ShowTooltip(string tooltip)
        {
        
        }

        public void OkClick()
        {
            m_action?.Invoke(PopupInfo.PopupAction.ACCEPT);
            Destroy(gameObject);
        }

        public void Hide()
        {
            transform.localScale = Vector3.zero;
        }

        public void UnHide()
        {
            transform.localScale = m_localScale;
        }

        public void AttachSyncView(SyncView syncView)
        {
            if (m_synchronizedElements.Contains(syncView))
                m_synchronizedElements.Add(syncView);
        }

        public void DisplayResult(Error error)
        {
            m_loader.SetActive(false);

            if (error.HasError)
                m_text.text = error.ErrorText.Style(ExperienceMachine.ErrorTextColorStyle);
            else
                m_text.text = error.ErrorText.Style(ExperienceMachine.CorrectTextColorStyle);

            if (Pair != null)
                Pair.DisplayResult(error);
        }

        public void DisplayProgress(string text)
        {
            m_loader.SetActive(true);
            m_text.text = text.Style(ExperienceMachine.ProgressTextColorStyle);

            if (Pair != null)
                Pair.DisplayProgress(text);
        }

        public void AcceptClick()
        {
            m_uiEngine.CloseLastScreen();
            m_onPopupClck?.Invoke(PopupInfo.PopupAction.ACCEPT);
        }

        public void RejectClick()
        {
            m_uiEngine.CloseLastScreen();
            m_onPopupClck?.Invoke(PopupInfo.PopupAction.REJECT);
        }

        public void CancelClick()
        {
            m_uiEngine.CloseLastScreen();
            m_onPopupClck?.Invoke(PopupInfo.PopupAction.CANCEL);
        }
    }
}
