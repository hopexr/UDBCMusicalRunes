using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using MusicalRunes;

public class GameManager : MonoBehaviour
{
    private readonly string savekey = "SaveKey";

    public static GameManager Instance
    {
        get;
        private set;
    }

    [Header ("Rune Settings")]
    public int initialSequenceSize = 3;
    [SerializeField]
    private float delayBetweenRunePreview = 0.3f;
    [SerializeField]
    private int initialBoardSize = 4;
    [SerializeField]
    private RectTransform runesHolder;
    [SerializeField]
    private List<Rune> availableRunePrefabs;
    public List<Rune> BoardRunes
    {
        get;
        private set;
    }
    private List<Rune> instantiatedBoardRunes;

    /// <summary>
    /// Keep track of the current rune sequence
    /// </summary>
    private List<int> currentRuneSequence;

    /// <summary>
    /// Current index of the Rune that's been played
    /// </summary>
    private int currentPlayIndex;
    private bool isRuneChoosingTime;

    public int currentRuneIndex => currentRuneSequence[currentPlayIndex];

    [Header ("Coin Settings")]
    [SerializeField]
    private int coinsPerRune = 1;
    [SerializeField]
    private int coinsPerRound = 10;

    [Header("Preview Settings")]
    [SerializeField]
    private GameObject[] spinlights;

    [Header("Powerup Settings")]
    [SerializeField]
    private List<Powerup> powerups;

    [Header("UI References")]
    [SerializeField]
    private TMP_Text coinsAmountText;
    [SerializeField]
    private TMP_Text highScoreText;
    [SerializeField]
    private Announcer announcer;

    public Action<int> coinsChanged;
    public Action sequenceCompleted;
    public Action runeActivated;

    public delegate void OnPowerupUpgradedDelegate(PowerupType upgradePowerup, int newLevel);
    public OnPowerupUpgradedDelegate powerupUpgraded;


    public int coinsAmount
    {
        get => saveData.coinsAmount;

        set
        {
            saveData.coinsAmount = value;
            coinsAmountText.text = coinsAmount.ToString();

            // trigger the coins changed action
            coinsChanged?.Invoke(value);
        }
    }

    private int highScore
    {
        get => saveData.highScore;

        set
        {
            saveData.highScore = value;
            highScoreText.text = highScore.ToString();
        }
    }

    private int currentRound;
    private SaveData saveData;

    public int GetPowerupLevel(PowerupType powerupType)
    {
        return saveData.GetUpgradableLevel(powerupType);
    }

    public void UpgradePowerup(PowerupType powerupType, int price)
    {
        if(price > coinsAmount)
        {
            throw new Exception("You is broke, can't buy this");
        }

        coinsAmount -= price;

        var newLevel = GetPowerupLevel(powerupType) + 1;
        saveData.SetUpgradeableLevel(powerupType, newLevel);
        Save();

        powerupUpgraded?.Invoke(powerupType, newLevel);
    }

    void Awake()
    {
        if(Instance != null)
        {
            throw new System.Exception($"Multiple game managers in the scene! {Instance} :: {this}");
        }

        Instance = this;

        LoadSaveData();
        InitializeBoard();
        InitializeSequence();
        InitializeUI();
        StartCoroutine(PlaySequencePreviewCoroutine(2));

    }

    private void InitializeUI()
    {
        highScoreText.text = saveData.highScore.ToString();
        coinsAmountText.text = coinsAmount.ToString();

    }

    private void Reset()
    {
        for (int i = runesHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(runesHolder.GetChild(i).gameObject);
        }

        availableRunePrefabs.AddRange(instantiatedBoardRunes);

        InitializeBoard();
        InitializeSequence();
    }

    private void AddRandomRuneToBoard()
    {
        var runePrefab = availableRunePrefabs[UnityEngine.Random.Range(0, availableRunePrefabs.Count)];

        availableRunePrefabs.Remove(runePrefab);
        instantiatedBoardRunes.Add(runePrefab);

        var rune = Instantiate(runePrefab, runesHolder);
        rune.SetUp(BoardRunes.Count);
        BoardRunes.Add(rune);
    }

    private void InitializeBoard()
    {
        BoardRunes = new List<Rune>(initialBoardSize);
        instantiatedBoardRunes = new List<Rune>();

        for (int i = 0; i < initialBoardSize ; i++)
        {
            AddRandomRuneToBoard();
        }
    }

