using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Networking
{
    [RequireComponent(typeof(PhotonTransformView))]
    public class OvrPlayerSync : MonoBehaviour, ISyncHelper, IPunObservable
    {

        public bool avatarSet = false;
        public int playerId;
        PhotonTransformView pView;
        public bool isRemotePlayer = false;
        private Transform fChild = null;
        private Transform camRig = null;

        public void Init(bool _avatarSet, bool _isRemotePlayer, int _playerId)
        {
            avatarSet = _avatarSet;
            isRemotePlayer = _isRemotePlayer;
            playerId = _playerId;
        }

        private void OnEnable()
        {
            //if (GetComponent<PhotonTransformView>() != null)
            //    pView = GetComponent<PhotonTransformView>();
            //else
            //    pView = gameObject.AddComponent<PhotonTransformView>();

            //this.GetComponent<PhotonView>().Synchronization = ViewSynchronization.UnreliableOnChange;
            //this.GetComponent<PhotonView>().ObservedComponents = new List<Component>();
            //if (this.GetComponent<PhotonView>().ObservedComponents.Count > 0)
            //    this.GetComponent<PhotonView>().ObservedComponents.Clear();
            //this.GetComponent<PhotonView>().ObservedComponents.Add(this.transform.GetComponent<PhotonTransformView>());
            //this.GetComponent<PhotonView>().ObservedComponents.Add(this);

            ////Debug.Log("______" + GetComponent<PhotonView>().ObservedComponents.Count);

            //pView.m_SynchronizePosition = true;
            //pView.m_SynchronizeRotation = true;

            //pView.m_PositionModel.TeleportEnabled = true;
        }

        //private void Update()
        //{
        //    if (!isRemotePlayer)
        //        return;

        //    if (fChild == null)
        //    {
        //        if (transform.childCount > 0)
        //            fChild = transform.GetChild(0);
        //        return;
        //    }

        //    fChild.localPosition = new Vector3(0, -1, 0);
        //    //fChild.rotation = Quaternion.identity;
        //}

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
          
        }

        public void PauseSync()
        {

        }

        public void ResumeSync()
        {

        }
    }
}
