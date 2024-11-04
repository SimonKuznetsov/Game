using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Ability : MonoBehaviour
{
    [SerializeField] private List<Button> _buttons;

    private readonly Dictionary<ProbabilityType, float> _probabilityData = new Dictionary<ProbabilityType, float>()
    {
        [ProbabilityType.Shield] = 25,
        [ProbabilityType.Heal] = 25,
        [ProbabilityType.ActiveFirstAbility] = 25,
        [ProbabilityType.ActiveThirdAbility] = 25,
    };

    private List<Button> _tempButton = new List<Button>();
    private bool _isSilence = false;
    private float _timeActiveButton;
    private int _arrowCount;
    private int _characterIndex;

    public void Init()
    {
        _characterIndex = GetCharacterIndex();

        _timeActiveButton = GetTimeActivButton();
        _arrowCount = GetArrowCount();
        StartCoroutine(WaitActiveButtons());
    }

    public void Use1()
    {
        Use1(_buttons[0]);
    }

    public void Use1(Button button)
    {
        button.interactable = false;

        switch (_characterIndex)
        {
            case 0:
                StartCoroutine(WaitSendDamage(button));
                break;
            case 1:
                StartCoroutine(WaitSendFireDamage(button));
                break;
            case 2:
                StartCoroutine(WaitSendRangeDamage(button));
                break;
            case 3:
                StartCoroutine(WaitSendRangeDamageByDirection(button));
                break;
        }
    }

    public void Use2(Button button)
    {
        button.interactable = false;

        switch (_characterIndex)
        {
            case 0:
                StartCoroutine(WaitPlayStan(button));
                break;
            case 1:
                StartCoroutine(WaitRunningProbabilities(button));
                break;
            case 2:
                StartCoroutine(WaitHealAndFireDamage(button));
                break;
            case 3:
                StartCoroutine(WaitSendMultiplyDamage(button));
                break;
        }
    }

    public void Use3(Button button)
    {
        button.interactable = false;

        switch (_characterIndex)
        {
            case 0:
                StartCoroutine(WaitActiveShield(button));
                break;
            case 1:
                StartCoroutine(WaitCreateFireRoad(button));
                break;
            case 2:
                StartCoroutine(WaitCreateBarrierWater(button));
                break;
            case 3:
                StartCoroutine(WaitCreateSphere(button));
                break;
        }
    }

    public void Use4(Button button)
    {
        button.interactable = false;

        switch (_characterIndex)
        {
            case 0:
                StartCoroutine(WaitFireLasso(button));
                break;
            case 1:
                StartCoroutine(WaitPlayPhoenix(button));
                break;
            case 2:
                StartCoroutine(WaitRemoveAllEffects(button));
                break;
            case 3:
                StartCoroutine(WaitSendInvulnerability(button));
                break;
        }
    }

    public void Use5(Button button)
    {
        button.interactable = false;

        switch (_characterIndex)
        {
            case 0:
                StartCoroutine(WaitSendDamageWithBow(button));
                break;
            case 1:
                StartCoroutine(WaitSendDamageWithBow(button));
                break;
            case 2:
                StartCoroutine(WaitSendDamageWithBow(button));
                break;
            case 3:
                StartCoroutine(WaitSendDamageWithBow(button));
                break;
        }
    }

    public void PlaySilence()
    {
        StartCoroutine(PlaySilence(CharacterDataInstance.Instance.ThirdCharacterData.TimePlaySilence, null));
    }

    private IEnumerator WaitActiveButtons()
    {
        List<Image> images = new List<Image>();

        foreach (var button in _buttons)
        {
            images.Add(button.targetGraphic.GetComponent<Image>());
            button.interactable = false;
        }

        float targetFillAmount = 1;
        float duration = 7;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            foreach (var image in images)
            {
                elapsedTime += Time.deltaTime;
                float lerpValue = Mathf.Clamp01(elapsedTime / duration);
                image.fillAmount = Mathf.Lerp(0, targetFillAmount, lerpValue);
                yield return null;
            }
        }

        foreach (var image in images)
            image.fillAmount = targetFillAmount;

        foreach (var button in _buttons)
            button.interactable = true;

    }

