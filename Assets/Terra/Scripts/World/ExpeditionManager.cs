using System.Collections.Generic;
using TerraTactix.Core;
using TerraTactix.Data;
using UnityEngine;

namespace TerraTactix.World
{
    public sealed class ExpeditionManager : MonoBehaviour
    {
        WorldProgressData world;
        readonly List<string> justUnlocked = new List<string>();

        public List<string> TakeJustUnlocked()
        {
            var copy = new List<string>(justUnlocked);
            justUnlocked.Clear();
            return copy;
        }

        public void Bind(WorldProgressData worldProgress)
        {
            world = worldProgress;
            if (world == null || world.regions == null)
                return;

            var opened = false;
            for (var i = 0; i < RegionCatalog.All.Length; i++)
            {
                var definition = RegionCatalog.All[i];
                var progress = GetProgress(definition.id);
                if (progress == null)
                    continue;
                // Harita rota secimini oyuncuya birakir; eski kayitlardaki kilitli
                // adalar da veri kaybi olmadan acik duruma tasinir.
                if (progress.state == RegionState.Locked)
                {
                    progress.state = RegionState.Available;
                    opened = true;
                }
                if (progress.state == RegionState.Captured && progress.campaignProgress < definition.requiredCampaigns)
                {
                    progress.campaignProgress = definition.requiredCampaigns;
                    opened = true;
                }

                if (progress.state == RegionState.Captured && UnlockNext(definition))
                    opened = true;
            }

            if (opened)
                GameBootstrap.Instance.Save.Save();
        }

        public RegionProgressEntry GetProgress(string regionId)
        {
            var regions = world.regions;
            for (var i = 0; i < regions.Count; i++)
            {
                if (regions[i].regionId == regionId)
                    return regions[i];
            }

            return null;
        }

        public RegionDefinition NextPlayable()
        {
            for (var i = 0; i < RegionCatalog.All.Length; i++)
            {
                var progress = GetProgress(RegionCatalog.All[i].id);
                if (progress == null)
                    continue;
                if (progress.state == RegionState.Available || progress.state == RegionState.PartiallyCaptured || progress.state == RegionState.Attacking)
                    return RegionCatalog.All[i];
            }

            return null;
        }

        public int CapturedCount()
        {
            var count = 0;
            for (var i = 0; i < world.regions.Count; i++)
            {
                if (world.regions[i].state == RegionState.Captured)
                    count++;
            }

            return count;
        }

        public void RegisterVictory(string regionId)
        {
            var progress = GetProgress(regionId);
            var definition = RegionCatalog.Get(regionId);
            if (progress == null || definition == null)
            {
                Debug.LogError($"ExpeditionManager: bölge bulunamadı ({regionId}).");
                return;
            }

            if (progress.state == RegionState.Captured)
                return;

            progress.campaignProgress++;
            progress.state = progress.campaignProgress >= definition.requiredCampaigns
                ? RegionState.Captured
                : RegionState.PartiallyCaptured;

            Debug.Log($"[Terra] Sefer {progress.campaignProgress}/{definition.requiredCampaigns} tamamlandı.");

            if (progress.state == RegionState.Captured)
            {
                UnlockNext(definition);
                GameEvents.RaiseRegionCaptured(regionId);
            }

            GameBootstrap.Instance.Save.Save();
        }

        public void RegisterDefeat(string regionId)
        {
            Debug.Log($"[Terra] Sefer başarısız, ilerleme artmadı ({regionId}).");
        }

        bool UnlockNext(RegionDefinition definition)
        {
            var next = definition.nextRegionIds;
            if (next == null)
                return false;

            var opened = false;
            for (var i = 0; i < next.Length; i++)
            {
                var progress = GetProgress(next[i]);
                if (progress == null || progress.state != RegionState.Locked)
                    continue;
                progress.state = RegionState.Available;
                justUnlocked.Add(next[i]);
                opened = true;
            }

            return opened;
        }
    }
}
