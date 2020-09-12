using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Core
{
    public enum Feature
    {
        GRAB = 0,
        TELEPORT,
        WAYPOINT_TELEPORT,

        ANNOTATION = 11,
        CROSS_SECTION,
        MEASUREMENT,
        EXPLODE_IMPLODE
    }


    public enum AccessController
    {
        ROLE,
        GRAB,
        CROSS_SECTION,
        UI
    }

    public interface IPermissions
    {
        bool IsAllowed(Feature feature);
        void Block(Feature feature, AccessController controller);
        void UnBlock(Feature feature, AccessController controller);
        void SetScenePermission(Feature feature, AccessController accessController, bool v);
    }

    public class Permissions : MonoBehaviour, IPermissions
    {
        private Dictionary<Feature, HashSet<AccessController>> m_accessPermissions = new Dictionary<Feature, HashSet<AccessController>>();

        public void Block(Feature feature, AccessController controller)
        {
            if (!m_accessPermissions.ContainsKey(feature))
                m_accessPermissions[feature] = new HashSet<AccessController>();

            m_accessPermissions[feature].Add(controller);
        }

        public void UnBlock(Feature feature, AccessController controller)
        {
            if (!m_accessPermissions.ContainsKey(feature))
                return;

            if (m_accessPermissions[feature].Contains(controller))
                m_accessPermissions[feature].Remove(controller);
        }

        public bool IsAllowed(Feature feature)
        {
            if (!m_accessPermissions.ContainsKey(feature))
                return true;
            return m_accessPermissions[feature].Count == 0;
        }

        public void SetScenePermission(Feature feature, AccessController controller, bool val)
        {
            if (val)
                UnBlock(feature, controller);
            else
                Block(feature, controller);
        }
    }
}
