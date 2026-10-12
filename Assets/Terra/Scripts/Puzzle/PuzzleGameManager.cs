using System.Collections;
using System.Collections.Generic;
using TerraTactix.Battle;
using TerraTactix.Core;
using TerraTactix.Data;
using TerraTactix.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TerraTactix.Puzzle
{
    public sealed class PuzzleGameManager : MonoBehaviour
    {
        PuzzleGrid grid;
        ScoreSystem score;
        ComboSystem combo;
        BattleTimer timer;
        BalanceConfiguration balance;
        BattleSessionData session;
        int colorCount = 5;
        bool resolving;
        Image[,] cells;
        TMP_Text[,] marks;
        float[,] flashes;
        RectTransform board;
        TMP_Text scoreLabel;
        TMP_Text timerLabel;
        TMP_Text comboLabel;
        bool playing;
        bool ended;
        float idle;
        bool hintOn;
        bool[,] hintCells;
        Vector2[,] slide;
        float[,] burst;
        bool dragging;
        Vector2Int dragOrigin;
        Vector2 dragPress;
        int motion;
        int cascadePopped;
        int burstTier;
        float burstAge;
        Image burstWash;
        Image burstRingA;
        Image burstRingB;
        Image burstPlate;
        Image[] burstSparks;
        RectTransform burstCard;
        TMP_Text burstTitle;
        TMP_Text burstCount;
        TMP_Text burstPlus;

        public float CellScreenWidth => board == null || grid == null ? 48f : board.rect.width * board.lossyScale.x / grid.Width;

        public void Begin(RectTransform boardRect, TMP_Text scoreText, TMP_Text timerText, TMP_Text comboText, RectTransform previewRoot)
        {
            board = boardRect;
            scoreLabel = scoreText;
            timerLabel = timerText;
            comboLabel = comboText;
            if (playing)
                return;
            session = GameBootstrap.Instance.Battles.Session;
            balance = GameBootstrap.Instance.Balance;
            if (session == null)
            {
                Debug.LogError("PuzzleGameManager: BattleSessionData eksik.");
                return;
            }

            if (!GameBootstrap.Instance.Battles.TrySpendHeart())
            {
                scoreLabel.text = Locale.T("Canın yok.\nMağaza veya çarktan can al.", "No hearts left.\nGet hearts from the shop or the wheel.");
                return;
            }

            grid = new PuzzleGrid(balance.gridWidth, balance.gridHeight);
            session.startedUnscaled = Time.unscaledTime;
            colorCount = ColorsFor(session);
            score = new ScoreSystem();
            combo = new ComboSystem();
            timer = new BattleTimer(session.timeLimit);
            BuildCells();
            BuildBurstLayer();
            FillBoard();
            playing = true;
            Refresh();
        }

        public void Rotate()
        {
        }

        public void HardDrop()
        {
        }

        public bool TrySwap(Vector2Int from, Vector2Int to)
        {
            if (!CanControl() || resolving)
                return false;
            if (!grid.IsInside(from.x, from.y) || !grid.IsInside(to.x, to.y))
                return false;
            if (Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y) != 1)
                return false;
            var a = grid.Get(from.x, from.y);
            var b = grid.Get(to.x, to.y);
            if (!a.occupied || !b.occupied || a.typeId == b.typeId)
                return false;
            StartCoroutine(AnimatedSwap(from, to));
            return true;
        }

        public bool BeginDrag(Vector2Int origin, Vector2 screen, Camera camera)
        {
            if (!CanControl() || resolving || grid == null || !grid.IsInside(origin.x, origin.y))
                return false;
            if (!grid.Get(origin.x, origin.y).occupied)
                return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(board, screen, camera, out dragPress))
                return false;
            motion++;
            dragging = true;
            dragOrigin = origin;
            ZeroSlides();
            return true;
        }

        public void DragGem(Vector2 screen, Camera camera)
        {
            if (!dragging || resolving || slide == null)
                return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(board, screen, camera, out var local))
                return;
            var size = CellSize();
            var delta = local - dragPress;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                delta = new Vector2(Mathf.Clamp(delta.x, -size.x, size.x), 0f);
            else
                delta = new Vector2(0f, Mathf.Clamp(delta.y, -size.y, size.y));
            ZeroSlides();
            slide[dragOrigin.x, dragOrigin.y] = delta;
            var step = Vector2Int.zero;
            if (Mathf.Abs(delta.x) > 1f)
                step.x = delta.x > 0f ? 1 : -1;
            else if (Mathf.Abs(delta.y) > 1f)
                step.y = delta.y > 0f ? 1 : -1;
            var next = dragOrigin + step;
            if (step != Vector2Int.zero && grid.IsInside(next.x, next.y) && grid.Get(next.x, next.y).occupied)
                slide[next.x, next.y] = -delta * 0.42f;
        }

        public void EndDrag()
        {
            if (!dragging)
                return;
            dragging = false;
            if (resolving || slide == null)
                return;
            var offset = slide[dragOrigin.x, dragOrigin.y];
            var size = CellSize();
            var horizontal = Mathf.Abs(offset.x) >= Mathf.Abs(offset.y);
            var mag = horizontal ? Mathf.Abs(offset.x) : Mathf.Abs(offset.y);
            var limit = Mathf.Max(8f, (horizontal ? size.x : size.y) * 0.32f);
            var target = dragOrigin;
            if (horizontal && Mathf.Abs(offset.x) > 0.5f)
                target.x += offset.x > 0f ? 1 : -1;
            else if (!horizontal && Mathf.Abs(offset.y) > 0.5f)
                target.y += offset.y > 0f ? 1 : -1;
            var originCell = grid.Get(dragOrigin.x, dragOrigin.y);
            var valid = mag >= limit
                && target != dragOrigin
                && grid.IsInside(target.x, target.y);
            if (valid)
            {
                var other = grid.Get(target.x, target.y);
                valid = other.occupied && other.typeId != originCell.typeId;
            }

            if (!valid)
            {
                StartCoroutine(EaseSlides());
                return;
            }

            StartCoroutine(AnimatedSwap(dragOrigin, target));
        }

        public void NoteActivity()
        {
            idle = 0f;
            hintOn = false;
        }

        public void Pause()
        {
            if (!playing || ended)
                return;
            if (GameBootstrap.Instance.State.ModalOpen)
                return;
            var card = UiFactory.Dialog(transform, Locale.T("Duraklat", "Paused"), Locale.T("Savaş durdu. Ayrılırsan sefer kaybedilir ve can geri gelmez.", "The battle is paused. If you leave, the expedition is lost and the heart is not returned."), null);
            var resume = card.Find("Close")?.GetComponentInChildren<TMP_Text>();
            if (resume != null)
                resume.text = Locale.T("Devam", "Resume");
            UiFactory.Button(card, "GiveUp", Locale.T("Teslim ol", "Give up"), UiTheme.Heart, new Vector2(0.15f, 0.24f), new Vector2(0.85f, 0.42f), () =>
            {
                Finish(false, Locale.T("Teslim oldun", "You gave up"));
            }, 22f);
        }

        void Update()
        {
            if (!CanControl())
                return;

            timer.Tick(Time.deltaTime);
            if (timer.Expired)
            {
                Finish(false, Locale.T("Süre bitti", "Time is up"));
                return;
            }

            if (!resolving)
            {
                idle += Time.deltaTime;
                if (!hintOn && idle >= 10f)
                    ArmHint();
            }

            Refresh();
        }

        void OnDestroy()
        {
            if (playing && !ended && GameBootstrap.Instance != null)
                GameBootstrap.Instance.Battles.Complete(false, score == null ? 0 : score.Value);
        }

        bool CanControl()
        {
            return playing && !ended && grid != null && !GameBootstrap.Instance.State.ModalOpen;
        }

        static int ColorsFor(BattleSessionData session)
        {
            switch (session.difficulty)
            {
                case "Easy": return 3;
                case "Normal": return 4;
                case "Hard": return 5;
                default: return 6;
            }
        }

        int MinMoves()
        {
            switch (session.difficulty)
            {
                case "Easy": return 8;
                case "Normal": return 5;
                case "Hard": return 4;
                default: return 3;
            }
        }

        void FillBoard()
        {
            for (var attempt = 0; attempt < 24; attempt++)
            {
                if (colorCount <= 4)
                    DealClustered();
                else
                    DealBalanced();
                if (!HasMatch() && CountMoves() >= MinMoves())
                    return;
            }

            DealClustered();
            RepairOpening();
            if (!HasMove())
                ForcePlayable();
            ZeroSlides();
        }

        void DealClustered()
        {
            for (var y = 0; y < grid.Height; y++)
            {
                var rowShift = y % colorCount;
                for (var x = 0; x < grid.Width; x++)
                {
                    var type = (x / 2 + rowShift) % colorCount;
                    if (CompletesRun(x, y, type))
                        type = (type + 1) % colorCount;
                    if (CompletesRun(x, y, type))
                        type = (type + 2) % colorCount;
                    grid.Set(x, y, new PuzzleCell { occupied = true, typeId = type, level = 1 });
                }
            }
        }

        void DealBalanced()
        {
            var total = grid.Width * grid.Height;
            var bag = new int[total];
            var index = 0;
            var each = total / colorCount;
            var extra = total % colorCount;
            for (var color = 0; color < colorCount; color++)
            {
                var count = each + (color < extra ? 1 : 0);
                for (var n = 0; n < count; n++)
                    bag[index++] = color;
            }

            for (var i = total - 1; i > 0; i--)
            {
                var swap = Random.Range(0, i + 1);
                var held = bag[i];
                bag[i] = bag[swap];
                bag[swap] = held;
            }

            index = 0;
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (CompletesRun(x, y, bag[index]))
                    {
                        for (var look = index + 1; look < total; look++)
                        {
                            if (CompletesRun(x, y, bag[look]))
                                continue;
                            var held = bag[index];
                            bag[index] = bag[look];
                            bag[look] = held;
                            break;
                        }
                    }

                    grid.Set(x, y, new PuzzleCell { occupied = true, typeId = bag[index], level = 1 });
                    index++;
                }
            }
        }

        void RepairOpening()
        {
            for (var step = 0; step < 48 && HasMatch(); step++)
            {
                var matches = CollectMatches();
                if (matches.Count == 0 || !RetintAway(matches[0]))
                    break;
            }

            if (!HasMatch() && HasMove())
                return;
            if (TryPlant(false) && !HasMatch() && HasMove())
                return;
            TryShuffle(true, true);
        }

        bool RetintAway(Vector2Int cell)
        {
            var mine = grid.Get(cell.x, cell.y);
            if (!mine.occupied)
                return false;
            var before = CollectMatches().Count;
            var best = before;
            var pick = new Vector2Int(-1, -1);
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (x == cell.x && y == cell.y)
                        continue;
                    var other = grid.Get(x, y);
                    if (!other.occupied || other.typeId == mine.typeId)
                        continue;
                    grid.Set(cell.x, cell.y, other);
                    grid.Set(x, y, mine);
                    var left = CollectMatches().Count;
                    grid.Set(cell.x, cell.y, mine);
                    grid.Set(x, y, other);
                    if (left < best)
                    {
                        best = left;
                        pick = new Vector2Int(x, y);
                    }
                }
            }

            if (pick.x < 0 || best >= before)
                return false;
            var chosen = grid.Get(pick.x, pick.y);
            grid.Set(cell.x, cell.y, chosen);
            grid.Set(pick.x, pick.y, mine);
            return true;
        }

        bool CompletesRun(int x, int y, int type)
        {
            return x >= 2 && Same(x - 1, y, type) && Same(x - 2, y, type)
                || y >= 2 && Same(x, y - 1, type) && Same(x, y - 2, type);
        }

        bool Same(int x, int y, int type)
        {
            var cell = grid.Get(x, y);
            return cell.occupied && cell.typeId == type;
        }

        IEnumerator AnimatedSwap(Vector2Int a, Vector2Int b)
        {
            var token = ++motion;
            resolving = true;
            NoteActivity();
            yield return MovePair(a, b, token, 0.12f);
            if (ended || token != motion)
            {
                resolving = false;
                yield break;
            }

            var first = grid.Get(a.x, a.y);
            var second = grid.Get(b.x, b.y);
            grid.Set(a.x, a.y, second);
            grid.Set(b.x, b.y, first);
            ZeroSlides();
            if (!HasMatch())
            {
                yield return MovePair(a, b, token, 0.12f);
                if (!ended && token == motion)
                {
                    grid.Set(a.x, a.y, first);
                    grid.Set(b.x, b.y, second);
                    ZeroSlides();
                }

                resolving = false;
                yield break;
            }

            yield return ResolveRoutine();
            resolving = false;
        }

        IEnumerator MovePair(Vector2Int a, Vector2Int b, int token, float duration)
        {
            var size = CellSize();
            var delta = new Vector2((b.x - a.x) * size.x, (b.y - a.y) * size.y);
            var startA = slide[a.x, a.y];
            var startB = slide[b.x, b.y];
            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (ended || token != motion)
                    yield break;
                elapsed += Time.deltaTime;
                var along = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                slide[a.x, a.y] = Vector2.Lerp(startA, delta, along);
                slide[b.x, b.y] = Vector2.Lerp(startB, -delta, along);
                yield return null;
            }

            if (token == motion && slide != null)
            {
                slide[a.x, a.y] = delta;
                slide[b.x, b.y] = -delta;
            }
        }

        IEnumerator EaseSlides(float duration = 0.12f)
        {
            var token = ++motion;
            var from = new Vector2[grid.Width, grid.Height];
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                    from[x, y] = slide[x, y];
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (ended || token != motion)
                    yield break;
                elapsed += Time.deltaTime;
                var along = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                for (var y = 0; y < grid.Height; y++)
                {
                    for (var x = 0; x < grid.Width; x++)
                        slide[x, y] = from[x, y] * along;
                }

                yield return null;
            }

            if (token == motion)
                ZeroSlides();
        }

        IEnumerator ResolveRoutine()
        {
            combo.Reset();
            cascadePopped = 0;
            var round = 0;
            while (round++ < 4 && !ended)
            {
            var safety = 0;
            var settled = false;
            while (safety++ < 48 && !ended)
            {
                var matches = CollectMatches();
                if (matches.Count == 0)
                {
                    if (OccupiedCount() == 0)
                    {
                        settled = true;
                        break;
                    }

                    Stabilize();
                    if (SlidePending())
                        yield return EaseSlides(0.22f);
                    if (HasMatch())
                        continue;
                    settled = true;
                    break;
                }
                combo.Register();
                cascadePopped += matches.Count;
                for (var i = 0; i < matches.Count; i++)
                    burst[matches[i].x, matches[i].y] = 0.001f;
                AddScore(balance.mergeScore * matches.Count * combo.Multiplier, true);
                GameEvents.RaiseLineCleared(1);
                GameEvents.RaiseMergePerformed();
                var elapsed = 0f;
                const float popTime = 0.2f;
                while (elapsed < popTime && !ended)
                {
                    elapsed += Time.deltaTime;
                    var along = Mathf.Clamp01(elapsed / popTime);
                    for (var i = 0; i < matches.Count; i++)
                        burst[matches[i].x, matches[i].y] = along;
                    yield return null;
                }

                var droppedFrom = new List<int>[grid.Width];
                for (var x = 0; x < grid.Width; x++)
                {
                    droppedFrom[x] = new List<int>();
                    for (var y = 0; y < grid.Height; y++)
                    {
                        if (burst[x, y] > 0f)
                        {
                            grid.Clear(x, y);
                            burst[x, y] = 0f;
                            continue;
                        }

                        if (grid.Get(x, y).occupied)
                            droppedFrom[x].Add(y);
                    }
                }

                grid.Collapse();
                ZeroSlides();
                var furthest = 0;
                for (var x = 0; x < grid.Width; x++)
                {
                    var sources = droppedFrom[x];
                    for (var i = 0; i < sources.Count; i++)
                    {
                        var distance = sources[i] - i;
                        if (distance <= 0)
                            continue;
                        slide[x, i] = new Vector2(0f, distance * CellSize().y);
                        if (distance > furthest)
                            furthest = distance;
                    }
                }

                if (furthest > 0)
                    yield return EaseSlides(0.1f + 0.04f * furthest);
            }

            if (ended)
                yield break;
            if (settled && OccupiedCount() > 0 && !HasMove() && !HasMatch())
            {
                Stabilize();
                if (SlidePending())
                    yield return EaseSlides(0.22f);
            }

            if (HasMatch())
                continue;
            break;
            }

            if (ended)
                yield break;
            AnnounceBurst();
            if (combo.Chain >= 2)
                GameEvents.RaiseComboReached(combo.Multiplier);
            if (OccupiedCount() == 0)
            {
                if (burstTier > 0)
                    yield return new WaitForSeconds(0.85f);
                HideBurst();
                Finish(true, Locale.T("Tüm taşlar yok oldu", "All the stones are gone"));
            }
        }

        int CountMoves()
        {
            var count = 0;
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (TryMoveProbe(x, y, x + 1, y))
                        count++;
                    if (TryMoveProbe(x, y, x, y + 1))
                        count++;
                }
            }

            return count;
        }

        void ForcePlayable()
        {
            Stabilize();
        }

        void Stabilize()
        {
            if (ended || grid == null)
                return;

            for (var guard = 0; guard < 6; guard++)
            {
                var occupied = OccupiedCount();
                if (occupied == 0)
                    return;
                if (occupied < 3)
                {
                    ClearOccupied();
                    return;
                }

                FoldShortColors(-1, -1);
                grid.Collapse();
                PackColumns();
                if (HasMatch() || HasMove())
                    return;

                StackForHeight(4);
                grid.Collapse();
                PackColumns();
                if (HasMatch() || HasMove())
                    return;
                if (PlantSwapPattern())
                    return;
                if (ForceLine())
                    return;
            }

            GuaranteeProgress();
        }

        void GuaranteeProgress()
        {
            var remaining = OccupiedCount();
            if (remaining <= 0)
                return;
            if (remaining < 3 || (grid.Width < 3 && grid.Height < 3))
            {
                ClearOccupied();
                return;
            }

            var color = DominantColor();
            var pieces = new List<PuzzleCell>(remaining);
            var origins = new List<Vector2Int>(remaining);
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    var cell = grid.Get(x, y);
                    if (!cell.occupied)
                        continue;
                    pieces.Add(cell);
                    origins.Add(new Vector2Int(x, y));
                    grid.Clear(x, y);
                }
            }

            ZeroSlides();
            var horizontal = grid.Width >= 3;
            var size = CellSize();
            for (var i = 0; i < pieces.Count; i++)
            {
                var target = horizontal
                    ? new Vector2Int(i % grid.Width, i / grid.Width)
                    : new Vector2Int(i / grid.Height, i % grid.Height);
                grid.Set(target.x, target.y, pieces[i]);
                slide[target.x, target.y] = new Vector2(
                    (origins[i].x - target.x) * size.x,
                    (origins[i].y - target.y) * size.y);
            }

            for (var i = 0; i < 3; i++)
            {
                var target = horizontal
                    ? new Vector2Int(i, 0)
                    : new Vector2Int(0, i);
                SetType(target.x, target.y, color);
            }

            // Bu noktada en az üç taş kesin eşleşir. Beklenmeyen bir durumda
            // oyuncuyu kilitlemek yerine kalan taşları tamamlanmış say.
            if (!HasMatch() && !HasMove())
                ClearOccupied();
        }

        void FoldShortColors(int protectX, int protectY)
        {
            for (var pass = 0; pass < colorCount; pass++)
            {
                var counts = ColorCounts();
                var weak = -1;
                var strong = -1;
                for (var i = 0; i < colorCount; i++)
                {
                    if (counts[i] <= 0)
                        continue;
                    if (counts[i] < 3 && (weak < 0 || counts[i] < counts[weak]))
                        weak = i;
                    if (strong < 0 || counts[i] > counts[strong])
                        strong = i;
                }

                if (weak < 0)
                    return;
                var protectedColor = grid.IsInside(protectX, protectY) && grid.Get(protectX, protectY).occupied
                    ? grid.Get(protectX, protectY).typeId
                    : -1;
                if (weak == protectedColor && counts[weak] <= 1)
                {
                    weak = -1;
                    for (var i = 0; i < colorCount; i++)
                    {
                        if (i == protectedColor || counts[i] <= 0 || counts[i] >= 3)
                            continue;
                        if (weak < 0 || counts[i] < counts[weak])
                            weak = i;
                    }

                    if (weak < 0)
                        return;
                }
                if (strong < 0 || strong == weak)
                {
                    if (protectedColor >= 0)
                        return;
                    PaintOccupied(weak < 0 ? 0 : weak);
                    return;
                }

                Retint(weak, strong, protectX, protectY);
            }
        }

        void PackColumns()
        {
            var heights = ColumnHeights();
            var cols = new List<int>();
            for (var x = 0; x < grid.Width; x++)
            {
                if (heights[x] > 0)
                    cols.Add(x);
            }

            if (cols.Count <= 1)
                return;

            var anchor = TallestColumn(heights);
            var anchorIndex = cols.IndexOf(anchor);
            if (anchorIndex < 0)
                return;
            var first = anchor - anchorIndex;
            var last = first + cols.Count - 1;
            var shift = 0;
            if (first < 0)
                shift = -first;
            if (last + shift >= grid.Width)
                shift -= last + shift - (grid.Width - 1);

            var targets = new int[cols.Count];
            var moved = false;
            for (var i = 0; i < cols.Count; i++)
            {
                targets[i] = first + i + shift;
                if (targets[i] != cols[i])
                    moved = true;
            }

            if (!moved)
                return;

            var copies = new PuzzleCell[cols.Count][];
            for (var i = 0; i < cols.Count; i++)
            {
                copies[i] = new PuzzleCell[grid.Height];
                for (var y = 0; y < grid.Height; y++)
                    copies[i][y] = grid.Get(cols[i], y);
                for (var y = 0; y < grid.Height; y++)
                    grid.Clear(cols[i], y);
            }

            var size = CellSize();
            for (var i = 0; i < cols.Count; i++)
            {
                var from = cols[i];
                var to = targets[i];
                for (var y = 0; y < grid.Height; y++)
                {
                    if (!copies[i][y].occupied)
                        continue;
                    grid.Set(to, y, copies[i][y]);
                    if (slide != null)
                        slide[to, y] += new Vector2((from - to) * size.x, 0f);
                }
            }
        }

        void StackForHeight(int need)
        {
            var size = CellSize();
            for (var guard = 0; guard < grid.Width * grid.Height; guard++)
            {
                var heights = ColumnHeights();
                var anchor = TallestColumn(heights);
                if (anchor < 0 || heights[anchor] >= need || heights[anchor] >= grid.Height)
                    return;
                var donor = -1;
                for (var x = 0; x < grid.Width; x++)
                {
                    if (x == anchor || heights[x] == 0)
                        continue;
                    if (donor < 0 || heights[x] < heights[donor])
                        donor = x;
                }

                if (donor < 0)
                    return;
                var top = heights[donor] - 1;
                var cell = grid.Get(donor, top);
                var carried = slide != null ? slide[donor, top] : Vector2.zero;
                grid.Clear(donor, top);
                if (slide != null)
                    slide[donor, top] = Vector2.zero;
                var destination = heights[anchor];
                grid.Set(anchor, destination, cell);
                if (slide != null)
                    slide[anchor, destination] = carried + new Vector2((donor - anchor) * size.x, (top - destination) * size.y);
            }
        }

        bool PlantSwapPattern()
        {
            var heights = ColumnHeights();
            var column = TallestColumn(heights);
            if (column < 0 || heights[column] < 4)
                return false;

            var main = DominantColor();
            var other = main == 0 ? 1 : 0;
            if (other >= colorCount)
                other = 0;
            SetType(column, 0, main);
            SetType(column, 1, main);
            SetType(column, 2, other);
            SetType(column, 3, main);
            FoldShortColors(column, 2);
            for (var n = 0; n < 8 && HasMatch(); n++)
                BreakOutsideMatch(column, main, other);
            return HasMove() || HasMatch();
        }

        void BreakOutsideMatch(int column, int main, int other)
        {
            var matches = CollectMatches();
            for (var i = 0; i < matches.Count; i++)
            {
                var cell = matches[i];
                if (cell.x == column && cell.y <= 3)
                    continue;
                SetType(cell.x, cell.y, other);
                return;
            }

            for (var i = 0; i < matches.Count; i++)
            {
                var cell = matches[i];
                var left = cell.x - 1;
                var right = cell.x + 1;
                if (grid.IsInside(left, cell.y) && grid.Get(left, cell.y).occupied && grid.Get(left, cell.y).typeId == main)
                {
                    SetType(left, cell.y, other);
                    return;
                }

                if (grid.IsInside(right, cell.y) && grid.Get(right, cell.y).occupied && grid.Get(right, cell.y).typeId == main)
                {
                    SetType(right, cell.y, other);
                    return;
                }
            }
        }

        bool ForceLine()
        {
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (x + 2 < grid.Width && OccupiedAt(new Vector2Int(x, y)) && OccupiedAt(new Vector2Int(x + 1, y)) && OccupiedAt(new Vector2Int(x + 2, y)))
                    {
                        var color = DominantColor();
                        SetType(x, y, color);
                        SetType(x + 1, y, color);
                        SetType(x + 2, y, color);
                        return true;
                    }

                    if (y + 2 < grid.Height && OccupiedAt(new Vector2Int(x, y)) && OccupiedAt(new Vector2Int(x, y + 1)) && OccupiedAt(new Vector2Int(x, y + 2)))
                    {
                        var color = DominantColor();
                        SetType(x, y, color);
                        SetType(x, y + 1, color);
                        SetType(x, y + 2, color);
                        return true;
                    }
                }
            }

            return false;
        }

        int[] ColorCounts()
        {
            var counts = new int[colorCount];
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    var cell = grid.Get(x, y);
                    if (cell.occupied && cell.typeId >= 0 && cell.typeId < colorCount)
                        counts[cell.typeId]++;
                }
            }

            return counts;
        }

        int DominantColor()
        {
            var counts = ColorCounts();
            var best = 0;
            for (var i = 1; i < counts.Length; i++)
            {
                if (counts[i] > counts[best])
                    best = i;
            }

            return best;
        }

        void Retint(int from, int to, int protectX, int protectY)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (x == protectX && y == protectY)
                        continue;
                    var cell = grid.Get(x, y);
                    if (!cell.occupied || cell.typeId != from)
                        continue;
                    cell.typeId = to;
                    grid.Set(x, y, cell);
                }
            }
        }

        void PaintOccupied(int color)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (grid.Get(x, y).occupied)
                        SetType(x, y, color);
                }
            }
        }

        void SetType(int x, int y, int color)
        {
            var cell = grid.Get(x, y);
            if (!cell.occupied)
                return;
            cell.typeId = color;
            grid.Set(x, y, cell);
        }

        void ClearOccupied()
        {
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (grid.Get(x, y).occupied)
                        grid.Clear(x, y);
                }
            }
        }

        int[] ColumnHeights()
        {
            var heights = new int[grid.Width];
            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    if (grid.Get(x, y).occupied)
                        heights[x] = y + 1;
                }
            }

            return heights;
        }

        int TallestColumn(int[] heights)
        {
            var best = -1;
            for (var x = 0; x < heights.Length; x++)
            {
                if (heights[x] <= 0)
                    continue;
                if (best < 0 || heights[x] > heights[best])
                    best = x;
            }

            return best;
        }

        bool SlidePending()
        {
            if (slide == null)
                return false;
            for (var y = 0; y < slide.GetLength(1); y++)
            {
                for (var x = 0; x < slide.GetLength(0); x++)
                {
                    if (slide[x, y].sqrMagnitude > 0.01f)
                        return true;
                }
            }

            return false;
        }

        Vector2 CellSize()
        {
            if (board == null || grid == null)
                return new Vector2(64f, 64f);
            var rect = board.rect;
            return new Vector2(Mathf.Max(8f, rect.width / grid.Width), Mathf.Max(8f, rect.height / grid.Height));
        }

        void ZeroSlides()
        {
            if (slide == null)
                return;
            for (var y = 0; y < slide.GetLength(1); y++)
            {
                for (var x = 0; x < slide.GetLength(0); x++)
                    slide[x, y] = Vector2.zero;
            }
        }

        System.Collections.Generic.List<Vector2Int> CollectMatches()
        {
            var found = new System.Collections.Generic.List<Vector2Int>();
            var mark = new bool[grid.Width, grid.Height];
            for (var y = 0; y < grid.Height; y++)
            {
                var run = 1;
                for (var x = 1; x <= grid.Width; x++)
                {
                    var continues = x < grid.Width && Same(x, y, grid.Get(x - 1, y).typeId) && grid.Get(x - 1, y).occupied;
                    if (continues)
                    {
                        run++;
                        continue;
                    }

                    if (run >= 3)
                    {
                        for (var i = 0; i < run; i++)
                            mark[x - 1 - i, y] = true;
                    }

                    run = 1;
                }
            }

            for (var x = 0; x < grid.Width; x++)
            {
                var run = 1;
                for (var y = 1; y <= grid.Height; y++)
                {
                    var continues = y < grid.Height && Same(x, y, grid.Get(x, y - 1).typeId) && grid.Get(x, y - 1).occupied;
                    if (continues)
                    {
                        run++;
                        continue;
                    }

                    if (run >= 3)
                    {
                        for (var i = 0; i < run; i++)
                            mark[x, y - 1 - i] = true;
                    }

                    run = 1;
                }
            }

            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (mark[x, y])
                        found.Add(new Vector2Int(x, y));
                }
            }

            return found;
        }

        bool HasMatch()
        {
            return CollectMatches().Count > 0;
        }

        bool HasMove()
        {
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (TryMoveProbe(x, y, x + 1, y) || TryMoveProbe(x, y, x, y + 1))
                        return true;
                }
            }

            return false;
        }

        bool TryMoveProbe(int x, int y, int nx, int ny)
        {
            if (!grid.IsInside(nx, ny))
                return false;
            var a = grid.Get(x, y);
            var b = grid.Get(nx, ny);
            if (!a.occupied || !b.occupied || a.typeId == b.typeId)
                return false;
            grid.Set(x, y, b);
            grid.Set(nx, ny, a);
            var match = HasMatch();
            grid.Set(x, y, a);
            grid.Set(nx, ny, b);
            return match;
        }

        bool TryPlant(bool allowMatch)
        {
            var counts = new int[colorCount];
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    var cell = grid.Get(x, y);
                    if (cell.occupied && cell.typeId >= 0 && cell.typeId < colorCount)
                        counts[cell.typeId]++;
                }
            }

            var color = -1;
            for (var i = 0; i < colorCount; i++)
            {
                if (counts[i] >= 3 && (color < 0 || counts[i] > counts[color]))
                    color = i;
            }

            if (color < 0)
                return false;
            var other = -1;
            for (var i = 0; i < colorCount; i++)
            {
                if (i != color && counts[i] > 0)
                {
                    other = i;
                    break;
                }
            }

            if (other < 0)
                return false;

            var dirs = new[] { Vector2Int.right, Vector2Int.up };
            var neighbors = new[] { Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };
            for (var d = 0; d < dirs.Length; d++)
            {
                var dir = dirs[d];
                for (var y = 0; y < grid.Height; y++)
                {
                    for (var x = 0; x < grid.Width; x++)
                    {
                        var line = new[]
                        {
                            new Vector2Int(x, y),
                            new Vector2Int(x, y) + dir,
                            new Vector2Int(x, y) + dir + dir
                        };
                        if (!OccupiedAt(line[0]) || !OccupiedAt(line[1]) || !OccupiedAt(line[2]))
                            continue;
                        for (var hole = 0; hole < 3; hole++)
                        {
                            for (var n = 0; n < neighbors.Length; n++)
                            {
                                var elbow = line[hole] + neighbors[n];
                                if (!OccupiedAt(elbow) || elbow == line[0] || elbow == line[1] || elbow == line[2])
                                    continue;
                                if (PaintMove(line, hole, elbow, color, other, allowMatch))
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        bool PaintMove(Vector2Int[] line, int hole, Vector2Int elbow, int color, int other, bool allowMatch)
        {
            var backup = Snapshot();
            var want = new[] { line[0], line[1], line[2], elbow };
            var types = new[] { color, color, color, color };
            types[hole] = other;
            var locked = new bool[grid.Width, grid.Height];
            for (var i = 0; i < want.Length; i++)
            {
                if (i == hole)
                    continue;
                if (!PullType(want[i], types[i], locked))
                {
                    Restore(backup);
                    return false;
                }

                locked[want[i].x, want[i].y] = true;
            }

            if (!PullType(line[hole], other, locked))
            {
                Restore(backup);
                return false;
            }

            var opened = TryMoveProbe(line[hole].x, line[hole].y, elbow.x, elbow.y);
            var matched = HasMatch();
            if (opened && !matched)
                return true;
            if (allowMatch && matched)
                return true;
            Restore(backup);
            return false;
        }

        bool PullType(Vector2Int dest, int type, bool[,] locked)
        {
            var here = grid.Get(dest.x, dest.y);
            if (!here.occupied)
                return false;
            if (here.typeId == type)
                return true;
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (locked[x, y] || (x == dest.x && y == dest.y))
                        continue;
                    var donor = grid.Get(x, y);
                    if (!donor.occupied || donor.typeId != type)
                        continue;
                    grid.Set(dest.x, dest.y, donor);
                    grid.Set(x, y, here);
                    return true;
                }
            }

            return false;
        }

        bool TryShuffle(bool needMove, bool avoidMatch)
        {
            var spots = new System.Collections.Generic.List<Vector2Int>();
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (grid.Get(x, y).occupied)
                        spots.Add(new Vector2Int(x, y));
                }
            }

            if (spots.Count < 4)
                return false;
            var backup = Snapshot();
            var types = new int[spots.Count];
            for (var i = 0; i < spots.Count; i++)
                types[i] = grid.Get(spots[i].x, spots[i].y).typeId;
            for (var attempt = 0; attempt < 80; attempt++)
            {
                for (var i = types.Length - 1; i > 0; i--)
                {
                    var swap = Random.Range(0, i + 1);
                    var held = types[i];
                    types[i] = types[swap];
                    types[swap] = held;
                }

                for (var i = 0; i < spots.Count; i++)
                {
                    var cell = grid.Get(spots[i].x, spots[i].y);
                    cell.typeId = types[i];
                    grid.Set(spots[i].x, spots[i].y, cell);
                }

                var matched = HasMatch();
                if (avoidMatch && matched)
                    continue;
                if (needMove)
                {
                    if (HasMove())
                        return true;
                    continue;
                }

                if (matched || HasMove())
                    return true;
            }

            Restore(backup);
            return false;
        }

        PuzzleCell[,] Snapshot()
        {
            var copy = new PuzzleCell[grid.Width, grid.Height];
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                    copy[x, y] = grid.Get(x, y);
            }

            return copy;
        }

        void Restore(PuzzleCell[,] copy)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                    grid.Set(x, y, copy[x, y]);
            }
        }

        bool OccupiedAt(Vector2Int cell)
        {
            return grid.IsInside(cell.x, cell.y) && grid.Get(cell.x, cell.y).occupied;
        }

        IEnumerator PlayRepair()
        {
            resolving = true;
            idle = 0f;
            if (SlidePending())
                yield return EaseSlides(0.22f);
            if (!ended && HasMatch())
                yield return ResolveRoutine();
            if (!ended)
                resolving = false;
        }

        void ArmHint()
        {
            if (hintCells == null || hintCells.GetLength(0) != grid.Width || hintCells.GetLength(1) != grid.Height)
                hintCells = new bool[grid.Width, grid.Height];
            else
                System.Array.Clear(hintCells, 0, hintCells.Length);

            hintOn = false;
            if (!HasMove())
            {
                ForcePlayable();
                if (ended)
                    return;
                if (SlidePending() || HasMatch())
                {
                    StartCoroutine(PlayRepair());
                    return;
                }

                if (!HasMove())
                    return;
            }

            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (TryMoveProbe(x, y, x + 1, y))
                    {
                        hintCells[x, y] = true;
                        hintCells[x + 1, y] = true;
                        hintOn = true;
                    }

                    if (TryMoveProbe(x, y, x, y + 1))
                    {
                        hintCells[x, y] = true;
                        hintCells[x, y + 1] = true;
                        hintOn = true;
                    }
                }
            }
        }

        int OccupiedCount()
        {
            var count = 0;
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (grid.Get(x, y).occupied)
                        count++;
                }
            }

            return count;
        }

        void Finish(bool won, string reason)
        {
            if (ended)
                return;
            ended = true;
            hintOn = false;
            playing = false;
            HideBurst();
            var result = GameBootstrap.Instance.Battles.Complete(won, score == null ? 0 : score.Value);
            Refresh();
            var title = won ? Locale.T("Sefer kazanıldı", "Expedition won") : Locale.T("Sefer kaybedildi", "Expedition lost");
            var capture = result.captured ? Locale.T("\nBölge fethedildi.", "\nRegion captured.") : string.Empty;
            var body = $"{reason}{capture}\n{Locale.T("Kayıt cihazda tutuldu, sıralamaya yüklenmedi.", "The result is saved on this device and was not uploaded.")}";
            var card = UiFactory.Dialog(transform, title, body, () =>
            {
                if (GameBootstrap.Instance != null)
                    GameBootstrap.Instance.Scenes.Load(SceneNames.WorldMap);
            });
            var bodyRect = card.Find("Body") as RectTransform;
            if (bodyRect != null)
            {
                bodyRect.anchorMin = new Vector2(0.08f, 0.58f);
                bodyRect.anchorMax = new Vector2(0.92f, 0.76f);
            }

            var pct = result.required < 1 ? 0f : Mathf.Clamp01(result.progress / (float)result.required);
            UiFactory.Image(card, "Track", new Color(0.05f, 0.08f, 0.12f, 1f), false, new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.56f));
            UiFactory.Image(card, "Fill", won ? UiTheme.Green : UiTheme.Heart, false, new Vector2(0.1f, 0.48f), new Vector2(0.1f + 0.8f * pct, 0.56f));
            UiFactory.Label(card, $"{Locale.T("Sefer", "Expedition")} {result.progress}/{result.required}", 18f, UiTheme.Ink, new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.56f));
            RewardChip(card, Locale.T("Skor", "Score"), result.score.ToString(), UiTheme.Yellow, 0.08f);
            RewardChip(card, Locale.T("Altın", "Gold"), "+" + result.gold, UiTheme.Gold, 0.37f);
            RewardChip(card, Locale.T("Deneyim", "Experience"), "+" + result.xp, new Color(0.55f, 0.85f, 1f), 0.66f);
        }

        static void RewardChip(Transform parent, string title, string value, Color color, float x)
        {
            UiFactory.Image(parent, title + "Chip", new Color(0.05f, 0.08f, 0.14f, 0.95f), false, new Vector2(x, 0.24f), new Vector2(x + 0.26f, 0.46f));
            UiFactory.Label(parent, title, 16f, UiTheme.Muted, new Vector2(x, 0.36f), new Vector2(x + 0.26f, 0.45f));
            UiFactory.Label(parent, value, 26f, color, new Vector2(x, 0.25f), new Vector2(x + 0.26f, 0.36f));
        }

        int AddScore(int amount, bool pop = false)
        {
            if (amount <= 0 || score == null)
                return 0;
            var multiplier = session != null && session.scoreMultiplier > 0f ? session.scoreMultiplier : 1f;
            var granted = Mathf.Max(1, Mathf.RoundToInt(amount * multiplier));
            score.Add(granted);
            GameBootstrap.Instance.Battles.ReportScore(score.Value);
            if (pop)
                Pop("+" + granted, 0.62f);
            return granted;
        }

        static int BurstTier(int stones)
        {
            if (stones >= 20)
                return 20;
            if (stones >= 15)
                return 15;
            if (stones >= 10)
                return 10;
            if (stones >= 5)
                return 5;
            return 0;
        }

        static int BurstBonus(int tier)
        {
            switch (tier)
            {
                case 5: return 50;
                case 10: return 120;
                case 15: return 220;
                case 20: return 400;
                default: return 0;
            }
        }

        void AnnounceBurst()
        {
            var tier = BurstTier(cascadePopped);
            cascadePopped = 0;
            if (tier <= 0)
                return;
            var granted = AddScore(BurstBonus(tier), false);
            ShowBurst(tier, granted);
            if (GameBootstrap.Instance != null && GameBootstrap.Instance.Audio != null)
                GameBootstrap.Instance.Audio.PlayCombo(tier);
        }

        void HideBurst()
        {
            burstTier = 0;
            if (burstWash != null)
                burstWash.color = Color.clear;
            if (burstRingA != null)
                burstRingA.color = Color.clear;
            if (burstRingB != null)
                burstRingB.color = Color.clear;
            if (burstSparks != null)
            {
                for (var i = 0; i < burstSparks.Length; i++)
                    burstSparks[i].gameObject.SetActive(false);
            }

            if (burstCard != null)
                burstCard.gameObject.SetActive(false);
            if (board != null)
                board.localScale = Vector3.one;
        }

        void ShowBurst(int tier, int bonus)
        {
            if (burstCard == null)
                return;
            burstTier = tier;
            burstAge = 0f;
            burstCard.gameObject.SetActive(true);
            burstCard.SetAsLastSibling();
            var accent = BurstAccent(tier);
            burstTitle.text = BurstTitle(tier);
            burstTitle.color = accent;
            burstTitle.fontSize = tier >= 20 ? 48f : tier >= 15 ? 42f : 36f;
            burstCount.text = tier.ToString();
            burstCount.color = Color.white;
            burstCount.fontSize = tier >= 20 ? 132f : tier >= 15 ? 118f : tier >= 10 ? 104f : 92f;
            burstPlus.text = "+" + bonus;
            burstPlus.color = accent;
            var sparks = tier >= 15 ? 8 : tier >= 10 ? 6 : 4;
            for (var i = 0; i < burstSparks.Length; i++)
                burstSparks[i].gameObject.SetActive(i < sparks);
            burstRingB.gameObject.SetActive(tier >= 10);
        }

        static string BurstTitle(int tier)
        {
            switch (tier)
            {
                case 10: return Locale.T("Süper kombo", "Super combo");
                case 15: return Locale.T("Harika kombo", "Great combo");
                case 20: return Locale.T("Efsane kombo", "Epic combo");
                default: return Locale.T("Kombo", "Combo");
            }
        }

        static Color BurstAccent(int tier)
        {
            switch (tier)
            {
                case 10: return new Color(0.4f, 0.95f, 1f, 1f);
                case 15: return new Color(0.82f, 0.52f, 1f, 1f);
                case 20: return new Color(1f, 0.86f, 0.32f, 1f);
                default: return new Color(1f, 0.78f, 0.28f, 1f);
            }
        }

        void BuildBurstLayer()
        {
            if (board == null || burstCard != null)
                return;
            burstWash = UiFactory.Image(board, "BurstWash", Color.clear, false, Vector2.zero, Vector2.one);
            burstWash.raycastTarget = false;
            burstRingA = CreateBurstCircle("BurstRingA", 80f);
            burstRingB = CreateBurstCircle("BurstRingB", 80f);
            burstSparks = new Image[8];
            for (var i = 0; i < burstSparks.Length; i++)
            {
                burstSparks[i] = CreateBurstCircle("BurstSpark" + i, 16f);
                burstSparks[i].gameObject.SetActive(false);
            }

            burstCard = UiFactory.Point(board, "BurstCard", new Vector2(0.5f, 0.58f), new Vector2(620f, 320f));
            burstPlate = UiFactory.Image(burstCard, "Plate", new Color(0.04f, 0.07f, 0.14f, 0.82f), false, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f));
            burstPlate.raycastTarget = false;
            burstTitle = UiFactory.Label(burstCard, string.Empty, 36f, Color.white, new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.9f));
            burstCount = UiFactory.Label(burstCard, string.Empty, 108f, Color.white, new Vector2(0.08f, 0.26f), new Vector2(0.92f, 0.72f));
            burstPlus = UiFactory.Label(burstCard, string.Empty, 40f, UiTheme.Yellow, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.3f));
            burstTitle.outlineWidth = 0.16f;
            burstCount.outlineWidth = 0.18f;
            burstPlus.outlineWidth = 0.14f;
            var ink = new Color(0.05f, 0.08f, 0.14f, 0.95f);
            burstTitle.outlineColor = ink;
            burstCount.outlineColor = ink;
            burstPlus.outlineColor = ink;
            burstCard.gameObject.SetActive(false);
            burstRingB.gameObject.SetActive(false);
        }

        Image CreateBurstCircle(string name, float size)
        {
            var rect = UiFactory.Point(board, name, new Vector2(0.5f, 0.5f), new Vector2(size, size));
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = UiFactory.Knob;
            image.raycastTarget = false;
            image.color = Color.clear;
            return image;
        }

        void PaintBurst()
        {
            if (burstWash == null)
                return;
            if (burstTier <= 0)
            {
                if (board != null && board.localScale != Vector3.one)
                    board.localScale = Vector3.one;
                return;
            }

            burstAge += Time.deltaTime;
            var duration = burstTier >= 20 ? 1.2f : 1.05f;
            if (burstAge >= duration)
            {
                HideBurst();
                return;
            }

            var t = Mathf.Clamp01(burstAge / duration);
            var fade = t < 0.58f ? 1f : 1f - (t - 0.58f) / 0.42f;
            var punch = t < 0.16f
                ? Mathf.Lerp(0.35f, 1.16f, t / 0.16f)
                : Mathf.Lerp(1.16f, 1f, Mathf.Clamp01((t - 0.16f) / 0.22f));
            var accent = BurstAccent(burstTier);
            var wash = burstTier >= 20 ? 0.42f : burstTier >= 15 ? 0.32f : burstTier >= 10 ? 0.24f : 0.16f;
            var washColor = accent;
            washColor.a = wash * fade * (t < 0.12f ? t / 0.12f : 1f);
            burstWash.color = washColor;

            var ring = Mathf.Lerp(70f, burstTier >= 20 ? 760f : burstTier >= 15 ? 640f : 520f, Mathf.Clamp01(burstAge / 0.55f));
            PaintRing(burstRingA, accent, ring, fade * 0.85f);
            if (burstRingB.gameObject.activeSelf)
                PaintRing(burstRingB, Color.white, ring * 0.72f, fade * 0.55f);

            var sparks = burstTier >= 15 ? 8 : burstTier >= 10 ? 6 : 4;
            var travel = burstTier >= 20 ? 340f : burstTier >= 15 ? 280f : burstTier >= 10 ? 220f : 160f;
            for (var i = 0; i < burstSparks.Length; i++)
            {
                var spark = burstSparks[i];
                if (i >= sparks)
                {
                    spark.gameObject.SetActive(false);
                    continue;
                }

                spark.gameObject.SetActive(true);
                var angle = i / (float)sparks * Mathf.PI * 2f + burstAge * (burstTier >= 15 ? 2.2f : 1.2f);
                var dist = Mathf.Lerp(24f, travel, Mathf.Clamp01(burstAge / 0.5f));
                spark.rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist * 0.72f);
                var size = Mathf.Lerp(burstTier >= 20 ? 28f : 18f, 8f, t);
                spark.rectTransform.sizeDelta = new Vector2(size, size);
                var color = i % 2 == 0 ? accent : Color.white;
                color.a = fade;
                spark.color = color;
            }

            var shake = (1f - t) * (burstTier >= 20 ? 22f : burstTier >= 15 ? 14f : burstTier >= 10 ? 8f : 4f);
            burstCard.anchoredPosition = new Vector2(Mathf.Sin(burstAge * 46f) * shake, Mathf.Cos(burstAge * 33f) * shake * 0.45f);
            burstCard.localScale = new Vector3(punch, punch, 1f);
            var plate = burstPlate.color;
            plate.a = 0.78f * fade;
            burstPlate.color = plate;
            TintText(burstTitle, fade);
            TintText(burstCount, fade);
            TintText(burstPlus, fade);
            if (board != null)
            {
                var bump = 1f + (1f - Mathf.Clamp01(burstAge / 0.28f)) * (burstTier >= 20 ? 0.04f : 0.022f);
                board.localScale = new Vector3(bump, bump, 1f);
            }
        }

        static void PaintRing(Image ring, Color color, float size, float alpha)
        {
            ring.rectTransform.sizeDelta = new Vector2(size, size * 0.62f);
            color.a = alpha;
            ring.color = color;
        }

        static void TintText(TMP_Text text, float alpha)
        {
            if (text == null)
                return;
            var color = text.color;
            color.a = alpha;
            text.color = color;
        }

        void Pop(string value, float y)
        {
            if (board == null)
                return;
            var rect = UiFactory.Point(board, "Pop", new Vector2(0.5f, y), new Vector2(180f, 48f));
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = UiFactory.Font;
            text.text = value;
            text.fontSize = 32f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = UiTheme.Yellow;
            text.fontStyle = FontStyles.Bold;
            text.raycastTarget = false;
            rect.gameObject.AddComponent<ScoreFloater>();
        }

        void Flash(int x, int y)
        {
            if (flashes == null || x < 0 || y < 0 || x >= flashes.GetLength(0) || y >= flashes.GetLength(1))
                return;
            flashes[x, y] = 0.22f;
        }

        void DropFlashes()
        {
            if (flashes == null)
                return;
            for (var x = 0; x < grid.Width; x++)
            {
                var write = 0;
                for (var y = 0; y < grid.Height; y++)
                {
                    if (!grid.Get(x, y).occupied)
                        continue;
                    if (write != y)
                    {
                        flashes[x, write] = flashes[x, y];
                        flashes[x, y] = 0f;
                    }

                    write++;
                }
            }
        }

        public bool ScreenToCell(Vector2 screen, Camera camera, out Vector2Int cell)
        {
            cell = default;
            if (board == null || grid == null)
                return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(board, screen, camera, out var local))
                return false;
            var rect = board.rect;
            if (rect.width <= 1f || rect.height <= 1f)
                return false;
            var nx = (local.x - rect.xMin) / rect.width;
            var ny = (local.y - rect.yMin) / rect.height;
            if (nx < 0f || ny < 0f || nx >= 1f || ny >= 1f)
                return false;
            cell = new Vector2Int(
                Mathf.Clamp(Mathf.FloorToInt(nx * grid.Width), 0, grid.Width - 1),
                Mathf.Clamp(Mathf.FloorToInt(ny * grid.Height), 0, grid.Height - 1));
            return true;
        }

        void BuildCells()
        {
            cells = new Image[grid.Width, grid.Height];
            marks = new TMP_Text[grid.Width, grid.Height];
            flashes = new float[grid.Width, grid.Height];
            slide = new Vector2[grid.Width, grid.Height];
            burst = new float[grid.Width, grid.Height];
            var insetX = 0.07f / grid.Width;
            var insetY = 0.07f / grid.Height;
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    var min = new Vector2((x + insetX) / grid.Width, (y + insetY) / grid.Height);
                    var max = new Vector2((x + 1f - insetX) / grid.Width, (y + 1f - insetY) / grid.Height);
                    var image = UiFactory.Image(board, "Cell", new Color(0.06f, 0.09f, 0.15f), false, min, max);
                    image.sprite = UiFactory.Tile;
                    image.type = Image.Type.Simple;
                    image.raycastTarget = false;
                    cells[x, y] = image;
                    var mark = UiFactory.Label(image.transform, string.Empty, 20f, Color.white, Vector2.zero, Vector2.one);
                    mark.fontStyle = FontStyles.Bold;
                    marks[x, y] = mark;
                }
            }

            var boardImage = board.GetComponent<Image>();
            if (boardImage != null)
                boardImage.raycastTarget = true;
            var pointer = board.gameObject.AddComponent<PuzzlePointer>();
            pointer.Game = this;
        }

        void Refresh()
        {
            if (cells == null)
                return;

            var empty = new Color(0.06f, 0.09f, 0.15f, 1f);
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (flashes[x, y] > 0f)
                        flashes[x, y] = Mathf.Max(0f, flashes[x, y] - Time.deltaTime);
                    var cell = grid.Get(x, y);
                    var gem = cell.occupied ? ArtLibrary.Gem(cell.typeId) : null;
                    Color color;
                    if (cell.occupied)
                    {
                        cells[x, y].sprite = gem != null ? gem : UiFactory.Tile;
                        color = gem != null ? Color.white : PieceCatalog.ColorFor(cell.typeId, 1);
                        if (flashes[x, y] > 0f)
                            color = Color.Lerp(color, new Color(1f, 0.97f, 0.75f), Mathf.Clamp01(flashes[x, y] / 0.22f));
                    }
                    else
                    {
                        cells[x, y].sprite = UiFactory.Tile;
                        color = empty;
                        if (flashes[x, y] > 0f)
                            color = Color.Lerp(empty, Color.white, Mathf.Clamp01(flashes[x, y] / 0.22f));
                    }
                    var scale = 1f;
                    var alpha = 1f;
                    if (burst != null && burst[x, y] > 0f)
                    {
                        var along = Mathf.Clamp01(burst[x, y]);
                        if (along < 0.4f)
                        {
                            scale = Mathf.Lerp(1f, 1.42f, along / 0.4f);
                            color = Color.Lerp(color, Color.white, along / 0.4f);
                        }
                        else
                        {
                            var fade = (along - 0.4f) / 0.6f;
                            scale = Mathf.Lerp(1.42f, 0.12f, fade);
                            alpha = Mathf.Lerp(1f, 0f, fade);
                        }
                    }
                    else if (hintOn && hintCells != null && hintCells[x, y] && cell.occupied)
                    {
                        var wave = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f);
                        color = Color.Lerp(color, new Color(1f, 0.97f, 0.62f), 0.4f + 0.6f * wave);
                        scale = Mathf.Lerp(1.02f, 1.08f, wave);
                    }
                    else if (dragging && dragOrigin.x == x && dragOrigin.y == y)
                        scale = 1.08f;
                    cells[x, y].preserveAspect = cell.occupied && gem != null;
                    cells[x, y].rectTransform.anchoredPosition = slide != null ? slide[x, y] : Vector2.zero;
                    cells[x, y].transform.localScale = new Vector3(scale, scale, 1f);
                    color.a = alpha;
                    cells[x, y].color = color;
                    marks[x, y].text = string.Empty;
                    marks[x, y].color = Color.white;
                }
            }

            if (scoreLabel != null && session != null && score != null)
            {
                var comboText = combo != null && combo.Chain >= 2 ? $"    Combo x{combo.Multiplier}" : string.Empty;
                scoreLabel.text = $"{Locale.T("Sefer", "Expedition")} {session.campaignIndex}    {Locale.T("Kalan", "Left")} {OccupiedCount()}    {Locale.T("Skor", "Score")} {score.Value}{comboText}";
            }
            if (timerLabel != null)
            {
                var total = Mathf.CeilToInt(timer.Remaining);
                timerLabel.text = $"{total / 60:00}:{total % 60:00}";
                var warn = timer.Remaining <= 15f;
                timerLabel.color = warn ? UiTheme.Heart : UiTheme.Ink;
                var pulse = warn ? 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 8f) : 1f;
                timerLabel.rectTransform.localScale = new Vector3(pulse, pulse, 1f);
            }

            if (comboLabel != null)
            {
                comboLabel.text = combo.Chain >= 2 ? $"Combo x{combo.Multiplier}" : "Combo";
                var pop = combo.Chain >= 2 ? 1f + Mathf.Min(0.35f, combo.Multiplier * 0.04f) : 1f;
                comboLabel.rectTransform.localScale = new Vector3(pop, pop, 1f);
            }

            PaintBurst();
        }

    }

    public sealed class ScoreFloater : MonoBehaviour
    {
        float age;
        Vector2 origin;
        TMP_Text text;

        void Awake()
        {
            text = GetComponent<TMP_Text>();
            origin = ((RectTransform)transform).anchoredPosition;
        }

        void Update()
        {
            age += Time.deltaTime;
            var t = age / 0.7f;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            var color = text.color;
            color.a = 1f - t;
            text.color = color;
            ((RectTransform)transform).anchoredPosition = origin + new Vector2(0f, 48f * t);
        }
    }

    public sealed class PuzzlePointer : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public PuzzleGameManager Game;
        bool tracking;

        public void OnPointerDown(PointerEventData eventData)
        {
            tracking = false;
            if (Game == null || !Game.ScreenToCell(eventData.position, eventData.pressEventCamera, out var origin))
                return;
            tracking = Game.BeginDrag(origin, eventData.position, eventData.pressEventCamera);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!tracking || Game == null)
                return;
            Game.DragGem(eventData.position, eventData.pressEventCamera);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!tracking || Game == null)
                return;
            tracking = false;
            Game.EndDrag();
        }
    }
}
