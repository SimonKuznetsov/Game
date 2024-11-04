using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Config/Character/4")]
public class FourthCharacterData : ScriptableObject
{
    [SerializeField, Range(0, 100)] private float _damagePercent;
    [SerializeField] private float _playerDistance;
    [SerializeField] private float _barrierSpeed;
    [SerializeField] private int _damage;
    [SerializeField] private int _damageBarrier;
    [SerializeField] private int _multiplyDamage;
    [SerializeField] private int _maxPlayer;
    [SerializeField] private int _healCount;
    [SerializeField] private int _arrowDamage;
    [SerializeField] private int _health;
    [SerializeField] private int _arrowCount;
    [SerializeField] private float _timeActiveButton;
    [SerializeField] private float _timePlaySilence;

    public float DamagePercent => _damagePercent;
    public float PlayerDistance => _playerDistance;
    public float BarrierSpeed => _barrierSpeed;
    public int Damage => _damage;
    public int DamageBarrier => _damageBarrier;
    public int MultiplyDamage => _multiplyDamage;
    public int MaxPlayer => _maxPlayer;
    public int HealCount => _healCount;
    public int ArrowDamage => _arrowDamage;
    public int Health => _health;
    public int ArrowCount => _arrowCount;
    public float TimeActiveButton => _timeActiveButton;
    public float TimePlaySilence => _timePlaySilence;
}
