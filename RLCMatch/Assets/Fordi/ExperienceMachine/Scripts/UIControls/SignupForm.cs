using Cornea.Web;
using Fordi.Core;
using Fordi.UI;
using Fordi.UI.MenuControl;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fordi.Networking;
using Fordi.Common;
using Fordi;

namespace RLC.View
{
    public class SignupForm : MenuScreen
    {
        [SerializeField]
        private TMP_InputField m_displayeName, m_email, m_password, m_dateOfBirth;
        [SerializeField]
        private Toggle m_maleToggle, m_femaleToggle;
        [SerializeField]
        private INetwork m_network;

        [SerializeField]
        private List<TMP_InputField> m_inputs = new List<TMP_InputField>();

        private int m_inputIndex = 0;


        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_network = IOCCore.Resolve<INetwork>();
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

        public void OpenCalendar()
        {
            m_uiEngine.OpenCalendar(new CalendarArgs()
            {
                ReferenceTime = DateTime.Now - new TimeSpan(1, 0, 0, 0),
                CalendarType = CalendarArgs.CalendarTypye.PREDATE,
                OnClick = (date) => {
                    m_dateOfBirth.text = date;
                },
                TimeForm = null
            });
        }

        public void SignUp()
        {
            m_uiEngine.DisplayProgress("Registering user...");

            m_webInterface.UserSignup(m_displayeName.text, m_maleToggle.isOn? Gender.MALE : Gender.FEMALE, m_password.text, m_email.text, m_dateOfBirth.text,
                (isNetworkError, message) =>
                {
                    Error error = new Error();

                    JsonData response = JsonMapper.ToObject(message);

                    if (response["status"].ToString() == "200")
                    {
                        m_network.EnterMeeting();
                        return;
                    }

                    error.ErrorCode = Error.E_NotFound;
                    error.ErrorText = response["message"].ToString();
                    m_uiEngine.DisplayResult(error, false);
                });
        }
    }
}