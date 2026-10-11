using System;
using System.IO;
using TerraTactix.Data;
using UnityEngine;

namespace TerraTactix.Core
{
    public sealed class SaveManager : MonoBehaviour
    {
        const string FileName = "terra-save.json";

        public GameSave Data { get; private set; }

        public void LoadOrCreate()
        {
            var path = SavePath();
            if (!File.Exists(path))
            {
                Data = CreateNew();
                Write(path, Data);
                Debug.Log("[Terra] Yeni kayıt oluşturuldu.");
                return;
            }

            try
            {
                var json = File.ReadAllText(path);
                Data = JsonUtility.FromJson<GameSave>(json);
                if (Data == null)
                    throw new InvalidDataException("SaveManager: kayıt boş.");
                Data.Sanitize();
                EnsureRegions();
                Debug.Log("[Terra] Kayıt yüklendi.");
            }
            catch (Exception exception)
            {
                PreserveCorrupt(path);
                Debug.LogError($"SaveManager: kayıt okunamadı, yedeklendi ve yeni kayıt açıldı. {exception.Message}");
                Data = CreateNew();
                Write(path, Data);
            }
        }

        public void Save()
        {
            if (Data == null)
                return;
            Data.Sanitize();
            Write(SavePath(), Data);
        }

        public void ResetProgress()
        {
            var settings = Data == null ? null : Data.settings;
            Data = CreateNew();
            if (settings != null)
                Data.settings = settings;
            Data.player.nameChosen = false;
            Data.player.playerName = string.Empty;
            Save();
            Debug.Log("[Terra] İlerleme sıfırlandı.");
        }

        public bool TryChooseName(string raw, out string message)
        {
            message = string.Empty;
            if (Data == null || Data.player == null)
                return false;
            if (Data.player.nameChosen)
            {
                message = Locale.T("İsim bir kez yazılır.", "The name is written once.");
                return false;
            }

            var name = PlayerData.CleanName(raw);
            if (name.Length == 0)
            {
                message = Locale.T("Bir isim yaz.", "Enter a name.");
                return false;
            }

            Data.player.playerName = name;
            Data.player.nameChosen = true;
            Save();
            return true;
        }

        public bool HasTransaction(string transactionId)
        {
            return Data != null && Data.rewardTransactionIds.Contains(transactionId);
        }

        public void RememberTransaction(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId) || HasTransaction(transactionId))
                return;
            Data.rewardTransactionIds.Add(transactionId);
        }

        public static GameSave CreateNew()
        {
            var balance = BalanceConfiguration.GetOrCreate();
            var save = new GameSave();
            save.economy.hearts = balance.startingHearts;
            save.economy.maxHearts = balance.startingMaxHearts;
            save.economy.gold = balance.startingGold;
            save.economy.gems = balance.startingGems;
            save.economy.attackSpeed = balance.startingAttackSpeed;
            save.world.regions = RegionCatalog.CreateInitialProgress();
            save.Sanitize();
            return save;
        }

        void EnsureRegions()
        {
            for (var i = 0; i < RegionCatalog.All.Length; i++)
            {
                var def = RegionCatalog.All[i];
                if (FindRegion(def.id) != null)
                    continue;
                Data.world.regions.Add(new RegionProgressEntry
                {
                    regionId = def.id,
                    state = def.startingState,
                    campaignProgress = def.startingState == RegionState.Captured ? def.requiredCampaigns : 0
                });
            }
        }

        RegionProgressEntry FindRegion(string id)
        {
            var regions = Data.world.regions;
            for (var i = 0; i < regions.Count; i++)
            {
                if (regions[i].regionId == id)
                    return regions[i];
            }

            return null;
        }

        static string SavePath()
        {
            return Path.Combine(Application.persistentDataPath, FileName);
        }

        static void Write(string path, GameSave data)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
        }

        static void PreserveCorrupt(string path)
        {
            var backup = path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".bak";
            try
            {
                File.Copy(path, backup, false);
            }
            catch (Exception exception)
            {
                Debug.LogError($"SaveManager: bozuk kayıt yedeklenemedi. {exception.Message}");
            }
        }
    }
}
