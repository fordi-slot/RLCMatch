using Cornea.Web;
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
        private StandalonePlayer m_male, m_female;

        [SerializeField]
        protected List<AssetReferenceWrapper> m_deps;

        public IPlayer Player { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public Platform Platform { get { return m_platform; } }

        private IExperienceMachine m_experienceMachine = null;
        private IAssetLoader m_assetLoader = null;
        private IWebInterface m_webInterface = null;

        private void Awake()
        {
            m_assetLoader = IOCCore.Resolve<IAssetLoader>();
            UserInterface = GetComponentInChildren<IUserInterface>(true);
            m_experienceMachine = IOCCore.Resolve<IExperienceMachine>();
            m_webInterface = IOCCore.Resolve<IWebInterface>();

            if (m_webInterface.UserInfo == null)
                Player = m_female.GetComponent<IPlayer>();
            else if (m_webInterface.UserInfo.gender == Gender.MALE)
                Player = m_male.GetComponent<IPlayer>();
            else
                Player = m_female.GetComponent <IPlayer>();

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
            m_assetLoader.LoadAndSpawn<GameObject>(new AssetArgs(m_deps[index].AssetReference.RuntimeKey.ToString(), m_deps[index].UnloadOnDestroy));

            //foreach (var item in m_deps)
            //    m_assetLoader.LoadAndSpawn<GameObject>(new AssetArgs(item.AssetReference.RuntimeKey.ToString(), item.UnloadOnDestroy));
        }
    }
}
