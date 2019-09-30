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
    WebCamTexture cam = null;
    private RenderTexture _targetRenderTexture;
    private Texture2D _cameraFeed;

    static Dictionary dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
    Point2f[][] corners;
    int[] ids;
    DetectorParameters parameters = DetectorParameters.Create();
    Point2f[][] rejected;

    [SerializeField] private TCPTestClient _client = null;

    private bool _waitForDelay = false;
    private bool _animationPlaying = false;

    // Start is called before the first frame update
    /*void Start()
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

        //cam.Play();
    }*/

    IEnumerator Start()
    {
        findWebCams();

        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("webcam found");
#if UNITY_EDITOR
            cam = new WebCamTexture();
            cam.Play();
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
            //cam.Play();
        }
        else
        {
            Debug.Log("webcam not found");
            cam = null;
        }
    }

    void findWebCams()
    {
        foreach (var device in WebCamTexture.devices)
        {
            Debug.Log("Name: " + device.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (!_waitForDelay && cam != null)
        {
            if (!_animationPlaying)
            {
                _animationPlaying = true;
                Debug.Log("Start animation!!!");
            }
            Mat image = TextureToMat(cam, null);
            CvAruco.DetectMarkers(image, dictionary, out corners, out ids, parameters, out rejected);

            List<int> alreadyFoundIds = new List<int>();
            bool alreadyFound = false;

            if (ids.Length > 0)
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    if (alreadyFoundIds.Count > 0)
                    {
                        foreach (int id in alreadyFoundIds)
                        {
                            if (ids[i] == id)
                            {
                                alreadyFound = true;
                                break;
                            }
                        }
                    }
                    if (!alreadyFound)
                    {
                        alreadyFoundIds.Add(ids[i]);
                        Debug.Log("IDS FOUND:");
                        Debug.Log(ids[i]);
                        if (!_client.playLocally)
                        {
                            //Send id to server

                        }
                        else
                        {
                            //TODO: spawn id shouldn't be 0
                            _client.InterpretMarker(ids[i],0);
                            _waitForDelay = true;
                            StartCoroutine(WaitAndReenable());
                        }
                    }
                }
            }
        }
#elif UNITY_IOS
        if (MiraController.TouchPressed && !_waitForDelay && cam != null)
        {
            if (!_animationPlaying)
            {
                _animationPlaying = true;
                cam.Play();
                Debug.Log("Start animation!!!");
            }
            Mat image = TextureToMat(cam, null);
            CvAruco.DetectMarkers(image, dictionary, out corners, out ids, parameters, out rejected);

            List<int> alreadyFoundIds = new List<int>();
            bool alreadyFound = false;

            if (ids.Length > 0)
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    if (alreadyFoundIds.Count > 0)
                    {
                        foreach (int id in alreadyFoundIds)
                        {
                            if (ids[i] == id)
                            {
                                alreadyFound = true;
                                break;
                            }
                        }
                    }
                    if (!alreadyFound)
                    {
                        alreadyFoundIds.Add(ids[i]);
                        Debug.Log("IDS FOUND:");
                        Debug.Log(ids[i]);
                        if (!_client.playLocally)
                        {
                            //Send id to server

                        }
                        else
                        {
                            _client.InterpretMarker(ids[i]);
                            _waitForDelay = true;
                            StartCoroutine(WaitAndReenable());
                        }
                    }
                }
            }
        }
        if (MiraController.TouchReleased)
        {
            //stop animation
            _animationPlaying = false;
            cam.Stop();
            Debug.Log("Stop animation!!!");
        }
#endif
    }

    //Add delay so when a marker is detected it doesn't detect it multiple times right away
    IEnumerator WaitAndReenable()
    {
        yield return new WaitForSeconds(2f);
        _waitForDelay = false;
    }
}
