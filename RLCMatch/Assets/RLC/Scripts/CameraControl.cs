using Fordi.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RLC.Core
{
    public enum CameraMode
    {
        THIRD_PERSON,
        FIRST_PERSON,
        INDEPENDENT
    }


    public interface ICameraControl
    {
        void SwitchMode(CameraMode mode);
    }

    public class CameraControl : MonoBehaviour, ICameraControl
    {
        //[SerializeField]
        //private StandalonePlayer m_male, m_female;

        [SerializeField]
        private StandalonePlayer m_currentPlayer;

        [SerializeField]
        private CameraTargetOrientationScript m_leftCam, m_rightCam;

        [SerializeField]
        private CameraMode m_cameraMode = CameraMode.FIRST_PERSON;

        public CameraMode CameraMode { get { return m_cameraMode; } }
        
        private CameraTargetOrientationScript m_currentCam;

        [Header("Controller Input")]
        public string horizontalInput = "Horizontal";
        public string verticallInput = "Vertical";

        public static bool FreeHand = false;

        private void Start()
        {
            m_currentCam = m_rightCam;
            
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
                FreeHand = !FreeHand;
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (m_cameraMode == CameraMode.FIRST_PERSON)
                    SwitchPlayer();
                else
                    SwitchIndependentCamera();
            }
        }

        private void SwitchIndependentCamera()
        {
            if (m_currentCam == m_leftCam)
            {
                m_leftCam.gameObject.SetActive(false);
                m_rightCam.gameObject.SetActive(true);
                m_currentCam = m_rightCam;
            }
            else
            {
                m_leftCam.gameObject.SetActive(true);
                m_rightCam.gameObject.SetActive(false);
                m_currentCam = m_leftCam;
            }
        }

        private void SwitchPlayer()
        {
            //if (m_currentPlayer == m_male)
            //{
            //    m_male.RequestHaltMovement(true);
            //    m_female.RequestHaltMovement(false);
            //    m_currentPlayer = m_female;
            //}
            //else
            //{
            //    m_male.RequestHaltMovement(false);
            //    m_female.RequestHaltMovement(true);
            //    m_currentPlayer = m_male;
            //}
        }

        public void SwitchMode(CameraMode mode)
        {
            return;
            //if (m_cameraMode == mode)
            //    return;

            //m_leftCam.gameObject.SetActive(mode == CameraMode.INDEPENDENT && m_currentCam == m_leftCam);
            //m_rightCam.gameObject.SetActive(mode == CameraMode.INDEPENDENT && m_currentCam == m_rightCam);
            //m_male.gameObject.SetActive(mode == CameraMode.FIRST_PERSON);
            //m_female.gameObject.SetActive(mode == CameraMode.FIRST_PERSON);

            //m_cameraMode = mode;
        }
    }
}