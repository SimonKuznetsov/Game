using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Photon.Pun;

#if UNITY_ADS
using UnityEngine.Advertisements;
#endif
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif

public class GameStateMultiplayer : GameState
{
    [SerializeField] private Ability _ability;
    [SerializeField] private Joystick _joystick;
    [SerializeField] private Text _health;
    [SerializeField] private Text _shield;

    static int s_DeadHash = Animator.StringToHash("Dead");

    public void UpdateHealthText(int health)
    {
        _health.text = health.ToString();
    }

    public override void Enter(AState from)
    {
        _ability.Init();
        _shield.enabled = false;
        _joystick.enabled = true;
        pauseButton.gameObject.SetActive(false);

        m_CountdownRectTransform = countdownText.GetComponent<RectTransform>();

        m_LifeHearts = new Image[k_MaxLives];
        for (int i = 0; i < k_MaxLives; ++i)
        {
            m_LifeHearts[i] = lifeRectTransform.GetChild(i).GetComponent<Image>();
        }

        if (MusicPlayer.instance.GetStem(0) != gameTheme)
        {
            MusicPlayer.instance.SetStem(0, gameTheme);
            CoroutineHandler.StartStaticCoroutine(MusicPlayer.instance.RestartAllStems());
        }

        m_AdsInitialised = false;
        m_GameoverSelectionDone = false;

        StartGame();
    }

    public void SetShield(int shieldCount, bool state)
    {
        SetShieldCount(shieldCount);
        _shield.enabled = state;
    }

    public void SetShieldCount(int shieldCount)
    {
        _shield.text = shieldCount.ToString();
    }

    public override void Exit(AState to)
    {
        canvas.gameObject.SetActive(false);

        ClearPowerup();
    }

    public override void StartGame()
    {
        canvas.gameObject.SetActive(true);
        pauseMenu.gameObject.SetActive(false);
        wholeUI.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(!trackManager.isTutorial);
        gameOverPopup.SetActive(false);

        sideSlideTuto.SetActive(false);
        upSlideTuto.SetActive(false);
        downSlideTuto.SetActive(false);
        finishTuto.SetActive(false);
        tutorialValidatedObstacles.gameObject.SetActive(false);

        if (!trackManager.isRerun)
        {
            m_TimeSinceStart = 0;
            trackManager.characterController.currentLife = trackManager.characterController.maxLife;
        }

        currentModifier.OnRunStart(this);

        m_IsTutorial = !PlayerData.instance.tutorialDone;
        trackManager.isTutorial = m_IsTutorial;

        if (m_IsTutorial)
        {
            tutorialValidatedObstacles.gameObject.SetActive(true);
            tutorialValidatedObstacles.text = $"0/{k_ObstacleToClear}";

            m_DisplayTutorial = true;
            trackManager.newSegmentCreated = segment =>
            {
                if (trackManager.currentZone != 0 && !m_CountObstacles && m_NextValidSegment == null)
                {
                    m_NextValidSegment = segment;
                }
            };

            trackManager.currentSegementChanged = segment =>
            {
                m_CurrentSegmentObstacleIndex = 0;

                if (!m_CountObstacles && trackManager.currentSegment == m_NextValidSegment)
                {
                    trackManager.characterController.currentTutorialLevel += 1;
                    m_CountObstacles = true;
                    m_NextValidSegment = null;
                    m_DisplayTutorial = true;

                    tutorialValidatedObstacles.text = $"{m_TutorialClearedObstacle}/{k_ObstacleToClear}";
                }
            };
        }

        m_Finished = false;
        m_PowerupIcons.Clear();

        StartCoroutine(trackManager.Begin());
    }

    public override string GetName()
    {
        return "Multiplayer";
    }

