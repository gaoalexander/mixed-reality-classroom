using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganellePosition : MonoBehaviour
{
    public SimulationController.Organelle correctOrganelle;

    [SerializeField] private bool _snapToCenter;
    [SerializeField] private Transform[] _snapPositions;

    private List<int> _organellesIn = new List<int>();
    private int _currentOrganelle = -1;
    
    public enum Status
    {
        Empty,
        Correct,
        Incorrect
    }
    public Status status = Status.Empty;
    public SimulationController.Organelle placedOrganelle = SimulationController.Organelle.None;

    private void OnTriggerEnter(Collider other)
    {
        OrganelleController organelle = other.GetComponent<OrganelleController>();
        _organellesIn.Add(organelle.id);
        if (organelle.currentOrganellePosition != null)
        {
            organelle.previousOrganellePositions.Add(organelle.currentOrganellePosition);
        }
        organelle.currentOrganellePosition = this;
    }

    private void OnTriggerExit(Collider other)
    {
        OrganelleController organelle = other.GetComponent<OrganelleController>();
        _organellesIn.Remove(organelle.id);
        if (organelle.previousOrganellePositions.Count == 0)
        {
            organelle.currentOrganellePosition = null;
        }
        else
        {
            int lastIndex = organelle.previousOrganellePositions.Count - 1;
            organelle.currentOrganellePosition = organelle.previousOrganellePositions[lastIndex];
            organelle.previousOrganellePositions.RemoveAt(lastIndex);
        }
        if (_currentOrganelle == organelle.id)
        {
            _currentOrganelle = -1;
            status = Status.Empty;
            placedOrganelle = SimulationController.Organelle.None;
        }
    }

    public void OnGrabFinished(OrganelleController organelleObj)
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

            if (_snapToCenter)
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

        if (_snapPositions.Length > 0)
        {
            int i = 0;
            float minDist = 10000f;
            int closest = -1;
            foreach (Transform transform in _snapPositions)
            {
                if (Vector3.Distance(startPos, transform.position) < minDist)
                {
                    minDist = Vector3.Distance(startPos, transform.position);
                    closest = i;
                }
                i++;
            }
            endPos = _snapPositions[closest].position;
            endRot = _snapPositions[closest].rotation;
        }

        float currentLerpTime = 0f;
        float percentage = 0f;
        float animTime = .5f;

        while (organelleObj.position != endPos || organelleObj.rotation != endRot)
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
