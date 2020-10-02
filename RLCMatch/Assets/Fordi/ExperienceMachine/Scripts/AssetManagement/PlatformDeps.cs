using Cornea.Web;
using Fordi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fordi.AssetManagement
{
    public class PlatformDeps : Deps
    {
        private int m_successfulLoad = 0;

        public static EventHandler AllDependenciesLoaded;

        private IWebInterface m_webInterface;

        private void Awake()
        {
            m_webInterface = IOCCore.Resolve<IWebInterface>();
            m_webInterface.OnAssetsLoaded += OnAssetsLoaded;
        }

        private void OnDestroy()
        {
            m_webInterface.OnAssetsLoaded -= OnAssetsLoaded;
        }

        private void OnAssetsLoaded(object sender, EventArgs e)
        {
            Debug.LogError("OnAssetsLoaded");
            foreach (var item in m_deps)
                m_assetLoader.LoadAndSpawn<GameObject>(new AssetArgs(item.AssetReference.RuntimeKey.ToString(), item.UnloadOnDestroy), (result) =>
                {
                    if (result.OperationException == null && result.Status == AsyncOperationStatus.Succeeded && result.Result != null)
                        m_successfulLoad++;
                    if (m_successfulLoad == m_deps.Count)
                        AllDependenciesLoaded?.Invoke(this, EventArgs.Empty);
                });
        }
    }
}
