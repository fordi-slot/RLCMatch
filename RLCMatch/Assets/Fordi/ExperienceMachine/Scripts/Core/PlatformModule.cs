using Fordi.AssetManagement;
using Fordi.Common;
using Fordi.Core;
using Fordi.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fordi.Platforms
{
    public interface IPlatformModule
    {
        IPlayer Player { get; }
        IUserInterface UserInterface { get; }
        Platform Platform { get; }
    }

    public class PlatformModule : MonoBehaviour, IPlatformModule
    {
        [SerializeField]
        private Platform m_platform;

        [SerializeField]
        protected List<AssetReferenceWrapper> m_deps;

        public IPlayer Player { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public Platform Platform { get { return m_platform; } }

        private IExperienceMachine m_experienceMachine = null;
        private IAssetLoader m_assetLoader = null;

        private void Awake()
        {
            m_assetLoader = IOCCore.Resolve<IAssetLoader>();
            Player = GetComponentInChildren<IPlayer>(true);
            UserInterface = GetComponentInChildren<IUserInterface>(true);
            m_experienceMachine = IOCCore.Resolve<IExperienceMachine>();
            m_experienceMachine.RegisterPlatform(this);
        }

        private IEnumerator Start()
        {
            if (m_deps.Count == 0)
                yield break;

            yield return null;

            int index = 0;
            if (m_deps.Count > 1)
                index = Random.Range(0, m_deps.Count);

            index = 1;

            m_assetLoader.LoadAndSpawn<GameObject>(new AssetArgs(m_deps[index].AssetReference.RuntimeKey.ToString(), m_deps[index].UnloadOnDestroy));

            //foreach (var item in m_deps)
            //    m_assetLoader.LoadAndSpawn<GameObject>(new AssetArgs(item.AssetReference.RuntimeKey.ToString(), item.UnloadOnDestroy));
        }
    }
}
