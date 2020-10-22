using DG.Tweening;
using Fordi;
using Fordi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace RLC.Animation
{
    public interface IAnimationSubject
    {
        string Key { get; }
        void Begin(AnimationPose pose, bool fade = true);
        void Stop();
        string GroupName { get; }
        AnimationPose CurrentPose { get; }
    }

    public class Pose : MonoBehaviour, IAnimationSubject
    {
        [SerializeField]
        private string m_key;

        public string Key { get { return m_key; } }

        public string GroupName {
            get
            {
                if (m_currentPose != null)
                    return m_currentPose.GroupName;
                else
                    return null;
            }
        }

        public AnimationPose CurrentPose { get { return m_currentPose; } }

        [SerializeField]
        private GameObject m_maleFade, m_femaleFade;
        [SerializeField]
        private GameObject m_male, m_female;
        [SerializeField]
        private GameObject m_maleHead, m_femaleHead;

        private IAnimationEngine m_animationEngine;

        private List<Material> m_fadeMats;

        private SkinnedMeshRenderer m_maleRenderer, m_femaleRenderer;

        private AnimationPose m_currentPose;

        private UnityEngine.Animation m_maleAnimation, m_femaleAnimation, m_maleFadeAnimation, m_femaleFadeAnimation;

        private void Awake()
        {
            m_animationEngine = IOCCore.Resolve<IAnimationEngine>();
            m_male.SetActive(false);
            m_female.SetActive(false);
            m_maleFade.SetActive(false);
            m_femaleFade.SetActive(false);
            m_maleAnimation = m_male.GetComponent<UnityEngine.Animation>();
            m_femaleAnimation = m_female.GetComponent<UnityEngine.Animation>();
            m_maleFadeAnimation = m_maleFade.GetComponent<UnityEngine.Animation>();
            m_femaleFadeAnimation = m_femaleFade.GetComponent<UnityEngine.Animation>();
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
        }

        public void Begin(AnimationPose pose, bool fade = true)
        {
            m_currentPose = pose;
            if (m_fadeMats == null)
            {
                m_fadeMats = new List<Material>();
                m_maleRenderer = m_male.GetComponentInChildren<SkinnedMeshRenderer>();
                m_femaleRenderer = m_female.GetComponentInChildren<SkinnedMeshRenderer>();
                m_fadeMats.AddRange(m_maleFade.GetComponentInChildren<SkinnedMeshRenderer>().materials);
                m_fadeMats.AddRange(m_femaleFade.GetComponentInChildren<SkinnedMeshRenderer>().materials);
            }

            if (!fade)
            {
                gameObject.SetActive(true);
                m_maleFade.SetActive(false);
                m_femaleFade.SetActive(false);
                m_maleRenderer.enabled = true;
                m_femaleRenderer.enabled = true;
                m_maleHead.SetActive(true);
                m_femaleHead.SetActive(true);
                m_male.SetActive(true);
                m_female.SetActive(true);

                PlayClip(m_maleAnimation, pose.MaleClip);
                PlayClip(m_femaleAnimation, pose.FemaleClip);
                return;
            }

            gameObject.SetActive(true);
            m_maleFade.SetActive(true);
            m_femaleFade.SetActive(true);
            m_male.SetActive(true);
            m_female.SetActive(true);
            m_maleHead.SetActive(false);
            m_femaleHead.SetActive(false);

            PlayClip(m_maleFadeAnimation, pose.MaleClip);
            PlayClip(m_femaleFadeAnimation, pose.FemaleClip);

            m_maleRenderer.enabled = false;
            m_femaleRenderer.enabled = false;

            foreach (var item in m_fadeMats)
            {
                item.color = Color.clear;
                item.DOColor(new Color(1, 1, 1, .4f), .5f);
            }

            Observable.Timer(TimeSpan.FromSeconds(.5f)).Subscribe(_ =>
            {
                m_maleFade.SetActive(false);
                m_femaleFade.SetActive(false);
                m_maleRenderer.enabled = true;
                m_femaleRenderer.enabled = true;
                m_maleHead.SetActive(true);
                m_femaleHead.SetActive(true);

                PlayClip(m_maleAnimation, pose.MaleClip);
                PlayClip(m_femaleAnimation, pose.FemaleClip);
            });
        }

        private void PlayClip(UnityEngine.Animation animation, AnimationClip clip)
        {
            clip.legacy = true;
            if (clip.name.ToLower().Contains("begin"))
                clip.wrapMode = WrapMode.Once;
            else
                clip.wrapMode = WrapMode.Loop;

            //clip.wrapMode = WrapMode.Loop;
            if (animation.GetClip(clip.name) == null)
                animation.AddClip(clip, clip.name);
            //Debug.LogError("Playing: " + clip.name);
            animation.Play(clip.name);
        }

        public void Stop()
        {
            gameObject.SetActive(false);
        }

    }
}