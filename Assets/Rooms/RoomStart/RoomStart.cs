using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using Cutscene;
using UnityEngine.Serialization;

public enum Island
{
    INTRODUZIONE = 0,
    CONTROLLO_INTERFERENZA = 1,
    INIBIZIONE_RISPOSTA = 2,
    MEMORIA_LAVORO = 3,
    FLESSIBILITA_COGNITIVA = 4,
    MEDIA_LITERACY = 5
}

public enum AreaType
{
    QUARTIERE = 0,
    STANZA = 1
}

public enum PositionType
{
    INIZIO,
    INTERMEZZO,
    COMPLETATO,
    POST_MEDIALITERACY,
}

public enum RoomChannel
{
    VISIVO,
    VERBALE,
    MEDIA_LITERACY
}

public enum RoomLevel
{
    LEVEL_01, LEVEL_02, LEVEL_11, LEVEL_12, LEVEL_21, LEVEL_22, LEVEL_31, LEVEL_32
}

public class RoomStart : MonoBehaviour
{
    public LoadingScreen loadingScreen;
    //public MenuController menu;
    public NewMenuController roomsMenu;
    public DemoMenuController demoMenu;
    public CutsceneLoader cutsceneLoader;

    public AssetReference town;

    [SerializeField]
    private LocalizedString completeString;

    [SerializeField]
    private LocalizedString completeMaxLevelString;

    [SerializeField]
    private LocalizedString completeNextLevelString;

    [SerializeField]
    private LocalizedString progressMaxLevelString;

    [SerializeField]
    private LocalizedString progressNextLevelString;

    [SerializeField]
    private LocalizedString failString;

    [SerializeField]
    private LocalizedString roomEnterString;

    [SerializeField]
    private LocalizedString cityEnterString;

    [SerializeField]
    private LocalizedString roomUnavailableString;

    [SerializeField]
    private LocalizedString invalidString;

    [SerializeField]
    private LocalizedString videoUnavailableString;

    [SerializeField]
    private LocalizedString sectionStartString;

    [SerializeField]
    private LocalizedString sectionMidpointString;

    [SerializeField]
    private LocalizedString sectionEndString;

    [SerializeField]
    private ElloCustomization ello;

#if UNITY_EDITOR
    [SerializeField]
    private bool forceShowVideo;
#endif
    
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

    void Start()
    {
        GameState gameState = GameState.Instance;

        foreach (Island island in gameState.levelBackend.completedIslands)
        {
            ello.ToggleHairColor(island, true);
        }

        ello.SetAccessories(gameState.accessorio1, gameState.accessorio2, gameState.accessorio3);

        if (gameState.testMode)
        {
            loadingScreen.gameObject.SetActive(false);
            openRoomsMenu();
            gameState.LoadTownAfterRoom = false;
        }
        else if (gameState.LoadTownAfterRoom)
        {
            gameState.LoadTownAfterRoom = false;
            gameState.FirstRoomLoad = true;
            loadingScreen.gameObject.SetActive(true);
            loadingScreen.LoadIsland(gameState.levelBackend.island);
        }
        else
        {
            // Don't repeat media literacy room
            bool mediaLiteracy = gameState.levelBackend.island == Island.MEDIA_LITERACY
                || gameState.levelBackend.roomChannel == RoomChannel.MEDIA_LITERACY;

            gameState.LoadTownAfterRoom = mediaLiteracy || !gameState.FirstRoomLoad;

            var showScrignoIntro = gameState.FirstRoomLoad && !ElliPrefs.GetRoomFirstEnterFlag(GameState.Instance.comprendoBackend.id.ToString());
            gameState.FirstRoomLoad = false;

            if (showScrignoIntro)
            {
                ElliPrefs.SetRoomFirstEnterFlag(GameState.Instance.comprendoBackend.id.ToString(), true);
                cutsceneLoader.Load("stanza_scrigno", SetTransitionToLevel);
            }
            else SetTransitionToLevel();
        }
    }

