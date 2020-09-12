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
        void Begin();
        void Stop();
    }

    public class Pose : MonoBehaviour, IAnimationSubject
    {
        [SerializeField]
        private string m_key;
        public string Key { get { return m_key; } }

        [SerializeField]
        private GameObject m_maleFade, m_femaleFade;
        [SerializeField]
        private GameObject m_male, m_female;
        [SerializeField]
        private GameObject m_maleHead, m_femaleHead;

        private IAnimationEngine m_animationEngine;

        private List<Material> m_fadeMats;

        private SkinnedMeshRenderer m_maleRenderer, m_femaleRenderer;

        private void Awake()
        {
            m_animationEngine = IOCCore.Resolve<IAnimationEngine>();
            m_animationEngine.RegisterSubject(this);
            m_male.SetActive(false);
            m_female.SetActive(false);
            m_maleFade.SetActive(false);
            m_femaleFade.SetActive(false);
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            m_animationEngine.DeregisterSubject(this);
        }

        public void Begin()
        {
            gameObject.SetActive(true);
            m_maleFade.SetActive(true);
            m_femaleFade.SetActive(true);
            m_male.SetActive(true);
            m_female.SetActive(true);
            m_maleHead.SetActive(false);
            m_femaleHead.SetActive(false);

            if (m_fadeMats == null)
            {
                m_fadeMats = new List<Material>();
                m_maleRenderer = m_male.GetComponentInChildren<SkinnedMeshRenderer>();
                m_femaleRenderer = m_female.GetComponentInChildren<SkinnedMeshRenderer>();
                m_fadeMats.AddRange(m_maleFade.GetComponentInChildren<SkinnedMeshRenderer>().materials);
                m_fadeMats.AddRange(m_femaleFade.GetComponentInChildren<SkinnedMeshRenderer>().materials);
            }

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
            });
        }

        public void Stop()
        {
            gameObject.SetActive(false);
        }

    }
}