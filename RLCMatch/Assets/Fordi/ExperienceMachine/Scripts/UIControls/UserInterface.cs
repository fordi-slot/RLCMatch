﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Fordi.Common;
using Fordi.Core;
using AudioType = Fordi.Core.AudioType;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fordi.Meeting;
using Fordi.Meetings.UI;
using Fordi.UI.MenuControl;
using RLC.View;

namespace Fordi.UI
{
    public class MenuArgs
    {
        public MenuItemInfo[] Items = new MenuItemInfo[] { };
        public AudioClip AudioClip = null;
        public Action<string[]> OnAction = null;
        public string Title = "";
        public bool Block = false;
        public bool BackEnabled = true;
        public bool Persist = true;
        public Vector2? Position = Vector2.zero;
        public object Data = null;
    }

    public class GridArgs : MenuArgs
    {
        public string RefreshCategory = null;
    }

    public class MeetingArgs : MenuArgs
    {
        public MeetingInfo MeetingInfo;
    }

    public class CalendarArgs : MenuArgs
    {
        public enum CalendarTypye
        {
            PREDATE,
            POSTDATE
        }
        public ITimeForm TimeForm;
        public Action<string> OnClick;
        public CalendarTypye CalendarType;
        public DateTime? ReferenceTime;
    }

    public interface IUserInterface
    {
        bool IsOpen { get; }
        BaseInputModule InputModule { get; }
        Platform Platform { get; }
        Canvas RootCanvas { get; }

        IScreen OpenMenu(MenuArgs args);
        IScreen OpenGridMenu(GridArgs args);
        IScreen OpenInventory(GridArgs args);
        IScreen OpenSettingsInterface(AudioClip clip);
        IScreen OpenAnnotationInterface(GridArgs args);
        IScreen OpenCalendar(CalendarArgs args);
        IScreen OpenMeeting(MeetingArgs args);
        IScreen OpenMeetingForm(FormArgs args);
        IScreen OpenSignupPage(MenuArgs args);
        IScreen OpenLoginPage(MenuArgs args);
        IScreen OpenContextUI(MenuArgs args);
        IScreen Popup(PopupInfo popupInfo);
        IScreen OpenForm(FormArgs args);
        IScreen DisplayMessage(MessageArgs args);
        IScreen ConfirmationPopup(PopupInfo args);
        IScreen DisplayResult(Error error, bool freshScreen = false);
        IScreen DisplayProgress(string text, bool freshScreen = false);
        IScreen Block(string message, bool includeRoot = false);

        void CloseLastScreen();
        void Close(IScreen screen);
        void Close();
        void GoBack();
        void ShowTooltip(string text);
        void ShowPreview(Sprite sprite);
        void DeactivateUI();
        void ShowUI();
        void Hide();
        void Unhide();
        void Unblock();
    }

    public abstract class UserInterface : MonoBehaviour, IUserInterface
    {
        #region INSPECTOR_REFRENCES
        [SerializeField]
        protected Platform m_platform;
        [SerializeField]
        protected MenuScreen m_mainMenuPrefab, m_gridMenuPrefab, m_inventoryMenuPrefab, m_textBoxPrefab, m_formPrefab, m_annotationInterface;
        [SerializeField]
        protected MenuScreen m_contextMenuPrefab;
        [SerializeField]
        protected MeetingPage m_meetingPagePrefab;
        [SerializeField]
        protected MeetingForm m_meetingFormPrefab;
        [SerializeField]
        protected SignupForm m_signupFormPrefab;
        [SerializeField]
        protected LoginForm m_loginFormPrefab;
        [SerializeField]
        protected SettingsPanel m_settingsInterfacePrefab;
        [SerializeField]
        protected MessageScreen m_genericLoader, m_messageBoxPrefab;
        [SerializeField]
        protected CalendarController m_calendarPrefab;
        [SerializeField]
        protected Transform m_screensRoot;
        [SerializeField]
        protected Popup m_popupPrefab;
        [SerializeField]
        protected Popup m_confirmationPopupPrefab;
        [SerializeField]
        protected Popup m_popup;
        [SerializeField]
        protected Canvas m_rootCanvas;
        #endregion