    public void OnRuneActivated(int index)
    {
        // TODO: Prevent Rune clicks when sequence is finished

        if(currentPlayIndex >= currentRuneSequence.Count)
        {
            return;
        }

        if (currentRuneSequence[currentPlayIndex] == index)
        {
            CorrectRuneSelected();
        }
        else
        {
            StartCoroutine(FailedSequence());
        }

    }


    private void InitializeSequence()
    {
        currentRuneSequence = new List<int>(initialSequenceSize);

        for (int i = 0; i < initialSequenceSize; i++)
        {
            currentRuneSequence.Add(UnityEngine.Random.Range(0, BoardRunes.Count));
        }
    }

    public Coroutine PlaySequencePreview(float startDelay = 1, bool resetPlayIndex = true)
    {
        if(resetPlayIndex)
        {
            currentPlayIndex = 0;
        }
        return StartCoroutine(PlaySequencePreviewCoroutine(startDelay));
    }

    private IEnumerator PlaySequencePreviewCoroutine(float startDelay = 1f)
    {
        SetPlayerInteractivity(false);
        yield return new WaitForSeconds(startDelay);

        // TODO: Animate each rune in turn
        EnablePreviewFeedback();

        string sequence = "Sequence: ";
        foreach (var index in currentRuneSequence)
        {
            yield return BoardRunes[index].ActivateRuneCoroutine();
            yield return new WaitForSeconds(delayBetweenRunePreview);

            sequence += $"{index}, ";
        }
        Debug.Log(sequence);



        DisablePreviewFeedback();
        SetPlayerInteractivity(true);
    }

    public void SetPlayerInteractivity(bool interactable)
    {
        foreach (var rune in BoardRunes)
        {
            if (interactable)
            {
                rune.EnableInteraction();
            }
            else
            {
                rune.DisableInteraction();
            }
        }

        foreach(var powerup in powerups)
        {
            powerup.Interactable = interactable;
        }
    }

    /// <summary>
    /// Sequence has finished and is incorrect.
    /// </summary>
    
    private IEnumerator FailedSequence()
    {
        SetPlayerInteractivity(false);

        announcer.ShowWrongRuneText();

        yield return new WaitForSeconds(2);

        if(currentRound > highScore)
        {
            highScore = currentRound;
            announcer.ShowHighScoreText(highScore);
            Save();
            yield return new WaitForSeconds(3);
        }

        Reset();
        currentPlayIndex = 0;
        currentRound = 0;
        yield return PlaySequencePreviewCoroutine(2);
    }

    /// <summary>
    /// When your sequence has finished with no mistakes
    /// </summary>
   
    
    private void CompletedSequence()
    {
        Debug.Log("Completed Sequence");

        coinsAmount += coinsPerRound;

        // currentRound = currentRound + 1
        currentRound++;
        Save();

        // trigger the sequence completed action
        sequenceCompleted?.Invoke();

        // creating a new rune sequence that builds off the previous
        currentRuneSequence.Add(UnityEngine.Random.Range(0, BoardRunes.Count));
        currentPlayIndex = 0;
        StartCoroutine(PlaySequencePreviewCoroutine(2));
    }

    /// <summary>
    /// When the player has selected the correct Rune.
    /// </summary>
    
    private void CorrectRuneSelected()
    {

        runeActivated?.Invoke();
        coinsAmount += coinsPerRune;

        currentPlayIndex++;

        if(currentPlayIndex >= currentRuneSequence.Count)
        {
            CompletedSequence();
        }
        else
        {
            Save();
        }
    }

    private void EnablePreviewFeedback()
    {
        foreach(var spinlight in spinlights)
        {
            spinlight.SetActive(true);
        }

        announcer.ShowPreviewText();
    }


    private void DisablePreviewFeedback()
    {
        foreach(var spinlight in spinlights)
        {
            spinlight.SetActive(false);
        }

        announcer.ShowSequenceText();
        
    }

    private void LoadSaveData()
    {
        if(PlayerPrefs.HasKey(savekey))
        {
            string serializedSaveData = PlayerPrefs.GetString(savekey);
            saveData = SaveData.Deserialize(serializedSaveData);

            return;
        }

        saveData = new SaveData(true);
    }

    private void Save()
    {
        string serializedSaveData = saveData.Serialize();
        PlayerPrefs.SetString(savekey, serializedSaveData);
        Debug.Log(serializedSaveData);
    }

}