    private void ShowExercizeResult()
    {
        string subtitle = "";

        switch (GameState.Instance.levelBackend.lastExercizeResult)
        {
            case EndExercizeSaveResponse.PASSATO:
                subtitle = completeString.GetLocalizedString();
                if (GameState.Instance.levelBackend.roomLevel == RoomLevel.LEVEL_32)
                    subtitle += completeMaxLevelString.GetLocalizedString();
                else
                    subtitle += completeNextLevelString.GetLocalizedString();
                break;
            case EndExercizeSaveResponse.IN_CORSO:
                if (GameState.Instance.levelBackend.roomLevel == RoomLevel.LEVEL_32)
                    subtitle = progressMaxLevelString.GetLocalizedString();
                else
                    subtitle = progressNextLevelString.GetLocalizedString();
                break;
            case EndExercizeSaveResponse.FALLITO:
                subtitle = failString.GetLocalizedString();
                break;
        }

        loadingScreen.ShowMessage(subtitle);
    }

    private void RoomNotReady()
    {
        loadingScreen.gameObject.SetActive(true);
        openRoomsMenu(false);
        loadingScreen.ShowMessage(roomUnavailableString.GetLocalizedString());
        
        Invoke(nameof(GoBackOnError), 1);
    }

    private void InvalidSelection()
    {
        loadingScreen.gameObject.SetActive(true);
        openRoomsMenu(false);

        loadingScreen.ShowMessage(invalidString.GetLocalizedString());
        Invoke(nameof(GoBackOnError), 1);
    }

    private void VideoNotReady()
    {
        loadingScreen.gameObject.SetActive(true);
        openRoomsMenu(false);

        loadingScreen.ShowMessage(videoUnavailableString.GetLocalizedString());
        Invoke(nameof(GoBackOnError), 1);
    }

    private void GoBackOnError()
    {
        if (GameState.Instance.testMode)
        {
            loadingScreen.gameObject.SetActive(false);
            openRoomsMenu();
        }
        else
        {
            LevelBackend backend = GameState.Instance.levelBackend;
            loadingScreen.LoadIsland(backend.island);
        }
    }

    private void openRoomsMenu(bool _open = true)
    {
        if (!_open)
        {
            roomsMenu.gameObject.SetActive(false);
            demoMenu.gameObject.SetActive(false);
            return;
        }
        
        if (DemoModeEnabled)
        {
            roomsMenu.gameObject.SetActive(false);
            demoMenu.gameObject.SetActive(true);
        }
        else
        {
            roomsMenu.gameObject.SetActive(true);
            demoMenu.gameObject.SetActive(false);
        }
    }

    private string GetPositionTypeDescription()
    {
        switch (GameState.Instance.levelBackend.positionType)
        {
            case PositionType.INIZIO:
                return sectionStartString.GetLocalizedString();
            case PositionType.INTERMEZZO:
                return sectionMidpointString.GetLocalizedString();
            case PositionType.COMPLETATO:
                return sectionEndString.GetLocalizedString();
        }
        return "";
    }

    private void SetTransitionToLevel()
    {
        if (GameState.Instance.levelBackend.lastExercizeResult == EndExercizeSaveResponse.NONE)
        {
            RequestCurrentLevel();
        }
        else
        {
            ShowExercizeResult();
            Invoke(nameof(RequestCurrentLevel), 4);
        }
    }

    private void RequestCurrentLevel()
    {
        LevelBackend backend = GameState.Instance.levelBackend;

        GameState.Instance.levelBackend.EnterRoom(res =>
        {
            loadingScreen.gameObject.SetActive(true);
            loadingScreen.LoadLevel(backend.island, backend.roomChannel, backend.roomLevel);
        },
        err => {
            Debug.Log(err.Message);
        });

    }

    public void VideoButtonPressed()
    {
        string id = null;

        if (DemoModeEnabled)
        {
            id = demoMenu.roomType.ToString().ToLower();
            openRoomsMenu(false);

            cutsceneLoader.Load(id, () => {
                openRoomsMenu(true);
            });
        }
        else
        {
            id = roomsMenu.roomType.ToString().ToLower();
            openRoomsMenu(false);

            cutsceneLoader.Load(id, () => {
                openRoomsMenu(true);
            });
        }
    }

