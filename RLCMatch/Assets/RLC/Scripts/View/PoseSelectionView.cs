using Fordi.Common;
using AL.UI;
using Fordi.UI.MenuControl;
using RLC.Animation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Fordi.Core;
using Fordi.Networking;
using Photon.Pun;
using Fordi.UI;
using System;

namespace RLC.UI
{
    public class PoseSelectionView : TabbedScreen
    {
        [SerializeField]
        private Button m_expandButton, m_collapseButton;
        [SerializeField]
        private Button m_cumButton, m_quitButton;

        private bool m_expanded = true;

        public static bool s_passiveFlag = false;

        private IAnimationEngine m_animationEngine;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_animationEngine = IOCCore.Resolve<IAnimationEngine>();
            m_animationEngine.InteractionStateChange += StateChange;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            m_animationEngine.InteractionStateChange -= StateChange;
        }

        private void StateChange(object sender, AnimationPose e)
        {
            if (e == null)
            {
                foreach (var item in m_toggleGroup.ActiveToggles())
                    item.isOn = false;
                return;
            }

            s_passiveFlag = true;
            var target = m_menuItems.Find(item => item.Item.Text == e.GroupName);
            ((Toggle)target.Selectable).isOn = true;
            ((IExpandableTabItem)target).SelectItem(e.Key);
            s_passiveFlag = false;
        }

        public override void OpenMenu(IUserInterface userInterface, MenuArgs args)
        {
            //foreach (var item in args.Items)
            //{
            //    item.IsValid = !PhotonNetwork.IsMasterClient;
            //}

            base.OpenMenu(userInterface, args);
            if (args.Items.Length == 0)
            {
                m_cumButton.gameObject.SetActive(false);
                m_quitButton.gameObject.SetActive(false);
            }
        }

        public void Expand()
        {
            if (m_expanded)
                return;

            RectTransform rectTransform = (RectTransform)transform;
            var width = rectTransform.sizeDelta.x;
            transform.position -= new Vector3(width, 0, 0);
            m_expanded = true;
            m_expandButton.gameObject.SetActive(false);
            m_collapseButton.gameObject.SetActive(true);
        }

        public void Collapse()
        {
            if (!m_expanded)
                return;
            RectTransform rectTransform = (RectTransform)transform;
            var width = rectTransform.sizeDelta.x;
            transform.position += new Vector3(width, 0, 0);
            m_expanded = false;
            m_expandButton.gameObject.SetActive(true);
            m_collapseButton.gameObject.SetActive(false);
        }

        public void Cum()
        {
            foreach (var item in m_toggleGroup.ActiveToggles())
                item.isOn = false;
            m_animationEngine.Cum();
        }
    }
}
