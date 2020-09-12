using Fordi.Common;
using Fordi.UI;
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
        void RegisterSubject(IAnimationSubject subject);
        void DeregisterSubject(IAnimationSubject subject);
        void SwitchToState(string state);
        void StopAll();
    }

    public class AnimationEngine : MonoBehaviour, IAnimationEngine
    {
        private Dictionary<string, IAnimationSubject> m_poses = new Dictionary<string, IAnimationSubject>();

        private IAnimationSubject m_currentSubject = null;
        private ICameraControl m_cameraControl;
        private IUIEngine m_uiEngine;

        private void Awake()
        {
            m_cameraControl = IOCCore.Resolve<ICameraControl>();
            m_uiEngine = IOCCore.Resolve<IUIEngine>();
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

        public void SwitchToState(string state)
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
            if (m_currentSubject != null)
                m_currentSubject.Stop();
            m_currentSubject = null;
            //m_cameraControl.SwitchMode(CameraMode.FIRST_PERSON);
        }
    }
}
