using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Config/Character/1")]
public class FirstCharacterData : ScriptableObject
{
    [SerializeField] private int _wallDamage;
    [SerializeField] private int _damage;
    [SerializeField] private int _arrowDamage;
    [SerializeField] private int _health;
    [SerializeField] private int _arrowCount;
    [SerializeField] private int _shieldCount;
    [SerializeField] private int _shieldPassiveCount;
    [SerializeField] private int _barrierSpeed;
    [SerializeField, Range(0, 100)] private int _chancePassiveShield;
    [SerializeField] private float _timeActiveButton;
    [SerializeField] private float _timePlayShieldPassive;

    public int WallDamage => _wallDamage;
    public int Damage => _damage;
    public int ArrowDamage => _arrowDamage;
    public int Health => _health;
    public int ArrowCount => _arrowCount;
    public int ShieldCount => _shieldCount;
    public int ShieldPassiveCount => _shieldPassiveCount;
    public int BarrierSpeed => _barrierSpeed;
    public int ChancePassiveShield => _chancePassiveShield;
    public float TimeActiveButton => _timeActiveButton;
    public float TimePlayShieldPassive => _timePlayShieldPassive;
}
