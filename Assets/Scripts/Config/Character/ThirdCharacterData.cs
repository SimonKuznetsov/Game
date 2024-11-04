using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Config/Character/3")]
public class ThirdCharacterData : ScriptableObject
{
    [SerializeField] private int _maxPlayer;
    [SerializeField] private float _playerRangeDistance;
    [SerializeField] private float _playerFireRangeDistance;
    [SerializeField] private float _barrierSpeed;
    [SerializeField] private int _damage;
    [SerializeField] private int _damageBarrier;
    [SerializeField] private int _damageWater;
    [SerializeField] private int _waterBarrierWaterCount;
    [SerializeField] private int _waterFireDamageCount;
    [SerializeField] private int _arrowDamage;
    [SerializeField] private int _health;
    [SerializeField] private int _arrowCount;
    [SerializeField] private int _healCount;
    [SerializeField] private int _waterEffectMax;
    [SerializeField] private int _tempWaterEffectMax;
    [SerializeField] private float _combustionTime;
    [SerializeField] private float _timeActiveButton;
    [SerializeField] private float _timePlaySilence;

    public float PlayerRangeDistance => _playerRangeDistance;
    public float PlayerFireRangeDistance => _playerFireRangeDistance;
    public int Damage => _damage;
    public float BarrierSpeed => _barrierSpeed;
    public int MaxPlayer => _maxPlayer;
    public int DamageBarrier => _damageBarrier;
    public int DamageWater => _damageWater;
    public int WaterBarrierWaterCount => _waterBarrierWaterCount;
    public int WaterFireDamageCount => _waterFireDamageCount;
    public int ArrowDamage => _arrowDamage;
    public int Health => _health;
    public int ArrowCount => _arrowCount;
    public int HealCount => _healCount;
    public int WaterEffectMax => _waterEffectMax;
    public int TempWaterEffectMax => _tempWaterEffectMax;
    public float CombustionTime => _combustionTime;
    public float TimeActiveButton => _timeActiveButton;
    public float TimePlaySilence => _timePlaySilence;
}
