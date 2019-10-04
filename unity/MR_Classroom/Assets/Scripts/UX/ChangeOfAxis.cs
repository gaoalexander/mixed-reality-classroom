using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeOfAxis : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        OrganelleController organelle = other.GetComponent<OrganelleController>();

        if (organelle != null)
        {
            organelle.ChangeMovingPlane();
            Debug.Log("Change moving wall!");
        }
    }
}
