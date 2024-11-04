using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Config/Character/2")]
public class SecondCharacterData : ScriptableObject
{
    [SerializeField] private int _damageFirstSkill;
    [SerializeField] private int _damageFireRoad;
    [SerializeField] private int _damageCombustion;
    [SerializeField] private int _arrowDamage;
    [SerializeField] private int _health;
    [SerializeField] private int _arrowCount;
    [SerializeField] private int _shieldCount;
    [SerializeField] private int _healCount;
    [SerializeField] private float _combustionTime;
    [SerializeField] private float _phoenixTime;
    [SerializeField] private int _fireRoadSpeed;
    [SerializeField] private float _timeActiveButton;
    [SerializeField] private float _timePlaySilence;

    public int DamageFirstSkill => _damageFirstSkill;
    public int DamageFireRoad => _damageFireRoad;
    public int DamageCombustion => _damageCombustion;
    public int ArrowDamage => _arrowDamage;
    public int Health => _health;
    public int ArrowCount  => _arrowCount;
    public int ShieldCount  => _shieldCount;
    public int HealCount => _healCount;
    public float CombustionTime => _combustionTime;
    public float PhoenixTime => _phoenixTime;
    public int FireRoadSpeed => _fireRoadSpeed;
    public float TimeActiveButton => _timeActiveButton;
    public float TimePlaySilence => _timePlaySilence;
}
