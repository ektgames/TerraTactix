using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TerraTactix.Core;
using TerraTactix.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace TerraTactix.Online
{
    [Serializable]
    public sealed class FirebaseLeaderboardConfig
    {
        public string databaseUrl = string.Empty;

        public bool IsReady
        {
            get
            {
                var url = Normalized();
                return url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    && (url.IndexOf(".firebaseio.com", StringComparison.OrdinalIgnoreCase) >= 0
                        || url.IndexOf(".firebasedatabase.app", StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        public string Normalized()
        {
            if (string.IsNullOrWhiteSpace(databaseUrl))
                return string.Empty;
            var url = databaseUrl.Trim().TrimEnd('/');
            if (url.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                url = url.Substring(0, url.Length - 5);
            return url;
        }

        public static FirebaseLeaderboardConfig Load()
        {
            var asset = Resources.Load<TextAsset>("firebase-leaderboard");
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
                return new FirebaseLeaderboardConfig();
            var config = JsonUtility.FromJson<FirebaseLeaderboardConfig>(asset.text);
            return config ?? new FirebaseLeaderboardConfig();
        }
    }

    public sealed class FirebaseLeaderboardService : ILeaderboardService
    {
        readonly string root;
        LeaderboardEntry[] cache = new LeaderboardEntry[0];

        public FirebaseLeaderboardService(FirebaseLeaderboardConfig config)
        {
            root = config == null ? string.Empty : config.Normalized();
        }

        public bool IsLive => true;

        public LeaderboardEntry[] GetTop()
        {
            return cache;
        }

        public IEnumerator Sync(Action<bool, bool, string> done)
        {
            string error = null;
            yield return ReadTop(null, message => error = message);
            if (OwnsFrozenName())
                StartFreshIdentity();

            var sent = false;
            yield return WriteLocal(ok => sent = ok);
            var fetched = false;
            yield return ReadTop(ok => fetched = ok, message => error = message);
            if (done != null)
                done(sent, fetched, error);
        }

        bool OwnsFrozenName()
        {
            var player = CurrentPlayer();
            if (player == null || !player.nameChosen || cache == null)
                return false;

            for (var i = 0; i < cache.Length; i++)
            {
                var row = cache[i];
                if (row == null || !row.isLocal)
                    continue;
                return !string.Equals(row.playerName, player.playerName, StringComparison.Ordinal);
            }

            return false;
        }

        static void StartFreshIdentity()
        {
            var app = GameBootstrap.Instance;
            var player = app == null || app.Save == null || app.Save.Data == null ? null : app.Save.Data.player;
            if (player == null)
                return;

            player.playerId = Guid.NewGuid().ToString("N");
            app.Save.Save();
        }

        static PlayerData CurrentPlayer()
        {
            var app = GameBootstrap.Instance;
            if (app == null || app.Save == null || app.Save.Data == null)
                return null;
            return app.Save.Data.player;
        }

        IEnumerator WriteLocal(Action<bool> done)
        {
            var app = GameBootstrap.Instance;
            var player = app == null || app.Save == null || app.Save.Data == null ? null : app.Save.Data.player;
            if (player == null || !player.nameChosen || string.IsNullOrEmpty(player.playerId))
            {
                done?.Invoke(false);
                yield break;
            }

            var regions = app.Expeditions == null ? 0 : app.Expeditions.CapturedCount();
            var score = Mathf.Clamp(player.totalScore, 0, 999999999);
            regions = Mathf.Clamp(regions, 0, 100);
            var body = "{\"name\":" + Quote(player.playerName)
                + ",\"score\":" + score
                + ",\"regions\":" + regions
                + ",\"updated\":" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "}";
            var url = root + "/players/" + player.playerId + ".json";
            using (var request = UnityWebRequest.Put(url, body))
            {
                request.timeout = 12;
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                var ok = request.result == UnityWebRequest.Result.Success;
                if (ok)
                    MarkUploaded(app);
                else
                    Debug.LogWarning("Leaderboard: kayıt yazılamadı. " + request.error);
                done?.Invoke(ok);
            }
        }

        IEnumerator ReadTop(Action<bool> done, Action<string> error)
        {
            var url = root + "/players.json?orderBy=%22score%22&limitToLast=100";
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 12;
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("Leaderboard: liste alınamadı. " + request.error);
                    error?.Invoke(request.error);
                    done?.Invoke(false);
                    yield break;
                }

                cache = Parse(request.downloadHandler.text, CurrentId());
                done?.Invoke(true);
            }
        }

        static void MarkUploaded(GameBootstrap app)
        {
            var list = app.Save.Data.pendingSubmissions;
            if (list == null)
                return;
            var changed = false;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] == null || list[i].uploaded)
                    continue;
                list[i].uploaded = true;
                changed = true;
            }

            if (changed)
                app.Save.Save();
        }

        static string CurrentId()
        {
            var app = GameBootstrap.Instance;
            if (app == null || app.Save == null || app.Save.Data == null || app.Save.Data.player == null)
                return string.Empty;
            return app.Save.Data.player.playerId ?? string.Empty;
        }

        public static LeaderboardEntry[] Parse(string json, string localId)
        {
            var rows = new List<LeaderboardEntry>();
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
                return rows.ToArray();

            var i = 0;
            Skip(json, ref i);
            if (i >= json.Length || json[i] != '{')
                return rows.ToArray();
            i++;
            while (i < json.Length)
            {
                Skip(json, ref i);
                if (i >= json.Length || json[i] == '}')
                    break;
                if (json[i] == ',')
                {
                    i++;
                    continue;
                }

                var id = ReadString(json, ref i);
                Skip(json, ref i);
                if (i < json.Length && json[i] == ':')
                    i++;
                Skip(json, ref i);
                var body = ReadObject(json, ref i);
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(body))
                    continue;
                if (!TryFields(body, out var name, out var score, out var regions))
                    continue;
                rows.Add(new LeaderboardEntry
                {
                    playerName = name,
                    score = score,
                    regions = regions,
                    isLocal = id == localId,
                    isMock = false
                });
            }

            rows.Sort((a, b) =>
            {
                var byScore = b.score.CompareTo(a.score);
                return byScore != 0 ? byScore : string.Compare(a.playerName, b.playerName, StringComparison.Ordinal);
            });
            if (rows.Count > 100)
                rows.RemoveRange(100, rows.Count - 100);
            for (var n = 0; n < rows.Count; n++)
                rows[n].rank = n + 1;
            return rows.ToArray();
        }

        static bool TryFields(string json, out string name, out int score, out int regions)
        {
            name = ReadNamedString(json, "name");
            score = ReadNamedInt(json, "score");
            regions = ReadNamedInt(json, "regions");
            if (string.IsNullOrEmpty(name))
                name = "—";
            if (name.Length > PlayerData.MaxNameLength)
                name = name.Substring(0, PlayerData.MaxNameLength);
            score = Mathf.Clamp(score, 0, 999999999);
            regions = Mathf.Clamp(regions, 0, 100);
            return json.Contains("\"score\"");
        }

        static string ReadNamedString(string json, string field)
        {
            var token = "\"" + field + "\"";
            var at = json.IndexOf(token, StringComparison.Ordinal);
            if (at < 0)
                return string.Empty;
            var i = at + token.Length;
            Skip(json, ref i);
            if (i >= json.Length || json[i] != ':')
                return string.Empty;
            i++;
            return ReadString(json, ref i);
        }

        static int ReadNamedInt(string json, string field)
        {
            var token = "\"" + field + "\"";
            var at = json.IndexOf(token, StringComparison.Ordinal);
            if (at < 0)
                return 0;
            var i = at + token.Length;
            Skip(json, ref i);
            if (i >= json.Length || json[i] != ':')
                return 0;
            i++;
            Skip(json, ref i);
            var start = i;
            while (i < json.Length && (char.IsDigit(json[i]) || json[i] == '-'))
                i++;
            if (start == i || !int.TryParse(json.Substring(start, i - start), out var value))
                return 0;
            return value;
        }

        static string ReadObject(string json, ref int i)
        {
            if (i >= json.Length || json[i] != '{')
                return string.Empty;
            var start = i;
            var depth = 0;
            var text = false;
            var escape = false;
            for (; i < json.Length; i++)
            {
                var c = json[i];
                if (text)
                {
                    if (escape)
                        escape = false;
                    else if (c == '\\')
                        escape = true;
                    else if (c == '"')
                        text = false;
                    continue;
                }

                if (c == '"')
                    text = true;
                else if (c == '{')
                    depth++;
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        i++;
                        return json.Substring(start, i - start);
                    }
                }
            }

            return string.Empty;
        }

        static string ReadString(string json, ref int i)
        {
            Skip(json, ref i);
            if (i >= json.Length || json[i] != '"')
                return string.Empty;
            i++;
            var builder = new StringBuilder();
            while (i < json.Length)
            {
                var c = json[i++];
                if (c == '\\' && i < json.Length)
                {
                    var next = json[i++];
                    builder.Append(next == 'n' ? '\n' : next);
                    continue;
                }

                if (c == '"')
                    break;
                builder.Append(c);
            }

            return builder.ToString();
        }

        static void Skip(string json, ref int i)
        {
            while (i < json.Length && char.IsWhiteSpace(json[i]))
                i++;
        }

        static string Quote(string value)
        {
            var builder = new StringBuilder(value.Length + 2);
            builder.Append('"');
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c == '\\' || c == '"')
                    builder.Append('\\');
                if (char.IsControl(c))
                    continue;
                builder.Append(c);
            }

            builder.Append('"');
            return builder.ToString();
        }
    }
}
