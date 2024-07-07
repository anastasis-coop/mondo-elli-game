using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MenuController : MonoBehaviour {

    public Island roomType;
    public RoomChannel roomChannel;
    public RoomLevel roomLevel;
    public AreaType areaType;
    public PositionType positionType;
    public int codingTileSet;
    public int codingDifficulty;

    public List<Toggle> areaTypeToggles;
    public List<Toggle> positionTypeToggles;
    public List<Toggle> roomTypeToggles;
    public List<Toggle> roomChannelToggles;
    public List<Toggle> roomLevelToggles;
    public List<Toggle> codingTileSetToggles;
    public List<Toggle> codingDifficultyToggles;

    public UnityEvent onVideo = new UnityEvent();
    public UnityEvent onStart = new UnityEvent();

    public GameObject[] roomColumns;
    public GameObject[] codingColumns;

    void Start() {
        if (GameState.Instance.testMode) {
            areaType = GameState.Instance.levelBackend.areaType;
            positionType = GameState.Instance.levelBackend.positionType;
            roomType = GameState.Instance.levelBackend.island;
            roomChannel = GameState.Instance.levelBackend.roomChannel;
            roomLevel = GameState.Instance.levelBackend.roomLevel;
            codingTileSet = GameState.Instance.levelBackend.CodingTileSetLevel;
            codingDifficulty = GameState.Instance.levelBackend.CodingLevel;
        }

        UpdateToggles(areaTypeToggles, (int)areaType);
        UpdateToggles(positionTypeToggles, (int)positionType);
        UpdateToggles(roomTypeToggles, (int)roomType);
        UpdateToggles(roomChannelToggles, (int)roomChannel);
        UpdateToggles(roomLevelToggles, (int)roomLevel);
        UpdateToggles(codingTileSetToggles, codingTileSet);
        UpdateToggles(codingDifficultyToggles, codingDifficulty);
    }

    public void UpdateColumns(int typeIndex)
    {
        areaType = (AreaType)typeIndex;

        foreach (GameObject column in roomColumns)
            column.SetActive(areaType == AreaType.STANZA);

        foreach (GameObject column in codingColumns)
            column.SetActive(areaType == AreaType.QUARTIERE);
    }

    private void TurnOffToggles(List<Toggle> toggles) {
        foreach (Toggle toggle in toggles) {
            toggle.isOn = false;
        }
    }

    private void UpdateToggles(List<Toggle> toggles, int index) {
        int i = 0;
        foreach (Toggle toggle in toggles) {
            toggle.isOn = (i == index);
            i++;
        }
    }

    private void UpdateVars() {
        areaType = (AreaType)areaTypeToggles.FindIndex(toggle => toggle.isOn);
        positionType = (PositionType)positionTypeToggles.FindIndex(toggle => toggle.isOn);
        roomType = (Island)roomTypeToggles.FindIndex(toggle => toggle.isOn);
        roomChannel = (RoomChannel)roomChannelToggles.FindIndex(toggle => toggle.isOn);
        roomLevel = (RoomLevel)roomLevelToggles.FindIndex(toggle => toggle.isOn);
        codingTileSet = codingTileSetToggles.FindIndex(toggle => toggle.isOn);
        codingDifficulty = codingDifficultyToggles.FindIndex(toggle => toggle.isOn);
    }

    public void VideoButtonPressed() {
        UpdateVars();
        onVideo.Invoke();
    }

    public void StartButtonPressed() {
        UpdateVars();
        onStart.Invoke();
    }
}
