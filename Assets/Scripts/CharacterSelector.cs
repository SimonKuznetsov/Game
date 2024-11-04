using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    [SerializeField] private List<Toggle> _toggles;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("CharacterIndex"))
        {
            int index = PlayerPrefs.GetInt("CharacterIndex");

            Toggle toggle = _toggles.GetElementByIndex(x => x == index);

            toggle.isOn = true;
        }
        else
        {
            PlayerPrefs.SetInt("CharacterIndex", 0);
        }
    }

    public void Select(CharacterSelectData characterSelect)
    {
        if (characterSelect.Toggle.isOn)
        {
            PlayerPrefs.SetInt("CharacterIndex", characterSelect.Index);
        }
    }
}
