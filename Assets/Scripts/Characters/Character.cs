using UnityEngine;

using Photon.Pun;
using static CharacterInputController;
using System.Collections;
using System.Linq;

/// <summary>
/// Mainly used as a data container to define a character. This script is attached to the prefab
/// (found in the Bundles/Characters folder) and is to define all data related to the character.
/// </summary>
public class Character : MonoBehaviourPun, IPunObservable
{
    private const float CombustionPerSecond = 1;

    [SerializeField] private Collider _rangeCollider;
    [SerializeField] private TextMesh _waterText;
    [SerializeField] private TextMesh _indicator;
    [SerializeField] private PlayerHealthDisplay _healthDisplay;
    [SerializeField] private float _speed;

    private Ability _ability;
    private Coroutine _waitActiveInticatorJob;
    private Coroutine _shieldJob;
    private Camera _playerCamera;
    private GameStateMultiplayer _gameState;
    private Vector3 _targetPlayerPosition;
    private Vector3 _targetCameraPosition;
    private Vector3 _defaultCameraPosition;
    private Vector3 _defaultPlayerPosition;
    private float _positionY;
    private float _runningTime;
    private float _tempRunningTime;
    private float _combustionMaxTime;
    private int _shieldCount;
    private int _characterIndex;
    private int _targetCount;
    private bool _isPhoenix = false;
    private bool _isBlockMove = false;
    private bool _isActiveShield = false;
    private bool _isCombustion = false;
    private bool _isHealPhoenix = false;
    private bool _isInvulnerability = false;

    private static int s_BlinkingValueHash = Shader.PropertyToID("_BlinkingValue");

    public Collider RangeCollider => _rangeCollider;
    public float PositionY => _positionY;
    public int WaterEffectCount { get; private set; }
    public int TempWaterEffectCount { get; private set; }
    public int StartHealth { get; private set; }
    public int Health { get; private set; }
    public bool IsMultiplyDamage { get; private set; } = false;

    private Joystick _joystick;
    private SkinnedMeshRenderer[] _renderers;
    private Vector3 _targetPosition;
    private float _jumpStart;
    private float _slideStart;
    private bool _isJump = false;
    private bool _isSlide = false;

    public string characterName;
    public int cost;
    public int premiumCost;

    public CharacterAccessories[] accessories;

    public Animator animator;
    public Sprite icon;

