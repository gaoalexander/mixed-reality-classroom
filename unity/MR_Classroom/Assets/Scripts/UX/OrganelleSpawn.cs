using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganelleSpawn : MonoBehaviour
{
    [SerializeField] private GameObject _organelleParent = null;

    [SerializeField] private GameObject _portalEffect;
    [SerializeField] private float _textureSpeed = .5f;

    private Material _portalMaterial = null;

    public bool activatePortal;
    public bool deactivatePortal;

    private void Start()
    {
        _portalMaterial = _portalEffect.GetComponent<Renderer>().material;
        _portalEffect.SetActive(false);
    }

    public void ActivatePortal()
    {
        StartCoroutine(ActivateAndScalePortal(0f, 1f, .5f));
    }

    public void DeactivatePortal()
    {
        StartCoroutine(ActivateAndScalePortal(1f, 0f, .35f));
    }

    IEnumerator ActivateAndScalePortal(float start, float end, float animTime)
    {
        _portalEffect.SetActive(true);

        float currentLerpTime = 0f;
        float percentage = 0f;

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
            yield return null;
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
