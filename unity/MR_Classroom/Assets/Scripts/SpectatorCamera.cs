using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wikitude;

public class SpectatorCamera : MonoBehaviour
{
    private WikitudeCamera _wikitudeCamera = null;

    private void Start()
    {
        _wikitudeCamera = FindObjectOfType<WikitudeCamera>();
        StartCoroutine(WaitAndDeleteAllMiraCameras());
    }

    private IEnumerator WaitAndDeleteAllMiraCameras()
    {
        yield return new WaitForSeconds(10f);
        _wikitudeCamera.EnableCameraRendering = false;
        Destroy(GameObject.Find("BackgroundCamera"));
        Camera.main.clearFlags = CameraClearFlags.Color;
        _wikitudeCamera.gameObject.SetActive(false);
        GameObject.Find("DistortionCameraL").SetActive(false);
        GameObject.Find("DistortionCameraR").SetActive(false);
    }
}
