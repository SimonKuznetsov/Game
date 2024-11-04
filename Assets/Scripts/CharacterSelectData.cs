using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class CharacterSelectData : MonoBehaviour
{
    [SerializeField] private int _index;

    private Toggle _toggle;

    public int Index => _index;
    public Toggle Toggle => _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
    }
}
