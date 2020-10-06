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
        public string GroupName;
        public string Key;
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
        public List<AnimationPose> AnimationPoses;
        [SerializeField]
        private Pose m_pose;

        private ICameraControl m_cameraControl;
        private IUIEngine m_uiEngine;
        private PhotonView m_photonView;

        public EventHandler<PlayerState> InteractionStateChange { get; set; }

        private Dictionary<string, AnimationPose> m_posesDictionary = new Dictionary<string, AnimationPose>();

        public PlayerState State { get; private set; }

        [HideInInspector]
        public List<AnimationClip> ValidFemaleClips = new List<AnimationClip>();
        [HideInInspector]
        public List<AnimationClip> ValidMaleClips = new List<AnimationClip>();

        private void Awake()
        {
            m_cameraControl = IOCCore.Resolve<ICameraControl>();
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
            m_photonView = GetComponent<PhotonView>();
            foreach (var item in AnimationPoses)
                m_posesDictionary[item.Key] = item;
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
            State = PlayerState.ACTIVE;
            InteractionStateChange?.Invoke(this, PlayerState.ACTIVE);

#if LOCAL_TEST
            ChangeState(state);
            return;
#endif

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
                    foreach (var item in m_posesDictionary)
                    {
                        Debug.LogError(item.Value.GroupName);
                    }
                    //m_cameraControl.SwitchMode(CameraMode.INDEPENDENT);
                    var newPose = m_posesDictionary[state];
                    m_pose.Stop();

                    bool fade = m_pose.GroupName == null || newPose.GroupName != m_pose.GroupName;

                    if (m_pose.GroupName == null)
                        Debug.LogError("m_pose.GroupName null");
                    else
                        Debug.LogError(m_pose.GroupName + " "  + newPose.Key + newPose.GroupName + " " + fade);
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
