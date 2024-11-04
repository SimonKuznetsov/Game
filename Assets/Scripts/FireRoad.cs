using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRoad : MonoBehaviour
{
    [SerializeField] private FireRoad _parent;
    [SerializeField] private bool _isInit;

    private Dictionary<Character, Coroutine> _coroutines = new Dictionary<Character, Coroutine>();
    private float _speed;
    private const float TimeDestroy = 60;

    public int ActorNumber { get; private set; }

    private void Start()
    {
        if (_isInit)
            return;

        _speed = CharacterDataInstance.Instance.SecondCharacterData.FireRoadSpeed;

        Destroy(gameObject, TimeDestroy);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Character character))
        {
            if (_parent == null)
                return;

            if (_parent.ActorNumber != character.photonView.Controller.ActorNumber && _coroutines.ContainsKey(character) == false)
            {
                var coroutine = StartCoroutine(character.WaitApplyDamage(CharacterDataInstance.Instance.SecondCharacterData.DamageFireRoad));
                _coroutines.Add(character, coroutine);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Character character))
        {
            if (_parent == null)
                return;

            if (_parent.ActorNumber != character.photonView.Controller.ActorNumber && _coroutines.TryGetValue(character, out Coroutine coroutine))
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                    coroutine = null;
                }

                _coroutines.Remove(character);
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
