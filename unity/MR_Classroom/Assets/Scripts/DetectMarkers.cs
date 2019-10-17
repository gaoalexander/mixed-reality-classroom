using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using Wikitude;
using static OpenCvSharp.Unity;

//namespace Wikitude
//{
public class DetectMarkers : MonoBehaviour
{
    WebCamTexture cam = null;
    //private RenderTexture _targetRenderTexture;
    //private Texture2D _cameraFeed;

    static Dictionary dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
    Point2f[][] corners;
    int[] ids;
    DetectorParameters parameters = DetectorParameters.Create();
    Point2f[][] rejected;

    [SerializeField] private TCPTestClient _client = null;

    private bool _waitForDelay = false;
    private bool _animationPlaying = false;

    [HideInInspector] public Camera camera = null;

    [SerializeField] private bool _showTestObject = false;
    [SerializeField] private GameObject _testObject = null;

    //private Texture2D _camTexture = null;

    [SerializeField] private int _framesToSkip = 10;

    private WikitudeCamera _wikitudeCamera = null;

    private RenderTexture _targetRenderTexture;
    private Texture2D _cameraFeed;

    [SerializeField] private bool _useWikitudeCamera = false;

    public bool startDetection = false;

    IEnumerator Start()
    {
        findWebCams();

        _wikitudeCamera = FindObjectOfType<WikitudeCamera>();
#if UNITY_EDITOR
        _useWikitudeCamera = false;
#elif UNITY_IOS
        _useWikitudeCamera = _useWikitudeCamera;
#else
        _useWikitudeCamera = false;
#endif
        if (_useWikitudeCamera)
        {
            Debug.Log("Wikitud Cameraaaaaaaaa!!! " + _wikitudeCamera.GetComponent<Camera>());
            //camera = _wikitudeCamera.GetComponent<Camera>();
            //camera.enabled = false;
            StartCoroutine(WaitAndDestroyBackgroundCamera());
        }
        else
        {
            _wikitudeCamera.enabled = false;
            _wikitudeCamera = null;
        }

        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        ProcessCamera();
    }

    private void ProcessCamera()
    {
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("Webcam found");
#if UNITY_EDITOR
            Debug.Log("Setting up editor");
            cam = new WebCamTexture();
#elif UNITY_IOS
            Debug.Log("Setting up iOS");
            if (!_useWikitudeCamera)
            {
                foreach (WebCamDevice camera in WebCamTexture.devices)
                {
                    if (camera.isFrontFacing)
                    {
                        string frontCamName = camera.name;
                        cam = new WebCamTexture(frontCamName);
                    }
                }
                //cam.Play();
            }
#else
            Debug.Log("Setting up other platforms");
            cam = new WebCamTexture();
            //cam.Play();
#endif
        }
        else
        {
            Debug.Log("Webcam not found!");
            cam = null;
        }

        if (!_showTestObject)
        {
            _testObject.SetActive(false);
        }

