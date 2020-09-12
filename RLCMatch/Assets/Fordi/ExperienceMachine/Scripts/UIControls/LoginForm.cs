using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fordi.UI.MenuControl
{
    public class LoginForm : MenuScreen
    {
        [SerializeField]
        private TMP_InputField m_email, m_password;

        [SerializeField]
        private List<TMP_InputField> m_inputs = new List<TMP_InputField>();

        private int m_inputIndex = 0;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            for (int i = 0; i < m_inputs.Count; i++)
            {
                int index = i;
                m_inputs[i].onSelect.AddListener((val) => m_inputIndex = index);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (m_inputs.Count == 0)
                return;

            if (m_userInterface.Platform == Platform.DESKTOP && m_uiEngine.ActiveModule == InputModule.STANDALONE && !m_blocker.gameObject.activeSelf && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab))
            {
                m_inputIndex--;
                if (m_inputIndex < 0)
                    m_inputIndex = m_inputs.Count - 1;
                m_inputs[m_inputIndex].Select();
                return;
            }

            if (m_uiEngine.ActiveModule == InputModule.STANDALONE && !m_blocker.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Tab))
            {
                m_inputIndex++;
                if (m_inputIndex > m_inputs.Count - 1)
                    m_inputIndex = 0;
                m_inputs[m_inputIndex].Select();
            }
        }

        protected override void Populate(MenuItemInfo[] items)
        {
            
        }

        public void OnSignUpClick()
        {
            m_uiEngine.OpenSignupPage(new MenuArgs()
            {
                Persist = false,
                Title = "SIGN UP"
            });
        }

        public void OnLoginClick()
        {
            if (m_onAction != null)
                m_onAction.Invoke(new string[] { m_email.text, m_password.text });
        }
    }
}
