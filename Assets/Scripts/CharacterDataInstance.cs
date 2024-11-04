using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDataInstance : MonoBehaviour
{
    [SerializeField] private FirstCharacterData _firstCharacterData;
    [SerializeField] private SecondCharacterData _secondCharacterData;
    [SerializeField] private ThirdCharacterData _thirdCharacterData;
    [SerializeField] private FourthCharacterData _fourthCharacterData;

    private static CharacterDataInstance _instance;

    public static CharacterDataInstance Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<CharacterDataInstance>();

            return _instance;
        }
    }

    public FirstCharacterData FirstCharacterData => _firstCharacterData;
    public SecondCharacterData SecondCharacterData => _secondCharacterData;
    public ThirdCharacterData ThirdCharacterData => _thirdCharacterData;
    public FourthCharacterData FourthCharacterData => _fourthCharacterData;
}
