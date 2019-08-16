using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapAndCheck : MonoBehaviour
{
    public SimulationController.Organelle organelle;
    public int id;

    public OrganellePosition currentOrganellePosition = null;

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
