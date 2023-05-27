using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using GhidraData;
using ImGuiNET;

namespace CorrelateSymbols;

public class Correlator
{
    const int MaxSearchDistance = 1000;
    readonly ProgramData _left;
    readonly ProgramData _right;
    readonly string _savePath;
    readonly Dictionary<GFunction, GFunction> _leftToRight = new();
    readonly Dictionary<GFunction, GFunction> _rightToLeft = new();
    readonly List<Entry> _entries = new();
    readonly byte[] _filter = new byte[256];
    string[]? _leftStrings;
    string[]? _rightStrings;

    float _matchThreshold = 0.2f;
    float _displayThreshold = 0.8f;
    int _searchDistance = 5;
    bool _dirty;
    bool _showMatched = true;
    bool _autoUpdate = true;
    bool _onlyShowNamed;
    bool _groupNamedFirst;
    string _filterText = "";

    record struct Entry(GFunction? Left, GFunction? Right, float Overlap);

    public Correlator(ProgramData left, ProgramData right, string savePath)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
        _savePath = savePath;

        Load();
        Refresh();
    }

    static readonly Regex SaveFormat = new(@"([0-9a-f]{8}) ([^ ]+) ([0-9a-f]{8}) ([^ ]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    void Load()
    {
        if (!File.Exists(_savePath))
        {
            // Match by name
            foreach (var left in _left.Functions)
            {
                if (_right.FunctionLookup.TryGetValue(left.Key, out var right))
                {
                    _leftToRight[left] = right;
                    _rightToLeft[right] = left;
                }
            }

            return;
        }

        _leftToRight.Clear();
        _rightToLeft.Clear();

        int lineNumber = 0;
        foreach (var line in File.ReadAllLines(_savePath))
        {
            var m = SaveFormat.Match(line);
            if (!m.Success)
                throw new InvalidOperationException($"Could not read line {lineNumber} in {_savePath}: \"{line}\"");

            var rightAddr = int.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
            var rightName = m.Groups[2].Value;
            var leftAddr = int.Parse(m.Groups[3].Value, NumberStyles.HexNumber);
            var leftName = m.Groups[4].Value;

            if (!_left.FunctionLookup.TryGetValue(new("", leftName), out var left))
                throw new InvalidOperationException($"Could not find function {leftName} in left data");
            if (!_right.FunctionLookup.TryGetValue(new("", rightName), out var right))
                throw new InvalidOperationException($"Could not find function {rightName} in right data");

            _leftToRight[left] = right;
            _rightToLeft[right] = left;

            lineNumber++;
        }
    }

    void Save()
    {
        using var sw = new StreamWriter(_savePath);
        foreach (var kvp in _leftToRight)
        {
            var left = kvp.Key;
            var right = kvp.Value;
            sw.WriteLine($"{right.Address:X8} {right.Key.Name} {left.Address:X8} {left.Key.Name}");
        }
    }

    void Refresh()
    {
        _dirty = false;
        _entries.Clear();
        var stringCache = new Dictionary<GFunction, string>();

        (GFunction Function, int Index)[] MakeArray(ProgramData program)
        {
            var array = program.Functions
                .OrderByDescending(x => x.Callees.Count + x.Callers.Count)
                .Where(x => !x.IsIgnored)
                .Select(x =>
                {
                    var index = -1;
                    return (x, index);
                })
                .ToArray();

            return array;
        }

        Dictionary<GFunction, int> MakeLookup((GFunction Function, int Index)[] array)
        {
            var lookup = new Dictionary<GFunction, int>();
            for (int i = 0; i < array.Length; i++)
                lookup[array[i].Function] = i;
            return lookup;
        }

        var orderedLeft = MakeArray(_left);
        var orderedRight = MakeArray(_right);
        var lookupLeft = MakeLookup(orderedLeft);
        var lookupRight = MakeLookup(orderedRight);

        void AddEntry(GFunction? left, GFunction? right, float overlap)
        {
            if (left != null && right != null)
            {
                var index1 = lookupLeft[left];
                var index2 = lookupRight[right];
                if (orderedLeft[index1].Index != -1) throw new InvalidOperationException("Tried to reassign index");
                if (orderedRight[index2].Index != -1) throw new InvalidOperationException("Tried to reassign index");
                orderedLeft[index1].Index = index2;
                orderedRight[index2].Index = index1;
            }

            _entries.Add(new Entry(left, right, overlap));
        }

        void Search(bool isCurrentRight, GFunction current, int index, (GFunction Function, int Index)[] ordered, ref float bestOverlap, ref GFunction? best)
        {
            if (index >= ordered.Length) return;
            if (ordered[index].Index != -1) return;

            var other = ordered[index].Function;
            var mapping = isCurrentRight ? _leftToRight : _rightToLeft;
            if (mapping.ContainsKey(other))
                return;

            var overlapLeft = isCurrentRight
                ? GetOverlap(other, current, stringCache)
                : GetOverlap(current, other, stringCache);

            if (!(overlapLeft > bestOverlap))
                return;

            bestOverlap = overlapLeft;
            best = other;
        }

        int i = 0;
        int j = 0;
        while (i < orderedLeft.Length && j < orderedRight.Length)
        {
            var left = orderedLeft[i].Function;
            var right = orderedRight[j].Function;

            if (_leftToRight.TryGetValue(left, out var match)) // Exact match left?
            {
                AddEntry(left, match, 1.0f);
                i++;
            }
            else if (_rightToLeft.TryGetValue(right, out match)) // Exact match right?
            {
                AddEntry(match, right, 1.0f);
                j++;
            }
            else // Find the best match on either side within the search distance
            {
                float bestOverlapLeft = 0.0f;
                float bestOverlapRight = 0.0f;
                GFunction? bestLeft = null;
                GFunction? bestRight = null;

                for (int d = 0; d < _searchDistance; d++)
                {
                    Search(true, right, i + d, orderedLeft, ref bestOverlapLeft, ref bestLeft);
                    Search(false, left, j + d, orderedRight, ref bestOverlapRight, ref bestRight);
                }

                if (bestOverlapLeft > bestOverlapRight && bestOverlapLeft > _matchThreshold)
                {
                    AddEntry(bestLeft, right, bestOverlapLeft);
                    j++;
                }
                else if (bestOverlapRight > bestOverlapLeft && bestOverlapRight > _matchThreshold)
                {
                    AddEntry(left, bestRight, bestOverlapRight);
                    i++;
                }
                else if (i > j)
                {
                    AddEntry(null, right, 0.0f); // Best match wasn't good enough - emit an umatched line
                    j++;
                }
                else
                {
                    AddEntry(left, null, 0.0f); // Best match wasn't good enough - emit an umatched line
                    i++;
                }
            }

            // Skip any entries that have already matched
            while (i < orderedLeft.Length && orderedLeft[i].Index != -1) i++;
            while (j < orderedRight.Length && orderedRight[j].Index != -1) j++;
        }

        // Handle unmatched trailing entries
        for (;i < orderedLeft.Length; i++)
            if (orderedLeft[i].Index == -1)
                _entries.Add(new Entry(orderedLeft[i].Function, null, 0));

        for (;j < orderedRight.Length; j++)
            if (orderedRight[j].Index == -1)
                _entries.Add(new Entry(orderedRight[j].Function, null, 0));

        _leftStrings = _entries.Select(x => LeftString(x.Left, true)).ToArray();
        _rightStrings = _entries.Select(x => RightString(x.Right, true)).ToArray();
    }

    float GetOverlap(GFunction left, GFunction right, Dictionary<GFunction, string> cache)
    {
        if (!cache.TryGetValue(left, out var leftString))
        {
            leftString = LeftString(left, false);
            cache[left] = leftString;
        }

        if (!cache.TryGetValue(right, out var rightString))
        {
            rightString = RightString(right, false);
            cache[right] = rightString;
        }

        if (leftString.Length == rightString.Length && leftString.Length == 0)
            return 0.0f;

        int distance = Util.LevenshteinDistance(leftString, rightString);
        float d = 1.0f - (float)distance / Math.Max(leftString.Length, rightString.Length);
        return d;
    }

    public void Draw()
    {
        ImGui.DragInt("Search Distance", ref _searchDistance, 1, 1, MaxSearchDistance);
        ImGui.DragFloat("Match Threshold", ref _matchThreshold, 0.001f, 0, 1.0f);
        ImGui.DragFloat("Display Threshold", ref _displayThreshold, 0.001f, 0, 1.0f);
        if (ImGui.Button("Rebuild") || (_autoUpdate && _dirty))
            Refresh();

        ImGui.SameLine();
        if (ImGui.Button("Save"))
            Save();

        ImGui.SameLine();
        ImGui.Checkbox("Auto-refresh", ref _autoUpdate);

        ImGui.SameLine();
        ImGui.Checkbox("Show matched", ref _showMatched);

        ImGui.SameLine();
        ImGui.Checkbox("Only show named", ref _onlyShowNamed);

        ImGui.SameLine();
        if (ImGui.Checkbox("Group named first", ref _groupNamedFirst))
            Refresh();

        ImGui.SameLine();
        if (ImGui.Button("Load"))
        {
            Load();
            Refresh();
        }

        if (ImGui.InputText("Filter", _filter, (uint)_filter.Length))
        {
            _filterText = Encoding.UTF8.GetString(_filter);
            int index = _filterText.IndexOf('\0');
            if (index != -1)
                _filterText = _filterText[..index];
        }

        if (_leftStrings == null || _rightStrings == null)
            return;

        ImGui.Columns(4);
        ImGui.SetColumnWidth(0, 30);
        ImGui.SetColumnWidth(1, 50);
        for (int i = 0; i < _entries.Count; i++)
        {
            var left = _entries[i].Left;
            var right = _entries[i].Right;
            var overlap = _entries[i].Overlap;
            if (overlap < _displayThreshold)
                continue;

            if (!_showMatched && overlap == 1.0f)
                continue;

            if (_onlyShowNamed && left?.IsNamed != true && right?.IsNamed != true)
                continue;

            if (_filterText.Length > 0)
            {
                bool show = 
                    left != null && left.Key.Name.Contains(_filterText) 
                 || right != null && right.Key.Name.Contains(_filterText);

                if (!show)
                    continue;
            }

            ImGui.PushStyleColor(ImGuiCol.Text, Color(overlap));

            if (left != null && right != null)
            {
                var matched = _leftToRight.ContainsKey(left);
                if (matched)
                {
                    if (ImGui.Button($"-##{i}"))
                    {
                        _leftToRight.Remove(left);
                        _rightToLeft.Remove(right);
                        _dirty = true;
                    }
                }
                else
                {
                    if (ImGui.Button($"+##{i}"))
                    {
                        _leftToRight[left] = right;
                        _rightToLeft[right] = left;
                        _dirty = true;
                    }
                }
            }
            else
                ImGui.TextUnformatted(".");

            ImGui.NextColumn();
            ImGui.TextUnformatted(overlap.ToString("F2"));

            ImGui.NextColumn();
            ImGui.TextUnformatted(_leftStrings[i]);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted($"{_entries[i].Left?.Address:X8} {_entries[i].Left?.Key.Name}");
                ImGui.EndTooltip();
            }

            ImGui.NextColumn();
            ImGui.TextUnformatted(_rightStrings[i]);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted($"{_entries[i].Left?.Address:X8} {_entries[i].Left?.Key.Name}");
                ImGui.EndTooltip();
            }
            ImGui.NextColumn();
            ImGui.PopStyleColor();
        }
    }

    static readonly Vector4 Zero          = new(0.5f,  0.5f, 0.5f, 1.0f); // grey
    static readonly Vector4 Quarter       = new(0.75f, 0.5f, 0.5f, 1.0f); // half-red
    static readonly Vector4 Half          = new(1.0f,  0.5f, 0.5f, 1.0f); // red
    static readonly Vector4 ThreeQuarters = new(1.0f,  1.0f, 0.5f, 1.0f); // yellow
    static readonly Vector4 One           = new(0.5f,  1.0f, 0.5f, 1.0f); // green
    static readonly Vector4 Exact         = new(0.5f,  0.5f, 1.0f, 1.0f); // blue
    static Vector4 Lerp(Vector4 a, Vector4 b, float t) => a + (b - a) * t;
    static Vector4 Color(float t)
    {
        int quadrant = (int)(t * 4);
        float remainder = 4 * (t - quadrant * 0.25f);
        return quadrant switch
        {
            0 => Lerp(Zero, Quarter, remainder),
            1 => Lerp(Quarter, Half, remainder),
            2 => Lerp(Half, ThreeQuarters, remainder),
            3 => Lerp(ThreeQuarters, One, remainder),
            _ => Exact
        };
    }

    void LeftList(StringBuilder sb, int startingIndex, IEnumerable<GFunction> list)
    {
        bool first = true;
        int lastIndex = startingIndex;
        foreach (var c in list.OrderBy(x => x.Index))
        {
            if (!first)
                sb.Append(' ');
            first = false;

            if (_leftToRight.ContainsKey(c))
                sb.Append(c.Key.Name);
            else
                sb.Append(c.Index - lastIndex);

            lastIndex = c.Index;
        }
    }

    void OrderedLeftList(StringBuilder sb, int startingIndex, IEnumerable<GFunction> list)
    {
        var parts = new List<string>();

        int lastIndex = startingIndex;
        foreach (var c in list.OrderBy(x => x.Index))
        {
            parts.Add(_leftToRight.ContainsKey(c) ? c.Key.Name : (c.Index - lastIndex).ToString());
            lastIndex = c.Index;
        }

        parts.Sort();
        for (int i = 0; i < parts.Count; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(parts[i]);
        }
    }

    string LeftString(GFunction? left, bool withName)
    {
        if (left == null)
            return "...";

        var sb = new StringBuilder();
        if (withName)
            sb.AppendFormat("{0:D3} {1:D3} {2}: ", left.Callees.Count, left.Callers.Count, left.Key.Name);
        else
            sb.AppendFormat("{0:D3} {1:D3} ", left.Callees.Count, left.Callers.Count);

        if (_groupNamedFirst)
        {
            OrderedLeftList(sb, left.Index, left.Callees);
            sb.Append(" | ");
            OrderedLeftList(sb, left.Index, left.Callers);
        }
        else
        {
            LeftList(sb, left.Index, left.Callees);
            sb.Append(" | ");
            LeftList(sb, left.Index, left.Callers);
        }

        return sb.ToString();
    }

    void RightList(StringBuilder sb, int startingIndex, IEnumerable<GFunction> list)
    {
        bool first = true;
        int lastIndex = startingIndex;
        foreach (var c in list.OrderBy(x => x.Index))
        {
            if (!first)
                sb.Append(' ');
            first = false;

            if (_rightToLeft.TryGetValue(c, out var left))
                sb.Append(left.Key.Name);
            else
                sb.Append(c.Index - lastIndex);

            lastIndex = c.Index;
        }
    }

    void OrderedRightList(StringBuilder sb, int startingIndex, IEnumerable<GFunction> list)
    {
        var parts = new List<string>();

        int lastIndex = startingIndex;
        foreach (var c in list.OrderBy(x => x.Index))
        {
            parts.Add(_rightToLeft.TryGetValue(c, out var left) ? left.Key.Name : (c.Index - lastIndex).ToString());
            lastIndex = c.Index;
        }

        parts.Sort();
        for (int i = 0; i < parts.Count; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(parts[i]);
        }
    }

    string RightString(GFunction? right, bool withName)
    {
        if (right == null)
            return "...";

        var sb = new StringBuilder();
        if (withName)
            sb.AppendFormat("{0:D3} {1:D3} {2}: ", right.Callees.Count, right.Callers.Count, right.Key.Name);
        else
            sb.AppendFormat("{0:D3} {1:D3} ", right.Callees.Count, right.Callers.Count);

        if (_groupNamedFirst)
        {
            OrderedRightList(sb, right.Index, right.Callees);
            sb.Append(" | ");
            OrderedRightList(sb, right.Index, right.Callers);
        }
        else
        {
            RightList(sb, right.Index, right.Callees);
            sb.Append(" | ");
            RightList(sb, right.Index, right.Callers);
        }

        return sb.ToString();
    }
}