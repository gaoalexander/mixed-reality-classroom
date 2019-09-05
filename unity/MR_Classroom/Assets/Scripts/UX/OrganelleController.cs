using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class OrganelleController : MonoBehaviour
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

        
        gameObject.SetActive(false);
    }


    public void sendPositionToServer(Vector3 pos)
    {
        client.SendTCPMessage(GrabRequest(pos).ToString());
    }
    public JSONNode GrabRequest(Vector3 position)
    {
        JSONNode node = new JSONObject();
        node["type"] = "object";
        node["lockid"] = client.id;
        node["uid"] = id;
        node["x"] = position.x;
        node["y"] = position.y;
        node["z"] = position.z;
        return node;
    }
}
