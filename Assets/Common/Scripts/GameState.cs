using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{

    public static GameState Instance
    {
        get; private set;
    }
    
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    //public int navTarget;
    public int pathNumber;

    public int accessorio1;
    public int accessorio2;
    public int accessorio3;

    public int cameraIndex;

    public bool firstLoad = true;
    public AreaType menuAreaSelected = AreaType.QUARTIERE;
    public PositionType currentPositionType = PositionType.INIZIO;
    public bool testMode;
    
    public ComprendoBackend comprendoBackend;
    public LevelBackend levelBackend;

    public bool FirstRoomLoad { get; set; } = true;
    public bool LoadTownAfterRoom { get; set; }
    public AssetReference town;

    public Dictionary<string, bool> restarted;

    // HACK we save session stars here for now
    public int CodingStars;
    public int RoomStars;
    
#if UNITY_EDITOR
    [Header("Editor Flags")]
    [SerializeField]
    public bool forceVideo;

    [SerializeField]
    public bool forceDemo;

    [SerializeField]
    public bool forceMediaLiteracy;
#endif

#if UNITY_EDITOR
    // Checking if we pressed play here
    public static bool StartedFromThisScene { get; private set; } = true;
#endif

    private void Awake()
    {
        // Controllo che l'istanza esista
        if (Instance == null)
        {
            // Se non esiste quella attuale diventa l'unica utilizzabile
            Instance = this;
            // Marco l'oggetto in modo che non venga distrutto al cambio scena
            DontDestroyOnLoad(gameObject);
            // Inizializzo il dizionario per il restart
            ClearRestarted();
        }
        // Se l'istanza esiste già distruggo l'oggetto corrente appena creato
        else
        {
#if UNITY_EDITOR
            StartedFromThisScene = false;
#endif
            Destroy(gameObject);
            return;
        }
        comprendoBackend = ScriptableObject.CreateInstance<ComprendoBackend>();
        levelBackend = ScriptableObject.CreateInstance<LevelBackend>();
    }
    public void ClearRestarted()
    {
        restarted = new Dictionary<string, bool>();
    }

    public void setRestarted()
    {
        string key = levelBackend.roomChannel.ToString() + levelBackend.roomLevel.ToString();
        restarted[key] = true;
    }

    public bool hasBeenRestarted()
    {
        string key = levelBackend.roomChannel.ToString() + levelBackend.roomLevel.ToString();
        if (restarted.ContainsKey(key))
        {
            return restarted[key];
        }
        else
        {
            return false;
        }
    }

    public void LoadSceneAfterRoom()
    {
        SceneManager.LoadScene("RoomStart");
    }

}