#region Character1
    private IEnumerator WaitPlayStan(Button button)
    {
        TrackManager.instance.CreateBarrierSlowdown();
        yield return WaitActiveButton(button, _timeActiveButton);
    }

    private IEnumerator WaitActiveShield(Button button)
    {
        TrackManager.instance.characterController.character.PlayShield(CharacterDataInstance.Instance.FirstCharacterData.ShieldCount);
        yield return WaitActiveButton(button, _timeActiveButton);
        TrackManager.instance.characterController.character.StopShield();
    }

    private IEnumerator WaitFireLasso(Button button)
    {
        Character player = MathExtensions.GetNearestPlayer();

        if (player != null)
        {
            player.SendBlockMove(true);
            yield return WaitActiveButton(button, _timeActiveButton);
            player.SendBlockMove(false);
        }
    }

    private IEnumerator WaitSendDamage(Button button)
    {
        Character player = MathExtensions.GetNearestPlayer();

        if (player != null)
            player.SendApplyDamage(CharacterDataInstance.Instance.FirstCharacterData.Damage);

        yield return WaitActiveButton(button, _timeActiveButton);
    }
#endregion

#region Character2
    private IEnumerator WaitSendFireDamage(Button button)
    {
        Character player = MathExtensions.GetNearestPlayer();

        if (player != null)
        {
            player.SendApplyDamage(CharacterDataInstance.Instance.SecondCharacterData.DamageFirstSkill);
            player.SendCombustion(true);
        }

        StartCoroutine(PlaySilence(CharacterDataInstance.Instance.SecondCharacterData.TimePlaySilence, button));
        yield return WaitActiveButton(button, _timeActiveButton);
    }

    private IEnumerator WaitRunningProbabilities(Button button)
    {
        int index = GetRandomIndex();

        switch ((ProbabilityType)index)
        {
            case ProbabilityType.Shield:
                TrackManager.instance.characterController.character.PlayShield(CharacterDataInstance.Instance.SecondCharacterData.ShieldCount);
                break;
            case ProbabilityType.Heal:
                TrackManager.instance.characterController.character.SendHeal(CharacterDataInstance.Instance.SecondCharacterData.HealCount, true);
                break;
            case ProbabilityType.ActiveFirstAbility:
                StartCoroutine(WaitSendFireDamage(button));
                break;
            case ProbabilityType.ActiveThirdAbility:
                StartCoroutine(WaitCreateFireRoad(button));
                break;
        }

        yield return WaitActiveButton(button, _timeActiveButton);
        TrackManager.instance.characterController.character.StopShield();
    }

    private IEnumerator WaitCreateFireRoad(Button button)
    {
        TrackManager.instance.CreateFireRoad();
        yield return WaitActiveButton(button, _timeActiveButton);
    }

    private IEnumerator WaitPlayPhoenix(Button button)
    {
        TrackManager.instance.characterController.character.SendPhoenix(true);
        yield return WaitActiveButton(button, CharacterDataInstance.Instance.SecondCharacterData.PhoenixTime);
        TrackManager.instance.characterController.character.SendPhoenix(false);
    }

    private int GetRandomIndex()
    {
        float total = 0;

        List<float> probabilities = _probabilityData.Values.ToList();

        foreach (float probabilitie in probabilities)
            total += probabilitie;

        double randomValue = Random.value * total;

        for (int i = 0; i < _probabilityData.Count; i++)
        {
            if (randomValue < probabilities[i])
                return i;
            else
                randomValue -= probabilities[i];
        }

        return probabilities.Count - 1;
    }

#endregion

