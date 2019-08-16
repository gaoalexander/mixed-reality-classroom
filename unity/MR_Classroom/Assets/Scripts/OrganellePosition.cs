using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganellePosition : MonoBehaviour
{
    public SimulationController.Organelle correctOrganelle;
    public bool snapToCenter;

    private List<int> _organellesIn = new List<int>();
    private int _currentOrganelle = -1;
    
    public enum Status
    {
        Empty,
        Correct,
        Incorrect
    }
    public Status status = Status.Empty;
    public SimulationController.Organelle placedOrganelle;

    private void OnTriggerEnter(Collider other)
    {
        _organellesIn.Add(other.GetComponent<SnapAndCheck>().id);
        other.GetComponent<SnapAndCheck>().currentOrganellePosition = this;
    }

    private void OnTriggerExit(Collider other)
    {
        _organellesIn.Remove(other.GetComponent<SnapAndCheck>().id);
        other.GetComponent<SnapAndCheck>().currentOrganellePosition = null;
        if (_currentOrganelle == other.GetComponent<SnapAndCheck>().id)
        {
            _currentOrganelle = -1;
            status = Status.Empty;
        }
    }

    public void OnGrabFinished(SnapAndCheck organelleObj)
    {
        if (_currentOrganelle == -1)
        {
            _currentOrganelle = organelleObj.id;
            placedOrganelle = organelleObj.organelle;

            if (organelleObj.organelle == correctOrganelle)
            {
                status = Status.Correct;
            }
            else
            {
                status = Status.Incorrect;
            }

            if (snapToCenter)
            {
                StartCoroutine(SnapObject(organelleObj.transform));
            }
        }
    }

    IEnumerator SnapObject(Transform organelleObj)
    {
        //position
        Vector3 startPos = organelleObj.position;
        Vector3 endPos = transform.position;

        //rotation
        Quaternion startRot = organelleObj.rotation;
        Quaternion endRot = transform.rotation;

        float currentLerpTime = 0f;
        float percentage = 0f;
        float animTime = .5f;

        while (organelleObj.position != transform.position || organelleObj.rotation != transform.rotation)
        {
            currentLerpTime += Time.deltaTime;
            if (currentLerpTime > animTime)
            {
                currentLerpTime = animTime;
            }

            percentage = currentLerpTime / animTime;
            //percentage = 1f - Mathf.Cos(percentage * Mathf.PI * 0.5f);
            percentage = percentage * percentage * percentage * (percentage * (6f * percentage - 15f) + 10f);

            organelleObj.position = Vector3.Lerp(startPos, endPos, percentage);
            organelleObj.rotation = Quaternion.Lerp(startRot, endRot, percentage);
            yield return null;
        }
    }
}
