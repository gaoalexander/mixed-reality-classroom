using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganellePosition : MonoBehaviour
{
    public SimulationController.Organelle[] correctOrganelles = new SimulationController.Organelle[0];

    [SerializeField] private bool _snapToCenter = false;
    [SerializeField] private Transform[] _snapPositions = new Transform[0];

    private bool _inFirst = false;

    private List<int> _organellesIn = new List<int>();
    private int[] _currentOrganelles = new int[1];

    public enum Status
    {
        Empty,
        Correct,
        Incorrect
    }
    public Status[] status = new Status[0];
    public SimulationController.Organelle[] placedOrganelles = new SimulationController.Organelle[0];

    private void OnEnable()
    {
        status = new Status[correctOrganelles.Length];
        placedOrganelles = new SimulationController.Organelle[correctOrganelles.Length];
        _currentOrganelles = new int[correctOrganelles.Length];

        for (int i = 0; i < correctOrganelles.Length; i++)
        {
            status[i] = Status.Empty;
            placedOrganelles[i] = SimulationController.Organelle.None;
            _currentOrganelles[i] = -1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (!_ignoreAfterFirst || !_inFirst)
        //{
        //_inFirst = true;

        OrganelleController organelle = other.GetComponent<OrganelleController>();

        if (!organelle.ignoreAfterFirst || organelle.ignoreAfterFirst && organelle.currentOrganellePosition == null)
        {
            _organellesIn.Add(organelle.id);
            if (organelle.currentOrganellePosition != null)
            {
                organelle.previousOrganellePositions.Add(organelle.currentOrganellePosition);
            }
            organelle.currentOrganellePosition = this;
        }
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        //if (_ignoreAfterFirst && _inFirst || !_ignoreAfterFirst)
        //{
        //_inFirst = false;

        OrganelleController organelle = other.GetComponent<OrganelleController>();

        if (!organelle.ignoreAfterFirst || organelle.ignoreAfterFirst && organelle.currentOrganellePosition == this)
        {
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
            int i = 0;
            foreach (int currentOrganelle in _currentOrganelles)
            {
                if (currentOrganelle == organelle.id)
                {
                    _currentOrganelles[i] = -1;
                    status[i] = Status.Empty;
                    placedOrganelles[i] = SimulationController.Organelle.None;
                    break;
                }
                i++;
            }
        }
        //}
    }

    public void OnGrabFinished(OrganelleController organelleObj)
    {
        List<int> wrongSpots = new List<int>();
        List<int> sameSpots = new List<int>();
        List<int> emptySpots = new List<int>();
        int i = 0;

        int finalIndex = -1;

        foreach (int currentOrganelle in _currentOrganelles)
        {
            if (organelleObj.organelle == correctOrganelles[i] && currentOrganelle == -1)
            {
                sameSpots.Add(i);
            }
            if (currentOrganelle == -1)
            {
                emptySpots.Add(i);
            }
            if (placedOrganelles[i] != correctOrganelles[i] && organelleObj.organelle == correctOrganelles[i])
            {
                wrongSpots.Add(i);
            }
            i++;
        }

        if (sameSpots.Count > 0)
        {
            finalIndex = sameSpots[0];
        }
        else if (emptySpots.Count > 0 && wrongSpots.Count > 0)
        {
            //pass the element from the wrong spot to the empty spot and fill the wrong spot with the current one
            _currentOrganelles[emptySpots[0]] = _currentOrganelles[wrongSpots[0]];
            placedOrganelles[emptySpots[0]] = placedOrganelles[wrongSpots[0]];

            if (placedOrganelles[emptySpots[0]] == correctOrganelles[emptySpots[0]])
            {
                status[emptySpots[0]] = Status.Correct;
            }
            else
            {
                status[emptySpots[0]] = Status.Incorrect;
            }
            finalIndex = wrongSpots[0];
        }
        else if (emptySpots.Count > 0)
        {
            finalIndex = emptySpots[0];
        }

        if (finalIndex != -1)
        {
            _currentOrganelles[finalIndex] = organelleObj.id;
            placedOrganelles[finalIndex] = organelleObj.organelle;

            if (organelleObj.organelle == correctOrganelles[finalIndex])
            {
                status[finalIndex] = Status.Correct;
            }
            else
            {
                status[finalIndex] = Status.Incorrect;
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
        float timePassed = 0f;

        while (organelleObj.position != endPos || organelleObj.rotation != endRot && timePassed <= animTime + .05f)
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

            timePassed += Time.deltaTime;
            yield return null;
        }
        organelleObj.GetComponent<OrganelleController>().sendSpawnToServer(endPos);
    }
}