        private const string YOUTUBE_PAGE = "https://www.youtube.com/telecomatics";
        private const string WEBSITE = "http://telecomatics.com/";
        private const string FACEBOOK_PAGE = "http://telecomatics.com/";
        private const string INSTAGRAM_PAGE = "http://telecomatics.com/";

        public virtual bool IsOpen { get { return m_screenStack.Count != 0; } }

        public EventHandler AudioInterruptionEvent { get; set; }
        public EventHandler ScreenChangeInitiated { get; set; }
        public EventHandler InputModuleChangeEvent { get; set; }

        public Platform Platform { get { return m_platform; } }

        public abstract BaseInputModule InputModule { get; }

        public Canvas RootCanvas { get { return m_rootCanvas; } }

        protected Stack<IScreen> m_screenStack = new Stack<IScreen>();

        protected IAudio m_audio;
        protected IExperienceMachine m_experienceMachine;
        protected ICommonResource m_commonResource;
        protected ISettings m_settings;
        protected IUIEngine m_uiEngine = null;

        protected IScreen m_blocker;

        protected Vector3 m_screenRootScale;

        protected BaseInputModule m_inputModule = null;

        protected virtual void Awake()
        {
            m_screenRootScale = m_screensRoot.localScale;
            m_audio = IOCCore.Resolve<IAudio>();
            m_experienceMachine = IOCCore.Resolve<IExperienceMachine>();
            m_commonResource = IOCCore.Resolve<ICommonResource>();
            m_settings = IOCCore.Resolve<ISettings>();
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
        }

        protected virtual IEnumerator Start()
        {
            yield return null;
            StartOverride();
        }

        protected virtual void StartOverride()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        #region CORE
        protected virtual void PrepareForNewScreen()
        {
            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                if (screen.Persist)
                    screen.Deactivate();
                else
                    m_screenStack.Pop().Close();
            }
        }

        protected virtual IScreen SpawnScreen(IScreen screenPrefab, bool enlarge = false, bool external = false)
        {
            m_screensRoot.gameObject.SetActive(true);
            PrepareForNewScreen();
            var menu = Instantiate(screenPrefab.Gameobject, m_screensRoot).GetComponent<IScreen>();
            if (!external)
                m_screenStack.Push(menu);
            return menu;
        }

        public virtual IScreen OpenMenu(MenuArgs args)
        {
            var menu = (MenuScreen)SpawnScreen(m_mainMenuPrefab, args.Items.Length > 4, false);
            menu.OpenMenu(this, args);
            m_menuOn = true;
            return menu;
        }

        public virtual IScreen OpenGridMenu(GridArgs args)
        {
            var menu = (MenuScreen)SpawnScreen(m_gridMenuPrefab);
            menu.OpenGridMenu(this, args);
            return menu;
        }

        public IScreen DisplayMessage(MessageArgs args)
        {
            var menu = (MessageScreen)SpawnScreen(m_messageBoxPrefab);
            menu.Init(this, args);
            return menu;
        }


        public IScreen ConfirmationPopup(PopupInfo args)
        {
            var popup = (Popup)SpawnScreen(m_confirmationPopupPrefab);
            popup.Show(args);
            return popup;
        }

        public virtual IScreen OpenAnnotationInterface(GridArgs args)
        {
            var menu = (MenuScreen)SpawnScreen(m_annotationInterface);
            menu.OpenGridMenu(this, args);
            return menu;
        }

        public virtual IScreen OpenInventory(GridArgs args)
        {
            var menu = (MenuScreen)SpawnScreen(m_inventoryMenuPrefab);
            menu.OpenGridMenu(this, args);
            m_inventoryOpen = true;
            return menu;
        }

