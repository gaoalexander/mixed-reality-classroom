using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GlowComposite : MonoBehaviour
{
	[Range (0, 10)]
	public float intensity = 2;

    public string glowCompositeName = "";

    private Material _compositeMat;

	void Start()
	{
        if (glowCompositeName.Length > 0)
        {
            _compositeMat = new Material(Shader.Find(glowCompositeName));
        }
        else
        {
            _compositeMat = new Material(Shader.Find("Hidden/GlowComposite"));
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        _compositeMat.SetFloat("_Intensity", intensity);
        Graphics.Blit(src, dst, _compositeMat, 0);
    }
}
