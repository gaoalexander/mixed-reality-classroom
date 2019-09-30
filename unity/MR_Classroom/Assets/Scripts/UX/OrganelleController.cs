using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SimpleJSON;

public class OrganelleController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public SimulationController.Organelle organelle = SimulationController.Organelle.None;
    public int id = -1;

    public bool ignoreAfterFirst = false;

    [SerializeField] private float _spawnScale = 0f;
    private float _originalScale = 0f;

    public OrganellePosition currentOrganellePosition = null;
    public List<OrganellePosition> previousOrganellePositions = new List<OrganellePosition>();

    //to test while no controller available
    public bool grabFinished;

    public bool scaleToSpawn;
    public bool scaleToOriginal;

    public bool locked = false;

    public OrganelleSpawn spawnContainer = null;

    public Transform trash = null;

    public TCPTestClient client;


    //NOTE: Variables from the Grab class below


    private bool isGrabbing = false;
    public bool hasBeenGrabbed = false;

    private RigidbodyConstraints originalConstraints;
    private Rigidbody rigidBody;

    private float lastTouchPosition;
    private float lastTouchCoor;

    public int objectId;

    public int lastGrabLoop = 0;
    public bool grabLooping = false;

    //NOTE: End of variables from Grab class;


    private void OnEnable()
    {
        if (_originalScale == 0f)
        {
            _originalScale = transform.localScale.x;
        }
    }

    public void OnGrabFinished()
    {
        if (trash == null)
        {
            if (currentOrganellePosition != null)
            {
                currentOrganellePosition.OnGrabFinished(this);
            }
            else
            {
                SetSpawnScale(true, .4f);
            }
        }
        else
        {
            SetSpawnScale(true, .4f);
            //make trash animation using trash.position
            StartCoroutine(TrashOrganelle(trash.position, .5f));
        }
    }

    public void OnGrabStarted()
    {
        SetSpawnScale(false, .4f);
        if(spawnContainer != null)
        {
            spawnContainer.organellesActive--;
            spawnContainer = null;
        }
    }

    //NOTE: GRAB CLASS START.
    private void Start()
    {

        lastTouchCoor = 0.5f;
        rigidBody = this.gameObject.GetComponent<Rigidbody>();
        originalConstraints = rigidBody.constraints;
    }

    private void Update()
    {
        if (grabFinished)
        {
            OnGrabFinished();
            grabFinished = false;
        }

        if (scaleToSpawn)
        {
            SetSpawnScale(true, .4f);
            scaleToSpawn = false;
        }
        if (scaleToOriginal)
        {
            SetSpawnScale(false, .4f);
            scaleToOriginal = false;
        }

        //NOTE: GRAB CLASS UPDATE

        //Debug.Log("Is Grabbing: " + isGrabbing);
        // stop grabbing if the user isn't clicking
        if (isGrabbing == true && MiraController.ClickButton == false)
        {
            //Debug.Log("Stop Grabbing!");
            rigidBody.constraints = originalConstraints;
            isGrabbing = false;
            hasBeenGrabbed = true;
            if (client.playLocally)
            {
                OnGrabFinished();
			}
			else
			{
				client.GrabReleased(objectId);
			}
		}

        if (isGrabbing == true)
        {
            // freeze the position of the physics simulation temporarily so the object doesn't
            // spiral out of control while its being interacted with
            // you could freeze the rotation as well if you wanted
            rigidBody.constraints = RigidbodyConstraints.FreezePosition;

            float touchInfluence = 0.0f;
            float thisTouch = 0.0f;
            float touchIncrement = 0.0f;
            if (MiraController.TouchHeld == true)
            {
                // MiraController.Touchpos.Y goes from 1 to 0 , near to far
                // we want to change this so the touchpad closer to the user returns negative values
                // and the upper half returns positive values
                thisTouch = MiraController.TouchPos.y;
                // now its 0.5 to -0.5
                thisTouch -= 0.5f;
                // now its -0.5 to 0.5
                thisTouch *= -1.0f;
                // scale it down so it's not too strong
                thisTouch *= 0.05f;
                touchInfluence = lastTouchPosition - thisTouch;

                if (lastTouchCoor > MiraController.TouchPos.y)
                {

                    touchIncrement = 0.1f;
                    lastTouchCoor = MiraController.TouchPos.y;
                }
                else if (lastTouchCoor < MiraController.TouchPos.y)
                {
                    touchIncrement = -0.1f;
                    lastTouchCoor = MiraController.TouchPos.y;
                }
            }
            lastTouchPosition = thisTouch;

            // get the distance from this object to the controller

            float currentDistance = (MiraController.Position - transform.position).magnitude;

            // the new distance of the grabbed object is the current distance,
            // adjusted by the users touch, in the direction it was from the controller


            Vector3 newLength = MiraController.Direction.normalized * (currentDistance + touchInfluence + touchIncrement);
            Vector3 newPosition = MiraController.Position + newLength;

            Vector3 reallyNewPositon = new Vector3(newPosition.x, 0.255f, newPosition.z);
            //transform.position = newPosition;
            //Debug.Log("REALLY NEW POSITION");
            //Debug.Log(reallyNewPositon);
            //Debug.Log("~~~~~~~~~~~~~~");

            if (!client.playLocally)
            {
                client.SendTCPMessage(GrabRequest(reallyNewPositon).ToString());
            }
            else
            {
                transform.position = reallyNewPositon;
            }
        }
    }

    public void SetSpawnScale(bool spawnScale, float animTime)
    {
        if (spawnScale)
        {
            if (animTime <= 0)
            {
                transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
            }
            else
            {
                StartCoroutine(ScaleAnimation(transform.localScale.x, _spawnScale, animTime));
            }
            SetIdle(true);
        }
        else
        {
            StartCoroutine(ScaleAnimation(transform.localScale.x, _originalScale, animTime));
            SetIdle(false);
        }
    }

    IEnumerator ScaleAnimation(float startScale, float endScale, float animTime)
    {
        float currentLerpTime = 0f;
        float percentage = 0f;

        float newScale = 0f;

        while (newScale != endScale)
        {
            currentLerpTime += Time.deltaTime;
            if (currentLerpTime > animTime)
            {
                currentLerpTime = animTime;
            }

            percentage = currentLerpTime / animTime;
            //percentage = 1f - Mathf.Cos(percentage * Mathf.PI * 0.5f);
            percentage = percentage * percentage * percentage * (percentage * (6f * percentage - 15f) + 10f);

            newScale = Mathf.Lerp(startScale, endScale, percentage);

            transform.localScale = new Vector3(newScale, newScale, newScale);
            yield return null;
        }
    }

    public void SetIdle(bool enabled)
    {
        StartCoroutine(IdleAnimation(enabled));
    }

    IEnumerator IdleAnimation(bool enabled)
    {
        Animator organelleAnimator = GetComponent<Animator>();

        if (enabled)
        {
            organelleAnimator.enabled = true;
            organelleAnimator.SetBool("Idle", true);
            organelleAnimator.SetBool("Static", false);
        }
        else
        {
            organelleAnimator.SetBool("Static", true);
            organelleAnimator.SetBool("Idle", false);
            yield return new WaitForEndOfFrame();
            while (organelleAnimator.IsInTransition(organelleAnimator.GetLayerIndex("Base Layer")))
            {
                yield return null;
            }
            organelleAnimator.enabled = false;
        }
    }

    IEnumerator TrashOrganelle(Vector3 endPos, float animTime)
    {
        //position
        Vector3 startPos = transform.position;

        float currentLerpTime = 0f;
        float percentage = 0f;

        float originalScale = transform.localScale.x;

        Vector3 newPos = Vector3.zero;

        while (newPos != endPos)
        {
            currentLerpTime += Time.deltaTime;
            if (currentLerpTime > animTime)
            {
                currentLerpTime = animTime;
            }

            percentage = currentLerpTime / animTime;
            //percentage = 1f - Mathf.Cos(percentage * Mathf.PI * 0.5f);
            percentage = percentage * percentage * percentage * (percentage * (6f * percentage - 15f) + 10f);

            newPos = Vector3.Lerp(startPos, endPos, percentage);

            transform.position = newPos;
            transform.localScale = new Vector3(originalScale * (1 - percentage), originalScale * (1 - percentage), originalScale * (1 - percentage));
            yield return null;
        }
        
        client.SetObjectInactive(objectId);
        if (client.playLocally)
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void sendPositionToServer(Vector3 pos)
    {
        if (!client.playLocally)
        {
            client.SendTCPMessage(GrabRequest(pos).ToString());
        }
    }


    //NOTE: Methods from the Grab class below.

    // these OnPointer functions are automatically called when
    // the pointer interacts with a game object that this script is attached to
    public void OnPointerDown(PointerEventData pointerData)
    {
        // onPointerDown is called every frame the pointer is held down on the object
        // we only want to grab objects if the click button was just pressed
        // this prevents multiple objects from unintentionally getting grabbed
        Debug.Log("On Pointer Down");
        if (MiraController.ClickButtonPressed)
        {
            Debug.Log("Click Button Pressed");
            isGrabbing = true;
            if (client.playLocally)
            {
                OnGrabStarted();
            }
        }
    }

    public void OnPointerUp(PointerEventData pointerData)
    {
        // onPointerDown is called every frame the pointer is held down on the object
        // we only want to grab objects if the click button was just pressed
        // this prevents multiple objects from unintentionally getting grabbed
        //Debug.Log("On Pointer Up");
        //isGrabbing = false;
        //OnGrabFinished();
    }

    public JSONNode GrabRequest(Vector3 position)
    {
        JSONNode node = new JSONObject();
        node["type"] = "object";
        node["lockid"] = client.id;
        node["uid"] = objectId;
        node["x"] = position.x;
        node["y"] = position.y;
        node["z"] = position.z;
        return node;
    }

    public bool isGrabbed()
    {
        return isGrabbing;
    }
}
