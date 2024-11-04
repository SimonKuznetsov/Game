using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierWater : MonoBehaviour
{
    [SerializeField] private BarrierWater _parent;
    [SerializeField] private bool _isInit;

    private bool _isEnter = false;
    private float _speed;
    private const float TimeDestroy = 60;

    public int ActorNumber { get; private set; }

    private void Start()
    {
        if (_isInit)
            return;

        _speed = CharacterDataInstance.Instance.ThirdCharacterData.BarrierSpeed;

        Destroy(gameObject, TimeDestroy);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Character character) && _isEnter == false)
        {
            _isEnter = true;

            if (_parent == null)
                return;

            if (_parent.ActorNumber != character.photonView.Controller.ActorNumber)
            {
                character.SendApplyDamage(CharacterDataInstance.Instance.ThirdCharacterData.DamageBarrier);
                character.SendAddWater(CharacterDataInstance.Instance.ThirdCharacterData.WaterBarrierWaterCount);
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
