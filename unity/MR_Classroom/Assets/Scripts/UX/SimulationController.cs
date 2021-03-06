﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SimulationController : MonoBehaviour
{
    public enum TypeOfCell
    {
        Animal,
        Plant,
        Prokaryotic
    }

    public enum Organelle
    {
        None,
        //Only animal
        AnimalPlasmaMembrane,
        //Only plant
        PlantPlasmaMembrane,
        PlantCellWall,
        Chloroplast,
        Vacuole,
        //Shared by animal and plant
        Nucleus,
        Centrosome,
        GolgiApparatus,
        RoughEndoplasmicReticulum,
        SmoothEndoplasmicReticulum,
        Mitochondria,
        Lysosome,
        //Shared by all
        Chromosome,
        Ribosome,
        //Only prokaryotes
        ProkaryoticPlasmaMembrane,
        ProkaryoticCellWall,
        Capsul,
        Flagellum
    }

    private List<OrganelleController> _loadedOrganelles = new List<OrganelleController>();
    private List<OrganellePosition> _correctPositions = new List<OrganellePosition>();

    public TypeOfCell currentCell;
    public TCPTestClient client;

    [SerializeField] private GameObject _builtACell;
    [SerializeField] private GameObject _cellDissection;

    [SerializeField] private CellController _animalCell;
    [SerializeField] private CellController _plantCell;
    [SerializeField] private CellController _prokaryoticCell;

    [SerializeField] private Transform _startPosition;

    //to test while no controller available
    public bool done;

    [SerializeField] private Text[] _titleTexts;
    [SerializeField] private GameObject _content;
    [SerializeField] private GameObject _feedback;

    public bool startSimulationDirectly = false;


    private void OnEnable()
    {
        //_loadedOrganelles = new List<SnapAndCheck>();
        int id = 0;
        foreach (OrganelleController organelle in _loadedOrganelles)
        {
            organelle.id = id;
            id++;
        }
    }

    private void Start()
    {
        _content.SetActive(false);
        if (startSimulationDirectly)
        {
            StartSimulation(0);
        }
    }

    public void StartSimulation(int simToLaunch)
    {
        _builtACell.SetActive(false);
        _animalCell.gameObject.SetActive(false);
        _plantCell.gameObject.SetActive(false);
        _prokaryoticCell.gameObject.SetActive(false);
        _cellDissection.SetActive(false);
        _content.SetActive(true);

        string title = "";

        if (simToLaunch == 0)
        {
            _builtACell.SetActive(true);

            switch (currentCell)
            {
                case TypeOfCell.Animal:
                    _animalCell.gameObject.SetActive(true);
                    _correctPositions = _animalCell.correctPositions;
                    //_animalCell.transform.position = _startPosition.position;

                    title = "Animal Cell";

                    break;
                case TypeOfCell.Plant:
                    _plantCell.gameObject.SetActive(true);
                    _correctPositions = _plantCell.correctPositions;
                    //_plantCell.transform.position = _startPosition.position;

                    title = "Plant Cell";

                    break;
                case TypeOfCell.Prokaryotic:
                    _prokaryoticCell.gameObject.SetActive(true);
                    _correctPositions = _prokaryoticCell.correctPositions;
                    //_prokaryoticCell.transform.position = _startPosition.position;

                    title = "Prokaryotic Cell";

                    break;
            }
        }
        else if (simToLaunch == 1)
        {
            _cellDissection.SetActive(true);

            title = "Cell Dissection";
        }

        foreach (Text titleText in _titleTexts)
        {
            titleText.text = title;
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
            foreach (OrganellePosition.Status status in organellePosition.status)
            {
                if (status == OrganellePosition.Status.Correct)
                {
                    correctOrganelles++;
                }
                if (status == OrganellePosition.Status.Empty)
                {
                    emptySpotsLeft = true;
                    break;
                }
                totalOrganelles++;
            }
        }

        string feedbackText = "";

        if (!emptySpotsLeft)
        {
            if (correctOrganelles == totalOrganelles)
            {
                Debug.Log("Congratulations you created the perfect cell!");
                feedbackText = "Congratulations you created the perfect cell!";
            }
            else
            {
                Debug.Log("Keep trying!");
                feedbackText = "Keep trying!";
            }
        }
        else
        {
            Debug.Log("You still have spots to fill in the cell!");
            feedbackText = "You still have spots to fill in the cell!";
        }

        _feedback.SetActive(true);
        foreach (Text text in _feedback.GetComponentsInChildren<Text>())
        {
            text.text = feedbackText;
        }
        StartCoroutine(WaitAndHide());
    }

    IEnumerator WaitAndHide()
    {
        yield return new WaitForSeconds(7f);

        _feedback.SetActive(false);
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