    public void StartButtonPressed()
    {
        GameState gameState = GameState.Instance;
        
        if (DemoModeEnabled)
        {
            gameState.levelBackend.areaType = demoMenu.areaType;
            gameState.levelBackend.island = demoMenu.roomType;
            gameState.levelBackend.roomChannel = demoMenu.roomChannel;
            gameState.levelBackend.roomLevel = demoMenu.roomLevel;
            gameState.levelBackend.CodingTileSetLevel = demoMenu.codingTileSet;
            gameState.levelBackend.CodingLevel = demoMenu.codingDifficulty;
            gameState.levelBackend.areaCompleted = (demoMenu.positionType == PositionType.COMPLETATO);
            gameState.firstLoad = (demoMenu.positionType == PositionType.INIZIO);
            gameState.pathNumber = (int)demoMenu.positionType;
            gameState.menuAreaSelected = demoMenu.areaType;
            
            if (demoMenu.roomType == Island.MEDIA_LITERACY
                || demoMenu.roomChannel == RoomChannel.MEDIA_LITERACY
                || demoMenu.positionType == PositionType.POST_MEDIALITERACY
#if UNITY_EDITOR
                || gameState.forceMediaLiteracy
#endif
               )
            {
                gameState.levelBackend.MediaLiteracyEnabled = true;
            }
            
            openRoomsMenu(false);
            loadingScreen.gameObject.SetActive(true);

            LevelBackend backend = gameState.levelBackend;
            Island island = backend.island;

            if (demoMenu.areaType == AreaType.STANZA)
            {
                RoomLevel level = backend.roomLevel;
                RoomChannel channel = backend.roomChannel;

                loadingScreen.LoadLevel(island, channel, level);
            }
            else
            {
                gameState.currentPositionType = demoMenu.positionType;

                loadingScreen.LoadIsland(island);
            }
        }
        else //ROOMS
        {
            if (roomsMenu.areaType == AreaType.STANZA && roomsMenu.roomType == Island.INTRODUZIONE)
            {
                InvalidSelection();
                return;
            }
            
            gameState.levelBackend.areaType = roomsMenu.areaType;
            gameState.levelBackend.island = roomsMenu.roomType;
            gameState.levelBackend.roomChannel = roomsMenu.roomChannel;
            gameState.levelBackend.roomLevel = roomsMenu.roomLevel;
            gameState.levelBackend.CodingTileSetLevel = roomsMenu.codingTileSet;
            gameState.levelBackend.CodingLevel = roomsMenu.codingDifficulty;
            gameState.levelBackend.areaCompleted = (roomsMenu.positionType == PositionType.COMPLETATO);
            gameState.firstLoad = (roomsMenu.positionType == PositionType.INIZIO);
            gameState.pathNumber = (int)roomsMenu.positionType;
            gameState.menuAreaSelected = roomsMenu.areaType;

            if (roomsMenu.roomType == Island.MEDIA_LITERACY
                || roomsMenu.roomChannel == RoomChannel.MEDIA_LITERACY
                || roomsMenu.positionType == PositionType.POST_MEDIALITERACY
#if UNITY_EDITOR
                || gameState.forceMediaLiteracy
#endif
               )
            {
                gameState.levelBackend.MediaLiteracyEnabled = true;
            }
            
            openRoomsMenu(false);
            loadingScreen.gameObject.SetActive(true);

            LevelBackend backend = gameState.levelBackend;
            Island island = backend.island;

            if (roomsMenu.areaType == AreaType.STANZA)
            {
                RoomLevel level = backend.roomLevel;
                RoomChannel channel = backend.roomChannel;

                loadingScreen.LoadLevel(island, channel, level);
            }
            else
            {
                gameState.currentPositionType = roomsMenu.positionType;

                loadingScreen.LoadIsland(island);
            }
        }
    }

}
