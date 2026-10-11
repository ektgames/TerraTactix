using System;
using System.Collections;
using TerraTactix.Core;
using UnityEngine;

namespace TerraTactix.Online
{
    public sealed class LeaderboardManager : MonoBehaviour
    {
        ILeaderboardService service;
        bool listening;
        Coroutine sync;

        public bool IsLive => service is FirebaseLeaderboardService;

        public void UseService(ILeaderboardService leaderboardService)
        {
            service = leaderboardService;
            if (listening)
                return;
            GameEvents.BattleCompleted += OnBattleFinished;
            listening = true;
        }

        void OnDisable()
        {
            if (!listening)
                return;
            GameEvents.BattleCompleted -= OnBattleFinished;
            listening = false;
        }

        public LeaderboardEntry[] GetTop()
        {
            if (service == null)
            {
                Debug.LogError("LeaderboardManager: sıralama servisi yok.");
                return new LeaderboardEntry[0];
            }

            return service.GetTop();
        }

        public void Refresh(Action<LeaderboardEntry[], bool> done)
        {
            if (service is FirebaseLeaderboardService firebase)
            {
                if (sync != null)
                    StopCoroutine(sync);
                sync = StartCoroutine(Run(firebase, done));
                return;
            }

            done?.Invoke(GetTop(), true);
        }

        IEnumerator Run(FirebaseLeaderboardService firebase, Action<LeaderboardEntry[], bool> done)
        {
            var fetched = false;
            yield return firebase.Sync((sent, ok, error) => fetched = ok);
            sync = null;
            done?.Invoke(firebase.GetTop(), fetched);
        }

        void OnBattleFinished()
        {
            if (service is FirebaseLeaderboardService firebase && sync == null)
                sync = StartCoroutine(Run(firebase, null));
        }

        public LeaderboardEntry LocalRow()
        {
            var app = GameBootstrap.Instance;
            var row = new LeaderboardEntry
            {
                rank = 0,
                playerName = app.Save.Data.player.playerName,
                score = app.Save.Data.player.totalScore,
                regions = app.Expeditions.CapturedCount(),
                isLocal = true,
                isMock = false
            };
            var top = GetTop();
            for (var i = 0; i < top.Length; i++)
            {
                if (!top[i].isLocal)
                    continue;
                row.rank = top[i].rank;
                break;
            }

            return row;
        }
    }
}
