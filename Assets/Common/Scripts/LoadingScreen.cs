using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("Widgets")]

    [SerializeField]
    private TextMeshProUGUI titleLabel;

    [SerializeField]
    private TextMeshProUGUI subtitleLabel;

    [SerializeField]
    private TextMeshProUGUI loadingLabel;

    [SerializeField]
    private Image[] framedImages;

    [SerializeField]
    private Image backgroundImage;

    [System.Serializable]
    public class IslandConfig
    {
        public Island Island;

        [SerializeField]
        private LocalizedString islandName;
        public string IslandName => islandName.GetLocalizedString();

        public Sprite Sprite;
        public Color IslandColor;
    }


    [System.Serializable]
    public class LevelConfig
    {
        public Island Island;
        public RoomChannel Channel;
        public RoomLevel Level;

        [SerializeField]
        public LocalizedString channelName;
        public string ChannelName => channelName.GetLocalizedString();

        [SerializeField]
        public LocalizedString levelName;
        public string LevelName => levelName.GetLocalizedString();

        public Sprite Sprite;
        public AssetReference Scene;
    }

    [Header("Configs")]

    [SerializeField]
    public IslandConfig[] islandConfigs;

    [SerializeField]
    public AssetReference TownScene;

    [SerializeField]
    public LevelConfig[] levelConfigs;

    public void LoadIsland(Island island)
    {
        IslandConfig islandConfig = Array.Find(islandConfigs, cfg => cfg.Island == island);

        Setup(islandConfig.IslandName, "", islandConfig.Sprite, islandConfig.IslandColor);

        StartCoroutine(LoadRoutine(TownScene));
    }

    public void LoadLevel(Island island, RoomChannel channel, RoomLevel level)
    {
        IslandConfig islandConfig = Array.Find(islandConfigs, cfg => cfg.Island == island);

        LevelConfig levelConfig;
        string title, subtitle;

        if (island is not Island.MEDIA_LITERACY)
        {
            levelConfig = Array.Find(levelConfigs,
                cfg => cfg.Island == island &&
                cfg.Channel == channel &&
                cfg.Level == level);

            title = $"{islandConfig.IslandName} - {levelConfig.ChannelName}";
            subtitle = levelConfig.LevelName;
        }
        else
        {
            levelConfig = Array.Find(levelConfigs, cfg => cfg.Island == island);
            title = $"{islandConfig.IslandName}";
            subtitle = string.Empty;
        }

        Setup(title, subtitle, levelConfig.Sprite, islandConfig.IslandColor);

        if (GameState.Instance.testMode)
        {
            GameState.Instance.ClearRestarted();
        }

        StartCoroutine(LoadRoutine(levelConfig.Scene));

    }

    public void ShowMessage(string message)
    {
        //TODO
    }

    private void Setup(string title, string subtitle, Sprite sprite, Color background)
    {
        titleLabel.text = title;
        subtitleLabel.text = subtitle;

        foreach (Image image in framedImages)
        {
            image.sprite = sprite;
        }

        backgroundImage.color = background;
    }

    private IEnumerator LoadRoutine(AssetReference scene)
    {
        loadingLabel.gameObject.SetActive(true);

        if (!LocalizationSettings.InitializationOperation.IsDone)
            yield return LocalizationSettings.InitializationOperation;

        yield return new WaitForSecondsRealtime(2);
        yield return scene.LoadSceneAsync();

        loadingLabel.gameObject.SetActive(false);
    }
}
