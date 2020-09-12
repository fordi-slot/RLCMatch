﻿using Invector.vCharacterController;
using RLC.Core;
using System;
using UnityEngine;

namespace Fordi.Core
{
    public class FordiThirdPersonInput : vThirdPersonInput
    {
        [Header("Controller Input")]
        public string ZoomInput = "Mouse ScrollWheel";

        [SerializeField]
        private bool m_onRest = false;
        public bool OnRest { get { return m_onRest; } set { m_onRest = value; } }

        [SerializeField]
        [Range(0, 1)]
        private float m_zoomSpeed = .7f;
        private float m_zoomAmount = 0;
        private float m_maximumZoom = 8;
        private float m_minimumZoom = .5f;

        //protected override void SprintInput()
        //{
        //    if (m_onRest && cc.isSprinting)
        //    {
        //        cc.Sprint(false);
        //        return;
        //    }

        //    base.SprintInput();
        //}

        protected override void CameraInput()
        {
            if (cc == null)
                InitilizeController();

            if (!cameraMain)
            {
                cameraMain = transform.parent.GetComponentInChildren<Camera>(true);
                cc.rotateTarget = cameraMain.transform;
            }

            if (cameraMain)
            {
                cc.UpdateMoveDirection(cameraMain.transform);
            }

            if (tpCamera == null)
                return;

            if (!InteractablelUI.PointerOnUI)
            {
                m_zoomAmount += 4 * m_zoomSpeed * Input.GetAxis(ZoomInput);
                m_zoomAmount = Mathf.Clamp(m_zoomAmount, m_minimumZoom, m_maximumZoom);
                tpCamera.defaultDistance = m_zoomAmount;
            }

            if (InteractablelUI.PointerOnUI || (!CameraControl.FreeHand && !Input.GetMouseButton(0)))
            {
                tpCamera.RotateCamera(0, 0);
                return;
            }

            var Y = Input.GetAxis(rotateCameraYInput);
            var X = Input.GetAxis(rotateCameraXInput);
            tpCamera.RotateCamera(X, Y);
        }

        protected override void InitializeTpCamera()
        {
            if (tpCamera == null)
            {
                tpCamera = transform.parent.GetComponentInChildren<vThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }
        }

        public override void MoveInput()
        {
            cc.input.x = m_onRest ? 0 : Input.GetAxis(horizontalInput);
            cc.input.z = m_onRest ? 0 : Input.GetAxis(verticallInput);
        }
    }
}