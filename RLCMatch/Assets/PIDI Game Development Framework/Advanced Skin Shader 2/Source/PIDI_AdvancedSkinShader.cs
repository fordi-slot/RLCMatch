

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedSkin2 {

    [System.Serializable]
    public class SkinProfile {

        public bool isSkinMaterial;

        public bool mobileMode;

        public bool enableDecals = false;

        public bool enableTranslucency = true;

        public Color skinColor = Color.white;
        public Color specColor = new Color(0.15f, 0.15f, 0.15f, 1.0f);
        public Color sssColor = Color.red;


        public float glossinessLevel = 0.4f;
        public float occlusionStrength = 1.0f;
        public float translucencyStrength = 1.0f;
        public float skinSurfaceWarp = 0.25f;


        public float microSkinUVScale = 24;
        public float debugTensionMap;
        public float minVertexDistance;

        public Texture2D colorMap;
        public Texture2D specGlossMap;
        public Texture2D translucencyMap;
        public Texture2D normalMap;
        public Texture2D occlusionMap;
        public Texture2D microSkinMap;
        public Texture2D skinDataMap;
        public Texture2D wrinklesMap;


        public int decal0BlendMode;
        public int decal1BlendMode;

        public int decal0UVSet;
        public int decal1UVSet;

        public Vector4 decal0Coords = new Vector4(1,1,0,0);
        public Vector4 decal1Coords = new Vector4(1,1,0,0);

        public Texture2D decal0Tex;
        public Texture2D decal1Tex;
        
        public Color decal0Color = Color.white;
        public Color decal1Color = Color.white;
        public Color decal0SpecCol = new Color(0.15f,0.15f,0.15f,0.5f);
        public Color decal1SpecCol = new Color( 0.15f, 0.15f, 0.15f, 0.5f );

        public Texture2D decal0SpecMap;
        public Texture2D decal1SpecMap;



    }


#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class PIDI_AdvancedSkinShader : MonoBehaviour {



#if UNITY_EDITOR
        public bool[] folds = new bool[32];
#endif

        private string version = "2.35";

        public string Version { get { return version; } }

        private Renderer rend;
        private Material[] sharedMats = new Material[1];

        [SerializeField] protected Texture2D defaultBump;

        public bool deferredMode;

        public bool srpMode;



        public Light[] deferredLights = new Light[4];


        public SkinProfile[] skinProfiles = new SkinProfile[1];

#if UNITY_EDITOR
        public int currentSkinProfile;
#endif

        public bool updateDynamically;
        public float updateTime = 0.015f;

        public Transform headBone;
        public Quaternion hRotation;

        private float timer = 0.015f;



        public void InitialSetup() {
            var slots = new string[GetComponent<Renderer>().sharedMaterials.Length];

            List<SkinProfile> skins = new List<SkinProfile>();

            for ( int i = 0; i < slots.Length; i++ ) {
                slots[i] = "MATERIAL SLOT " + i;
                if ( skinProfiles.Length > i ) {
                    skins.Add( skinProfiles[i] == null ? new SkinProfile() : skinProfiles[i] );
                }
                else {
                    skins.Add( new SkinProfile() );
                }

                var shaderName = GetComponent<SkinnedMeshRenderer>().sharedMaterials[i].shader.name;

                skins[i].isSkinMaterial = shaderName.Contains( "Advanced Skin Shader" );

                if ( skins[i].isSkinMaterial ) {
                    skins[i].mobileMode = shaderName.Contains( "Mobile" );

                }

            }

            skinProfiles = skins.ToArray();
        }


        public void OnEnable() {

            InitialSetup();

            UpdateSkinMaterials();
        }


        public void Update() {

#if UNITY_EDITOR

            if ( !Application.isPlaying ) {
                UpdateSkinMaterials();
            }

#endif
            if ( Application.isPlaying ) {
                bool dynamicWrinkles = false;

                if ( (dynamicWrinkles || updateDynamically) && Time.timeSinceLevelLoad > timer ) {
                    UpdateSkinMaterials();
                    timer = Time.timeSinceLevelLoad + (Application.platform == RuntimePlatform.Android ? (updateTime * 8.0f) : (updateTime));
                }
            }
        }


        public void UpdateSkinMaterials() {

            if ( rend ) {
                rend.SetPropertyBlock( null );
            }
            else {
                rend = GetComponent<SkinnedMeshRenderer>();
                return;
            }

#if UNITY_2018_1_OR_NEWER
            for ( int i = 0; i < rend.sharedMaterials.Length; i++ ) {
                rend.SetPropertyBlock( null, i );
            }
#endif


            Vector4[] lightsPosDir = new Vector4[4], lightsColor = new Vector4[4], lightsData = new Vector4[4];

            if (deferredMode) {
                for (int i = 0; i < 4; i++) {
                    if (deferredLights[i]) {
                        lightsPosDir[i] = deferredLights[i].type == LightType.Directional ? -deferredLights[i].transform.forward : deferredLights[i].transform.position;
                        lightsPosDir[i].w = deferredLights[i].type == LightType.Directional ? 0 : 1;
                        lightsColor[i] = deferredLights[i].color;
                        lightsColor[i].w = deferredLights[i].intensity;
                        lightsData[i].x = deferredLights[i].range;
                        lightsData[i].y = deferredLights[i].shadows == LightShadows.None ? 0 : 1;

                        if (deferredLights[i].type == LightType.Spot) {
                            var lDot = Vector3.Dot((deferredLights[i].transform.position - transform.position).normalized, deferredLights[i].transform.forward);
                            var spotAtten = lDot > (1 - (deferredLights[i].spotAngle / 90.0f)) ? 1 : 0;

                            lightsColor[i].x *= spotAtten;
                            lightsColor[i].y *= spotAtten;
                            lightsColor[i].z *= spotAtten;
                        }
                    }
                }
            }

            for (int i = 0; i < skinProfiles.Length; i++) {
                if (skinProfiles[i].isSkinMaterial) {

                    var mat = new MaterialPropertyBlock();
                    GetPropertyBlock(ref mat, i);

                    mat.SetColor("_PSkinColor", skinProfiles[i].skinColor);

                    mat.SetTexture("_PSkinMainTex", skinProfiles[i].colorMap ? skinProfiles[i].colorMap : Texture2D.whiteTexture);
                    mat.SetTexture("_PSkinBaseNormals", skinProfiles[i].normalMap ? skinProfiles[i].normalMap : defaultBump? defaultBump:Texture2D.blackTexture);

                    mat.SetTexture("_PSkinOcclusionMap", skinProfiles[i].occlusionMap ? skinProfiles[i].occlusionMap : Texture2D.whiteTexture);
                    mat.SetFloat("_PSkinOcclusionLevel", skinProfiles[i].occlusionStrength);

                    mat.SetTexture("_PSkinSpecMap", skinProfiles[i].specGlossMap ? skinProfiles[i].specGlossMap : Texture2D.whiteTexture);
                    mat.SetColor("_PSkinSpecColor", skinProfiles[i].specColor);
                    mat.SetFloat("_PSkinGlossiness", skinProfiles[i].glossinessLevel);

                    mat.SetColor("_PSkinSSSColor", skinProfiles[i].sssColor);

                    mat.SetTexture("_PSkinMicroSkin", skinProfiles[i].microSkinMap ? skinProfiles[i].microSkinMap : Texture2D.whiteTexture);
                    mat.SetFloat("_PSkinMicroSkinTiling", skinProfiles[i].microSkinUVScale);


                    mat.SetTexture("_PSkinTranslucencyMap", skinProfiles[i].translucencyMap ? skinProfiles[i].translucencyMap : Texture2D.blackTexture);
                    mat.SetFloat("_PSkinTranslucencyLevel", skinProfiles[i].translucencyStrength);

                    if (headBone) {

                        var rotEuler = (Quaternion.Inverse(headBone.rotation) * hRotation);

                        mat.SetVector("_PSkinHeadBonePos", headBone.position);
                        mat.SetVector("_PSkinHeadBoneRotC", new Vector4(rotEuler.w,rotEuler.x,rotEuler.y,rotEuler.z));

                    }

                    var shName = sharedMats[i].shader.name;

                    mat.SetTexture( "_PSkinDecal0Tex", skinProfiles[i].enableDecals ? skinProfiles[i].decal0Tex ? skinProfiles[i].decal0Tex : Texture2D.blackTexture:Texture2D.blackTexture);
                    mat.SetTexture( "_PSkinDecal1Tex", skinProfiles[i].enableDecals ? skinProfiles[i].decal1Tex ? skinProfiles[i].decal1Tex : Texture2D.blackTexture:Texture2D.blackTexture);
                    
                    mat.SetTexture( "_PSkinDecal0SpecMap", skinProfiles[i].enableDecals ? skinProfiles[i].decal0SpecMap ? skinProfiles[i].decal0SpecMap : Texture2D.whiteTexture:Texture2D.whiteTexture);
                    mat.SetTexture( "_PSkinDecal1SpecMap", skinProfiles[i].enableDecals ? skinProfiles[i].decal1SpecMap ? skinProfiles[i].decal1SpecMap : Texture2D.whiteTexture:Texture2D.whiteTexture);

                    mat.SetColor( "_PSkinDecal0Color", skinProfiles[i].decal0Color );
                    mat.SetColor( "_PSkinDecal0SpecColor", skinProfiles[i].decal0SpecCol );
                    
                    mat.SetColor( "_PSkinDecal1Color", skinProfiles[i].decal1Color );
                    mat.SetColor( "_PSkinDecal1SpecColor", skinProfiles[i].decal1SpecCol );

                    mat.SetFloat( "_PSkinDecal0BlendMode", skinProfiles[i].decal0BlendMode );
                    mat.SetFloat( "_PSkinDecal1BlendMode", skinProfiles[i].decal1BlendMode );

                    mat.SetFloat( "_PSkinDecal0UV", skinProfiles[i].decal0UVSet );
                    mat.SetFloat( "_PSkinDecal1UV", skinProfiles[i].decal1UVSet );

                    mat.SetVector( "_PSkinDecal0UVTransform", skinProfiles[i].decal0Coords );
                    mat.SetVector( "_PSkinDecal1UVTransform", skinProfiles[i].decal1Coords );

                    if (deferredMode) {
                        mat.SetVector("_PSkinWorldPos", transform.position);

                        if (!srpMode && Camera.current)
                            mat.SetVector("_PSkinViewDir", Camera.current.transform.forward);

                        mat.SetVectorArray("_PSkinDefLightsPosDir", lightsPosDir);
                        mat.SetVectorArray("_PSkinDefLightsColor", lightsColor);
                        mat.SetVectorArray("_PSkinDefLightsData", lightsData);

                    }


                    SetPropertyBlock(mat, i);
                }
                else {
                    var mat = new MaterialPropertyBlock();
                    mat.Clear();
                    SetPropertyBlock(mat, i);
                }
            }

        }



        void GetPropertyBlock( ref MaterialPropertyBlock block, int index ) {

            if ( !rend ) {
                rend = GetComponent<Renderer>();
            }

            if ( rend ) {
                if ( sharedMats.Length != rend.sharedMaterials.Length ) {
                    SetPropertyBlock( null, index );
                    sharedMats = rend.sharedMaterials;
                }

                sharedMats = rend.sharedMaterials;
            }

            if ( block == null || index < 0 || !rend || sharedMats.Length <= index ) {
                return;
            }
            else {
#if UNITY_2018_OR_NEWER
                rend.GetPropertyBlock(block, index);
#else
                sharedMats = rend.sharedMaterials;
                var t = sharedMats[0];
                sharedMats[0] = sharedMats[index];
                rend.sharedMaterials = sharedMats;
                rend.GetPropertyBlock( block );
                sharedMats[0] = t;
                rend.sharedMaterials = sharedMats;
#endif
            }
        }


        void SetPropertyBlock( MaterialPropertyBlock block, int index ) {

            if ( !rend ) {
                rend = GetComponent<Renderer>();
            }

            if ( rend ) {
                if ( sharedMats.Length != rend.sharedMaterials.Length ) {
                    var tempMats = rend.sharedMaterials;
                    rend.sharedMaterials = sharedMats;

                    rend.SetPropertyBlock( null );
#if UNITY_2018_1_OR_NEWER
                    for ( int i = 0; i < sharedMats.Length; i++ ) {
                        rend.SetPropertyBlock( null, i );
                    }
#endif
                    rend.sharedMaterials = tempMats;
                }

                sharedMats = rend.sharedMaterials;
            }


            if ( index < 0 || !rend || rend.sharedMaterials.Length <= index ) {
                return;
            }
            else {
                if ( skinProfiles[index].isSkinMaterial ) {
#if UNITY_2018_1_OR_NEWER
					rend.SetPropertyBlock( block, index );
#else
                    sharedMats = rend.sharedMaterials;
                    var t = sharedMats[0];
                    sharedMats[0] = sharedMats[index];
                    rend.sharedMaterials = sharedMats;
                    rend.SetPropertyBlock( block );
                    sharedMats[0] = t;
                    rend.sharedMaterials = sharedMats;
#endif
                }
                else {

#if UNITY_2018_1_OR_NEWER
					//rend.SetPropertyBlock( block, index );
#else
                    sharedMats = rend.sharedMaterials;
                    var t = sharedMats[0];
                    sharedMats[0] = sharedMats[index];
                    rend.sharedMaterials = sharedMats;
                    rend.SetPropertyBlock( block );
                    sharedMats[0] = t;
                    rend.sharedMaterials = sharedMats;

#endif
                }
            }
        }




    }



}