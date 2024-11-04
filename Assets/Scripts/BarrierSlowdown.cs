using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierSlowdown : MonoBehaviour
{
    [SerializeField] private BarrierSlowdown _parent;
    [SerializeField] private bool _isInit;

    private float _speed;
    private const float TimeDestroy = 60;

    public int ActorNumber { get; private set; }

    private void Start()
    {
        if (_isInit)
            return;

        _speed = CharacterDataInstance.Instance.FirstCharacterData.BarrierSpeed;

        Destroy(gameObject, TimeDestroy);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Character character))
        {
            if (_parent == null)
                return;

            if (_parent.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                StartCoroutine(character.WaitPlayStan());
            }
        }
    }

    public void Init(int actorNumber)
    {
        ActorNumber = actorNumber;
    }

    public void Update()
    {
        if (_isInit)
            return;

        transform.position += _speed * Time.deltaTime * Vector3.back;
    }
}
