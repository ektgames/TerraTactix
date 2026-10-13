using System;
using System.Collections;
using System.Collections.Generic;
using TerraTactix.Core;
using UnityEngine;

namespace TerraTactix.Online
{
    public sealed class LeaderboardManager : MonoBehaviour
    {
        ILeaderboardService service;
        bool listening;
        Coroutine sync;
        readonly List<Action<LeaderboardEntry[], bool>> waiting = new List<Action<LeaderboardEntry[], bool>>();

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
                if (done != null)
                    waiting.Add(done);
                if (sync != null)
                    return;
                sync = StartCoroutine(Run(firebase, false));
                return;
            }

            done?.Invoke(GetTop(), true);
        }

        IEnumerator Run(FirebaseLeaderboardService firebase, bool force)
        {
            var fetched = false;
            yield return firebase.Sync((sent, ok, error) => fetched = ok, force);
            sync = null;
            if (waiting.Count == 0)
                yield break;
            var callbacks = waiting.ToArray();
            waiting.Clear();
            var rows = firebase.GetTop();
            for (var i = 0; i < callbacks.Length; i++)
                callbacks[i]?.Invoke(rows, fetched);
        }

        void OnBattleFinished()
        {
            if (service is FirebaseLeaderboardService firebase && sync == null)
                sync = StartCoroutine(Run(firebase, true));
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
                countryCode = PlayerCountry.CurrentCode(),
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
