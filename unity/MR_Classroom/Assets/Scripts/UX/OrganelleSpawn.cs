using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganelleSpawn : MonoBehaviour
{
    public OrganelleController _organelleToSpawn = null;
    [SerializeField] private Transform _spawnPosition = null;

    [SerializeField] private GameObject _portalEffect;
    [SerializeField] private float _textureSpeed = .5f;

    private Material _portalMaterial = null;

    public bool activatePortal;
    public bool deactivatePortal;

    public int organellesActive = 0;

    private void Start()
    {
        _portalMaterial = _portalEffect.GetComponent<Renderer>().material;

        //only for testing
        if (_organelleToSpawn != null)
        {
            activatePortal = true;
            //ActivatePortal();
            //StartCoroutine(WaitRandomAndSpawn());
        }
        else
        {
            _portalEffect.SetActive(false);
        }
    }

    //only for testing
    IEnumerator WaitRandomAndSpawn()
    {
        float randomWaitTime = Random.Range(2f, 5f);
        Debug.Log("Random Wait Time: " + randomWaitTime);
        yield return new WaitForSeconds(0f);
        Debug.Log("Activate Portal!!!!!!!!!!!!!!!");
        ActivatePortal();
    }

    public void ActivatePortal()
    {
        _organelleToSpawn.GetComponent<OrganelleController>().sendSpawnToServer(_spawnPosition.transform.position);
        _portalEffect.SetActive(true);

        LeanTween.cancel(_portalEffect);
        LeanTween.scale(_portalEffect, Vector3.one, .5f).setOnComplete(
            ()=> {
                _organelleToSpawn.trash = null;
                _organelleToSpawn.currentOrganellePosition = null;
                _organelleToSpawn.previousOrganellePositions = new List<OrganellePosition>();
                _organelleToSpawn.gameObject.SetActive(true);
                _organelleToSpawn.SetSpawnScale(true, 0f);
                _organelleToSpawn.spawnContainer = this;
                _organelleToSpawn.transform.position = _spawnPosition.transform.position;

                organellesActive++;

                LeanTween.delayedCall(2f, () => {
                    DeactivatePortal();
                    _organelleToSpawn.GetComponent<OrganelleController>().locked = false;
                });
            });
        //StartCoroutine(ActivateAndScalePortal(0f, 1f, .5f));
    }

    public void DeactivatePortal()
    {
        LeanTween.cancel(_portalEffect);
        LeanTween.scale(_portalEffect, Vector3.zero, .35f).setOnComplete(
            ()=> {
                _portalEffect.transform.localScale = new Vector3(1f, 1f, 0f);
                _portalEffect.SetActive(false);
            });
        //StartCoroutine(ActivateAndScalePortal(1f, 0f, .35f));
    }

    IEnumerator ActivateAndScalePortal(float start, float end, float animTime)
    {
        //Debug.Log("Activate And Scale Portal!!!");
        _portalEffect.SetActive(true);

        float currentLerpTime = 0f;
        float percentage = 0f;

        if (start == 0f)
        {
            _organelleToSpawn.GetComponent<OrganelleController>().sendSpawnToServer(_spawnPosition.transform.position);
        }

        while (Mathf.Abs(start - _portalEffect.transform.localScale.z) < 1)
        {
            currentLerpTime += Time.deltaTime;
            if (currentLerpTime > animTime)
            {
                currentLerpTime = animTime;
            }

            percentage = currentLerpTime / animTime;
            //percentage = 1f - Mathf.Cos(percentage * Mathf.PI * 0.5f);
            percentage = percentage * percentage * percentage * (percentage * (6f * percentage - 15f) + 10f);

            _portalEffect.transform.localScale = new Vector3(_portalEffect.transform.localScale.x, _portalEffect.transform.localScale.y, Mathf.Lerp(start, end, percentage));
            //Debug.Log("Scaleeeeeee");
            yield return null;
        }

        if (start == 0f)
        {
            _organelleToSpawn.trash = null;
            _organelleToSpawn.currentOrganellePosition = null;
            _organelleToSpawn.previousOrganellePositions = new List<OrganellePosition>();
            _organelleToSpawn.gameObject.SetActive(true);
            _organelleToSpawn.SetSpawnScale(true, 0f);
            _organelleToSpawn.spawnContainer = this;
            _organelleToSpawn.transform.position = _spawnPosition.transform.position;

            organellesActive++;

            yield return new WaitForSeconds(2f);
            DeactivatePortal();
            _organelleToSpawn.GetComponent<OrganelleController>().locked = false;
        }
    }

    private void Update()
    {
        if (_portalEffect.activeSelf)
        {
            float offsetY = Time.time * _textureSpeed;
            _portalMaterial.mainTextureOffset = new Vector2(0, offsetY);
        }

        if (activatePortal)
        {
            ActivatePortal();
            activatePortal = false;
        }

        if (deactivatePortal)
        {
            DeactivatePortal();
            deactivatePortal = false;
        }
    }
}