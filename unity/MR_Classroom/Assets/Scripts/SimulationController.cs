using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public enum Organelle
    {
        //Only animal
        Centrosomes,
        //Only plant
        Chloroplasts,
        Vacuole,
        //Shared by plant and prokaryotes
        CellWall,
        //Shared by animal and plant
        Nucleus,
        GolgiApparatus,
        RoughEndoplasmicReticulum,
        SmoothEndoplasmicReticulum,
        Mitochondria,
        Lysosome,
        //Shared by all
        Chromosomes,
        PlasmaMembrane,
        Ribosome,
        //Only prokaryotes
        Capsul,
        Flagellum
    }

    [SerializeField] private List<SnapAndCheck> _loadedOrganelles = new List<SnapAndCheck>();
    [SerializeField] private List<OrganellePosition> _correctPositions = new List<OrganellePosition>();

    //to test while no controller available
    public bool done;

    private void OnEnable()
    {
        //_loadedOrganelles = new List<SnapAndCheck>();
        int id = 0;
        foreach (SnapAndCheck organelle in _loadedOrganelles)
        {
            organelle.id = id;
            id++;
        }
    }

    public void LoadOrganelle()
    {
        //load organelle, add script with values and add it to list
    }

    public void Done()
    {
        int totalOrganelles = 0;
        int correctOrganelles = 0;

        bool emptySpotsLeft = false;

        foreach (OrganellePosition organellePosition in _correctPositions)
        {
            if (organellePosition.status == OrganellePosition.Status.Correct)
            {
                correctOrganelles++;
            }
            if (organellePosition.status == OrganellePosition.Status.Empty)
            {
                emptySpotsLeft = true;
                break;
            }
            totalOrganelles++;
        }

        if (!emptySpotsLeft)
        {
            if (correctOrganelles == totalOrganelles)
            {
                Debug.Log("Congratulations you created the perfect cell!");
            }
            else
            {
                Debug.Log("Keep trying!");
            }
        }
        else
        {
            Debug.Log("You still have spots to fill in the cell!");
        }
    }

    private void Update()
    {
        if (done)
        {
            Done();
            done = false;
        }
    }
}