        public virtual IScreen OpenMeeting(MeetingArgs args)
        {
            MeetingPage menu = (MeetingPage)SpawnScreen(m_meetingPagePrefab);
            menu.OpenMeeting(this, args);
            m_menuOn = true;
            return menu;
        }

        public virtual IScreen Popup(PopupInfo popupInfo)
        {
            var popup = (Popup)SpawnScreen(m_popupPrefab);
            popup.Show(popupInfo);
            return popup;
        }

        public IScreen OpenContextUI(MenuArgs args)
        {
            if (!args.Position.HasValue)
                args.Position = new Vector2(Screen.width/2, -Screen.height/2);

            m_screensRoot.gameObject.SetActive(true);
            PrepareForNewScreen();
            var menu = Instantiate(m_contextMenuPrefab, m_screensRoot).GetComponent<IScreen>();
            var screen = menu as MenuScreen;
            var rectTransform = screen.transform as RectTransform;
            screen.transform.position = new Vector3(args.Position.Value.x, args.Position.Value.y -rectTransform.sizeDelta.y, screen.transform.position.z);
            m_screenStack.Push(menu);
            ((MenuScreen)menu).OpenMenu(this, args);
            m_menuOn = true;
            return menu;
        }

        public virtual void CloseLastScreen()
        {
            //if (m_screenStack.Count > 0)
            //{
            //    Debug.LogError("Closing: " + m_screenStack.Peek().Gameobject.name);
            //}


            ScreenChangeInitiated?.Invoke(this, EventArgs.Empty);

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Pop();
                screen.Close();
            }

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                screen.Reopen();
                if (!(screen is IForm))
                   m_uiEngine.RefreshDesktopMode();
            }
            else
            {
                m_uiEngine.RefreshDesktopMode();
            }
        }

        public virtual void Close(IScreen screenToBeClosed)
        {
            //Debug.LogError("Close last screen");
            if (m_screenStack.Count == 0 || m_screenStack.Peek() != screenToBeClosed)
            {
                if (!m_screenStack.Contains(screenToBeClosed))
                    screenToBeClosed.Close();
                return;
            }

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Pop();
                screen.Close();
            }

            if (m_screenStack.Count > 0)
            {
                var screen = m_screenStack.Peek();
                screen.Reopen();
                if (!(screen is IForm))
                   m_uiEngine.RefreshDesktopMode();
                //Debug.LogError("opening: " + screen.Gameobject.name);
            }
            else
            {
                m_uiEngine.RefreshDesktopMode();
            }
        }

        public virtual void Close()
        {
            //Debug.LogError("Close");
            foreach (var item in m_screenStack)
                item.Close();
            m_screenStack.Clear();
            m_menuOff = true;

            m_uiEngine.RefreshDesktopMode();
        }

        public void GoBack()
        {
            //if (m_screenStack.Count > 0)
            //    m_screenStack.Pop().Close();
            //if (m_screenStack.Count > 0)
            //    m_screenStack.Peek().Reopen();
            CloseLastScreen();
        }

        public void ShowTooltip(string text)
        {
            if (m_screenStack.Count > 0)
            {
                m_screenStack.Peek().ShowTooltip(text);
            }
        }

        public void ShowPreview(Sprite sprite)
        {
            if (m_screenStack.Count > 0)
            {
                m_screenStack.Peek().ShowPreview(sprite);
            }
        }

        public IScreen OpenSettingsInterface(AudioClip clip)
        {
            var menu = OpenInterface(m_settingsInterfacePrefab, m_settingsInterfacePrefab, true, true);
            ((SettingsPanel)menu).OpenMenu(this, null);
            return menu;
        }

        public virtual IScreen OpenMeetingForm(FormArgs args)
        {
            var menu = (MeetingForm)SpawnScreen(m_meetingFormPrefab);
            menu.OpenForm(this, args);
            return menu;
        }


        public IScreen OpenSignupPage(MenuArgs args)
        {
            var menu = (SignupForm)SpawnScreen(m_signupFormPrefab);
            menu.OpenMenu(this, args);
            return menu;
        }


        public IScreen OpenLoginPage(MenuArgs args)
        {
            var menu = SpawnScreen(m_loginFormPrefab);

            ((MenuScreen)menu).OpenMenu(this, args);
            m_menuOn = true;
            return menu;
        }

        public virtual IScreen OpenForm(FormArgs args)
        {
            var menu = SpawnScreen(m_formPrefab);
            if (!(menu is Form))
                throw new InvalidOperationException();

            ((Form)menu).OpenForm(this, args);
            m_menuOn = true;
            return menu;
        }

        //Not handled properly for VR screen
        public abstract IScreen OpenCalendar(CalendarArgs args);

        private IScreen OpenInterface(MenuScreen screenPrefab, MenuScreen dScreenPrefab, bool block = true, bool persist = false)
        {
            var menu = (MenuScreen)SpawnScreen(screenPrefab);
            menu.Init(this, block, persist);
            return menu;
        }
        #endregion

        #region GUIDE_CONDITIONS
        protected bool m_menuOn = false, m_menuOff = false, m_inventoryOpen = false;

        public bool MenuOn()
        {
            var val = m_menuOn;
            if (m_menuOn)
                m_menuOn = false;
            return val;
        }

        public bool MenuOff()
        {
            var val = m_menuOff;
            if (m_menuOff)
                m_menuOff = false;
            return val;
        }

        public bool InventoryOpen()
        {
            var val = m_inventoryOpen;
            if (m_inventoryOpen)
                m_inventoryOpen = false;
            return val;
        }
        #endregion

        public void DeactivateUI()
        {
            if (m_screenStack.Count == 0)
                return;
            m_screenStack.Peek().Deactivate();
        }

        public void ShowUI()
        {
            if (m_screenStack.Count == 0)
                return;
            m_screenStack.Peek().Reopen();
        }

        public IScreen DisplayResult(Error error, bool freshScreen = false)
        {
            if (!freshScreen && m_screenStack.Count > 0)
            {
                m_screenStack.Peek().DisplayResult(error);
                return m_screenStack.Peek();
            }
            return null;
        }

        public virtual IScreen DisplayProgress(string text, bool freshScreen = false)
        {
            if (!freshScreen && m_screenStack.Count > 0)
            {
                //Debug.LogError(m_screenStack.Peek().Gameobject.name);
                m_screenStack.Peek().DisplayProgress(text);
                return m_screenStack.Peek();
            }
            else if (freshScreen)
            {
                var menu = (MessageScreen)SpawnScreen(m_genericLoader);
                menu.Init(this, new MessageArgs()
                {
                    Persist = false,
                    Block = true,
                    Text = text,
                    BackEnabled = false
                });
                return menu;
            }
            return null;
        }


        public virtual void Hide()
        {
            //Debug.LogError("Hide");
            foreach (var item in m_screenStack)
                item.Hide();
        }

        public virtual void Unhide()
        {
            //Debug.LogError("Unhide");
            foreach (var item in m_screenStack)
                item.UnHide();
        }

        public virtual void Unblock()
        {
            m_screensRoot.localScale = m_screenRootScale;

            if (m_blocker != null)
                m_blocker.Close();

            m_blocker = null;

            if (m_screenStack.Count > 0)
                m_screenStack.Peek().Reopen();

            if (m_screenStack.Count == 0)
                m_menuOff = true;
        }

        public virtual IScreen Block(string message, bool includeRoot = false)
        {
            if (includeRoot)
                m_screensRoot.localScale = Vector3.zero;
            else
                m_screensRoot.localScale = m_screenRootScale;

            if (m_blocker != null)
            {
                m_blocker.Reopen();
                return m_blocker;
            }
            else
            {
                var menu = (MessageScreen)SpawnScreen(m_genericLoader, false, true);
                menu.Init(this, new MessageArgs()
                {
                    Persist = false,
                    Block = true,
                    Text = message,
                    BackEnabled = false
                });

                m_blocker = menu;
                m_menuOn = true;
                return menu;
            }
        }
    }
}
