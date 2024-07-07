using System;
using System.Collections;
using System.Collections.Generic;
using Start;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NewMenuController : MonoBehaviour
{
    [NonSerialized]
    public Island roomType = Island.INIBIZIONE_RISPOSTA;
    [NonSerialized]
    public RoomChannel roomChannel = RoomChannel.VISIVO;
    [NonSerialized]
    public RoomLevel roomLevel = RoomLevel.LEVEL_01;
    [NonSerialized]
    public AreaType areaType = AreaType.QUARTIERE;
    [NonSerialized]
    public PositionType positionType = PositionType.INIZIO;
    [NonSerialized]
    public int codingTileSet;
    [NonSerialized]
    public int codingDifficulty;
    
    public TextMeshProUGUI title;
    public GameObject demoHintMessage;

    [Header("Area Buttons")] 
    public ToggleGroup areaTypeToggleGroup;
    public List<Toggle> areaTypeToggles;

    [Header("Common")]
    public ToggleGroup quartiereToggleGroup;
    public List<Toggle> quartiereToggles;
    
    [Header("Quartieri")]
    public GameObject quartieriPanel;
    public List<Sprite> quartiereImages;
    public ToggleGroup faseToggleGroup;
    public List<Toggle> faseToggles;
    public ToggleGroup difficoltaToggleGroup;
    public List<Toggle> difficoltaToggles;
    public ToggleGroup tessereExtraToggleGroup;
    public List<Toggle> tessereExtraToggles;
    public Image quartiereImage;
    
    [Header("Stanze")]
    public GameObject stanzePanel;
    public ToggleGroup canaleToggleGroup;
    public List<Toggle> canaleToggles;
    public ToggleGroup livelloToggleGroup;
    public List<Toggle> livelloToggles;
    public Image powerImage;
    public List<Sprite> powerImages;
    
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
            //// Menu was accessed with "rooms" cheat
            //if (GameState.Instance.testMode)
            //    return true;

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

            //Disable Quartieri
            quartiereToggles[2].interactable = false;
            quartiereToggleGroup.UnregisterToggle(quartiereToggles[2]);
            quartiereToggles[3].interactable = false;
            quartiereToggleGroup.UnregisterToggle(quartiereToggles[3]);
            quartiereToggles[4].interactable = false;
            quartiereToggleGroup.UnregisterToggle(quartiereToggles[4]);
            quartiereToggles[5].interactable = false;
            quartiereToggleGroup.UnregisterToggle(quartiereToggles[5]);
            
            //Disable levels
            livelloToggles[0].interactable = false;
            livelloToggleGroup.UnregisterToggle(livelloToggles[0]);
            livelloToggles[1].interactable = false;
            livelloToggleGroup.UnregisterToggle(livelloToggles[1]);
            livelloToggles[4].interactable = false;
            livelloToggleGroup.UnregisterToggle(livelloToggles[4]);
            livelloToggles[5].interactable = false;
            livelloToggleGroup.UnregisterToggle(livelloToggles[5]);
            livelloToggles[6].interactable = false;
            livelloToggleGroup.UnregisterToggle(livelloToggles[6]);
            livelloToggles[7].interactable = false;
            livelloToggleGroup.UnregisterToggle(livelloToggles[7]);
            
            //Disable Canale
            canaleToggles[2].interactable = false;
            canaleToggleGroup.UnregisterToggle(canaleToggles[2]);
            
            //Disable Fase
            faseToggles[3].interactable = false;
            faseToggleGroup.UnregisterToggle(faseToggles[3]);
            
            demoHintMessage.SetActive(true);
        }

        //DISABLE ML
        if (!MediaLiteracyEnabled)
        {
            //Disable Quartiere
            quartiereToggles[5].interactable = false;
            quartiereToggleGroup.UnregisterToggle(quartiereToggles[5]);
            
            //Disable Fase
            faseToggles[3].interactable = false;
            faseToggleGroup.UnregisterToggle(faseToggles[3]);
            
            //Disable Canale
            canaleToggles[2].interactable = false;
            canaleToggleGroup.UnregisterToggle(canaleToggles[2]);
        }
        
        UpdateToggles(areaTypeToggles, (int)areaType);
        UpdateToggles(faseToggles, (int)positionType);
        UpdateToggles(quartiereToggles, (int)roomType);
        UpdateToggles(canaleToggles, (int)roomChannel);
        UpdateToggles(livelloToggles, (int)roomLevel);
        UpdateToggles(tessereExtraToggles, codingTileSet);
        UpdateToggles(difficoltaToggles, codingDifficulty);
    }
    public void OnAreaButtonPressed(bool _b)
    {
        if (_b)
        {
            updateMainContentPanel((AreaType)areaTypeToggles.FindIndex(toggle => toggle.isOn));   
        }
    }

    public void OnQuartiereToggleChanged(bool _b)
    {
        if(_b)
            UpdateIsleImages();
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
        areaType = (AreaType)areaTypeToggles.FindIndex(toggle => toggle.isOn);
        positionType = (PositionType)faseToggles.FindIndex(toggle => toggle.isOn);
        roomType = (Island)quartiereToggles.FindIndex(toggle => toggle.isOn);
        roomChannel = (RoomChannel)canaleToggles.FindIndex(toggle => toggle.isOn);
        roomLevel = (RoomLevel)livelloToggles.FindIndex(toggle => toggle.isOn);
        codingTileSet = tessereExtraToggles.FindIndex(toggle => toggle.isOn);
        codingDifficulty = difficoltaToggles.FindIndex(toggle => toggle.isOn);
    }

    #region UIUpdate
    
    private void updateMainContentPanel(AreaType _area)
    {
        quartieriPanel.SetActive(_area == AreaType.QUARTIERE);
        stanzePanel.SetActive(_area == AreaType.STANZA);

        switch (_area)
        {
            case AreaType.QUARTIERE:
                quartiereToggles[0].gameObject.SetActive(true);
                quartiereToggleGroup.RegisterToggle(quartiereToggles[0]);
                break;
            case AreaType.STANZA:
                if (quartiereToggles[0].isOn) quartiereToggles[1].isOn = true;
                quartiereToggles[0].gameObject.SetActive(false);
                quartiereToggleGroup.UnregisterToggle(quartiereToggles[0]);
                break;
            default:
                break;
        }
    }

    private void UpdateIsleImages()
    {
        quartiereImage.sprite = quartiereImages[quartiereToggles.FindIndex(toggle => toggle.isOn)];
        powerImage.sprite = powerImages[quartiereToggles.FindIndex(toggle => toggle.isOn)];
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
