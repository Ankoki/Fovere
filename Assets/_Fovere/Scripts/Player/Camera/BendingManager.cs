using UnityEngine;
using UnityEngine.Rendering;

namespace Fovere
{
    public class BendingManager : MonoBehaviour
    {
        private const string BendingFeature = "_EnableBending";

        private void Awake()
        {
            if (Application.isPlaying)
                Shader.EnableKeyword(BendingFeature);
            else
                Shader.DisableKeyword(BendingFeature);
            Debug.Log("BendingFeature Enabled: " + Shader.IsKeywordEnabled(BendingFeature));
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
                return;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            Shader.DisableKeyword(BendingFeature);
        }

        private static void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
        {
            cam.cullingMatrix = Matrix4x4.Ortho(-99, 99, -99, 99, 0.001f, 99) *
                                cam.worldToCameraMatrix;
        }

        private static void OnEndCameraRendering(ScriptableRenderContext ctx, Camera cam)
        {
            cam.ResetCullingMatrix();
        }

    }
}