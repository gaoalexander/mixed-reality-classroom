using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using Wikitude;
using static OpenCvSharp.Unity;

public class DetectMarkers : MonoBehaviour
{
    WebCamTexture cam;
    private RenderTexture _targetRenderTexture;
    private Texture2D _cameraFeed;

    static Dictionary dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
    Point2f[][] corners;
    int[] ids;
    DetectorParameters parameters = DetectorParameters.Create();
    Point2f[][] rejected;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        cam = new WebCamTexture();
#elif UNITY_IOS
        foreach (WebCamDevice camera in WebCamTexture.devices)
        {
            if (camera.isFrontFacing)
            {
                string frontCamName = camera.name;
		        cam = new WebCamTexture(frontCamName);  
            }
        }
#endif

        cam.Play();
    }

    // Update is called once per frame
    void Update()
    {

            Mat image =  TextureToMat(cam, null);
            CvAruco.DetectMarkers(image, dictionary, out corners, out ids, parameters, out rejected);

		    if (ids.Length > 0)
		    {
                for(int i =0; i < ids.Length; i++)
			    {
				    Debug.Log("IDS FOUND:");
				    Debug.Log(ids[i]);
			    }
		    }
        
    }

  

}
