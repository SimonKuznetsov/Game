using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextAnimation : MonoBehaviour
{
    [SerializeField] private string _message;

    private const float _cooldown = 0.5f;
    private const int _maxDotCount = 3;

    private Coroutine _coroutine;
    private Text _text;

    private void Awake()
    {
        _text = GetComponent<Text>();
    }

    private void OnEnable()
    {
        _coroutine = StartCoroutine(PlayAnimation());
    }

    private void OnDisable()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);
    }

    private IEnumerator PlayAnimation()
    {
        var cooldown = new WaitForSeconds(_cooldown);

        while (true)
        {
            string tempMessage = _message;

            for (int i = 0; i < _maxDotCount; i++)
            {
                tempMessage += ".";
                yield return cooldown;

                _text.text = tempMessage;
            }

            yield return cooldown;
            _text.text = _message;
        }
    }
}
