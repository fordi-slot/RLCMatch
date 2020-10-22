using Fordi.Common;
using Fordi.UI;
using Photon.Pun;
using RLC.Core;
using RLC.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace RLC.Animation
{
    public interface IAnimationEngine
    {
        PlayerState State { get; }
        EventHandler<AnimationPose> InteractionStateChange { get; set; }
        AnimationPose CurrentPose { get; }
        void SwitchToState(string state);
        void StopAll();
        void Cum();
        void StartSexRoutine();
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

        private const string ORGASM_POSE = "orgasm_male0";

        private ICameraControl m_cameraControl;
        private IUIEngine m_uiEngine;
        private PhotonView m_photonView;

        public EventHandler<AnimationPose> InteractionStateChange { get; set; }

        private Dictionary<string, AnimationPose> m_posesDictionary = new Dictionary<string, AnimationPose>();

        public PlayerState State { get; private set; }

        [HideInInspector]
        public List<AnimationClip> ValidFemaleClips = new List<AnimationClip>();
        [HideInInspector]
        public List<AnimationClip> ValidMaleClips = new List<AnimationClip>();

        public AnimationPose CurrentPose { get { return m_pose.CurrentPose; } }

        private IEnumerator m_sexRoutine = null;

        private void Awake()
        {
            m_cameraControl = IOCCore.Resolve<ICameraControl>();
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
            m_photonView = GetComponent<PhotonView>();
            foreach (var item in AnimationPoses)
                m_posesDictionary[item.Key] = item;
        }

        private void Update()
        {
#if LOCAL_TEST
            if (Input.GetKeyDown(KeyCode.C))
            {
                var keysArray = m_posesDictionary.Keys.ToArray();
                var key = keysArray[UnityEngine.Random.Range(0, m_posesDictionary.Values.Count)];
                var pose = m_posesDictionary[key];
                SwitchToState(pose.Key);
            }
#endif
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
            if (PoseSelectionView.s_passiveFlag)
                return;

#if LOCAL_TEST
            ChangeState(state);
            State = PlayerState.ACTIVE;
            return;
#endif
            m_photonView.RPC("RPC_SwitchToState", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, state);
            State = PlayerState.ACTIVE;
        }

        public void StartSexRoutine()
        {
            if (m_sexRoutine != null)
                StopCoroutine(m_sexRoutine);
            m_sexRoutine = CoSexRoutine();
            StartCoroutine(m_sexRoutine);
        }

        private IEnumerator CoSexRoutine()
        {
            yield return null;
            foreach (var item in m_posesDictionary.Keys)
            {
                yield return new WaitUntil(() => m_pose.IsDoneYet);
                SwitchToState(item);
                yield return new WaitForSeconds(2);
            }
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

                    bool fade = m_pose.GroupName == null || newPose.GroupName != m_pose.GroupName;
                    m_pose.Begin(newPose, fade);
                    InteractionStateChange?.Invoke(this, m_pose.CurrentPose);
                    m_observable = null;
                });
                //m_uiEngine.Fade();
            }
            else
                Debug.LogError("Key not found: " + state);
        }

        public void StopAll()
        {
            State = PlayerState.IDLE;
            m_pose.Stop();
            InteractionStateChange?.Invoke(this, null);
        }

        public void Cum()
        {
            SwitchToState(ORGASM_POSE);
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
