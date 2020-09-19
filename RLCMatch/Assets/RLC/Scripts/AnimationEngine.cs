using Fordi.Common;
using Fordi.UI;
using Photon.Pun;
using RLC.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace RLC.Animation
{
    public interface IAnimationEngine
    {
        PlayerState State { get; }
        EventHandler<PlayerState> InteractionStateChange { get; set; }
        void RegisterSubject(IAnimationSubject subject);
        void DeregisterSubject(IAnimationSubject subject);
        void SwitchToState(string state);
        void StopAll();
    }

    public enum PlayerState
    {
        IDLE,
        ACTIVE
    }


    public class AnimationEngine : MonoBehaviour, IAnimationEngine
    {
        private Dictionary<string, IAnimationSubject> m_poses = new Dictionary<string, IAnimationSubject>();

        private IAnimationSubject m_currentSubject = null;
        private ICameraControl m_cameraControl;
        private IUIEngine m_uiEngine;
        private PhotonView m_photonView;

        public EventHandler<PlayerState> InteractionStateChange { get; set; }

        public PlayerState State { get; private set; }

        private void Awake()
        {
            m_cameraControl = IOCCore.Resolve<ICameraControl>();
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
            m_photonView = GetComponent<PhotonView>();
        }

        public void RegisterSubject(IAnimationSubject subject)
        {
            m_poses[subject.Key] = subject;
        }

        public void DeregisterSubject(IAnimationSubject subject)
        {
            if (m_poses.ContainsKey(subject.Key))
                m_poses.Remove(subject.Key);
        }

        private IObservable<long> m_observable = null;

        private string m_lastState = "";

        public void SwitchToState(string state)
        {
            if (m_lastState == state)
                return;

            m_lastState = state;
            Debug.LogError("FiringStateChange: Active");
            State = PlayerState.ACTIVE;
            InteractionStateChange?.Invoke(this, PlayerState.ACTIVE);
            m_photonView.RPC("RPC_SwitchToState", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, state);
        }

        private void ChangeState(string state)
        {
            if (m_observable != null)
                return;

            if (m_poses.ContainsKey(state))
            {
                var observable = Observable.TimerFrame(20).Subscribe(_ =>
                {
                    //m_cameraControl.SwitchMode(CameraMode.INDEPENDENT);
                    var subject = m_poses[state];
                    if (m_currentSubject != null)
                        m_currentSubject.Stop();
                    m_currentSubject = subject;
                    m_currentSubject.Begin();
                    m_observable = null;
                });
                //m_uiEngine.Fade();
            }
        }

        public void StopAll()
        {
            State = PlayerState.IDLE;
            if (m_currentSubject != null)
                m_currentSubject.Stop();
            m_currentSubject = null;
            InteractionStateChange?.Invoke(this, PlayerState.IDLE);
            //m_cameraControl.SwitchMode(CameraMode.FIRST_PERSON);
        }

        #region NETWORK_EVENTS

        [PunRPC]
        void RPC_SwitchToState(int senderId, string state)
        {
            ChangeState(state);
        }
        #endregion
    }
}
