using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Fordi.Common;
using Fordi.Core;
using AudioType = Fordi.Core.AudioType;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fordi.Meeting;
using Fordi.Meetings.UI;

namespace Fordi.UI.MenuControl
{
    public enum InputModule
    {
        STANDALONE,
        OCULUS
    }

    public class Sound
    {
        public float Time { get; set; }
        public AudioClip Clip { get; private set; }
        public Sound(float time, AudioClip clip)
        {
            Time = time;
            Clip = clip;
        }
    }


    public class VRMenu : UserInterface
    {
        public override BaseInputModule InputModule
        {
            get
            {
                throw new NotImplementedException();
                return m_inputModule;
            }
        }

        public override IScreen OpenCalendar(CalendarArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
