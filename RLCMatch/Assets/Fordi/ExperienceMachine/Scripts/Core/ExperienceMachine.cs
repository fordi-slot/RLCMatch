﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Fordi.Common;
using Fordi.UI;
using Fordi.UI.MenuControl;
using Fordi.Platforms;
using System;
using UnityEngine.XR;
using Fordi.AssetManagement;
using UniRx;
using Fordi.Plugins;
using TMPro;
using Cornea.Web;
using LitJson;

namespace Fordi.Core
{
    public enum AppMode
    {
        TRAINING,
        APPLICATION
    }

    public enum ExperienceType
    {
        HOME,
        NATURE,
        MANDALA,
        ABSTRACT,
        GLOBAL,
        MEETING,
        LOBBY
    }

    public enum GameplayMode
    { 
        NONE,
        PAUSED,
        RUNNING,
    }

    public class ModuleArgs : EventArgs
    {
        public IPlatformModule PlatformModule;
    }

    public interface IExperienceMachine
    {
        void ExecuteMenuCommand(MenuClickArgs args);
        void CanExecuteMenuCommand(MenuItemValidationArgs args);
        IExperience GetExperience(ExperienceType experience);
        Transform GetNextTeleportAnchor();
        IExperience GetExperience(string name);
        void SetExperience(IExperience experience);
        void UpdateResourceSelection(MenuClickArgs args);
        void LoadExperience();
        void SetQualityLevel(GraphicsQuality quality);
        void SetAmbienceAudioVolume(float volume);
        void OpenSceneMenu();
        ExperienceType CurrentExperience { get; }
        bool IsRunning { get; }
        void RegisterPlatform(IPlatformModule module);
        IPlayer Player { get; }
        EventHandler<ModuleArgs> OnModuleRegistration { get; set; }
    }

    public interface IOperationEngine
    {
        void Run();
        void Stop();
        void ResetAndCleanup();
    }

    public class Error
    {
        public const int OK = 0;
        public const int E_Exception = 1;
        public const int E_NotFound = 2;
        public const int E_AlreadyExist = 3;
        public const int E_InvalidOperation = 4;
        public const int E_NetworkIssue = 5;

        public int ErrorCode;

        public string ErrorText;

        public bool HasError
        {
            get { return ErrorCode != 0; }
        }

        public Error()
        {
            ErrorCode = OK;
        }

        public Error(int errorCode)
        {
            ErrorCode = errorCode;
        }

        public override string ToString()
        {
            return string.Format("Error({0}): {1}", ErrorCode, ErrorText);
        }
    }

