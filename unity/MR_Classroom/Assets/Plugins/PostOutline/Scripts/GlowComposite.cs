﻿using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GlowComposite : MonoBehaviour
{
	[Range (0, 10)]
	public float intensity = 2;

	private Material _compositeMat;

	void OnEnable()
	{
		_compositeMat = new Material(Shader.Find("Hidden/GlowComposite"));
    }

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		_compositeMat.SetFloat("_Intensity", intensity);
        Graphics.Blit(src, dst, _compositeMat, 0);
	}
}
