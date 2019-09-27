using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitialSetupUI : MonoBehaviour
{
    [SerializeField] private TCPTestClient _client = null;

    [SerializeField] private Toggle[] _toggles = new Toggle[2];

    [SerializeField] private Dropdown _dropdown = null;

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

        Debug.Log("Active Toggle: " + activeToggle + ", Dropdown: " + _dropdown.value);

        if (activeToggle == 1)
        {
            _client.playLocally = false;
            _client.id = _dropdown.value;
        }

        _client.InitTcpClient();

        gameObject.SetActive(false);
    }
}