#region Character3
    private IEnumerator WaitSendRangeDamage(Button button)
    {
        List<Character> players = GetNearestPlayersByDistance(CharacterDataInstance.Instance.ThirdCharacterData.PlayerRangeDistance, CharacterDataInstance.Instance.ThirdCharacterData.MaxPlayer);
        StartCoroutine(WaitActiveSphere());

        foreach (var player in players)
        {
            if (player != null)
            {
                player.SendApplyDamage(CharacterDataInstance.Instance.ThirdCharacterData.Damage);
                player.SendAddWater(CharacterDataInstance.Instance.ThirdCharacterData.WaterFireDamageCount);
            }
        }

        yield return WaitActiveButton(button, _timeActiveButton);
    }

    private IEnumerator WaitHealAndFireDamage(Button button)
    {
        List<Character> players = GetNearestPlayersByDistance(CharacterDataInstance.Instance.ThirdCharacterData.PlayerFireRangeDistance, 1);

        foreach (var player in players)
        {
            if (player != null)
            {
                player.SendAddWater(CharacterDataInstance.Instance.ThirdCharacterData.WaterFireDamageCount);
                player.SendCombustion(true);
            }
        }

        TrackManager.instance.characterController.character.SendHeal(CharacterDataInstance.Instance.ThirdCharacterData.HealCount, true);

        yield return WaitActiveButton(button, _timeActiveButton);
    }

    private IEnumerator WaitRemoveAllEffects(Button button)
    {
        List<Character> players = GetPlayers();

        foreach (var player in players)
        {
            player.SendRemoveAllEffect();
        }

        yield return WaitActiveButton(button, _timeActiveButton);
    }

    private IEnumerator WaitCreateBarrierWater(Button button)
    {
        TrackManager.instance.CreateBarrierWater();
        yield return WaitActiveButton(button, _timeActiveButton);
    }
    #endregion

#region Character4
    private IEnumerator WaitSendRangeDamageByDirection(Button button)
    {
        List<Character> players = GetNearestPlayersBySphere(CharacterDataInstance.Instance.FourthCharacterData.PlayerDistance, CharacterDataInstance.Instance.FourthCharacterData.MaxPlayer);

        int damage = CharacterDataInstance.Instance.FourthCharacterData.Damage;

        if (players.Count > 1)
        {
            float percent = 1 - (players.Count - 1) * (float)CharacterDataInstance.Instance.FourthCharacterData.DamagePercent / 100;

            if (percent < 0)
            {
                damage = 0;
            }
            else
            {
                damage = Mathf.CeilToInt(damage * percent);
            }
        }

        foreach (var player in players)
        {
            if (player != null)
                player.SendApplyDamage(damage);
        }

        TrackManager.instance.characterController.character.SendHeal(CharacterDataInstance.Instance.FourthCharacterData.HealCount, true);

        yield return WaitActiveButton(button, _timeActiveButton);
    }

    private IEnumerator WaitSendInvulnerability(Button button)
    {
        TrackManager.instance.characterController.character.SendInvulnerability(true);
        StartCoroutine(PlaySilence(CharacterDataInstance.Instance.FourthCharacterData.TimePlaySilence, button));
        yield return WaitActiveButton(button, _timeActiveButton);
        TrackManager.instance.characterController.character.SendInvulnerability(false);
        TrackManager.instance.characterController.character.SendHeal(CharacterDataInstance.Instance.FourthCharacterData.HealCount, true);
    }

    private IEnumerator WaitSendMultiplyDamage(Button button)
    {
        TrackManager.instance.characterController.character.SendMultiplyDamage(true);
        yield return WaitActiveButton(button, _timeActiveButton);
        TrackManager.instance.characterController.character.SendMultiplyDamage(false);
        TrackManager.instance.characterController.character.SendHeal(CharacterDataInstance.Instance.FourthCharacterData.HealCount, true);
    }

    private IEnumerator WaitCreateSphere(Button button)
    {
        TrackManager.instance.CreateSphere();
        TrackManager.instance.characterController.character.SendHeal(CharacterDataInstance.Instance.FourthCharacterData.HealCount, true);
        yield return WaitActiveButton(button, _timeActiveButton);
    }
#endregion

    private IEnumerator WaitSendDamageWithBow(Button button)
    {
        GetImageFromButton(button).fillAmount = 0;
        yield return new WaitForSecondsRealtime(2f);

        if (_arrowCount <= 0)
            yield break;

        _arrowCount--;

        Character player = MathExtensions.GetNearestPlayer();

        if (player != null)
            player.SendApplyDamage(GetArrowDamage());

        yield return WaitActiveButton(button, _timeActiveButton);

        if (_arrowCount <= 0)
            button.interactable = false;
    }