    [Header("Sound")]
    public AudioClip jumpSound;
    public AudioClip hitSound;
    public AudioClip deathSound;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        _joystick = FindObjectOfType<Joystick>();
        _gameState = FindObjectOfType<GameStateMultiplayer>(true);
    }

    private void Start()
    {
        Health = GetHealth();
        StartHealth = Health;

        _gameState.UpdateHealthText(Health);

        if (photonView == null)
            return;

        RangeCollider.gameObject.SetActive(false);
        _ability = FindObjectOfType<Ability>();
        _waterText.gameObject.SetActive(false);
        _indicator.gameObject.SetActive(false);
        _healthDisplay.UpdateText(Health);

        if (photonView.IsMine)
        {
            _characterIndex = GetCharacterIndex();
            _playerCamera = TrackManager.instance.characterController.Camera;
            _defaultCameraPosition = _playerCamera.transform.localPosition;
            _defaultPlayerPosition = transform.localPosition;

            _healthDisplay.Hide();

            switch (_characterIndex)
            {
                case 1:
                    _combustionMaxTime = CharacterDataInstance.Instance.SecondCharacterData.CombustionTime + 1;
                    break;
                case 2:
                    _combustionMaxTime = CharacterDataInstance.Instance.ThirdCharacterData.CombustionTime + 1;
                    break;
            }
       
        
        }
    }

    public void Heal(int heal)
    {
        Health += heal;

        Health = Mathf.Clamp(Health, 0, StartHealth);

        if (photonView.IsMine)
            _gameState.UpdateHealthText(Health);
    }

    public void SetMultiplyDamage(bool isMultiplyDamage)
    {
        IsMultiplyDamage = isMultiplyDamage;
    }

    public void SetCombustion(bool isCombustion)
    {
        _isCombustion = isCombustion;

        if (isCombustion)
        {
            _runningTime = 0;
            _tempRunningTime = 0;
        }
    }

    public void SendRemoveAllEffect()
    {
        photonView.RPC(nameof(RemoveAllEffectNetwork), RpcTarget.AllBuffered, TempWaterEffectCount, photonView.ViewID);
    }

    public void SendPhoenix(bool isPhoenix)
    {
        photonView.RPC(nameof(SetPhoenixNetwork), RpcTarget.AllBuffered, isPhoenix, photonView.ViewID);
    }

    public void SendCombustion(bool isCombustion)
    {
        photonView.RPC(nameof(PlayCombustionNetwork), RpcTarget.AllBuffered, isCombustion, photonView.ViewID);
    }

    public void SendHeal(int heal, bool isShowInticator)
    {
        photonView.RPC(nameof(HealNetwork), RpcTarget.AllBuffered, heal, isShowInticator, photonView.ViewID);
    }

    public void SendInvulnerability(bool state)
    {
        photonView.RPC(nameof(SetInvulnerabilityNetwork), RpcTarget.AllBuffered, state, photonView.ViewID);
    }

    public void SendMultiplyDamage(bool state)
    {
        photonView.RPC(nameof(SetSendMultiplyDamageNetwork), RpcTarget.AllBuffered, state, photonView.ViewID);
    }

    public void PlayShield(int count)
    {
        SendActiveShield(count);
        UpdateShieldDisplay(true);
    } 

    public void StopShield()
    {
        if (_shieldJob != null)
        {
            StopCoroutine(_shieldJob);
            _shieldJob = null;
        }

        SendActiveShield(0);
        UpdateShieldDisplay(false);
    }

    public void SetShield(int count)
    {
        _shieldCount = count;
        _isActiveShield = count > 0;
    }

    public void ClearWaterEffect()
    {
        TempWaterEffectCount = 0;

        UpdateWaterText();
    }

    private void UpdateWaterText()
    {
        _waterText.gameObject.SetActive(TempWaterEffectCount > 0);
        _waterText.text = TempWaterEffectCount.ToString();
    }

    private void UpdateShieldDisplay(bool isActive)
    {
        _gameState.SetShield(_shieldCount, isActive);
    }

    public void ApplyDamage(int damage, Vector3 direction = default, bool isFirstDamage = false)
    {
        if (_isInvulnerability)
            return;

        if (IsMultiplyDamage && photonView.IsMine && isFirstDamage == false)
        {
            var players = MathExtensions.GetNearestPlayersByDirection(this, direction, 999, CharacterDataInstance.Instance.FourthCharacterData.MaxPlayer).Where(player => player.photonView.ViewID != photonView.ViewID);

            foreach (var player in players)
                player.SendApplyDamage(CharacterDataInstance.Instance.FourthCharacterData.MultiplyDamage);
        }

        if (_isActiveShield)
        {
            int currentDamage = _shieldCount - damage;

            if (currentDamage > 0)
            {
                _shieldCount -= damage;
                _gameState.SetShieldCount(_shieldCount);
            }
            else
            {
                Health -= Mathf.Abs(currentDamage);
                StopShield();
            }

            if (_characterIndex == 0)
            {
                if (_shieldCount < CharacterDataInstance.Instance.FirstCharacterData.ShieldPassiveCount)
                    TryPlayShield();
            }
        }
        else
        {
            Health -= damage;

            if (_characterIndex == 0)
                TryPlayShield();
        }

        ShowInticator($"-{damage}");

        if (Health <= 0 && photonView.IsMine)
        {
            if (_isPhoenix && _isHealPhoenix == false)
            {
                _runningTime = 999;
                SendCombustion(false);
                ShowInticator(string.Empty);
                _isHealPhoenix = true;
                int health = StartHealth / 100 * 10;

                Health = health;
            }
            else
            {
                TrackManager.instance.characterController.currentLife = 0;

                var currentPlayer = TrackManager.instance.characterController.character;

                var otherPlayers = FindObjectsOfType<Character>().ToList();
                otherPlayers.Remove(currentPlayer);

                foreach (var otherPlayer in otherPlayers)
                    otherPlayer.gameObject.SetActive(false);
            }
        }
    }

    private void TryPlayShield()
    {
        int chance = Random.Range(0, 101);

        if (chance <= CharacterDataInstance.Instance.FirstCharacterData.ChancePassiveShield)
        {
            if (photonView.IsMine)
            {
                StopShield();
                _shieldJob = StartCoroutine(WaitPlayShield());
            }
        }
    }

    public void SetBlockMove(bool isBlockMove)
    {
        _isBlockMove = isBlockMove;
    }

    public void SetInvulnerability(bool isInvulnerability)
    {
        _isInvulnerability = isInvulnerability;
    }

    public void SetPhoenix(bool isPhoenix)
    {
        _isPhoenix = isPhoenix;

        if (isPhoenix == false)
        {
            _isHealPhoenix = false;
        }
    }

    public void AddWater(int count)
    {
        WaterEffectCount += count;
        TempWaterEffectCount += count;

        if (TempWaterEffectCount >= CharacterDataInstance.Instance.ThirdCharacterData.TempWaterEffectMax)
        {
            TempWaterEffectCount = 0;

            if (photonView.IsMine)
            {
                SendApplyDamage(50);
                _ability.PlaySilence();
            }
        }

        if (WaterEffectCount >= CharacterDataInstance.Instance.ThirdCharacterData.WaterEffectMax)
            WaterEffectCount = 0;

        UpdateWaterText();
    }

    public void SendActiveShield(int shieldCount)
    {
        photonView.RPC(nameof(PlayShieldNetwork), RpcTarget.AllBuffered, shieldCount, photonView.ViewID);
    }

    public void SendApplyDamage(int damage)
    {
        photonView.RPC(nameof(ApplyDamageNetwork), RpcTarget.AllBuffered, damage, MathExtensions.GetReverseDirection(), photonView.ViewID);
    }

    public void SendAddWater(int count)
    {
        if (WaterEffectCount + count >= CharacterDataInstance.Instance.ThirdCharacterData.WaterEffectMax)
        {
            WaterEffectCount = 0;
            _ability.Use1();
        }

        photonView.RPC(nameof(AddWaterNetwork), RpcTarget.AllBuffered, count, photonView.ViewID);
    }

    public void SendBlockMove(bool isBlockMove)
    {
        photonView.RPC(nameof(BlockMoveNetwork), RpcTarget.AllBuffered, isBlockMove, photonView.ViewID);
    }

    [PunRPC]
    public void AddWaterNetwork(int count, int viewId)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        if (view.TryGetComponent(out Character character))
        {
            character.AddWater(count);
        }
    }

    [PunRPC]
    public void RemoveAllEffectNetwork(int count, int viewId)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        if (view.TryGetComponent(out Character character))
        {
            character.ApplyDamage(CharacterDataInstance.Instance.ThirdCharacterData.DamageWater * count, MathExtensions.GetReverseDirection());
            character.ClearWaterEffect();
        }
    }

    [PunRPC]
    public void SetPhoenixNetwork(bool isPhoenix, int viewId)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        if (view.TryGetComponent(out Character character))
        {
            character.SetPhoenix(isPhoenix);
        }
    }

    [PunRPC]
    public void SetInvulnerabilityNetwork(bool isInvulnerability, int viewId)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        if (view.TryGetComponent(out Character character))
        {
            character.SetInvulnerability(isInvulnerability);
        }
    }

    [PunRPC]
    public void SetSendMultiplyDamageNetwork(bool isMultiplyDamage, int viewId)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        if (view.TryGetComponent(out Character character))
        {
            character.SetMultiplyDamage(isMultiplyDamage);
        }
    }

    [PunRPC]
    public void HealNetwork(int heal, bool isShowInticator, int viewId)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        if (view.TryGetComponent(out Character character))
        {
            character.Heal(heal);

            if (isShowInticator)
                character.ShowInticator($"+{heal}");
        }
    }

    [PunRPC]
    public void PlayCombustionNetwork(bool isCombustion, int viewId)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        if (view.TryGetComponent(out Character character))
        {
            character.SetCombustion(isCombustion);
        }
    }

    [PunRPC]
    public void BlockMoveNetwork(bool isBlockMove, int viewId)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        if (view.TryGetComponent(out Character character))
        {
            character.SetBlockMove(isBlockMove);
        }
    }

    [PunRPC]
    public void PlayShieldNetwork(int count, int viewId)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        if (view.TryGetComponent(out Character character))
        {
            character.SetShield(count);
        }
    }

    [PunRPC]
    public void ApplyDamageNetwork(int damage, Vector3 direction, int viewId)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        if (view.TryGetComponent(out Character character))
        {
            character.ApplyDamage(damage, direction);

            if (view.IsMine)
                _gameState.UpdateHealthText(Health);
        }
    }

    public void StartPlayAnimation()
    {
        if (PhotonNetwork.IsConnected == false)
            return;

        if (photonView == null)
            return;

        if (photonView.IsMine == false)
            animator.Play("Start");
    }

    public void StartRun()
    {
        if (PhotonNetwork.IsConnected == false)
            return;

        if (photonView == null)
            return;

        if (photonView.IsMine == false)
        {
            animator.Play(s_RunStartHash);
            animator.SetBool(s_MovingHash, true);
        }
    }

    public void SetupAccesory(int accessory)
    {
        for (int i = 0; i < accessories.Length; ++i)
        {
            accessories[i].gameObject.SetActive(i == PlayerData.instance.usedAccessory);
        }
    }

    public void ExecuteState(State state)
    {
        if (PhotonNetwork.IsConnected == false)
            return;

        photonView.RPC(nameof(ExecuteStateNetwork), RpcTarget.OthersBuffered, (int)state);
    }

    [PunRPC]
    public void ExecuteStateNetwork(int state)
    {
        switch ((State)state)
        {
            case State.Left:
                //_targetPosition = ChangeLane(-1);
                break;
            case State.Right:
                //_targetPosition = ChangeLane(1);
                break;
            case State.Jump:
                Jump();
                break;
            case State.Slide:
                Slide();
                break;
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnected == false)
            return;

        if (photonView == null)
            return;

        if (photonView.IsMine == false)
        {
            _healthDisplay.UpdateText(Health);

            //transform.localPosition = new Vector3(_positionX, _positionY, transform.localPosition.z);

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, new Vector3(transform.localPosition.x, verticalTargetPosition.y, transform.localPosition.z), TrackManager.instance.characterController.laneChangeSpeed * Time.deltaTime);

            if (_isJump)
            {
                if (TrackManager.instance.isMoving)
                {
                    float correctJumpLength = TrackManager.instance.characterController.jumpLength * (1.0f + TrackManager.instance.speedRatio);
                    float ratio = (TrackManager.instance.worldDistance - _jumpStart) / correctJumpLength;

                    if (ratio >= 1.0f)
                    {
                        _isJump = false;
                        animator.SetBool(s_JumpingHash, false);

                    }
                    else
                    {
                        _positionY = Mathf.Sin(ratio * Mathf.PI) * TrackManager.instance.characterController.jumpHeight;
                    }
                }
            }

            if (_isSlide)
            {
                float correctSlideLength = TrackManager.instance.characterController.slideLength * (1.0f + TrackManager.instance.speedRatio);
                float ratio = (TrackManager.instance.worldDistance - _slideStart) / correctSlideLength;

                if (ratio >= 1.0f)
                {
                    StopSliding();
                }
            }
        }
        else
        {   
            #region Character2
            if (_isCombustion)
            {
                _runningTime += Time.deltaTime;
                _tempRunningTime += Time.deltaTime;

                if (CombustionPerSecond < _tempRunningTime)
                {
                    if (_isHealPhoenix == false)
                        ApplyDamage(CharacterDataInstance.Instance.SecondCharacterData.DamageCombustion);

                    if (photonView.IsMine)
                        _gameState.UpdateHealthText(Health);
                    _tempRunningTime = 0;
                }

                if (_combustionMaxTime < _runningTime)
                {
                    SendCombustion(false);
                }
            }
            #endregion

            if (_isBlockMove)
                return;

            _positionY = transform.localPosition.y;

            float positionX = transform.position.x + _joystick.Horizontal * Time.deltaTime * _speed;
            float x = Mathf.Clamp(positionX, -5, 5);
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
            //transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }
    }

    public Vector3 ChangeLane(int direction)
    {
        int targetLane = TrackManager.instance.characterController.m_CurrentLane + direction;

        if (targetLane < 0 || targetLane > 2)
            return transform.localPosition;

        TrackManager.instance.characterController.m_CurrentLane = targetLane;
        return new Vector3((TrackManager.instance.characterController.m_CurrentLane - 1) * TrackManager.instance.laneOffset, 0, 0);
    }

    public void Jump()
    {
        if (_isJump == false)
        {
            if (_isSlide)
                StopSliding();

            float correctJumpLength = TrackManager.instance.characterController.jumpLength * (1.0f + TrackManager.instance.speedRatio);
            _jumpStart = TrackManager.instance.worldDistance;
            float animSpeed = k_TrackSpeedToJumpAnimSpeedRatio * (TrackManager.instance.speed / correctJumpLength);

            animator.SetFloat(s_JumpingSpeedHash, animSpeed);
            animator.SetBool(s_JumpingHash, true);

            _isJump = true;
        }
    }

    public void StopSliding()
    {
        if (_isSlide)
        {
            _isSlide = false;
            animator.SetBool(s_SlidingHash, false);
        }
    }

    public void Slide()
    {
        if (_isSlide == false)
        {
            if (_isJump)
                StopJumping();

            _isSlide = true;

            float correctSlideLength = TrackManager.instance.characterController.slideLength * (1.0f + TrackManager.instance.speedRatio);
            _slideStart = TrackManager.instance.worldDistance;
            float animSpeed = k_TrackSpeedToJumpAnimSpeedRatio * (TrackManager.instance.speed / correctSlideLength);

            animator.SetFloat(s_JumpingSpeedHash, animSpeed);
            animator.SetBool(s_SlidingHash, true);
        }
    }

    public void StopJumping()
    {
        if (_isJump)
        {
            _isJump = false;
            animator.Play(s_RunStartHash);
        }
    }

    private IEnumerator WaitPlayShield()
    {
        PlayShield(CharacterDataInstance.Instance.FirstCharacterData.ShieldPassiveCount);
        yield return new WaitForSeconds(CharacterDataInstance.Instance.FirstCharacterData.TimePlayShieldPassive);
        StopShield();
    }

    public IEnumerator WaitApplyDamage(int damage, bool isCombustion = true, bool isUpdateText = false)
    {
        if (isCombustion)
            SetCombustion(true);

        var waitForSeconds = new WaitForSeconds(1);
        bool isFirstDamage = false;

        while (true)
        {
            ApplyDamage(damage, MathExtensions.GetPlayerDirection(), isFirstDamage);
            isFirstDamage = true;

            if (photonView.IsMine)
                _gameState.UpdateHealthText(Health);

            yield return waitForSeconds;
        }
    }

    public IEnumerator WaitPlayStan()
    {
        for (int i = 0; i < 2; i++)
        {
            ApplyDamage(CharacterDataInstance.Instance.FirstCharacterData.WallDamage, MathExtensions.GetReverseDirection());

            if (photonView.IsMine)
                _gameState.UpdateHealthText(Health);
            yield return new WaitForSeconds(1);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(TrackManager.instance.characterController.verticalTargetPosition);
            stream.SendNext(_positionY);
            stream.SendNext(_isCombustion);
            stream.SendNext(Health);
        }
        else
        {
            verticalTargetPosition = (Vector3)stream.ReceiveNext();
            _positionY = (float)stream.ReceiveNext();
            _isCombustion = (bool)stream.ReceiveNext();
            Health = (int)stream.ReceiveNext();
        }
    }

    public Vector3 verticalTargetPosition;

    private void ShowInticator(string content)
    {
        photonView.RPC(nameof(ShowInticatorNetwork), RpcTarget.AllBuffered, content, photonView.ViewID);
    }

    [PunRPC]
    public void ShowInticatorNetwork(string content, int viewId)
    {
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        if (view.TryGetComponent(out Character character))
        {
            character.PlayInticator(content);
        }
    }

    public void PlayInticator(string content)
    {
        if (_waitActiveInticatorJob != null)
        {
            StopCoroutine(_waitActiveInticatorJob);
            _waitActiveInticatorJob = null;
        }

        _waitActiveInticatorJob = StartCoroutine(WaitActiveInticator(content));
    }

    private IEnumerator WaitActiveInticator(string content)
    {
        _indicator.gameObject.SetActive(true);
        _indicator.text = content;
        yield return new WaitForSeconds(2);
        _indicator.gameObject.SetActive(false);
    }

    private int GetCharacterIndex()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("CharacterIndex", out var result))
        {
            return (int)result;
        }

        return 0;
    }

    private int GetHealth()
    {
        switch (_characterIndex)
        {
            case 0:
                return CharacterDataInstance.Instance.FirstCharacterData.Health;
            case 1:
                return CharacterDataInstance.Instance.SecondCharacterData.Health;
            case 2:
                return CharacterDataInstance.Instance.ThirdCharacterData.Health;
            case 3:
                return 40000;
        }

        return 0;
    }
}
