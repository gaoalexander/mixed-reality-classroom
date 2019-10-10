using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitialSetupUI : MonoBehaviour
{
    [SerializeField] private TCPTestClient _client = null;

    [SerializeField] private Toggle[] _toggles = new Toggle[2];

    [SerializeField] private Dropdown _userIdDropdown = null;
    [SerializeField] private Dropdown _trackingDropdown = null;

    [SerializeField] private Image _playLocallyPanel = null;
    [SerializeField] private Image _playOnlinePanel = null;

    [SerializeField] private Color _selectedColor = Color.black;

    [SerializeField] private GameObject _chooseSimPanel = null;
    [SerializeField] private Dropdown _simDropdown = null;

    [SerializeField] private GameObject _startButton = null;
    [SerializeField] private GameObject _okButton = null;

    private int _simToLaunch = 0;

    public void StartClicked()
    {
        int i = 0;
        int activeToggle = 0;
        foreach (Toggle toggle in _toggles)
        {
            if (toggle.isOn)
            {
                activeToggle = i;
            }
            i++;
        }

        Debug.Log("Active Toggle: " + activeToggle + ", UserID: " + _userIdDropdown.value + ", Tracking: " + _trackingDropdown.value);

        if (activeToggle == 1)
        {
            _client.playLocally = false;
            _client.id = _userIdDropdown.value;
        }
        else if (_trackingDropdown.value == 1)
        {
            _client.tracking = false;
        }

        _client.simToLaunch = _simToLaunch;

        _client.InitTcpClient();

        gameObject.SetActive(false);
    }

    public void ChangeSelected(int id)
    {
        if (id == 0)
        {
            _playLocallyPanel.enabled = true;
            _playLocallyPanel.color = _selectedColor;
            _playOnlinePanel.enabled = false;
        }
        else
        {
            _playOnlinePanel.enabled = true;
            _playOnlinePanel.color = _selectedColor;
            _playLocallyPanel.enabled = false;
        }
    }

    public void OkClicked()
    {
        _simToLaunch = _simDropdown.value;
        _chooseSimPanel.SetActive(false);
        _okButton.SetActive(false);
        _startButton.SetActive(true);
        _playLocallyPanel.gameObject.SetActive(true);
        if (_simDropdown.value == 0)
        {
            _playOnlinePanel.gameObject.SetActive(true);
        }
    }
}