using System;
using TerraTactix.Core;
using TerraTactix.Data;
using UnityEngine;

namespace TerraTactix.Battle
{
    public struct BattleResult
    {
        public int score;
        public int gold;
        public bool won;
        public bool captured;
        public int progress;
        public int required;
        public string regionName;
        public int xp;
    }

    public sealed class BattleManager : MonoBehaviour
    {
        public BattleSessionData Session { get; private set; }
        public bool HasFreshResult { get; private set; }
        public BattleResult FreshResult { get; private set; }

        public bool TakeFreshResult(out BattleResult result)
        {
            result = FreshResult;
            var fresh = HasFreshResult;
            HasFreshResult = false;
            return fresh;
        }

        public void Prepare(RegionDefinition region)
        {
            if (region == null)
            {
                Debug.LogError("BattleManager: bölge olmadan savaş hazırlanamaz.");
                Session = null;
                return;
            }

            var progress = GameBootstrap.Instance.Expeditions.GetProgress(region.id);
            Session = new BattleSessionData
            {
                regionId = region.id,
                campaignIndex = progress == null ? 1 : progress.campaignProgress + 1,
                resultId = Guid.NewGuid().ToString("N")
            };
            DifficultyResolver.Apply(Session);
        }

        public bool TrySpendHeart()
        {
            if (Session == null)
            {
                Debug.LogError("BattleManager: BattleSessionData eksik, can harcanamadı.");
                return false;
            }

            if (Session.heartSpent)
                return true;

            var economy = GameBootstrap.Instance.Economy;
            if (!economy.CanPlay())
                return false;

            Session.heartSpent = true;
            ReportedScore = 0;
            WriteActiveBattle();
            if (!economy.ConsumeHeart())
            {
                Session.heartSpent = false;
                ClearActiveBattle();
                GameBootstrap.Instance.Save.Save();
                return false;
            }

            return true;
        }

        public int ReportedScore { get; private set; }

        public void ReportScore(int score)
        {
            ReportedScore = Mathf.Max(0, score);
            var pin = GameBootstrap.Instance.Save.Data.activeBattle;
            if (pin != null && pin.open)
                pin.score = ReportedScore;
        }

        public bool AbandonOpenBattle()
        {
            var pin = GameBootstrap.Instance.Save.Data.activeBattle;
            if ((Session == null || !Session.heartSpent) && pin != null && pin.open)
                RestoreActiveBattle(pin);
            if (Session == null || !Session.heartSpent || Session.resolved)
                return false;

            var score = ReportedScore;
            if (pin != null && pin.score > score)
                score = pin.score;
            Complete(false, score);
            return true;
        }

        public BattleResult Complete(bool won, int score)
        {
            var result = new BattleResult { score = Mathf.Max(0, score), won = won };
            if (Session == null)
            {
                Debug.LogError("BattleManager: BattleSessionData eksik, sonuç yazılamadı.");
                return result;
            }

            if (Session.resolved)
                return result;

            Session.resolved = true;
            var app = GameBootstrap.Instance;
            var player = app.Save.Data.player;
            player.totalScore += result.score;
            if (result.score > player.bestScore)
                player.bestScore = result.score;
            result.xp = GrantXp(player, won ? app.Balance.winXp : app.Balance.defeatXp);

            var definition = RegionCatalog.Get(Session.regionId);
            result.regionName = definition == null ? Session.regionId : definition.displayName;
            result.required = definition == null ? 1 : definition.requiredCampaigns;
            var progress = app.Expeditions.GetProgress(Session.regionId);

            if (won)
            {
                if (app.Rewards.TryApply("battle:" + Session.resultId, RewardType.Gold, Session.goldReward))
                    result.gold += Session.goldReward;

                var wasCaptured = progress != null && progress.state == RegionState.Captured;
                app.Expeditions.RegisterVictory(Session.regionId);
                progress = app.Expeditions.GetProgress(Session.regionId);
                result.captured = progress != null && progress.state == RegionState.Captured && !wasCaptured;
                if (result.captured)
                {
                    var captureGold = app.Balance.captureGoldReward;
                    if (app.Rewards.TryApply("capture:" + Session.regionId, RewardType.Gold, captureGold))
                        result.gold += captureGold;
                }

                GameEvents.RaiseBattleWon();
            }
            else
            {
                app.Expeditions.RegisterDefeat(Session.regionId);
            }

            progress = app.Expeditions.GetProgress(Session.regionId);
            result.progress = progress == null ? 0 : progress.campaignProgress;
            RememberSubmission(app, result.score);
            FreshResult = result;
            HasFreshResult = true;
            ClearActiveBattle();
            GameEvents.RaiseBattleCompleted();
            app.Save.Save();
            return result;
        }

        void WriteActiveBattle()
        {
            var pin = GameBootstrap.Instance.Save.Data.activeBattle ?? new ActiveBattleSave();
            GameBootstrap.Instance.Save.Data.activeBattle = pin;
            pin.open = true;
            pin.regionId = Session.regionId;
            pin.campaignIndex = Session.campaignIndex;
            pin.difficulty = Session.difficulty;
            pin.goldReward = Session.goldReward;
            pin.resultId = Session.resultId;
            pin.score = 0;
        }

        void ClearActiveBattle()
        {
            var pin = GameBootstrap.Instance.Save.Data.activeBattle;
            if (pin == null)
                return;
            pin.open = false;
            pin.score = 0;
        }

        void RestoreActiveBattle(ActiveBattleSave pin)
        {
            Session = new BattleSessionData
            {
                regionId = pin.regionId,
                campaignIndex = pin.campaignIndex,
                difficulty = pin.difficulty,
                goldReward = pin.goldReward,
                resultId = string.IsNullOrEmpty(pin.resultId) ? Guid.NewGuid().ToString("N") : pin.resultId,
                heartSpent = true,
                resolved = false
            };
            ReportedScore = Mathf.Max(0, pin.score);
        }

        void RememberSubmission(GameBootstrap app, int score)
        {
            var duration = Session.startedUnscaled > 0f ? Time.unscaledTime - Session.startedUnscaled : 0f;
            if (duration < 0f)
                duration = 0f;

            var list = app.Save.Data.pendingSubmissions;
            list.Add(new BattleSubmission
            {
                submissionId = Session.resultId,
                playerId = app.Players.PlayerId,
                score = score,
                regionId = Session.regionId,
                campaignIndex = Session.campaignIndex,
                difficulty = Session.difficulty,
                duration = duration,
                timestamp = DateTime.UtcNow.ToString("o"),
                uploaded = false
            });
            while (list.Count > 20)
                list.RemoveAt(0);
        }

        public void Clear()
        {
            Session = null;
            HasFreshResult = false;
        }

        static int GrantXp(PlayerData player, int amount)
        {
            if (player == null || amount <= 0)
                return 0;

            player.xp += amount;
            while (player.xp >= player.xpToNext)
            {
                player.xp -= player.xpToNext;
                player.level++;
                player.xpToNext = 100 + (player.level - 1) * 25;
            }

            return amount;
        }
    }
}
