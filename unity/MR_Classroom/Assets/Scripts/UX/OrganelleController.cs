using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        _originalScale = transform.localScale.x;
    }

    public void OnGrabFinished()
    {
        if (currentOrganellePosition != null)
        {
            currentOrganellePosition.OnGrabFinished(this);
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
            SetSpawnScale(true, 0f);
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
        }
        else
        {
            StartCoroutine(ScaleAnimation(transform.localScale.x, _originalScale, animTime));
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
}
