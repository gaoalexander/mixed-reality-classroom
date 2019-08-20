using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GlowPrePass : MonoBehaviour
{
    private static RenderTexture PrePass;
    private static RenderTexture Blurred;

    private Material _blurMat;

    public float size = 1f;

    public string glowPrePassTexName = "";
    public string glowBlurredTexName = "";

    void Start()
    {
        PrePass = new RenderTexture(1024, 1024, 24);
        PrePass.antiAliasing = QualitySettings.antiAliasing;
        Blurred = new RenderTexture(1024 >> 1, 1024 >> 1, 0);

        var camera = GetComponent<Camera>();
        var glowShader = Shader.Find("Hidden/GlowReplace");
        camera.targetTexture = PrePass;
        camera.SetReplacementShader(glowShader, "Glowable");

        if (glowPrePassTexName.Length > 0)
        {
            Shader.SetGlobalTexture(glowPrePassTexName, PrePass);
        }
        else
        {
            Shader.SetGlobalTexture("_GlowPrePassTex", PrePass);
        }

        if (glowBlurredTexName.Length > 0)
        {
            Shader.SetGlobalTexture(glowBlurredTexName, Blurred);
        }
        else
        {
            Shader.SetGlobalTexture("_GlowBlurredTex", Blurred);
        }

        _blurMat = new Material(Shader.Find("Hidden/Blur"));
        _blurMat.SetVector("_BlurSize", new Vector2(Blurred.texelSize.x * 1.5f * size, Blurred.texelSize.y * 1.5f * size));
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Debug.Log(glowBlurredTexName);
        Graphics.Blit(src, dst);

        Graphics.SetRenderTarget(Blurred);
        GL.Clear(false, true, Color.clear);

        Graphics.Blit(src, Blurred);

        for (int i = 0; i < 4; i++)
        {
            var temp = RenderTexture.GetTemporary(Blurred.width, Blurred.height);
            Graphics.Blit(Blurred, temp, _blurMat, 0);
            Graphics.Blit(temp, Blurred, _blurMat, 1);
            RenderTexture.ReleaseTemporary(temp);
        }
    }
}
