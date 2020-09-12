using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Core
{
    public enum ControlType
    {
        NONE,
        CROSS_SECTION,
        MEASUREMENT,
        EXPLODE_IMPLODE
    }
    public class ControlConfigurations
    {
        public MeasurementConfiguration Measurement { get; set; } = new MeasurementConfiguration();
        public CrossSectionConfiguration CrossSection { get; set; } = new CrossSectionConfiguration();
        public ExplodeImplodeConfiguration ExplodeImplode { get; set; } = new ExplodeImplodeConfiguration();
        public ControlType SelectedControl { get; set; } = ControlType.NONE;
    }

    public class CrossSectionConfiguration
    {
        public enum CrossSectionType
        {
            CUBE_SECTION
        }

        public CrossSectionType Mode { get; set; }
    }

    public class MeasurementConfiguration
    {
        public enum CrossSectionType
        {
            LINE,
            CIRCLE,
            ANGLE
        }

        public CrossSectionType Mode { get; set; } = CrossSectionType.LINE;
    }

    public class ExplodeImplodeConfiguration
    {
        public bool X, Y, Z = true;
        public float ExplodeImpact = 1;
    }
}
