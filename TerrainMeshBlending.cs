using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class TerrainMeshBlending : MonoBehaviour
{
    public float m_tiling = 1f;
    public float m_falloff = 1f;
    public float m_minHeight = -20f;
    public float m_maxHeight = 20f;
    public float m_blendingRadius = 50f;
    public int m_heightMapResolution = 1024;

    public Texture m_albedoMap;
    public Texture m_normalMap;
    public Texture m_maosMap;

    private const string LAYER_NAME = "Terrain";
    private const float CAM_HEIGHT = 100f;

    private Camera m_topDownCamera;
    private RenderTexture m_renderTexture;

    public void OnEnable()
    {
        m_topDownCamera = GetComponent<Camera>();

        var blendMapShader = Shader.Find("EDW/MeshBlending/TerrainHeightMap");
        if (!blendMapShader)
            return;

        m_topDownCamera.SetReplacementShader(blendMapShader, "");

        UpdateValues();

#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorApplication.update += OnUpdate;
#endif
    }

    public void OnDisable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorApplication.update -= OnUpdate;
#endif

        m_topDownCamera.ResetReplacementShader();
        m_topDownCamera.targetTexture = null;
    }

    /// <summary>
    /// In-Game we use the LateUpdate() method for updating shader params.
    /// </summary>
    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        OnUpdate();
    }

    private void OnUpdate()
    {
        var currCam = Camera.current;
        if (!currCam)
            return;

        if (currCam == m_topDownCamera)
            return;

        var camPos = currCam.transform.position;
        transform.position = new Vector3(camPos.x, CAM_HEIGHT, camPos.z);
        Shader.SetGlobalVector("_TerrainCurrPos", transform.position);
    }

    private void UpdateValues()
    {
        if (!m_topDownCamera || !Mathf.IsPowerOfTwo(m_heightMapResolution))
            return;

        if (!m_renderTexture || m_renderTexture.width != m_heightMapResolution || m_renderTexture.height != m_heightMapResolution)
        {
            m_renderTexture = new RenderTexture(m_heightMapResolution, m_heightMapResolution, 16, RenderTextureFormat.ARGBFloat)
            {
                name = "TerrainHeightMap"
            };
            if (!m_renderTexture.Create())
            {
                Debug.LogError("Couldn't create render texture!");
                return;
            }
        }

        m_topDownCamera.orthographicSize = m_blendingRadius;
        m_topDownCamera.targetTexture = m_renderTexture;

        // Update shader values.
        Shader.SetGlobalFloat("_TerrainTiling", m_tiling);
        Shader.SetGlobalFloat("_TerrainFalloff", m_falloff);
        Shader.SetGlobalFloat("_TerrainMinHeight", m_minHeight);
        Shader.SetGlobalFloat("_TerrainMaxHeight", m_maxHeight);
        Shader.SetGlobalFloat("_TerrainOrthoSize", m_blendingRadius * 2f);
        Shader.SetGlobalTexture("_TerrainAlbedoTex", m_albedoMap);
        Shader.SetGlobalTexture("_TerrainNormalTex", m_normalMap);
        Shader.SetGlobalTexture("_TerrainMaosTex", m_maosMap);
        Shader.SetGlobalTexture("_TerrainHeightMap", m_renderTexture);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateValues();
    }
#endif

    private List<MeshRenderer> GetTerrainMeshes(int layer)
    {
        return FindObjectsOfType<MeshRenderer>()
            .Where(mf => mf.hideFlags == HideFlags.None && mf.gameObject.layer == layer).ToList();
    }
}
