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
        void SwitchToState(string state);
        void StopAll();
    }

    [Serializable]
    public class AnimationPose
    {
        public string Key;
        public string GroupName;
        public AnimationClip MaleClip;
        public AnimationClip FemaleClip;
    }

    public enum PlayerState
    {
        IDLE,
        ACTIVE
    }


    public class AnimationEngine : MonoBehaviour, IAnimationEngine
    {
        [SerializeField]
        private List<AnimationPose> m_animationPoses;
        [SerializeField]
        private Pose m_pose;

        private ICameraControl m_cameraControl;
        private IUIEngine m_uiEngine;
        private PhotonView m_photonView;

        public EventHandler<PlayerState> InteractionStateChange { get; set; }

        private Dictionary<string, AnimationPose> m_posesDictionary = new Dictionary<string, AnimationPose>();

        public PlayerState State { get; private set; }

        private void Awake()
        {
            m_cameraControl = IOCCore.Resolve<ICameraControl>();
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
            m_photonView = GetComponent<PhotonView>();
        }

        public void RegisterSubject(IAnimationSubject subject)
        {
        }

        public void DeregisterSubject(IAnimationSubject subject)
        {
            
        }

        private IObservable<long> m_observable = null;

        private string m_lastState = "";

        public void SwitchToState(string state)
        {
            Debug.LogError("FiringStateChange: Active");
            State = PlayerState.ACTIVE;
            InteractionStateChange?.Invoke(this, PlayerState.ACTIVE);
            m_photonView.RPC("RPC_SwitchToState", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, state);
        }

        private void ChangeState(string state)
        {
            if (m_lastState == state)
                return;

            m_lastState = state;

            if (m_observable != null)
                return;

            if (m_posesDictionary.ContainsKey(state))
            {
                var observable = Observable.TimerFrame(20).Subscribe(_ =>
                {
                    //m_cameraControl.SwitchMode(CameraMode.INDEPENDENT);
                    var newPose = m_posesDictionary[state];
                    m_pose.Stop();

                    if (newPose != null)
                        Debug.LogError(newPose.GroupName);

                    bool fade = newPose.GroupName == null || newPose.GroupName != m_pose.GroupName;
                    m_pose.Begin(newPose, fade);
                    m_observable = null;
                });
                //m_uiEngine.Fade();
            }
        }

        public void StopAll()
        {
            State = PlayerState.IDLE;
            m_pose.Stop();
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
