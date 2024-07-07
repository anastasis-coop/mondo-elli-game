using System;
using System.Collections;
using System.Collections.Generic;
using Start;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DemoMenuController : MonoBehaviour
{
    [NonSerialized]
    public Island roomType = Island.CONTROLLO_INTERFERENZA;
    [NonSerialized]
    public RoomChannel roomChannel = RoomChannel.VISIVO;
    [NonSerialized]
    public RoomLevel roomLevel = RoomLevel.LEVEL_11;
    [NonSerialized]
    public AreaType areaType = AreaType.QUARTIERE;
    [NonSerialized]
    public PositionType positionType = PositionType.INIZIO;
    [NonSerialized]
    public int codingTileSet;
    [NonSerialized]
    public int codingDifficulty;
    
    public TextMeshProUGUI title;
    public GameObject videoButton;
    public GameObject playButton;
    public Button playButtonComponent;
    public Image quartiereImage;
    public Image powerImage;
    public List<Sprite> powerImages;
    public List<Sprite> quartiereImages;

    [Header("Funzioni Esecutive")]
    public ToggleGroup quartiereToggleGroup;
    public List<Toggle> quartiereToggles;

    [Header("Content")] public GameObject contentPanel;
    
    [Header("Attivita'")]
    public GameObject attivitaPanel;
    public ToggleGroup attivitaToggleGroup;
    public List<Toggle> attivitaToggles;
    
    [Header("Difficolta'")]
    public GameObject difficoltaPanel;
    public ToggleGroup difficoltaToggleGroup;
    public List<Toggle> difficoltaToggles;
    
    [Header("Difficolta'")]
    public GameObject livelloPanel;
    public ToggleGroup livelloToggleGroup;
    public List<Toggle> livelloToggles;
    
    [Header("Blank Content")] 
    public GameObject blankContentPanel;
    public Image blankContentQuartiereImage;
    
    [Header("Events")]
    public UnityEvent onVideo = new UnityEvent();
    public UnityEvent onStart = new UnityEvent();

    public bool DemoModeEnabled
    {
        get
        {
#if UNITY_EDITOR
            if (GameState.Instance.forceDemo) return true;
#endif
            return GameState.Instance.levelBackend.Demo;
        }
    }

    public bool MediaLiteracyEnabled
    {
        get
        {
#if UNITY_EDITOR
            if (GameState.Instance.forceMediaLiteracy || GameState.StartedFromThisScene)
                return true;
#endif
            return GameState.Instance.levelBackend.MediaLiteracyEnabled;
        }
    }

    void Start()
    {
        if (GameState.Instance.testMode) {
            areaType = GameState.Instance.levelBackend.areaType;
            positionType = GameState.Instance.levelBackend.positionType;
            roomType = GameState.Instance.levelBackend.island;
            roomChannel = GameState.Instance.levelBackend.roomChannel;
            roomLevel = GameState.Instance.levelBackend.roomLevel;
            codingTileSet = GameState.Instance.levelBackend.CodingTileSetLevel;
            codingDifficulty = GameState.Instance.levelBackend.CodingLevel;
        }

        //DEMO MODE
        if (DemoModeEnabled)
        {
            areaType = AreaType.QUARTIERE;
            positionType = PositionType.INIZIO;
            roomType = Island.INTRODUZIONE;
            roomChannel = RoomChannel.VISIVO;
            roomLevel = RoomLevel.LEVEL_11;
            codingTileSet = 0;
            codingDifficulty = 0;
        }
    }
    
    public void OnQuartiereToggleChanged(bool _b)
    {
        if (_b)
        {
            UpdateIsleAndPowerImages();
            //accendere o spegnere GIOCA
            int selectedQuartiereIndex = quartiereToggles.IndexOf(quartiereToggleGroup.GetFirstActiveToggle());

            switch (selectedQuartiereIndex)
            {
                case 0://intro
                    playButtonComponent.interactable = false;
                    contentPanel.SetActive(false);
                    blankContentPanel.SetActive(true);
                    break;
                case 1://interferenza
                    playButtonComponent.interactable = true;
                    contentPanel.SetActive(true);
                    blankContentPanel.SetActive(false);
                    break;
                case 2://inibizione
                    playButtonComponent.interactable = false;
                    contentPanel.SetActive(false);
                    blankContentPanel.SetActive(true);
                    break;
                case 3://memoria
                    playButtonComponent.interactable = false;
                    contentPanel.SetActive(false);
                    blankContentPanel.SetActive(true);
                    break;
                case 4://flessibilita'
                    playButtonComponent.interactable = false;
                    contentPanel.SetActive(false);
                    blankContentPanel.SetActive(true);
                    break;
                case 5://media literacy
                    playButtonComponent.interactable = false;
                    contentPanel.SetActive(false);
                    blankContentPanel.SetActive(true);
                    break;
            }
        }
            
    }

    public void OnAttivitaToggleChanged(bool _b)
    {
        if (!_b) return;
        
        int selectedAttivita = attivitaToggles.IndexOf(attivitaToggleGroup.GetFirstActiveToggle());
        switch (selectedAttivita)
        {
            case 0:
                difficoltaPanel.SetActive(true);
                livelloPanel.SetActive(false);
                break;
            case 1:
                difficoltaPanel.SetActive(false);
                livelloPanel.SetActive(true);
                break;
            case 2:
                difficoltaPanel.SetActive(false);
                livelloPanel.SetActive(true);
                break;
        }
    }
    
    public void VideoButtonPressed() 
    {
        UpdateVars();
        onVideo.Invoke();
    }

    public void StartButtonPressed() 
    {   
        UpdateVars();
        onStart.Invoke();
    }

    private void UpdateVars()
    {
        int selectedActivity = attivitaToggles.IndexOf(attivitaToggleGroup.GetFirstActiveToggle());
        
        //quartiere o stanza scrigno
        areaType = selectedActivity == 0
            ? AreaType.QUARTIERE
            : AreaType.STANZA;

        //posizione della sessione
        positionType = PositionType.INIZIO;
        
        //funzione esecutiva
        roomType = (Island)quartiereToggles.IndexOf(quartiereToggleGroup.GetFirstActiveToggle());
        
        //canale
        switch (selectedActivity)
        {
            case 0:
                roomChannel = RoomChannel.VISIVO; //indifferente
                break;
            case 1:
                roomChannel = RoomChannel.VISIVO;
                break;
            case 2:
                roomChannel = RoomChannel.VERBALE;
                break;
            case 3:
                roomChannel = RoomChannel.MEDIA_LITERACY;
                break;
        }
        
        //livello
        roomLevel = (RoomLevel)livelloToggles.IndexOf(livelloToggleGroup.GetFirstActiveToggle());

        //difficolta' coding
        int selectedCodingDiff = difficoltaToggles.IndexOf(difficoltaToggleGroup.GetFirstActiveToggle());
        switch (selectedCodingDiff)
        {
            case 0:
                codingDifficulty = 1;
                codingTileSet = 0;
                break;
            case 1:
                codingDifficulty = 2;
                codingTileSet = 3;
                break;
            case 2:
                codingDifficulty = 3;
                codingTileSet = 5;
                break;
        }
    }

    #region UIUpdate
    
    private void UpdateIsleAndPowerImages()
    {
        quartiereImage.sprite = quartiereImages[quartiereToggles.IndexOf(quartiereToggleGroup.GetFirstActiveToggle())];
        powerImage.sprite = powerImages[quartiereToggles.IndexOf(quartiereToggleGroup.GetFirstActiveToggle())];
        
        blankContentQuartiereImage.sprite = quartiereImages[quartiereToggles.IndexOf(quartiereToggleGroup.GetFirstActiveToggle())];
    }
    
    private void UpdateToggles(List<Toggle> toggles, int index) {
        int i = 0;
        foreach (Toggle toggle in toggles) {
            toggle.isOn = (i == index);
            i++;
        }
    }
    #endregion
}
