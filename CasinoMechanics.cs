using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RandomWinMultiplier;
using UnityEngine.SceneManagement;

public class CasinoUI : MonoBehaviour
{
    //UI Fields
    [Header("UI Buttons")]
    /// <summary>
    /// This is the button we enter play mode with
    /// </summary>
    public Button playButton;
    /// <summary>
    /// Should we increase our bet? If so, use this field
    /// </summary>
    public Button increaseBetButton;
    /// <summary>
    /// Should we decrease our bet? If so, use this field
    /// </summary>
    public Button decreaseBetButton;
    /// <summary>
    /// Select a random button in chests button array for the pooper
    /// </summary>
    [Tooltip("This will be assigned at runtime!")]
    public Button pooperChest;
    [Header("UI Texts")]
    /// <summary>
    /// UI Text for currentBalance <see cref="currentBalance"/>
    /// </summary>
    public Text currentBalanceText;
    /// <summary>
    /// UI Text for currentDenomination
    /// </summary>
    public Text currentDenominationText;
    /// <summary>
    /// Show the amount won on the previous turn
    /// </summary>
    public Text lastGameWinAmountText;
    [Header("Scoll Rect for Pick")]
    /// <summary>
    /// The Scroll Rect that holds all objects
    /// </summary>
    public ScrollRect slotMachineScrollRect;
    [Header("UI Sprites")]
    /// <summary>
    /// Show an open treasure chest after selection
    /// </summary>
    public Sprite openTreasureChest;
    /// <summary>
    /// Default, unopened style treasure chest
    /// </summary>
    public Sprite defaultTreasureChest;
    [Header("Audio & SFX")]
    /// <summary>
    /// Used for playing a sound when getting a win multiplier in a chest pick
    ///</summary>
    public AudioSource sfxAudioSource;
    /// <summary>
    /// Used for playing a sound when getting the highest win multiplier in a chest pick, jackpot
    ///</summary>
    public AudioSource audioSourceJackpot;
    /// <summary>
    /// Used for playing a sound when getting the lowest win multiplier in a chest pick, instant loss
    ///</summary>
    public AudioSource audioSourceInstantLoss;
}


public class CasinoMechanics : CasinoUI
{
    [Header("Class & Object References")]
    /// <summary>
    /// Get the <see cref="CasinoUI"/> class as an object
    /// </summary>
    public CasinoUI casinoUI = new CasinoUI();
    /// <summary>
    /// Create an object for winMultipliers <see cref="CasinoSpinSlots"/> class for reference
    /// </summary>
    public WinMultipliers winMulitpliersObject = new WinMultipliers();
    /// <summary>
    /// How much cash do we have?
    /// </summary>
    private float currentBalance = 10.00f;
    /// <summary>
    /// How much are we betting? Start with lowest value in <see cref="possibleDenominationValues"/>
    /// </summary>
    private float currentDenomination;
    /// <summary>
    /// Where are we indexed in our <see cref="possibleDenominationValues"/> Array
    /// </summary>
    private int denominationIndex;
    //Create a float array of the four possible denomination values
    private float[] possibleDenominationValues = { 0.25f, 0.50f, 1f, 5f };
    /// <summary>
    /// How much have we won from the last turn
    /// </summary>
    private float lastGameWinAmount;
    /// <summary>
    /// Choose a random Win Multiplier Amount
    /// </summary>
    private int randomWinMultiplier;
    /// <summary>
    /// Are we currently in play mode? Where we can select a chest
    /// </summary>
    public bool isPlaying;
    /// <summary>
    /// lists of chests Buttons
    /// </summary>
    public List<Button> chestsList = new List<Button>();

    // Start is called before the first frame update
    void Start()
    {
        //Set currentDenomination to first, lowest value
        currentDenomination = 0.25f;
        //Set currentDenomination, index of Array to first element
        denominationIndex = 0;
        //Set Text Component to there defaults
        casinoUI.currentBalanceText.text = "Balance:$ " + currentBalance.ToString();
        casinoUI.currentDenominationText.text = "Bet:$ " + currentDenomination.ToString();
        //Disable Chest Selection on Start of Play
        Button[] chests = casinoUI.slotMachineScrollRect.content.GetComponentsInChildren<Button>();
        chestsList.AddRange(chests);
        for (int i = 0; i < chests.Length; i++)
        {
            //Pooper selection
            pooperChest = chests[Random.Range(0, chests.Length)];
        }
        Debug.Log("Pooper chest is " + pooperChest);
        for (int i = 0; i < chests.Length; i++)
        {
            chests[i].interactable = false;
        }
        lastGameWinAmount = 0.00f;
        lastGameWinAmountText.text = "Last Game Win : " + lastGameWinAmount;
    }

