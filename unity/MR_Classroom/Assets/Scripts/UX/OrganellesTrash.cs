using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganellesTrash : MonoBehaviour
{
    [SerializeField] private Transform _finalPosition;

    private void OnTriggerEnter(Collider other)
    {
        OrganelleController organelle = other.GetComponent<OrganelleController>();

        if (organelle != null)
        {
            organelle.trash = _finalPosition;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        OrganelleController organelle = other.GetComponent<OrganelleController>();

        if (organelle != null)
        {
            organelle.trash = null;
        }
    }
}
