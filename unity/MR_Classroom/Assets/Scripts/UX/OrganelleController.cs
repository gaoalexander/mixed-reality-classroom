using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganelleController : MonoBehaviour
{
    public SimulationController.Organelle organelle;
    public int id;

    public OrganellePosition currentOrganellePosition = null;
    public List<OrganellePosition> previousOrganellePositions = new List<OrganellePosition>();

    //to test while no controller available
    public bool grabFinished;

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
    }
}