    // Update is called once per frame
    void Update()
    {
        //If we do not have enough funds, disable the play button, else we can play
        if (isPlaying && currentDenomination > currentBalance)
        {
            playButton.interactable = false;
        } else if(isPlaying && currentDenomination < currentBalance)
        {
            playButton.interactable = false;
        } else if(!isPlaying)
        {
            playButton.interactable = true;
        }
    }
    //Enter the 'play' mode for this mini-game
    public void BeginPlay()
    {
        //Check if the amount we bet is atleast greater than or equal the amount we have in funds
        if (currentBalance >= currentDenomination)
        {
            //Set isPlaying bool to true indictating we are playing to pick a chest
            isPlaying = true;
            //Enable Chest Selection
            for (int i = 0; i < chestsList.Count; i++)
            {
                chestsList[i].image.sprite = defaultTreasureChest;
                chestsList[i].interactable = true;
            }
            //Disable inputs
            casinoUI.playButton.interactable = false;
            casinoUI.increaseBetButton.interactable = false;
            casinoUI.decreaseBetButton.interactable = false;

            currentBalance -= currentDenomination;
            casinoUI.currentBalanceText.text = "Balance:$ " + currentBalance.ToString();

            //Reset last win amount on 'Play' button press
            lastGameWinAmount = 0.0f;
            lastGameWinAmountText.text = "Last Game Win : " + lastGameWinAmount;
            Debug.Log("Win Multiplier is " + randomWinMultiplier);
        } 
    }

    //Selects certain Win Multiplier by percentage weighting
    private void SelectWinModifier()
    {

        //Pick a Win Multiplier
        var rng = new System.Random();

        int value = rng.Next(0, 100);
        if (value <= 5)
        {
            randomWinMultiplier = (int)winMulitpliersObject.SelectRandomMultiplier(WinMultipliers.randomMultiplier.fivePercentLoss);
            audioSourceJackpot.Play();
        }
        else if (value <= 15)
        {
            randomWinMultiplier = (int)winMulitpliersObject.SelectRandomMultiplier(WinMultipliers.randomMultiplier.fifteenPercentLoss);
            sfxAudioSource.Play();
        }
        else if (value <= 30)
        {
            randomWinMultiplier = (int)winMulitpliersObject.SelectRandomMultiplier(WinMultipliers.randomMultiplier.thirtyPercentLoss);
            sfxAudioSource.Play();
        }
        else if (value <= 50)
        {
            randomWinMultiplier = (int)winMulitpliersObject.SelectRandomMultiplier(WinMultipliers.randomMultiplier.fiftyPercentLoss);
            audioSourceInstantLoss.Play();
        }
        else if (value > 50)
        {
            randomWinMultiplier = (int)winMulitpliersObject.SelectRandomMultiplier(WinMultipliers.randomMultiplier.fiftyPercentLoss);
            audioSourceInstantLoss.Play();
        }
        Debug.Log("Random Value is " + value);
    }

    //Called when we click on a tressure chest
    public void PickChest()
    {
        //Change selected image to show an open chest sprite
        EventSystem.current.currentSelectedGameObject.GetComponent<Button>().image.sprite = openTreasureChest;
        //Call function to select win modifier
        SelectWinModifier();
        lastGameWinAmount = currentDenomination * randomWinMultiplier;
        EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text = lastGameWinAmount.ToString();
        Button selectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        //Disable Chest Selection
        Button[] chests = casinoUI.slotMachineScrollRect.content.GetComponentsInChildren<Button>();
        for (int i = 0; i < chests.Length; i++)
        {
            Debug.Log("Selected is " + selectedButton);
            chests[i].interactable = false;
        }
        //Remove the selected button from the list
        chestsList.Remove(selectedButton);
        //If you are trying to pick a chest, already selected, prevent it
        if (!chestsList.Contains(selectedButton))
            selectedButton.interactable = false;
        //Check  if the chest we have selected is the pooper chest
        if (selectedButton == pooperChest)
        {
            selectedButton.GetComponentInChildren<Text>().text = "Pooper";
            currentBalance = 0;
            Debug.Log("You have picked the pooper chest");
        }
        //Enable inputs
        casinoUI.playButton.interactable = true;
        casinoUI.increaseBetButton.interactable = true;
        casinoUI.decreaseBetButton.interactable = true;
        lastGameWinAmountText.text = "Last game win amount: " + lastGameWinAmount.ToString();
        Debug.Log("Last game win amount " + lastGameWinAmount);
        currentBalance += lastGameWinAmount;
        //Set bool to false for isPlaying so we can press play again, we are out of play mode
        isPlaying = false;
    }
    //Increase your bet amount
    public void increaseDenominationValue()
    {
        //Check if your bet amount is less than the Max possible amount
        if (currentDenomination < possibleDenominationValues.Max())
        {
            denominationIndex++;
            currentDenomination = possibleDenominationValues[denominationIndex];
            casinoUI.currentDenominationText.text = "Bet:$ " + currentDenomination.ToString();
            Debug.Log(currentDenomination);
            //Debug.Log("Index is " + denominationIndex);
        }
    }
    //Decrease your bet amount
    public void decreaseDenominationValue()
    {
        //Check if your bet is at least higher than the minimum amount
        if (currentDenomination > possibleDenominationValues.Min())
        {
            denominationIndex--;
            currentDenomination = possibleDenominationValues[denominationIndex];
            casinoUI.currentDenominationText.text = "Bet:$ " + currentDenomination.ToString();
            Debug.Log(currentDenomination);
            //Debug.Log("Index is " + denominationIndex);
        }
    }
    //Call this function to exit the game and close the application
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game");
    }
    public void ReloadGame()
    {
        SceneManager.LoadScene(0);
    }
}
