using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Core
{
    /// <summary>
    /// This is preserved between scenes.
    /// </summary>
    public static class Selection
    {
        public static ColorGroup ColorGroup { get; set; }
        public static string Location { get; set; }
        public static AudioClip Music { get; set; }
        public static AudioClip VoiceOver { get; set; }
        public static ExperienceType ExperienceType { get; set; } = ExperienceType.HOME;
        public static string MusicGroup { get; set; }
    }
}
