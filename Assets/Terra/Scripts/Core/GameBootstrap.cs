using TerraTactix.Achievements;
using TerraTactix.Battle;
using TerraTactix.Data;
using TerraTactix.Economy;
using TerraTactix.Missions;
using TerraTactix.Online;
using TerraTactix.UI;
using TerraTactix.Wheel;
using TerraTactix.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerraTactix.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        public SaveManager Save { get; private set; }
        public AudioManager Audio { get; private set; }
        public EconomyManager Economy { get; private set; }
        public RewardService Rewards { get; private set; }
        public ShopManager Shop { get; private set; }
        public ExpeditionManager Expeditions { get; private set; }
        public BattleManager Battles { get; private set; }
        public DailyWheelManager Wheel { get; private set; }
        public MissionManager Missions { get; private set; }
        public AchievementManager Achievements { get; private set; }
        public LeaderboardManager Leaderboard { get; private set; }
        public SceneLoader Scenes { get; private set; }
        public GameStateManager State { get; private set; }
        public GameManager Game { get; private set; }
        public UIManager Ui { get; private set; }
        public BalanceConfiguration Balance { get; private set; }
        public IPlayerService Players { get; private set; }
        public ICloudSaveService Cloud { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (Instance != null)
                return;

            var host = new GameObject("GameBootstrap");
            host.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;

            Balance = BalanceConfiguration.GetOrCreate();
            Players = new MockPlayerService();
            Cloud = new MockCloudSaveService();

            Save = gameObject.AddComponent<SaveManager>();
            Audio = gameObject.AddComponent<AudioManager>();
            Economy = gameObject.AddComponent<EconomyManager>();
            Rewards = gameObject.AddComponent<RewardService>();
            Shop = gameObject.AddComponent<ShopManager>();
            Expeditions = gameObject.AddComponent<ExpeditionManager>();
            Battles = gameObject.AddComponent<BattleManager>();
            Wheel = gameObject.AddComponent<DailyWheelManager>();
            Missions = gameObject.AddComponent<MissionManager>();
            Achievements = gameObject.AddComponent<AchievementManager>();
            Leaderboard = gameObject.AddComponent<LeaderboardManager>();
            Scenes = gameObject.AddComponent<SceneLoader>();
            State = gameObject.AddComponent<GameStateManager>();
            Game = gameObject.AddComponent<GameManager>();
            Ui = gameObject.AddComponent<UIManager>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            gameObject.AddComponent<TerraDebugTools>();
#endif

            Save.LoadOrCreate();
            var firebase = FirebaseLeaderboardConfig.Load();
            Leaderboard.UseService(firebase.IsReady
                ? new FirebaseLeaderboardService(firebase)
                : new MockLeaderboardService());
            Rebind();
            SettleAbandonedBattle();
            Ui.BuildCanvas();
            Ui.PlayStartupIntro();

            var scene = SceneManager.GetActiveScene().name;
            var target = StartScene();
            if (scene == SceneNames.Boot || scene == "SampleScene" || scene != target)
                StartCoroutine(OpenSceneNextFrame(target));
            else
            {
                RememberScene(scene);
                Ui.Show(scene);
            }
        }

        bool leftBattle;

        void OnApplicationPause(bool paused)
        {
            if (Save == null || Save.Data == null)
                return;
            if (paused)
            {
                var battleScene = SceneManager.GetActiveScene().name == SceneNames.Battle;
                if (!Application.isEditor)
                    leftBattle = Battles.AbandonOpenBattle();
                if (leftBattle || (!Application.isEditor && battleScene))
                    Save.Data.resumeScene = SceneNames.WorldMap;
                else
                    RememberScene(SceneManager.GetActiveScene().name);
                Save.Save();
                return;
            }

            if (!leftBattle)
                return;
            leftBattle = false;
            if (SceneManager.GetActiveScene().name == SceneNames.Battle)
                Scenes.Load(SceneNames.WorldMap);
        }

        void OnApplicationQuit()
        {
            if (Save == null || Save.Data == null)
                return;
            var abandoned = Battles.AbandonOpenBattle();
            var scene = SceneManager.GetActiveScene().name;
            if (abandoned || scene == SceneNames.Battle)
                Save.Data.resumeScene = SceneNames.WorldMap;
            else
                RememberScene(scene);
            Save.Save();
        }

        void SettleAbandonedBattle()
        {
            if (!Battles.AbandonOpenBattle())
                return;
            Save.Data.resumeScene = SceneNames.WorldMap;
            Save.Save();
        }

        string StartScene()
        {
            return SceneNames.MainMenu;
        }

        public void RememberScene(string sceneName)
        {
            if (Save == null || Save.Data == null || string.IsNullOrEmpty(sceneName))
                return;
            if (sceneName == SceneNames.Boot || sceneName == "SampleScene" || sceneName == SceneNames.Battle)
                return;
            if (Save.Data.resumeScene == sceneName)
                return;
            Save.Data.resumeScene = sceneName;
            Save.Save();
        }

        System.Collections.IEnumerator OpenSceneNextFrame(string sceneName)
        {
            yield return null;
            if (!Scenes.Load(sceneName))
                Ui.Show(sceneName);
        }

        public void Rebind()
        {
            var data = Save.Data;
            Locale.Bind(data.settings.language);
            Audio.Apply(data.settings);
            Economy.Bind(data.economy);
            Rewards.Bind(Save, Economy);
            Shop.Bind(data.shop);
            Expeditions.Bind(data.world);
            Wheel.Bind(data.wheel);
            Missions.Bind(data.missions);
            Achievements.Bind(data.achievements);
        }

        public void ResetProgress()
        {
            StartCoroutine(ResetRoutine());
        }

        System.Collections.IEnumerator ResetRoutine()
        {
            yield return null;
            Save.ResetProgress();
            Rebind();
            Battles.Clear();
            if (!Scenes.Load(SceneNames.MainMenu))
                Ui.Show(SceneNames.MainMenu);
        }
    }
}
