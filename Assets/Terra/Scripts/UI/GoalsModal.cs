using TerraTactix.Achievements;
using TerraTactix.Core;
using TerraTactix.Data;
using TerraTactix.Missions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraTactix.UI
{
    public static class GoalsModal
    {
        const float RowHeight = 112f;

        public static void Show(Transform parent, bool achievements)
        {
            var title = achievements ? Locale.T("Başarımlar", "Achievements") : Locale.T("Görevler", "Missions");
            var card = UiFactory.Dialog(parent, title, string.Empty, null);
            var body = card.Find("Body");
            if (body != null)
                body.gameObject.SetActive(false);

            if (achievements)
                BuildAchievements(card);
            else
                BuildMissions(card);
        }

        static void BuildMissions(Transform card)
        {
            var missions = GameBootstrap.Instance.Missions;
            var content = List(card, MissionManager.Definitions.Length);
            for (var i = 0; i < MissionManager.Definitions.Length; i++)
            {
                var definition = MissionManager.Definitions[i];
                var entry = missions.Get(definition.id);
                var row = Row(content, i);
                var label = UiFactory.Label(row, MissionCaption(definition, entry), 20f, UiTheme.Ink, new Vector2(0.02f, 0.08f), new Vector2(0.62f, 0.92f), TextAlignmentOptions.Left);
                if (entry.claimed)
                {
                    Status(row, Locale.T("Alındı", "Claimed"), ClaimedPlate, ClaimedInk);
                    continue;
                }

                if (entry.progress < definition.target)
                {
                    Status(row, Locale.T("Kilitli", "Locked"), new Color(0.14f, 0.18f, 0.26f, 1f), UiTheme.Muted);
                    continue;
                }

                var id = definition.id;
                UnityEngine.UI.Button claim = null;
                claim = UiFactory.Button(row, "Claim" + id, Locale.T("Al", "Claim"), UiTheme.Green, new Vector2(0.66f, 0.18f), new Vector2(0.96f, 0.82f), () =>
                {
                    if (!missions.TryClaim(id, out var message))
                    {
                        UiFactory.Dialog(card, Locale.T("Görev", "Mission"), message, null);
                        return;
                    }

                    label.text = MissionCaption(definition, missions.Get(id));
                    claim.gameObject.SetActive(false);
                    var shadow = row.Find("Claim" + id + "Shadow");
                    if (shadow != null)
                        shadow.gameObject.SetActive(false);
                    Status(row, Locale.T("Alındı", "Claimed"), ClaimedPlate, ClaimedInk);
                    UiFactory.Dialog(card, Locale.T("Görev", "Mission"), message, null);
                }, 22f);
            }
        }

        static void BuildAchievements(Transform card)
        {
            var achievements = GameBootstrap.Instance.Achievements;
            var content = List(card, AchievementManager.Definitions.Length);
            for (var i = 0; i < AchievementManager.Definitions.Length; i++)
            {
                var definition = AchievementManager.Definitions[i];
                var entry = achievements.Get(definition.id);
                var row = Row(content, i);
                var label = UiFactory.Label(row, MissionCaption(definition, entry), 20f, UiTheme.Ink, new Vector2(0.02f, 0.08f), new Vector2(0.62f, 0.92f), TextAlignmentOptions.Left);
                if (entry.claimed)
                {
                    Status(row, Locale.T("Alındı", "Claimed"), ClaimedPlate, ClaimedInk);
                    continue;
                }

                if (entry.progress < definition.target)
                {
                    Status(row, Locale.T("Kilitli", "Locked"), new Color(0.14f, 0.18f, 0.26f, 1f), UiTheme.Muted);
                    continue;
                }

                var id = definition.id;
                UnityEngine.UI.Button claim = null;
                claim = UiFactory.Button(row, "Claim" + id, Locale.T("Al", "Claim"), UiTheme.Green, new Vector2(0.66f, 0.18f), new Vector2(0.96f, 0.82f), () =>
                {
                    if (!achievements.TryClaim(id, out var message))
                    {
                        UiFactory.Dialog(card, Locale.T("Başarım", "Achievement"), message, null);
                        return;
                    }

                    label.text = MissionCaption(definition, achievements.Get(id));
                    claim.gameObject.SetActive(false);
                    var shadow = row.Find("Claim" + id + "Shadow");
                    if (shadow != null)
                        shadow.gameObject.SetActive(false);
                    Status(row, Locale.T("Alındı", "Claimed"), ClaimedPlate, ClaimedInk);
                    UiFactory.Dialog(card, Locale.T("Başarım", "Achievement"), message, null);
                }, 22f);
            }
        }

        static string MissionCaption(GoalDefinition definition, ProgressEntry entry)
        {
            return $"{Locale.GoalTitle(definition.id, definition.title)}\n{Locale.GoalBody(definition.id, definition.description)}\n{entry.progress}/{definition.target}";
        }

        static readonly Color ClaimedPlate = UiTheme.Gold;
        static readonly Color ClaimedInk = new Color(0.22f, 0.14f, 0.04f, 1f);

        static void Status(Transform row, string text, Color plate, Color ink)
        {
            var badge = UiFactory.Image(row, "Status", plate, false, new Vector2(0.66f, 0.18f), new Vector2(0.96f, 0.82f));
            badge.raycastTarget = false;
            UiFactory.Label(badge.transform, text, 22f, ink, Vector2.zero, Vector2.one);
        }

        static RectTransform List(Transform card, int count)
        {
            var viewport = UiFactory.Rect(card, "List", new Vector2(0.04f, 0.24f), new Vector2(0.96f, 0.76f));
            var hit = viewport.gameObject.AddComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0.04f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;
            scroll.inertia = true;
            scroll.viewport = viewport;

            var contentGo = new GameObject("Rows", typeof(RectTransform));
            contentGo.transform.SetParent(viewport, false);
            var content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, Mathf.Max(count * RowHeight, 1f));
            scroll.content = content;
            return content;
        }

        static RectTransform Row(RectTransform content, int index)
        {
            var row = UiFactory.Rect(content, "Row" + index, new Vector2(0f, 1f), new Vector2(1f, 1f));
            row.pivot = new Vector2(0.5f, 1f);
            row.sizeDelta = new Vector2(-16f, RowHeight - 8f);
            row.anchoredPosition = new Vector2(0f, -index * RowHeight);
            return row;
        }
    }
}