    public override void Tick()
    {
        _joystick.enabled = trackManager.characterController.m_IsRunning;

        if (trackManager.isLoaded)
        {
            CharacterInputController chrCtrl = trackManager.characterController;

            m_TimeSinceStart += Time.deltaTime;

            if (chrCtrl.currentLife <= 0)
            {
                pauseButton.gameObject.SetActive(false);
                
                chrCtrl.CleanConsumable();

                if (chrCtrl.character != null)
                    chrCtrl.character.animator.SetBool(s_DeadHash, true);
                chrCtrl.characterCollider.koParticle.gameObject.SetActive(true);
                StartCoroutine(WaitForGameOver());
            }

            // Consumable ticking & lifetime management
            List<Consumable> toRemove = new List<Consumable>();
            List<PowerupIcon> toRemoveIcon = new List<PowerupIcon>();

            for (int i = 0; i < chrCtrl.consumables.Count; ++i)
            {
                PowerupIcon icon = null;
                for (int j = 0; j < m_PowerupIcons.Count; ++j)
                {
                    if (m_PowerupIcons[j].linkedConsumable == chrCtrl.consumables[i])
                    {
                        icon = m_PowerupIcons[j];
                        break;
                    }
                }

                chrCtrl.consumables[i].Tick(chrCtrl);
                if (!chrCtrl.consumables[i].active)
                {
                    toRemove.Add(chrCtrl.consumables[i]);
                    toRemoveIcon.Add(icon);
                }
                else if (icon == null)
                {
                    // If there's no icon for the active consumable, create it!
                    GameObject o = Instantiate(PowerupIconPrefab);

                    icon = o.GetComponent<PowerupIcon>();

                    icon.linkedConsumable = chrCtrl.consumables[i];
                    icon.transform.SetParent(powerupZone, false);

                    m_PowerupIcons.Add(icon);
                }
            }

            for (int i = 0; i < toRemove.Count; ++i)
            {
                toRemove[i].Ended(trackManager.characterController);

                Addressables.ReleaseInstance(toRemove[i].gameObject);
                if (toRemoveIcon[i] != null)
                    Destroy(toRemoveIcon[i].gameObject);

                chrCtrl.consumables.Remove(toRemove[i]);
                m_PowerupIcons.Remove(toRemoveIcon[i]);
            }

            UpdateUI();

            currentModifier.OnRunTick(this);
        }
    }

    protected override void OnApplicationPause(bool pauseStatus)
    {
    }

    protected override void OnApplicationFocus(bool focusStatus)
    {
    }

    public override void Pause(bool displayMenu = true)
    {
        
    }

    public override void Resume()
    {
        
    }

    public override void QuitToLoadout()
    {
        // Used by the pause menu to return immediately to loadout, canceling everything.
        Time.timeScale = 1.0f;
        AudioListener.pause = false;
        trackManager.End();
        trackManager.isRerun = false;
        PlayerData.instance.Save();
        manager.SwitchState("Loadout");
    }

    public override void UpdateUI()
    {
        coinText.text = trackManager.characterController.coins.ToString();
        premiumText.text = trackManager.characterController.premium.ToString();

        for (int i = 0; i < 3; ++i)
        {

            if (trackManager.characterController.currentLife > i)
            {
                m_LifeHearts[i].color = Color.white;
            }
            else
            {
                m_LifeHearts[i].color = Color.black;
            }
        }

        scoreText.text = trackManager.score.ToString();
        multiplierText.text = "x " + trackManager.multiplier;

        distanceText.text = Mathf.FloorToInt(trackManager.worldDistance).ToString() + "m";

        if (trackManager.timeToStart >= 0)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = Mathf.Ceil(trackManager.timeToStart).ToString();
            m_CountdownRectTransform.localScale = Vector3.one * (1.0f - (trackManager.timeToStart - Mathf.Floor(trackManager.timeToStart)));
        }
        else
        {
            m_CountdownRectTransform.localScale = Vector3.zero;
        }

        // Consumable
        if (trackManager.characterController.inventory != null)
        {
            inventoryIcon.transform.parent.gameObject.SetActive(true);
            inventoryIcon.sprite = trackManager.characterController.inventory.icon;
        }
        else
            inventoryIcon.transform.parent.gameObject.SetActive(false);
    }

    public override IEnumerator WaitForGameOver()
    {
        m_Finished = true;
        trackManager.StopMove();

        // Reseting the global blinking value. Can happen if game unexpectly exited while still blinking
        Shader.SetGlobalFloat("_BlinkingValue", 0.0f);

        yield return new WaitForSeconds(2.0f);
        if (currentModifier.OnRunEnd(this))
        {
            if (trackManager.isRerun)
                manager.SwitchState("GameOver");
            else
                OpenGameOverPopup();

            PhotonNetwork.Disconnect();
        }
    }

    public override void ClearPowerup()
    {
        for (int i = 0; i < m_PowerupIcons.Count; ++i)
        {
            if (m_PowerupIcons[i] != null)
                Destroy(m_PowerupIcons[i].gameObject);
        }

        trackManager.characterController.powerupSource.Stop();

        m_PowerupIcons.Clear();
    }

    public override void OpenGameOverPopup()
    {
        premiumForLifeButton.interactable = PlayerData.instance.premium >= 3;

        premiumCurrencyOwned.text = PlayerData.instance.premium.ToString();

        ClearPowerup();

        gameOverPopup.SetActive(true);
    }

    public override void GameOver()
    {
        manager.SwitchState("GameOver");
    }
}
