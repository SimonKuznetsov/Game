using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDisplay : MonoBehaviour
{
    [SerializeField] private Text _number;
    [SerializeField] private Text _name;
    [SerializeField] private Text _score;

    public void Init(int number, Player player)
    {
        _number.text = number.ToString();
        _name.text = player.NickName;

        if (player.CustomProperties.TryGetValue("Score", out var value))
        {
            _score.text = value.ToString();
        }
        else
        {
            _score.enabled = false;
        }
    }
}