    /// <summary>
    /// Uses state design pattern.
    /// All Experience Classes are its states.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class ExperienceMachine : MonoBehaviour, IExperienceMachine
    {
        private IExperience m_home, m_lobby, m_meeting, m_currentExperience;
        private IAudio m_audio;
        private ISettings m_settings;
        private IPlayer m_player;
        private IUIEngine m_uiEngine = null;
        private IPluginHook m_pluginHook = null;
        private IWebInterface m_webInterface = null;

        public const string DynamicAmbienceTag = "DynamicAmbience";
        public const string CorrectTextColorStyle = "Correct";
        public const string ErrorTextColorStyle = "Error";
        public const string ProgressTextColorStyle = "Progress";

        #region GUIDE_CONDITIONS
        private bool m_clicked = false;
        #endregion

        public static AppMode AppMode { get; set; } = AppMode.APPLICATION;

        private ExperienceType m_currentExperienceType;
        public ExperienceType CurrentExperience { get { return m_currentExperienceType; } }

        private bool m_isRunning = false;
        public bool IsRunning { get { return m_isRunning; } }

        public IPlayer Player { get { return m_player; } }

        public EventHandler<ModuleArgs> OnModuleRegistration { get; set; }

        private const string MeetingScene = "Meeting";

        private void Awake()
        {
            m_isRunning = true;
            m_home = GetComponentInChildren<Home>();
            m_lobby = GetComponentInChildren<Lobby>();
            m_meeting = GetComponentInChildren<MeetingExperience>();
            m_audio = IOCCore.Resolve<IAudio>();
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
            m_settings = IOCCore.Resolve<ISettings>();
            m_pluginHook = IOCCore.Resolve<IPluginHook>();
            m_webInterface = IOCCore.Resolve<IWebInterface>();

            if (SceneManager.GetActiveScene().name == Networking.Network.MeetingRoom || SceneManager.GetActiveScene().name == Networking.Network.PrivateMeetingLocation)
            {
                Selection.Location = SceneManager.GetActiveScene().name;
                Selection.ExperienceType = ExperienceType.MEETING;
            }

            SetExperience(GetExperience(Selection.ExperienceType));
            UIInteractionBase.OnClick += Click;
            ResetGuideConditions();
            m_pluginHook.AllPlatformDependenciesLoaded += AllPlatformDependenciesLoaded;
        }

        private void OnDestroy()
        {
            UIInteractionBase.OnClick -= Click;
            m_pluginHook.AllPlatformDependenciesLoaded -= AllPlatformDependenciesLoaded;
        }

        private IEnumerator Start()
        {
            yield return null;
            MinimumWindowSize.Set(1472, 828);
            if (m_currentExperienceType == ExperienceType.HOME)
                Screen.SetResolution(1472, 828, FullScreenMode.Windowed);
            yield return null;

            if (m_currentExperienceType == ExperienceType.HOME)
                OpenLoginPage();
            yield return new WaitForSeconds(3);

            if (DateTime.Now > new DateTime(2020, 11, 6))
            {
                m_uiEngine.DisplayProgress("APP VALIDITY EXPIRED.\n\nPlease contact administrator for support.", true);
            }

#if LOCAL_TEST
            if (CurrentExperience != ExperienceType.HOME)
                AllPlatformDependenciesLoaded(null, EventArgs.Empty);
#endif
        }

        private void Update()
        {
            if (m_settings.SelectedPreferences.DesktopMode)
                return;
        }

        private void AllPlatformDependenciesLoaded(object sender, EventArgs e)
        {
            Debug.LogError("AllPlatformDependenciesLoaded");
            Observable.TimerFrame(2).Subscribe(_ =>
            {
                m_currentExperience.OnLoad();
            });
        }

        private void ResetGuideConditions()
        {
            m_clicked = false;
        }

        public IExperience GetExperience(ExperienceType experience)
        {
            switch (experience)
            {
                case ExperienceType.HOME:
                    return m_home;
                case ExperienceType.MEETING:
                    return m_meeting;
                case ExperienceType.LOBBY:
                    return m_lobby;
                default:
                    return null;
            }
        }

        public void SetExperience(IExperience experience)
        {
            m_currentExperience = experience;
            if (m_currentExperience == m_home)
                m_currentExperienceType = ExperienceType.HOME;
            if (m_currentExperience == m_lobby)
                m_currentExperienceType = ExperienceType.LOBBY;
            if (m_currentExperience == m_meeting)
                m_currentExperienceType = ExperienceType.MEETING;
        }

#region EXPERIENCE_INTERFACE
        public void CanExecuteMenuCommand(MenuItemValidationArgs args)
        {
            args.IsValid = m_currentExperience.CanExecuteMenuCommand(args.Command);
        }

        public void ExecuteMenuCommand(MenuClickArgs args)
        {
            m_currentExperience.ExecuteMenuCommand(args);
        }
        
        public void Play()
        {
            m_currentExperience.Play();
        }

        public void Pause()
        {
            m_currentExperience.Pause();
        }

        public void Resume()
        {
            m_currentExperience.Resume();
        }

        public void Stop()
        {
            m_currentExperience.Stop();
        }
        
        public void ToggleMenu()
        {
            m_currentExperience.ToggleMenu();
        }

        public void GoBack()
        {
            m_currentExperience.GoBack();
        }

        public IExperience GetExperience(string name)
        {
            switch (name.ToLower())
            {
                case "home":
                    return m_home;
                case "meeting":
                    return m_meeting;
                case "lobby":
                    return m_lobby;
                default:
                    return null;
            }
        }
        
        public void UpdateResourceSelection(MenuClickArgs args)
        {
            //Debug.LogError("UpdateResourceSelection");
            if (args.Data != null && args.Data is ColorGroup)
            {
                var experience = GetExperience(Selection.ExperienceType);
                experience.UpdateResourceSelection(args);
            }
            else
            {
                if (args.Data != null && ((ExperienceResource)args.Data).ResourceType == ResourceType.EXPERIENCE)
                {
                    m_home.UpdateResourceSelection(args);
                }
                else
                {
                    var experience = GetExperience(Selection.ExperienceType);
                    experience.UpdateResourceSelection(args);
                }
            }
        }

        public void LoadExperience()
        {
            //Debug.LogError("LoadExperience: " + Selection.Location + " " + Selection.ExperienceType.ToString());
            AudioArgs args = new AudioArgs(null, AudioType.MUSIC)
            {
                FadeTime = 2,
                Done = () =>
                {
                    if (PhotonNetwork.InRoom)
                        PhotonNetwork.LoadLevel(Selection.Location);
                    else
                        SceneManager.LoadSceneAsync(Selection.Location);
                }
            };
            m_audio.Stop(args);
           
            m_player.FadeOut();

            AudioArgs voArgs = new AudioArgs(null, AudioType.VO)
            {
                FadeTime = 2,
            };
            m_audio.Stop(voArgs);

            m_uiEngine.DisplayProgress("Loading: " + Selection.Location, true);
            //SceneManager.LoadScene(Selection.Location);
        }
#endregion

        private void OnApplicationQuit()
        {
#if !UNITY_EDITOR
            System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
            MinimumWindowSize.Reset();
        }

        public Transform GetNextTeleportAnchor()
        {
            return m_currentExperience.GetNextTeleportAnchor();
        }

        public void SetQualityLevel(GraphicsQuality quality)
        {
            //QualitySettings.SetQualityLevel((int)quality, false);
        }

        public void Click(object sender, PointerEventData eventData)
        {
            m_clicked = true;

            //if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            //{
            //    Debug.LogError("Action tru");
            //}
            //else
            //    Debug.LogError("Action false");

            StartCoroutine(ResetClick());
        }

        private IEnumerator ResetClick()
        {
            yield return null;

            //if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            //{
            //    Debug.LogError("Action tru");
            //}
            //else
            //    Debug.LogError("Action false");

            m_clicked = false;
            //Debug.LogError(m_clicked);
        }

        public bool ClickCondition()
        {
            return m_clicked;
        }

        public bool Teleported()
        {
            throw new NotImplementedException();
            return false;
        }

        public bool WaypointTeleported()
        {
            throw new NotImplementedException();
            return false;
        }

        public void SetAmbienceAudioVolume(float volume)
        {
            GameObject[] dynamicParticles = GameObject.FindGameObjectsWithTag(DynamicAmbienceTag);
            foreach (var item in dynamicParticles)
            {
                var audiSources = item.GetComponentsInChildren<AudioSource>();
                foreach (var audioSource in audiSources)
                    audioSource.volume = volume;
            }
        }

        public void OpenSceneMenu()
        {
            m_uiEngine.Close();
            ToggleMenu();
        }

        public void RegisterPlatform(IPlatformModule module)
        {
            if (m_player != null && module.Platform == Platform.DESKTOP)
                module.Player.GameObject.SetActive(false);

            if (m_player == null || module.Platform == Platform.VR)
            {

                if (m_player != null)
                    m_player.GameObject.SetActive(false);

                m_player = module.Player;
                m_player.GameObject.SetActive(true);
            }

            m_uiEngine.RegisterInterface(module.UserInterface);

            OnModuleRegistration?.Invoke(this, new ModuleArgs() { PlatformModule = module });

            if (module.Platform == Platform.VR && !XRSettings.enabled)
            {
                StartCoroutine(LoadDevice("Oculus"));
            }
        }

        IEnumerator LoadDevice(string newDevice)
        {
            if (String.Compare(XRSettings.loadedDeviceName, newDevice, true) != 0)
            {
                XRSettings.LoadDeviceByName(newDevice);
                yield return null;
                XRSettings.enabled = true;
                Debug.LogError("Loaded: " + newDevice);
            }
        }

        #region UI

        private void OpenLoginPage()
        {
            var usernameInput = new MenuItemInfo
            {
                Path = "Email",
                Text = "Emaiil",
                Command = "Email",
                Icon = null,
                Data = TMP_InputField.ContentType.Standard,
                CommandType = MenuCommandType.FORM_INPUT
            };

            var passwordInput = new MenuItemInfo
            {
                Path = "Password",
                Text = "Password",
                Command = "Password",
                Icon = null,
                Data = TMP_InputField.ContentType.Password,
                CommandType = MenuCommandType.FORM_INPUT
            };

            MenuItemInfo[] formItems = new MenuItemInfo[] { usernameInput, passwordInput };
            MenuArgs args = new MenuArgs()
            {
                Items = new MenuItemInfo[] { },
                OnAction = (inputs) =>
                {

                    m_uiEngine.DisplayProgress("Validating user...");

                    m_webInterface.UserLogin(inputs[0], inputs[1],
                        (isNetworkError, message) =>
                        {
                            Error error = new Error();

                            JsonData validateUserLoginResult = JsonMapper.ToObject(message);

                            if (validateUserLoginResult["status"].ToString() == "200")
                            {
                                return;
                            }

                            error.ErrorCode = Error.E_NotFound;
                            error.ErrorText = validateUserLoginResult["message"].ToString();
                            m_uiEngine.DisplayResult(error, false);
                        });
                }
            };
            m_uiEngine.OpenLoginPage(args);
        }
        #endregion
    }
}