        if (_wikitudeCamera != null && _useWikitudeCamera)
        {
            CreateTextures();
        }
        //_wikitudeCamera.EnableCameraRendering = true;
        //_wikitudeCamera.RequestInputFrameRendering = true;
        //Wikitude.Frame
        //Texture2D cameraTexture = _wikitudeCamera.CameraTexture;
    }

    IEnumerator WaitAndDestroyBackgroundCamera()
    {
        yield return new WaitForEndOfFrame();
        if (GameObject.Find("BackgroundCamera"))
        {
            GameObject.Find("BackgroundCamera").GetComponent<Camera>().depth = -1000;
        }
        _wikitudeCamera.GetComponent<Camera>().enabled = false;
        //GameObject.Find("BackgroundCamera").GetComponent<Camera>().enabled = false;
        //Debug.Log(GameObject.Find("BackgroundCamera").GetComponent<Camera>());
        //Destroy(GameObject.Find("BackgroundCamera"));
        //Camera.main.clearFlags = CameraClearFlags.Color;
    }

    private void CreateTextures()
    {
        _targetRenderTexture = new RenderTexture(_wikitudeCamera.CameraTexture.width, _wikitudeCamera.CameraTexture.height, 24, RenderTextureFormat.ARGB32);
        _cameraFeed = new Texture2D(_wikitudeCamera.CameraTexture.width, _wikitudeCamera.CameraTexture.height, TextureFormat.ARGB32, false);
    }

    void findWebCams()
    {
        foreach (var device in WebCamTexture.devices)
        {
            Debug.Log("Name: " + device.name);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (startDetection)
        {
            //Check for the image after a certain amount of frames to improve performance
            if (Time.frameCount % _framesToSkip == 0)
            {
                //Debug.Log("Check Aruco! " + Time.frameCount);
#if UNITY_EDITOR
                if (!_waitForDelay && cam != null)
                {
                    Mat image = TextureToMat(cam, null);

                    ImageProcessingAndDetection(image);

                    if (_testObject != null && _showTestObject)
                    {
                        _testObject.GetComponent<MeshRenderer>().material.mainTexture = cam;
                    }
                }
#elif UNITY_IOS
                if (!_waitForDelay && (_wikitudeCamera != null || cam != null))
                {
                    //if (!_animationPlaying)
                    //{
                    //    _animationPlaying = true;
                    //    Debug.Log("Start animation!!!");
                    //}

                    if (_useWikitudeCamera)
                    {
                        StartCoroutine(CheckForMarkersFromWikitudeCameraFeed(_wikitudeCamera));
                    }
                    else
                    {
                        Mat image = TextureToMat(cam, null);

                        ImageProcessingAndDetection(image);

                        if (_testObject != null && _showTestObject)
                        {
                            _testObject.GetComponent<MeshRenderer>().material.mainTexture = cam;
                            //Debug.Log("Camera Info: " + _wikitudeCamera.InputFrameWidth + " x " + _wikitudeCamera.InputFrameHeight + " , " + _wikitudeCamera.FlashMode + " , " + _wikitudeCamera.FocusMode + " , " + _wikitudeCamera.ZoomLevel);
                            //Debug.Log("Render: " + _wikitudeCamera.EnableCameraRendering + " , " + _wikitudeCamera.RequestInputFrameRendering);
                            //_testObject.GetComponent<MeshRenderer>().material.mainTexture = (Texture2D)_wikitudeCamera.CameraTexture;
                            //image = TextureToMat((Texture2D)_wikitudeCamera.CameraTexture, null);

                            //_testObject.GetComponent<MeshRenderer>().material.mainTexture = _camTexture;
                            //image = TextureToMat(_camTexture, null);
                        }
                    }

                    /*Color32[] pix = ((Texture2D)_wikitudeCamera.CameraTexture).GetPixels32();
                    Debug.Log("Is readable????????? " + _wikitudeCamera.CameraTexture.isReadable + " , " + pix.Length + " , " + _wikitudeCamera.CameraTexture.width + "x" + _wikitudeCamera.CameraTexture.height + " , " + camera.targetTexture);
                    Texture2D camTexture = new Texture2D(_wikitudeCamera.CameraTexture.width, _wikitudeCamera.CameraTexture.height);
                    camTexture.SetPixels32(pix);
                    camTexture.Apply();

                    Mat image = TextureToMat(camTexture, null);
                    if (_testObject != null && _showTestObject)
                    {
                        _testObject.GetComponent<MeshRenderer>().material.mainTexture = camTexture;
                    }
                    CvAruco.DetectMarkers(image, dictionary, out corners, out ids, parameters, out rejected);

                    //Debug.Log("After detecting markers: " + _wikitudeCamera.MirroredFrame + " , " + corners.Length + " , " + ids.Length);

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

                                    _client.InterpretMarker(ids[i], -1);
                                    _waitForDelay = true;
                                    StartCoroutine(WaitAndReenable());
                                }
                            }
                        }
                    }*/
                }
                /*if (MiraController.TouchpadButton && !_waitForDelay && cam != null)
                {
        Debug.Log("Touch Pressed");
                    if (!_animationPlaying)
                    {
                        _animationPlaying = true;
                //_wikitudeCamera.enabled = false;
                        //cam.Play();
                        Debug.Log("Start animation!!!");
                    }
                    Mat image = TextureToMat(cam, null);
            if (_testObject != null)
            {
                //_testObject.GetComponent<MeshRenderer>().material.mainTexture = cam;
                _testObject.GetComponent<MeshRenderer>().material.mainTexture = (Texture2D)_wikitudeCamera.CameraTexture;
                image = TextureToMat((Texture2D)_wikitudeCamera.CameraTexture, null);
        }
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
                                    _client.InterpretMarker(ids[i], -1);
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
                //_wikitudeCamera.enabled = true;
                    //cam.Stop();
                    Debug.Log("Stop animation!!!");
                }*/
#else
        if (!_waitForDelay && cam != null)
            {
                //if (!_animationPlaying)
                //{
                //    _animationPlaying = true;
                //    Debug.Log("Start animation!!!");
                //}

                /*Color32[] pix = ((Texture2D)_wikitudeCamera.CameraTexture).GetPixels32();
                Debug.Log("Is readable????????? " + _wikitudeCamera.CameraTexture.isReadable + " , " + pix.Length + " , " + _wikitudeCamera.CameraTexture.width + "x" + _wikitudeCamera.CameraTexture.height);
                Texture2D camTexture = new Texture2D(_wikitudeCamera.CameraTexture.width, _wikitudeCamera.CameraTexture.height);
                camTexture.SetPixels32(pix);
                camTexture.Apply();

                Mat image = TextureToMat(camTexture, null);
                if (_testObject != null && _showTestObject)
                {
                    _testObject.GetComponent<MeshRenderer>().material.mainTexture = camTexture;
                }*/

                //Color32[] pix = _wikitudeCamera.CameraTexture.GetPixels32();

                //_camTexture = new Texture2D(_wikitudeCamera.CameraTexture.width, _wikitudeCamera.CameraTexture.height);
                //_camTexture.SetPixels32(pix);
                //_camTexture.Apply();

                //Mat image = TextureToMat(_wikitudeCamera.CameraTexture as Texture2D, null);
                //Debug.Log(cam.width + "x" + cam.height);
                Mat image = TextureToMat(cam, null);

                ImageProcessingAndDetection(image);

                if (_testObject != null && _showTestObject)
                {
                    _testObject.GetComponent<MeshRenderer>().material.mainTexture = cam;
                    //Debug.Log("Camera Info: " + _wikitudeCamera.InputFrameWidth + " x " + _wikitudeCamera.InputFrameHeight + " , " + _wikitudeCamera.FlashMode + " , " + _wikitudeCamera.FocusMode + " , " + _wikitudeCamera.ZoomLevel);
                    //Debug.Log("Render: " + _wikitudeCamera.EnableCameraRendering + " , " + _wikitudeCamera.RequestInputFrameRendering);
                    //_testObject.GetComponent<MeshRenderer>().material.mainTexture = (Texture2D)_wikitudeCamera.CameraTexture;
                    //image = TextureToMat((Texture2D)_wikitudeCamera.CameraTexture, null);

                    //_testObject.GetComponent<MeshRenderer>().material.mainTexture = _camTexture;
                    //image = TextureToMat(_camTexture, null);
                }
                /*
                CvAruco.DetectMarkers(image, dictionary, out corners, out ids, parameters, out rejected);
                //Debug.Log(image.Width + "x" + image.Height + " , " + ids.Length + " , " + corners.Length + " , " + rejected.Length);

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
                                //_client.SendMessage()

                            }
                            else
                            {
                                _client.InterpretMarker(ids[i], -1);
                                _waitForDelay = true;
                                StartCoroutine(WaitAndReenable());
                            }
                        }
                    }
                }*/
            }
#endif
            }
        }
    }

    public void StartCamera()
    {
#if UNITY_EDITOR
        if (cam != null)
        {
            cam.Play();
        }
#elif UNITY_IOS
        if (!_useWikitudeCamera && cam != null)
        {
            cam.Play();
        }
#else
        if (cam != null)
        {
            cam.Play();
        }
#endif
    }

    private IEnumerator CheckForMarkersFromWikitudeCameraFeed(WikitudeCamera wikitudeCamera_)
    {
        //if (_testObject != null && _showTestObject)
        //{
        //    _testObject.GetComponent<MeshRenderer>().material.mainTexture = wikitudeCamera_.CameraTexture;
        //}
        yield return new WaitForEndOfFrame();

        //if (_targetRenderTexture.width != Screen.width || _targetRenderTexture.height != Screen.height)
        //{
        //    CreateTextures();
        //}

        Graphics.Blit(wikitudeCamera_.CameraTexture, _targetRenderTexture);
        RenderTexture.active = _targetRenderTexture;

        _cameraFeed.ReadPixels(new UnityEngine.Rect(0, 0, _cameraFeed.width, _cameraFeed.height), 0, 0);
        _cameraFeed.Apply();

        //Color32[] pixels32 = _cameraFeed.GetPixels32();
        //Debug.Log("Colors count: " + pixels32.Length + " / Colors: " + pixels32[1000] + " , " + pixels32[2000] + " , " + pixels32[3000]);

        Mat image = TextureToMat(_cameraFeed, null);
        //Mat grey = new Mat();
        Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
        Cv2.Flip(image, image, FlipMode.X);

        ImageProcessingAndDetection(image);

        if (_testObject != null)
        {
            _testObject.GetComponent<MeshRenderer>().material.mainTexture = _cameraFeed;
            //_testObject.GetComponent<MeshRenderer>().material.mainTexture = wikitudeCamera_.CameraTexture;
        }
    }

    private void ImageProcessingAndDetection(Mat image)
    {
        CvAruco.DetectMarkers(image, dictionary, out corners, out ids, parameters, out rejected);
        //Debug.Log(image.Width + "x" + image.Height + " , CountNonZero: " + image.CountNonZero() + " , " + ids.Length + " , " + corners.Length + " , " + rejected.Length);

        List<int> alreadyFoundIds = new List<int>();
        bool alreadyFound = false;

        if (ids.Length > 0)
        {
            if (!_client.playLocally)
            {
                _client.SetSpawnIds(ids);
            }
            else
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
                        _client.InterpretMarker(ids[i], -1);
                    }
                }
                _waitForDelay = true;
                StartCoroutine(WaitAndReenable());
            }
        }
    }

    //Add delay so when a marker is detected it doesn't detect it multiple times right away
    private IEnumerator WaitAndReenable()
    {
        yield return new WaitForSeconds(3f);
        _waitForDelay = false;
    }
}
//}