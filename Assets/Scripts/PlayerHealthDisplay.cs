using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthDisplay : MonoBehaviour
{
    [SerializeField] private TextMesh _text;

    public void Hide()
    {
        _text.gameObject.SetActive(false);
    }

    public void UpdateText(int health)
    {
        _text.text = health.ToString();
    }
}