#region Effect
    private IEnumerator WaitActiveSphere()
    {
        TrackManager.instance.characterController.character.RangeCollider.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        TrackManager.instance.characterController.character.RangeCollider.gameObject.SetActive(false);
    }

    private IEnumerator PlaySilence(float delay, Button currentButton)
    {
        _isSilence = true;

        foreach (var button in _buttons)
            button.interactable = false;

        yield return new WaitForSeconds(delay);

        _isSilence = false;

        foreach (var button in _buttons)
        {
            if (_tempButton.Contains(button))
            {
                if (currentButton != null)
                {
                    if (button == currentButton)
                        continue;
                }
                else
                {
                    continue;
                }
            }

            button.interactable = true;
        }
    }

    private IEnumerator WaitActiveButton(Button button, float duration)
    {
        if (_tempButton.Contains(button) == false)
            _tempButton.Add(button);

        Image image = button.targetGraphic.GetComponent<Image>();
        button.interactable = false;

        float targetFillAmount = 1;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float lerpValue = Mathf.Clamp01(elapsedTime / duration);
            image.fillAmount = Mathf.Lerp(0, targetFillAmount, lerpValue);
            yield return null;
        }

        image.fillAmount = targetFillAmount;

        if (_tempButton.Contains(button))
            _tempButton.Remove(button);

        button.interactable = _isSilence == false;
    }

    private Image GetImageFromButton(Button button)
    {
        return button.targetGraphic.GetComponent<Image>();
    }

    private List<Character> GetNearestPlayersByDistance(float distance, int maxPlayer)
    {
        List<Character> characters = new List<Character>();
        var currentPlayer = TrackManager.instance.characterController.character;
        var otherPlayers = GetPlayers();

        foreach (var otherPlayer in otherPlayers)
        {
            float playerDistance = Vector3.Distance(currentPlayer.transform.position, otherPlayer.transform.position);

            if (playerDistance <= distance)
            {
                if (characters.Count == maxPlayer)
                    return characters;

                characters.Add(otherPlayer);
            }
        }

        return characters;
    }

    private List<Character> GetNearestPlayersBySphere(float distance, int maxPlayer)
    {
        Vector3 direction = MathExtensions.GetPlayerDirection();
        Vector3 nextDirection = direction == Vector3.right ? Vector3.left : Vector3.right;

        List<Character> characters = MathExtensions.GetNearestPlayersByDirection(TrackManager.instance.characterController.character, direction, distance, maxPlayer);
        var nearestPlayers = MathExtensions.GetNearestPlayersByDirection(TrackManager.instance.characterController.character, nextDirection, distance, maxPlayer);

        foreach (var nearestPlayer in nearestPlayers)
        {
            if (characters.Contains(nearestPlayer) == false)
                characters.Add(nearestPlayer);
        }

        return characters;
    }

    private List<Character> GetPlayers()
    {
        var currentPlayer = TrackManager.instance.characterController.character;

        var otherPlayers = FindObjectsOfType<Character>().ToList();
        otherPlayers.Remove(currentPlayer);

        return otherPlayers;
    }
    #endregion

    private int GetArrowDamage()
    {
        switch (_characterIndex)
        {
            case 0:
                return CharacterDataInstance.Instance.FirstCharacterData.ArrowDamage;
            case 1:
                return CharacterDataInstance.Instance.SecondCharacterData.ArrowDamage;
            case 2:
                return CharacterDataInstance.Instance.ThirdCharacterData.ArrowDamage;
            case 3:
                return CharacterDataInstance.Instance.FourthCharacterData.ArrowDamage;
        }

        return 0;
    }

    private int GetCharacterIndex()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("CharacterIndex", out var result))
        {
            return (int)result;
        }

        return 0;
    }

    private float GetTimeActivButton()
    {
        switch (_characterIndex)
        {
            case 0:
                return CharacterDataInstance.Instance.FirstCharacterData.TimeActiveButton;
            case 1:
                return CharacterDataInstance.Instance.SecondCharacterData.TimeActiveButton;
            case 2:
                return CharacterDataInstance.Instance.ThirdCharacterData.TimeActiveButton;
            case 3:
                return CharacterDataInstance.Instance.FourthCharacterData.TimeActiveButton;
        }

        return 0;
    }

    private int GetArrowCount()
    {
        switch (_characterIndex)
        {
            case 0:
                return CharacterDataInstance.Instance.FirstCharacterData.ArrowCount;
            case 1:
                return CharacterDataInstance.Instance.SecondCharacterData.ArrowCount;
            case 2:
                return CharacterDataInstance.Instance.ThirdCharacterData.ArrowCount;
            case 3:
                return CharacterDataInstance.Instance.FourthCharacterData.ArrowCount;
        }

        return 0;
    }
}
