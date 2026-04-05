using IoListWallet.MainControls.Panels;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static IoListWallet.App;
using static IoListWallet.AppSave;

namespace IoListWallet.Structs;

using BucketItem = (object Key, IComparable Compare, IComparable Compare2, GroupBorder? Group, byte Selected, string[] Texts);
using FilterItem = (ulong Match, IComparable Compare, GroupBorder? Group);

internal class IoFilter//(List<FilterType> Filters, ushort SortIndex, ushort SortIndex2, bool Increasing, bool Increasing2)
{
    [Save] internal List<FilterType> Filters { get; set { field = value; IoPanel.Dirty(); } } = [];
    [Save] internal ushort SortIndex { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } // which Filter will be primary sorted?
    [Save] internal ushort SortIndex2 { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } // which Filter will be secondary sorted?
    [Save] internal bool Increasing { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
    [Save] internal bool Increasing2 { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
    [Save] internal byte Size { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = 1;
    internal void SetSize(byte size) {
        Size = size;
        foreach (var f in Filters)
            f.SetSize(size);
    }
    internal void RemoveSearch(byte index) {
        --Size;
        foreach (var f in Filters)
            f.RemoveSearch(index);
    }
}
internal class FilterType//(string name, ulong disabled, bool group) 
{
    [Save] internal string Name { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = "";
    [Save] internal ulong Disabled { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = ~(ulong)0; // full bitmask
    [Save] internal bool Group { get; set { field = value; IoPanel.Dirty(); } } = false;
    internal virtual FilterItem Process(string data) => (Disabled, default!, null);
    internal virtual void SetSize(int size) { }
    internal virtual ulong RemoveSearch(int index) {
        var mask = (((ulong)1 << index) - 1);
        Disabled = (Disabled & mask) | ((Disabled >> 1) & ~mask);
        return mask;
    }
}
// Data = "Text"
internal class FilterText : FilterType {
    [Save] internal string dRegexp { get; set { field = value; IoPanel.Dirty(); } } = "";
    [Save] internal List<string> Regexp { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = [""];
    internal override FilterItem Process(string Data) {
        ulong mask = 0;
        for (int i = 0; i < Regexp.Count; ++i)
            mask |= Regex.IsMatch(Data, Regexp[i]) ? (ulong)1 << i : 0;
        return (mask | base.Process(Data).Match, Data, Group && Data.Length > 0 ? new GroupBorderText(Data[0]) : null);
    }
    internal override void SetSize(int size) { 
        while (Regexp.Count < size) Regexp.Add(dRegexp); 
        while (Regexp.Count > size) Regexp.RemoveAt(Regexp.Count - 1);
    }
    internal override ulong RemoveSearch(int index) {
        Regexp.RemoveAt(index);
        return base.RemoveSearch(index);
    }
}
// Data = (long)Value
internal class FilterNumber : FilterType {
    internal struct R {
        public R() { Min = 0; Max = ulong.MaxValue; }
        public R(ulong min, ulong max) { Min = min; Max = max; }
        [Save] internal ulong Min { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
        [Save] internal ulong Max { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
    }
    [Save] internal R dRange { get; set { field = value; IoPanel.Dirty(); } } = new();
    [Save] internal List<R> Range { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = [new()];
    [Save] internal ulong MinMin { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = 0;
    [Save] internal ulong MaxMin { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = ulong.MaxValue;
    [Save] internal ulong MinMax { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = 0;
    [Save] internal ulong MaxMax { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = ulong.MaxValue;
    internal override FilterItem Process(string Data) {
        ulong mask = 0;
        var parsed = ulong.TryParse(Data, out var n);
        for (int i = 0; i < Range.Count; ++i)
            mask |= parsed && n >= Range[i].Min & n <= Range[i].Max ? (ulong)1 << i : 0;
        return (mask | base.Process(Data).Match, n, Group ? new GroupBorderNumber(n) : null);
    }
    internal override void SetSize(int size) { 
        while (Range.Count < size) Range.Add(dRange);
        while (Range.Count > size) Range.RemoveAt(Range.Count - 1);
    }
    internal override ulong RemoveSearch(int index) {
        Range.RemoveAt(index);
        return base.RemoveSearch(index);
    }
}
// Data = (int)Epoch;(DateTime)DateTime
internal class FilterEpoch : FilterType {
    internal struct R {
        public R() { Min = 0; Max = ushort.MaxValue; }
        public R(ushort min, ushort max) { Min = min; Max = max; }
        [Save] internal ushort Min { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
        [Save] internal ushort Max { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
    }
    [Save] internal R dRange { get; set { field = value; IoPanel.Dirty(); } } = new();
    [Save] internal List<R> Range { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = [new()];
    internal override FilterItem Process(string Data) {
        var S = Data.Split(";");
        DateTime.TryParse(S[1], out var d);
        ulong mask = 0;
        var parsed = ushort.TryParse(S[0], out var n);
        for (int i = 0; i < Range.Count; ++i)
            mask |= parsed && n >= Range[i].Min & n <= Range[i].Max ? (ulong)1 << i : 0;
        return (mask | base.Process(Data).Match, d, Group ? new GroupBorderEpoch(n) : null);
    }
    internal override void SetSize(int size) { 
        while (Range.Count < size) Range.Add(dRange); 
        while (Range.Count > size) Range.RemoveAt(Range.Count - 1); 
    }
    internal override ulong RemoveSearch(int index) {
        Range.RemoveAt(index);
        return base.RemoveSearch(index);
    }
}
// Data = (int)Enum
internal class FilterEnum : FilterType {
    internal struct R {
        public R() { Match = 0; Rule = (byte)CompareRule.Equal; }
        public R(ushort match, byte rule) { Match = match; Rule = rule; }
        [Save] internal ushort Match { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
        [Save] internal byte Rule { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
    }
    [Save] internal R dRule { get; set { field = value; IoPanel.Dirty(); } } = new();
    [Save] internal List<R> Rule { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = [new()];
    [Save] internal string EnumType { get; set { field = value; IoPanel.Dirty(); } } = "";
    [Save] internal ushort Min { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = 0;
    [Save] internal ushort Max { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = ushort.MaxValue;

    internal override FilterItem Process(string Data) {
        ulong mask = 0;
        var parsed = ushort.TryParse(Data, out var m);
        for (int i = 0; i < Rule.Count; ++i)
            mask |= parsed && ((CompareRule)Rule[i].Rule switch {
                CompareRule.Less => m < Rule[i].Match,
                CompareRule.NotMore => m <= Rule[i].Match,
                CompareRule.Equal => m == Rule[i].Match,
                CompareRule.NotLess => m >= Rule[i].Match,
                CompareRule.More => m > Rule[i].Match,
                _ => m != Rule[i].Match
            }) ? (ulong)1 << i : 0;
        return (mask | base.Process(Data).Match, m, Group ? new GroupBorderEnum(m, EnumType) : null);
    }
    internal override void SetSize(int size) { 
        while (Rule.Count < size) Rule.Add(dRule); 
        while (Rule.Count > size) Rule.RemoveAt(Rule.Count - 1); 
    }
    internal override ulong RemoveSearch(int index) {
        Rule.RemoveAt(index);
        return base.RemoveSearch(index);
    }
}
// Data = (DateTime)DateTime
internal class FilterDate : FilterType {
    internal struct R {
        public R() => Min = Max = DateTime.Now; 
        public R(DateTime min, DateTime max) { Min = min; Max = max; }
        [Save] internal DateTime Min { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
        [Save] internal DateTime Max { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
    }
    [Save] internal R dRange { get; set { field = value; IoPanel.Dirty(); } } = new();
    [Save] internal List<R> Range { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = [new()];
    internal override FilterItem Process(string Data) {
        var parsed = DateTime.TryParse(Data, out var d);
        ulong mask = 0;
        for (int i = 0; i < Range.Count; ++i)
            mask |= parsed && d >= Range[i].Min & d <= Range[i].Max ? (ulong)1 << i : 0;
        return (mask | base.Process(Data).Match, d, Group ? new GroupBorderDate(d) : null);
    }
    internal override void SetSize(int size) {
        while (Range.Count < size) Range.Add(dRange); 
        while (Range.Count > size) Range.RemoveAt(Range.Count - 1); 
    }
    internal override ulong RemoveSearch(int index) {
        Range.RemoveAt(index);
        return base.RemoveSearch(index);
    }
}
// Data = "Token";(long)Value
internal class FilterToken : FilterType {
    internal struct R {
        public R() { Ticker = ""; Min = 0; Max = ulong.MaxValue; }
        public R(string ticker, ulong min, ulong max) { Ticker = ticker; Min = min; Max = max; }
        [Save] internal string Ticker { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
        [Save] internal ulong Min { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
        [Save] internal ulong Max { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } }
    }
    [Save] internal R dCond { get; set { field = value; IoPanel.Dirty(); } } = new();
    [Save] internal ulong DisabledValues { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = ~(ulong)0; // full bitmask
    [Save] internal List<R> Cond { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = [new()];
    [Save] internal ulong MinMin { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = 0;
    [Save] internal ulong MaxMin { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = ulong.MaxValue;
    [Save] internal ulong MinMax { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = 0;
    [Save] internal ulong MaxMax { get; set { if (field == value) return; field = value; IoPanel.Dirty(); } } = ulong.MaxValue;
    internal override FilterItem Process(string Data) {
        var S = Data.Split(";");
        ulong MaxV = 0;
        ulong[] v = new ulong[S.Length];
        bool[] p = new bool[S.Length];
        for (int d = 0; d < Data.Length; ++d)
            p[d] = ulong.TryParse(S[d], out v[d]);
        ulong mask = 0;
        var parsed = ulong.TryParse(Data, out var n);
        for (int i = 0; i < Cond.Count; ++i) {
            bool success = false;
            for (int d = 0; d < Data.Length; ++d)
                if (S[d++].Contains(Cond[i].Ticker) && p[d]) {
                    var _v = v[d];
                    if ((DisabledValues & ((ulong)1 << i)) > 0 || (_v >= Cond[i].Min && _v <= Cond[i].Max)) {
                        success = true;
                        if (v[d] > MaxV)
                            MaxV = v[d];
                    }
                }
            mask |= parsed && success ? (ulong)1 << i : 0;
        }
        return (mask | base.Process(Data).Match, MaxV, Group ? new GroupBorderNumber(MaxV) : null);
    }
    internal override void SetSize(int size) { 
        while (Cond.Count < size) Cond.Add(dCond); 
        while (Cond.Count > size) Cond.RemoveAt(Cond.Count - 1);
    }
    internal override ulong RemoveSearch(int index) {
        Cond.RemoveAt(index);
        var mask = base.RemoveSearch(index);
        DisabledValues = (Disabled & mask) | ((DisabledValues >> 1) & ~mask);
        return mask;
    }
}
internal abstract class GroupBorder {
    internal string Text = "";
}
internal class GroupBorderText : GroupBorder {
    internal GroupBorderText(char c) => Text = c.ToString();
}
internal class GroupBorderNumber : GroupBorder {
    internal GroupBorderNumber(ulong i) {
        // Text = "10^" + 
        var log = (int)Math.Log10(i);
        var mag = (log - 1) / 3;
        if (mag > 0) {
            i >>= (mag - 1) * 3;
            Text = (i % 1000).ToString();
            i /= 1000;
            if (i <= 0)
                return;
            Text = i.ToString() + "." + Text + mag switch {
                0 => "",
                1 => "K",
                2 => "M",
                3 => "B",
                4 => "T",
                5 => "q",
                6 => "Q",
                7 => "s",
                8 => "S",
                9 => "O",
                10 => "N",
                11 => "D",
                12 => "U",
                _ => ""
            } + " (E" + log.ToString() + ")";
        } else Text = i.ToString();
    }
}
internal class GroupBorderEnum : GroupBorder {
    internal GroupBorderEnum(ushort i, string t) 
        => Text = ETypes.TryGetValue(t, out var type) ? GetEnumText((Enum)Enum.ToObject(type, i)) : "";
}
internal class GroupBorderDate : GroupBorder {
    internal GroupBorderDate(DateTime i) => Text = i.Date.ToShortDateString();
}
internal class GroupBorderEpoch : GroupBorder {
    internal GroupBorderEpoch(ushort i) => Text = i.ToString();
}
internal enum CompareRule : byte {
    Less = 0,
    NotMore = 1,
    Equal = 2,
    NotLess =3,
    More = 4,
    NotEqual = 5
}

internal enum SortType : byte {
    Logarithmic = 0,
    Linear = 1,
    Alphabetic = 2
}
// TODO use Minimal constructor fof nonBucket
internal class Bucket {
    internal static Comparison<BucketItem> Compare(bool Inc, bool Inc2) => Inc
        ? Inc2
            ? (a, b) => { var c = a.Compare.CompareTo(b.Compare); return c != 0 ? c : a.Compare2.CompareTo(b.Compare2); }
            : (a, b) => { var c = a.Compare.CompareTo(b.Compare); return c != 0 ? c : b.Compare2.CompareTo(a.Compare2); }
        : Inc2
            ? (a, b) => { var c = b.Compare.CompareTo(a.Compare); return c != 0 ? c : a.Compare2.CompareTo(b.Compare2); }
            : (a, b) => { var c = b.Compare.CompareTo(a.Compare); return c != 0 ? c : b.Compare2.CompareTo(a.Compare2); };

    //internal Comparison<BucketItem> Compare;
    // = (a, b) => { var c = a.Compare.CompareTo(b.Compare); return c != 0 ? c : a.Compare2.CompareTo(b.Compare2); };

    internal BucketItem[]? ResultArray = null;

    internal ushort Depth = 0, Depth2 = 0;
    internal SortType SortType, SortType2;
    internal int FillingTask = -1;
    internal ushort Tasks = 0, SplitChildren = 0;
    internal Bucket[] SplitBucket = new Bucket[1 << (GetProfile?.SplitChildren ?? DPro.SplitChildren)];
    internal Bucket? Next = null;// Introsorted = null;
    internal List<BucketItem>[] Elements;
    internal IComparable Min = 0, Max = 0, Min2 = 0, Max2 = 0;
    internal IComparable[] MinT, MaxT, MinT2, MaxT2;
    //internal HashSet<char> Chars, Chars2;
    internal byte Sorted = 2;
    internal int Count = 0;
    internal Bucket(Bucket MoveFrom) {
        Elements = MoveFrom.Elements;
        MinT = MoveFrom.MinT;
        MaxT = MoveFrom.MaxT;
        MinT2 = MoveFrom.MinT2;
        MaxT2 = MoveFrom.MaxT2;
        Min = MoveFrom.Min;
        Max = MoveFrom.Max;
        Count = MoveFrom.Count;
        Sorted = MoveFrom.Sorted;
        SortType = MoveFrom.SortType;
        SortType2 = MoveFrom.SortType2;
        SplitBucket = new Bucket[1 << (GetProfile?.SplitChildren ?? DPro.SplitChildren)];
    }
    internal Bucket(ushort UsedTasks, SortType sortType, SortType sortType2, ushort depth = 0, ushort depth2 = 0, bool init = false) {
        SplitBucket = new Bucket[1 << (GetProfile?.SplitChildren ?? DPro.SplitChildren)];
        MinT = new IComparable[UsedTasks];
        MaxT = new IComparable[UsedTasks];
        MinT2 = new IComparable[UsedTasks];
        MaxT2 = new IComparable[UsedTasks];
        SortType = sortType;
        SortType2 = sortType2;
        Elements = new List<BucketItem>[UsedTasks];
        if (init)
            for (int t = 0; t < UsedTasks; ++t) 
                Elements[t] = [];
    }
    internal Bucket(ushort UsedTasks, SortType SortType, SortType SortType2) {
        SplitBucket = [];//new Bucket[1 << (GetProfile?.SplitChildren ?? DPro.SplitChildren)];
        MinT = new IComparable[UsedTasks];
        MaxT = new IComparable[UsedTasks];
        MinT2 = new IComparable[UsedTasks];
        MaxT2 = new IComparable[UsedTasks];
        this.SortType = SortType;
        this.SortType2 = SortType2;
        Elements = new List<BucketItem>[UsedTasks];
    }
    // Use this one for IntroMerge sort
    internal void InsertThreadedSimple(ushort taskIndex, BucketItem E) {
        Elements[taskIndex].Add(E);
        Interlocked.Increment(ref Count);
    }
    // USe this one for BucketIntro sort
    internal void InsertThreaded(ushort taskIndex, BucketItem E) {
        if (SortType == SortType.Alphabetic) {
            var C = E.Compare as string ?? "";
            if (C.Length > (int)MaxT[taskIndex])
                MaxT[taskIndex] = C.Length;
        } else {
            if (Elements[taskIndex].Count > 0) {
                if (E.Compare.CompareTo(MinT[taskIndex]) < 0)
                    MinT[taskIndex] = E.Compare;
                if (E.Compare.CompareTo(MaxT[taskIndex]) > 0)
                    MaxT[taskIndex] = E.Compare;
            } else MinT[taskIndex] = MaxT[taskIndex] = E.Compare;
        }
        if (SortType2 == SortType.Alphabetic) {
            var C = E.Compare2 as string ?? "";
            if (C.Length > (int)MaxT2[taskIndex])
                MaxT2[taskIndex] = C.Length;
        } else {
            if (Elements[taskIndex].Count > 0) {
                if (E.Compare2.CompareTo(MinT2[taskIndex]) < 0)
                    MinT2[taskIndex] = E.Compare2;
                if (E.Compare2.CompareTo(MaxT2[taskIndex]) > 0)
                    MaxT2[taskIndex] = E.Compare2;
            } else MinT[taskIndex] = MaxT[taskIndex] = E.Compare;
        }
        InsertThreadedSimple(taskIndex, E);
    }
    // call this after all elements were added via InsertThreaded
    internal void CollapseMinMax() {
        if (Elements.Length <= 0)
            return;
        Max = MaxT[0];
        Max2 = MaxT2[0];
        for (int t = 1; t < Elements.Length; ++t)
            if (MaxT[t].CompareTo(Max) > 0) {
                Max = MaxT[t];
                Max2 = MaxT2[t];
            }
        if (SortType == SortType.Alphabetic) {
            // TODO collect occurences at depth
        } else { 
            Min = MinT[0]; // start with list[0], then compare with all the other list
            for (int t = 1; t < Elements.Length; ++t) 
                if (MinT[t].CompareTo(Min) < 0)
                    Min = MinT[t];
        }
        if (SortType2 == SortType.Alphabetic) {
            // TODO collect occurences at depth
        } else {
            Min2 = MinT2[0]; // start with list[0], then compare with all the other list
            for (int t = 1; t < Elements.Length; ++t)
                if (MinT2[t].CompareTo(Min2) < 0)
                    Min2 = MinT2[t];
        }
        Sorted = (byte)(SortType == SortType.Alphabetic
            ? Count <= 1 || Depth >= (ushort)Max ? 2 : 0
            : Min == Max 
                ? SortType2 == SortType.Alphabetic 
                    ? Count <= 1 || Depth2 >= (ushort)Max2 ? 1 : 0 
                    : Min2 == Max2 ? 2 : 1
                : 0);
    }
    internal uint TotalCount() {
        for (Bucket? b = Next; b != null; b = b.Next)
            Count += b.Count;
        return (uint)Count;
    }
    internal List<BucketItem> ConcatAll() {
        /*int NewCount = 0; // this Introsorted branching was used for the old Greedy version of the distribution algorithm
        for (Bucket? b = this; b != null; b = b.Next)
            for (Bucket? i = b; i != null; i = i.Introsorted)
                NewCount += i.Count;
        Count = NewCount;*/
        for (Bucket? b = Next; b != null; b = b.Next)
            Count += b.Count;
        List<BucketItem> result = new(Count); 
        for (Bucket? b = this; b != null; b = b.Next)
            //for (Bucket? i = b; i != null; i = i.Introsorted)
                foreach (var T in b.Elements)
                    result.AddRange(T);
        Elements = [result];
        /*Introsorted = */Next = null; // unlink since I've absorbed the whole Next->Next... LinkedList
        return result;
    }
    // concat all task[0..Used] subbuckets into task[0] bucket
    internal void Concat() {
        if (Elements.Length == 0 || Elements[0] == null || Elements[0].Count == Count)
            return; // already concatenated
        List<BucketItem> result = new(Count);
        foreach (var T in Elements)
            result.AddRange(T);
        Elements = [result];
    }
}
internal static class IntroSort {
    internal static void Sort(Bucket input, bool Inc1, bool Inc2) {
        var Compare = Bucket.Compare(Inc1, Inc2);
        input.Concat(); // need to concat the lists together to intro sort them
        if(input.Elements != null && input.Elements.Length > 0)
            input.Elements[0].Sort(Compare);
    }
}

// 132 lines
internal static class ParallelIntroMergeSort {
    [StructLayout(LayoutKind.Sequential)]
    internal class Merge { // the rest of the properties also need FieldOffset!!!
        internal Merge? A, B;
        internal Memory<BucketItem> AList, BList, OutList;
        internal BucketItem[] OutArray;
        internal int privateOutDone = 0;
        private long p1, p2, p3, p4, p5, p6, p7, p8; // Padding to push sharedOutDone to a new cache line (64-128 bytes)
        internal int sharedOutDone = 0;
        internal Merge(Merge? A, Merge? B, Memory<BucketItem> AL, Memory<BucketItem> BL) {
            this.A = A; this.B = B;
            OutList = new Memory<BucketItem>(OutArray = new BucketItem[(AList = AL).Span.Length + (BList = BL).Span.Length]);
        }
    }
    // input.Elements = List<BucketItem>[], input.Count = Sum(input.Elements[all].Count)
    private static ushort MergeSpin = 0, MergeVolatileBuffer = 0;
    internal static void Sort(Bucket input, bool Inc1, bool Inc2, CancellationToken? cancel) {
        if (input.Elements.Length < 1 || input.Count < 2)
            return; // if empty or already sorted (only 0-1 elements)
        MergeVolatileBuffer = (GetProfile?.MergeVolatileBuffer ?? DPro.MergeVolatileBuffer);
        MergeSpin = (GetProfile?.MergeSpin ?? DPro.MergeSpin);
        var compare = Bucket.Compare(Inc1, Inc2);
        ushort usedTasks = RequestFreeTasks(ushort.MaxValue, 1), chunks = (ushort)(usedTasks + 1), chunkSize = (ushort)Math.Ceiling((float)input.Count / chunks);
        int output = 0, usedOutput = 0, cyc = -1, chunkOffset = 0,
            VolatileBufferMask = (1 << MergeVolatileBuffer) - 1;
        BucketItem[] flat = new BucketItem[input.Count];
        foreach (var e in input.Elements) {
            e.CopyTo(flat, chunkOffset);
            chunkOffset += e.Count;
        }
        Memory<BucketItem>[] chunk = new Memory<BucketItem>[chunks];
        for (ushort i = 0; i < usedTasks; ++i) 
            chunk[i] = new Memory<BucketItem>(flat, i * chunkSize, chunkSize);
        var usedFlat = usedTasks * chunkSize;
        chunk[usedTasks] = new Memory<BucketItem>(flat, usedFlat, flat.Length - usedFlat);
        // IntroSort the chunks
        Task[] tasks = new Task[usedTasks];
        for (int t = 0; t < usedTasks; ++t) {
            int localT = t; // Parallel chunk[1 - UsedTasks]:
            tasks[t] = Task.Run(() => chunk[localT].Span.Sort(compare));
        }
        chunk[0].Span.Sort(compare); // This thread will take the chunk[0]
        Task.WaitAll(tasks);
        // Initialize job data:
        bool isMem = false;
        Memory<BucketItem> mem = chunk[0];
        var taskData = new List<(Merge, Merge?, Merge?, Memory<BucketItem>, Memory<BucketItem>)>[chunks];
        for (int i = 0; i < chunks; ++i)
            taskData[i] = [];
        Merge? memM = null;
        List<Merge> merge = [];
        // Merge all pairs of chunks:
        for (int i = chunks; 0 <= --i;)
            if (isMem = !isMem) 
                mem = chunk[i];
            else {
                merge.Add(new Merge(null, null, mem, chunk[i]));
                ++output;
            }
        // Merge all remaining pair of merges:
        while (output > usedOutput)
            if (isMem = !isMem) 
                mem = (memM = merge[usedOutput++]).OutList;
            else {
                merge.Add(new Merge(memM, merge[usedOutput], mem, merge[usedOutput++].OutList));
                ++output;
            }
        // Assign those Merge jobs to the SplitTasks in optimal pattern to make all avaliable jobs busy without racing for the most distibuted time:
        void AddJobs(Merge m) {
            if (m.B != null) {
                if(m.A != null)
                    AddJobs(m.A);
                AddJobs(m.B);
            } // else if(m.A != null) AddJobs(m.A); // if B is null, then A must be null as well
            taskData[cyc = (cyc + 1) % chunks].Add((m, m.A, m.B, m.AList, m.BList));
        }
        AddJobs(merge[merge.Count - 1]);
        // Process the jobs:
        void ProcessData(List<(Merge MergeO, Merge? MergeA, Merge? MergeB, Memory<BucketItem> InputA, Memory<BucketItem> InputB)> data) {
            foreach (var (MergeO, MergeA, MergeB, InputA, InputB) in data) {
                if (cancel?.IsCancellationRequested ?? false) {
                    Volatile.Write(ref MergeO.sharedOutDone, MergeO.OutArray.Length);
                    return;
                }
                int a = 0, b = 0;
                bool isA = MergeA != null, isB = MergeB != null;
                bool first = true;
                Span<BucketItem> spanA = InputA.Span, spanB = InputB.Span;
                int aDone = 0, bDone = 0;
                while (true) {
                    // Check if we have to wait for lower merge node to finish more items.
                    // Volatile.Read is batched, and only updated after the last update is finished
                    if ((isA && a < MergeA.OutArray.Length && a >= aDone && a >= (aDone = Volatile.Read(ref MergeA.sharedOutDone)))
                        || (isB && b < MergeB.OutArray.Length && b >= bDone && b >= (bDone = Volatile.Read(ref MergeB.sharedOutDone)))
                    ) {
                        // Must wait for lower merge node to fill more unprocessed items
                        if(first)
                            Thread.SpinWait(MergeSpin);
                        first = false;
                        Thread.Yield();
                        continue;
                    }
                    first = true;
                    void O(BucketItem I) {
                        MergeO.OutArray[MergeO.privateOutDone++] = I;
                        if ((MergeO.privateOutDone & VolatileBufferMask) == 0) { // let the higher merge node know in small incremental batches
                            Volatile.Write(ref MergeO.sharedOutDone, MergeO.privateOutDone);
                            if (cancel?.IsCancellationRequested ?? false)
                                return;
                        }
                    }
                    if (spanA.Length <= a) {
                        if (spanB.Length <= b) {
                            Volatile.Write(ref MergeO.sharedOutDone, MergeO.privateOutDone); // let the higher merge node know about the finished remainder
                            break; // nothing remaining - finished
                        }
                        O(spanB[b++]); // only b remaining
                    } else // only a remaining or b is smaller : a is smaller
                        O(spanB.Length <= b || compare(spanA[a], spanB[b]) < 0 ? spanA[a++] : spanB[b++]); 
                }
            }
        }
        // Task process:
        for (int t = 1; t < chunks; ++t) {
            int localT = t;
            tasks[t] = Task.Run(() => ProcessData(taskData[localT]));
        }
        ProcessData(taskData[0]); // this thread will take the taskData[0]
        Task.WaitAll(tasks);
        // Parallel.ForEach process: Parallel.ForEach(Partitioner.Create(taskData, EnumerablePartitionerOptions.NoBuffering), po, (data, state) => ProcessData(data));
        var o = merge[usedOutput - 1].OutArray;
        input.ResultArray = o;
        input.Count = o.Length;
        ReturnTasks(usedTasks);
    }
}
// 282 lines
// WARNING: The bucket sort part if alphabetic sorting can only take ASCII characters, not unicode!
internal class ParallelBucketIntroSort {

    private Comparison<BucketItem> Compare;
    private bool Inc1, Inc2;
    private static ushort MinMaxBucketSize = 0, SplitChildren = 0, DistributionTiers = 0;
    internal static void Sort(Bucket input, bool Inc1, bool Inc2, CancellationToken? cancel) => new ParallelBucketIntroSort(input, Inc1, Inc2, cancel);
    internal ParallelBucketIntroSort(Bucket input, bool Inc1, bool Inc2, CancellationToken? cancel) {
        MinMaxBucketSize = (GetProfile?.MinMaxBucketSize ?? DPro.MinMaxBucketSize);
        SplitChildren = (GetProfile?.SplitChildren ?? DPro.SplitChildren);
        DistributionTiers = (GetProfile?.DistributionTiers ?? DPro.DistributionTiers);
        Compare = Bucket.Compare(this.Inc1 = Inc1, this.Inc2 = Inc2);
        // STAGE 1 - keep splitting every large bucket into smaller buckets with a bucket sort, until we have a nice and balanced set.
        // STAGE 2 - introsort all the buckets in parallel. Then if you jsut concatenate them, it will all be sorted
        Stage2_ParallelIntrosort(input, Stage1_ParallelBucketSort(input, cancel), cancel);
    }

    #region Stage1
    private int Stage1_ParallelBucketSort(Bucket input, CancellationToken? cancel) {
        List<Bucket> LargeBuckets = [input];
        int MaxBucketSize = Math.Max(1 << MinMaxBucketSize, input.Count / (3 * AllocMaxTasks));
        if (input.Count > MaxBucketSize) while (LargeBuckets.Count > 0) {
            PerformBucketSort(LargeBuckets, DistributeBucket(out ushort UsedTasks, LargeBuckets, cancel), cancel);
            FinishLargeBuckets(ref LargeBuckets, MaxBucketSize, cancel);
            ReturnTasks(UsedTasks);
        }
        return MaxBucketSize;
    }
    private List<List<(Bucket, List<int>)>> DistributeBucket(out ushort UsedTasks, List<Bucket> LargeBuckets, CancellationToken? cancel) {

        // Count how many element we will sort in total
        ushort totalCount = 0;
        foreach (var LB in LargeBuckets)
            totalCount += (ushort)LB.Count;
        //if (totalCount == 0)
        //    break; // No more large buckets, stop the splitting loop

        // Allocate Tasks
        ushort taskCount = (ushort)Math.Max(1, totalCount / 64);
        UsedTasks = RequestFreeTasks(ushort.MaxValue, 1);
        ushort SplitTasks = (ushort)(UsedTasks + 1);
        if (SplitTasks > taskCount) {
            ReturnTasks((ushort)(SplitTasks - taskCount));
            UsedTasks = (ushort)((SplitTasks = taskCount) - 1); // we won't even need as many Tasks as are available, return some
        }

        // This code aims to divide the little lists as subtasks evenly across the available tasks, so that each tasks would sort similar number of elements
        List<List<(Bucket, List<int>)>> taskData = [];
        int IL = 0, IE = 0, t = 0;
        ushort filledElements = 0;
        double taskProgress = (double)totalCount / SplitTasks, progress = taskProgress;
        List<int> SubTasks = [];
        List<(Bucket, List<int>)> SubTasksAll = [];
        while (true) {
            var B = LargeBuckets[IL];

            // if element counter overflows the limit for current task, switch tothe next task to try assign this to
            ushort toFill = (ushort)B.Elements[IE].Count;
            while (filledElements + toFill >= progress) {
                if (cancel?.IsCancellationRequested ?? false)
                    return taskData;
                taskData.Add(SubTasksAll);
                SubTasks = [];
                SubTasksAll = [];
                ++t;
                progress += taskProgress;
            }

            // Assign this current little list as subtask of t, make fresh lists for separate buckets as a small bucket sort optimization
            if (SubTasks.Count == 0)
                SubTasksAll.Add((B, []));
            filledElements += toFill;
            SubTasks.Add(IE++);

            // if this large bucket hasn't yet been assigned to this task, increment the counter how many tasks will touch this bucket
            if (B.FillingTask != t) {
                B.FillingTask = t;
                ++B.Tasks;
            }

            // if large bucket distributed, go to the next bucket
            if (IE >= B.Elements.Length) {
                if (++IL >= LargeBuckets.Count) {
                    // that was the last bucket, finish assigning whatever we has prepared, and finish
                    if (SubTasks.Count > 0)
                        taskData.Add(SubTasksAll);
                    break;
                }
                SubTasks = [];
                SubTasksAll = [];
            }
        }
        return taskData;
    }
    private void PerformBucketSort(List<Bucket> LargeBuckets, List<List<(Bucket, List<int>)>> taskData, CancellationToken? cancel)  {
        // prepare the children buckets that the Large buckets sort their range into
        var sc = 1 << SplitChildren;
        foreach (var LB in LargeBuckets) {
            if (cancel?.IsCancellationRequested ?? false)
                return;
            var Range = (long)LB.Max - (long)LB.Min;
            if (/*Range == 0*/LB.Sorted > 0) {
                // Compare1 is sorted, sort by Compare2 now
                Range = (long)LB.Max2 - (long)LB.Min2;
                LB.SplitChildren = LB.SortType2 == SortType.Alphabetic ? (ushort)256 : (ushort)Math.Min(sc, Range);
                if (LB.SplitChildren < sc || (LB.SortType2 != SortType.Alphabetic && Range <= (long)LB.Min2 / 2)) // Switch to linear, if the range is getting absolutely or relatively small
                    LB.SortType2 = SortType.Linear; // absolutely - there are enough buckets to linear sort al lvalues exactly, relative - if the range is smaller than min, then the log curve is already linear-like 
                for (int C = 0; C < LB.SplitChildren; ++C)
                    LB.SplitBucket[C] = new Bucket(LB.Tasks, LB.SortType, LB.SortType2, LB.Depth, (ushort)(LB.Depth2 + 1), true);
            } else {
                // sort by Compare1
                LB.SplitChildren = LB.SortType == SortType.Alphabetic ? (ushort)256 : (ushort)Math.Min(sc, Range);
                if (LB.SplitChildren < sc || (LB.SortType != SortType.Alphabetic && Range <= (long)LB.Min / 2)) // Switch to linear, if the range is getting absolutely or relatively small
                    LB.SortType = SortType.Linear; // absolutely - there are enough buckets to linear sort al lvalues exactly, relative - if the range is smaller than min, then the log curve is already linear-like 
                for (int C = 0; C < LB.SplitChildren; ++C)
                    LB.SplitBucket[C] = new Bucket(LB.Tasks, LB.SortType, LB.SortType2, (ushort)(LB.Depth + 1), LB.Depth2, true);
            }
        }
        void TaskFunc(ushort taskIndex) {
            var SubTasks = taskData[(int)taskIndex];
            foreach ((Bucket Bucket, List<int> BucketIndex) B in SubTasks) {
                if (cancel?.IsCancellationRequested ?? false)
                    return;
                // thanks to separated lists, i can precompute this:
                var Parent = B.Bucket;
                var E = Parent.Elements;
                var SN = Parent.SplitChildren;
                var S = Parent.SplitBucket;
                if (Parent.Min == Parent.Max) {
                    if (Parent.SortType2 == SortType.Logarithmic) {
                        // logarithmic is diving the bucket intervals into logarithmically sized steps (tighter buckets at lower values)
                        // useful when the distribution of values follow the power law
                        double totalRange = Math.Log2((long)Parent.Max2) - Math.Log2(Math.Max(1, (long)Parent.Min2));
                        var bucketRange = (totalRange + 1) / SN;
                        foreach (var i in B.BucketIndex)
                            foreach (var element in E[i])
                                S[(int)((Math.Log2((long)element.Compare2) - Math.Log2(Math.Max(1, (long)Parent.Min2))) / bucketRange)].InsertThreaded(taskIndex, element);
                    } else {
                        long totalRange = (long)Parent.Max2 - (long)Parent.Min2;
                        var bucketRange = (totalRange + 1) / SN;
                        if (Parent.SortType2 == SortType.Alphabetic)
                            foreach (var i in B.BucketIndex)
                                foreach (var element in E[i])
                                    S[(int)(((long)element.Compare2 - (long)Parent.Min2) / bucketRange)].InsertThreaded(taskIndex, element);
                        else
                            foreach (var i in B.BucketIndex)
                                foreach (var element in E[i])
                                    S[(int)((((string)element.Compare2)[(int)Parent.Depth2] - (long)Parent.Min2) / bucketRange)].InsertThreaded(taskIndex, element);
                    }
                } else {
                    if (Parent.SortType == SortType.Logarithmic) {
                        // logarithmic is diving the bucket intervals into logarithmically sized steps (tighter buckets at lower values)
                        // useful when the distribution of values follow the power law
                        double totalRange = Math.Log2((long)Parent.Max) - Math.Log2(Math.Max(1, (long)Parent.Min));
                        var bucketRange = (totalRange + 1) / SN;
                        foreach (var i in B.BucketIndex)
                            foreach (var element in E[i])
                                S[(int)((Math.Log2((long)element.Compare) - Math.Log2(Math.Max(1, (long)Parent.Min))) / bucketRange)].InsertThreaded(taskIndex, element);
                    } else {
                        long totalRange = (long)Parent.Max - (long)Parent.Min;
                        var bucketRange = (totalRange + 1) / SN;
                        if (Parent.SortType == SortType.Alphabetic)
                            foreach (var i in B.BucketIndex)
                                foreach (var element in E[i])
                                    S[(int)(((long)element.Compare - (long)Parent.Min) / bucketRange)].InsertThreaded(taskIndex, element);
                        else
                            foreach (var i in B.BucketIndex)
                                foreach (var element in E[i])
                                    S[(int)((((string)element.Compare)[(int)Parent.Depth] - (long)Parent.Min) / bucketRange)].InsertThreaded(taskIndex, element);
                    }
                }
            }
        }
        Task[] tasks = new Task[taskData.Count-1];
        for (ushort t = 1; t < taskData.Count; ++t) {
            ushort _t = t; // to be safe that the correct value gets used in the Task
            tasks[t - 1] = Task.Run(() => TaskFunc(_t)); // Parallelize taskData[1..taskData.Count-1]
        }
        TaskFunc(0); // this thread will take the taskData[0]
        Task.WaitAll(tasks);
    }
    private void FinishLargeBuckets(ref List<Bucket> LargeBuckets, int MaxBucketSize, CancellationToken? cancel) {
        // Clear the elements in the large buckets, since we already redistributed the elements into the children buckets
        List<Bucket> NewLargeBuckets = [];
        foreach (var LB in LargeBuckets) {
            if (cancel?.IsCancellationRequested ?? false)
                return;
            //LB.Elements = [];
            LB.Count = 0; // this should be good enough
            //LB.Sorted = 2; // not sure if this is even necessary
            var PrevNext = LB.Next;
            var B = LB;
            if (LB.Min == LB.Max ? Inc2 : Inc1) for (int i = 0; i < LB.SplitChildren; ++i) { // Increasing:
                    var C = LB.SplitBucket[i];
                    if (C.Count > 0) {
                        C.CollapseMinMax(); // merge the min and max values in the bucket's lists
                        if ((B.Next = B = C).Count >= MaxBucketSize) // and link those Children between LB and its successor
                            NewLargeBuckets.Add(C); // and if any child is still large put it into the queue to be bucket sorted deeper
                    }
                }
            else for (ushort i = LB.SplitChildren; 0 <= --i; ) { // Decreasing (reverse bucket linkage)
                    var C = LB.SplitBucket[i];
                    if (C.Count > 0) {
                        C.CollapseMinMax(); // merge the min and max values in the bucket's lists
                        if ((B.Next = B = C).Count >= MaxBucketSize) // and link those Children between LB and its successor
                            NewLargeBuckets.Add(C); // and if any child is still large put it into the queue to be bucket sorted deeper
                    }
                }
            B.Next = PrevNext;
        }
        LargeBuckets = NewLargeBuckets;
    }
    #endregion

    #region Stage2
    private void Stage2_ParallelIntrosort(Bucket input, int MaxBucketSize, CancellationToken? cancel) {
        PerformIntrosort(DistributeIntro(input, out ushort UsedTasks, MaxBucketSize, cancel), cancel);
        ReturnTasks(UsedTasks);
        //input.ConcatAll(); // not needed single the processor will be iterating thorugh everything, and it would jsut break the TotalCount call there
    }
    private (int TaskIndex, List<Bucket> Bucket)[] DistributeIntro(Bucket input, out ushort UsedTasks, int MaxBucketSize, CancellationToken? cancel) {
        UsedTasks = RequestFreeTasks(ushort.MaxValue, 1);
        ushort SplitTasks = (ushort)(UsedTasks + 1), usedTasks = UsedTasks;
        // If the input was never split, move its elements to a new next bucket, because the algorith below requires the root to not have elements
        if (input.Count > 0) {
            var PrevNext = input.Next;
            (input.Next = new Bucket(input)).Next = PrevNext;
            input.Count = 0;
        }
        // init task data
        (int TaskIndex, List<Bucket> Bucket)[] taskData = new (int TaskIndex, List<Bucket> Bucket)[SplitTasks];
        for (int t = 0; t < SplitTasks; ++t)
            taskData[t] = (t, []);
        // Distribute Introsort subtasks evenly:
        float[] costs = new float[SplitTasks];
        // Bucketsort the lists into tiers of their counts
        var dtiers = 1 << DistributionTiers;
        var tiers = new List<Bucket>[dtiers];
        for (int i = dtiers; 0 <= --i; tiers[i] = []) ;
        var b = input;
        int zigI = 0;
        var tierSize = (float)MaxBucketSize / dtiers;
        while (b.Next != null) {
            tiers[(int)((b = b.Next).Count / tierSize)].Add(b); // the root bucket (input) is skipped because i ensured it has no elements.
            b.Concat(); // introsort needs concatenated list
        }
        // smart zigzag fill into tasks:
        int tier = dtiers;
        while (0 <= --tier) {
            if (cancel?.IsCancellationRequested ?? false)
                return taskData;
            int ti = 0;
            void ZigZag() {
                while (true) { // looping to exhaust the tiers[t]
                    while (true) { // looping until condition for the next zig-zag iteration is met
                        if (zigI == 0
                            ? costs[0] > costs[usedTasks] // When on the left end, keep adding until above the right end
                            : zigI == -usedTasks
                            ? costs[usedTasks] > costs[0] // When on the right end, keep adding until above the left end
                            : zigI > 0
                            ? costs[zigI] > costs[1 + zigI] // When in the middle and going right, keep adding until above the next one to the right
                            : costs[-zigI] > costs[-1 - zigI] // When in the middle and going left, keep adding until above the next one to the left
                        ) break;
                        int i = Math.Abs(zigI); // make the negatives zig-zag
                        taskData[i].Bucket.Add(b = tiers[tier][ti++]);
                        costs[i] += b.Count * MathF.Log2(b.Count); // O(NlogN) of typical Introsort
                        if (ti >= tiers[tier].Count)
                            return; // step out of both while(true)
                    }
                    if (++zigI >= usedTasks) // keep cycling thorugh -UsedTasks..UsedTasks-1, positives zig forward, and negative zag backward
                        zigI = -(int)usedTasks;
                }
            }
            ZigZag();
        }
        return taskData;
    }
    private void PerformIntrosort((int TaskIndex, List<Bucket> Bucket)[] taskData, CancellationToken? cancel) {
        // run all introsorts in parallel
        void TaskFunc((int taskIndex, List<Bucket> SubTasks) MyTaskData) {
            foreach (var S in MyTaskData.SubTasks) {
                if (cancel?.IsCancellationRequested ?? false)
                    return;
                S.Elements[0].Sort(Compare);
            }
        }
        var tasks = new Task[taskData.Length - 1];
        for (int t = 1; t < taskData.Length; ++t) { 
            int _t = t; // to be safe that the corrct value gets used in the Task
            tasks[t-1] = Task.Run(() => TaskFunc(taskData[_t])); // Parallelize taskData[1..taskData.Length-1]
        }
        TaskFunc(taskData[0]); // this thread will take taskData[0]
        // Wait for tasks to finish
        Task.WaitAll(tasks);
    }
    #endregion
}
/* switch (SortType) {
    case Enum:
        TMin = 0; TMax = Enum.Size; Sort = Linear;
        break;
    case CoinValue:
        TMin = minimumUtxoValue; TMax = TotalBalance; / Or MaxUtxo if known / Sort = Logarithmic;
        break: case DateTime:
        TMin = Synced.BeginTime; TMax = Synced.EndTime; Sort = Linear;
        break;
    case Alphabetic:
        //??? Oops I'm afraid this might be harder to sort, but maybe treating each next char as a bucket...?
        TMin = Char.Smallest; TMax = Char.Largest; Sort = LinearAlphabetic;
}*/
#region DEPRECATED - BucketIntro greedy distribution
// DEPRECATED
/*private static void DistributeGreedy(Bucket input, int UsedTasks, (int TaskIndex, List<Bucket> Bucket)[] taskData2) {
    // Greedy version - perfect distribution, but slow. It keeps taking the largest value and putting it into the smallest bucket
    // it also first zig-zag all the smallest values to make the value list smaller in first pass, foir the tax of lower precision.
    Bucket b = input, pb = input, maxB, maxPB;
    float[] costs = new float[UsedTasks];
    int cycT = 0;
    long max = 0;

    bool FindCostliestList() {
        if (b.Count > max) {
            max = (maxB = b).Count;
            maxPB = pb;
        }
        pb = b;
        if (b.Next == null)
            return false;
        b = b.Next;
        return true;
    }

    // First run of finding the costlies also looks for already sorted and too small ones to remove from the list early in one pass, for performance
    do {
        // Already sorted ones and very small ones go to Introsorted branches:
        if (b.Sorted || b.Count <= 8) {
            if (!b.Sorted) {
                b.Concat(); // very small ones will be immediately distributed into
                taskData2[cycT = (cycT + 1) % UsedTasks].Bucket.Add(b);
            }
            if ((pb.Introsorted = pb = b).Next is not Bucket N)
                break;
            b = pb.Next = N;
            continue;
        }
    } while (FindCostliestList());

    while (max > 0) {
        pb = b = input;
        while (FindCostliestList()) ;
        if (max == 0)
            break; // everything is distributed - end the infinite loop
        pb.Next = (pb.Introsorted = b).Next; // remove it from regulat list, and attach it to introsorted branch, that we will no longer search to introsort
        b.Concat(); // introsort needs concatenated list

        // Find least overloaded Task and put the costliest task into it
        int minT = -1;
        float C, minF = float.MaxValue;
        for (int t = 0; t < UsedTasks; ++t)
            if ((C = costs[t]) < minF) {
                minF = C;
                minT = t;
            }
        taskData2[minT].Bucket.Add(b);
        costs[minT] += b.Count * MathF.Log2(b.Count);
    }
}*/
#endregion
#region DEPRECATED - Previous IntroMerge
/*bool redistribute;
        if (input.Elements.Length == chunks) {
            int sum = 0, diff = 0;
            foreach (var e in input.Elements)
                sum += e.Count;
            sum /= input.Elements.Length;
            foreach (var e in input.Elements)
                diff += Math.Abs(sum - e.Count);
            redistribute = diff > 2 * chunkSize; // input.Elements sizes too uneven - redistribute
        } else redistribute = true; // different number of input.Elements and tasks - redistribute
        if (redistribute) {
            // Redistribute the unevenly sized intem.Element[] into evenly sized chunks:
            chunk = new List<BucketItem>[chunks];
            List<BucketItem> currentElements = input.Elements[0], currentChunk;
            int elementI = 1, remainingElements = currentElements.Count, filledElements = 0, filledSplit = 0;
            void RemainingChunk() {
                for (int f = 0; f < remainingElements; ++f) // TODO memcpy this loop?
                    currentChunk[filledSplit + f] = currentElements[filledElements + f];
                filledSplit += remainingElements;
                filledElements = 0; // next element list
                remainingElements = elementI < input.Elements.Length ? (currentElements = input.Elements[elementI++]).Count : 0; 
            }
            // Fill all the chunks except the last one uniformly (they all have the same size "chunkSize", the last one can be smaller)
            for (int i = usedTasks; 0 <= i; --i) {
                currentChunk = chunk[i] = new(chunkSize);
                filledSplit = 0;
                while (true) {
                    var remainingSplit = chunkSize - filledSplit;
                    if (remainingElements > remainingSplit) {
                        for (int f = 0; f < remainingSplit; ++f) // TODO memcpy this loop?
                            currentChunk[filledSplit + f] = currentElements[filledElements + f];
                        filledElements += remainingSplit;
                        break;
                    } else RemainingChunk();
                }
            }
            // Fill the rest to the last (first because reversed) chunk (it is guaranteed that the remaing elements won't overflow the last chunk, as it's at most as large as the rest)
            currentChunk = chunk[0];
            while (remainingElements > 0) 
                RemainingChunk();
        } else chunk = input.Elements;*/
/*internal static class Simple_ParallelIntroMergeSort {
    internal const int DefaultSpin = 42;
    internal static int Spin = DefaultSpin;
   
    internal static void Sort(Bucket input, bool Inc1, bool Inc2) {
        var Compare = Bucket.Compare(Inc1, Inc2);
        int UsedTasks = RequestFreeTasks(int.MaxValue, 1);
        ParallelOptions po = new() { MaxDegreeOfParallelism = UsedTasks };
        Parallel.ForEach(input.Elements, po, (list, state) => list.Sort(Compare));
        int output = 0, usedOutput = 0;
        bool isMem = false;
        List<BucketItem> mem = [];
        List<(int OutputIndex,
            List<BucketItem> InputA,
            Merge? MergeA,
            List<BucketItem> InputB,
            Merge? MergeB
        )> taskData = [];
        Merge? memM = null;
        List<Merge> merge = [];
        void Add(List<BucketItem> now, Merge? nowM) {
            merge.Add(new());
            taskData.Add((output++, mem, memM, now, nowM));
        }
        for (int i = 0; i < input.Elements.Length; ++i) 
            if (isMem = !isMem) {
                mem = input.Elements[i];
                memM = null;
            } else Add(input.Elements[i], null);
        while (output > usedOutput) {
            if (isMem = !isMem) {
                mem = (memM = merge[usedOutput++]).outList;
            } else {
                Add(merge[usedOutput].outList, merge[usedOutput]);
                ++usedOutput;
            }
        }
        Parallel.ForEach(taskData, po, (job, state) => {
            int a = 0, b = 0;
            while (true) {
                if ((job.MergeA != null && 0 <= job.MergeA.outDone && a > job.MergeA.outDone)
                    || (job.MergeB != null && 0 <= job.MergeB.outDone && b > job.MergeB.outDone)
                ) {
                    Thread.SpinWait(Spin);
                    continue;
                }
                void O(BucketItem I) {
                    var m = merge[job.OutputIndex];
                    m.outList.Add(I);
                    ++m.outDone; // this element is alraedy ready for other task to read, so let it know it can, with this variable.
                }
                void A() => O(job.InputA[a++]);
                void B() => O(job.InputB[b++]);
                if (job.InputA.Count <= a) {
                    if (job.InputB.Count <= b)
                        break; // nothing remaining - finished
                    B(); // only b remaining
                } else if (job.InputB.Count <= b)
                    A(); // only a remianing
                else if (Compare(job.InputA[a], job.InputB[b]) > 0)
                    B(); // a is smaller
                else
                    A(); // b is smaller
            }
            
        });
        input.Elements = [merge[usedOutput - 1].outList];
        ReturnTasks(UsedTasks);
    }
}
internal enum MergeFunction {
    Linear,
    Const,
    Exponential
}

internal static class ParallelIntroMergeSort {
    internal const int DefaultSpin = 42;
    internal const MergeFunction DefaultMergeFunction = MergeFunction.Linear;
    internal const int DefaultConstBatch = 1;

    internal static int Spin = DefaultSpin;
    internal static MergeFunction MergeFunction = DefaultMergeFunction;
    internal static int ConstBatch = DefaultConstBatch;

    internal static void Sort(Bucket input, bool Inc1, bool Inc2) {
        var Compare = Bucket.Compare(Inc1, Inc2);
        int UsedTasks = RequestFreeTasks(int.MaxValue, 1);
        ParallelOptions po = new() { MaxDegreeOfParallelism = UsedTasks };
        Parallel.ForEach(input.Elements, po, (list, state) => list.Sort(Compare));
        int output = 0, usedOutput = 0, l = 0;
        bool isMem = false;
        List<BucketItem> mem = [];
        List<(int OutputIndex,
            List<BucketItem> InputA, Merge? MergeA,
            List<BucketItem> InputB, Merge? MergeB
        )> jobs = [];
        List<List<(int OutputIndex,
            List<BucketItem> InputA, Merge? MergeA,
            List<BucketItem> InputB, Merge? MergeB
        )>> taskData = [];
        Merge? memM = null;
        List<Merge> merge = [];
        void Add(List<BucketItem> now, Merge? nowM) {
            bool Condition() {
                return MergeFunction switch {
                    MergeFunction.Const => ConstBatch <= l,
                    MergeFunction.Exponential => output - usedOutput <= l,
                    MergeFunction.Linear => Math.Log2(output - usedOutput) <= l,
                    _ => true,
                };
            }
            ++l;
            merge.Add(new());
            jobs.Add((output++, mem, memM, now, nowM));
            if (Condition()) { taskData.Add(jobs); jobs = []; l = 0; }
        }
        for (int i = 0; i < input.Elements.Length; ++i) 
            if (isMem) {
                mem = input.Elements[i];
                memM = null;
            } else Add(input.Elements[i], null);
        while (output > usedOutput)
            if (isMem = !isMem) mem = (memM = merge[usedOutput++]).outList;
            else {
                Add(merge[usedOutput].outList, merge[usedOutput]);
                ++usedOutput;
            } 
        var partitioner = Partitioner.Create(taskData, EnumerablePartitionerOptions.NoBuffering);
        Parallel.ForEach(partitioner, po, (data, state) => {
            foreach (var job in data) {
                int a = 0, b = 0;
                while (true) {
                    if ((job.MergeA != null && 0 <= job.MergeA.outDone && a > job.MergeA.outDone)
                        || (job.MergeB != null && 0 <= job.MergeB.outDone && b > job.MergeB.outDone)
                    ) {
                        Thread.SpinWait(Spin);
                        continue;
                    }
                    void O(BucketItem I) {
                        var m = merge[job.OutputIndex];
                        m.outList.Add(I);
                        ++m.outDone; // this element is alraedy ready for other task to read, so let it know it can, with this variable.
                    }
                    void A() => O(job.InputA[a++]);
                    void B() => O(job.InputB[b++]);
                    if (job.InputA.Count <= a) {
                        if (job.InputB.Count <= b)
                            break; // nothing remaining - finished
                        B(); // only b remaining
                    } else if (job.InputB.Count <= b)
                        A(); // only a remianing
                    else if (Compare(job.InputA[a], job.InputB[b]) > 0)
                        B(); // a is smaller
                    else
                        A(); // b is smaller
                }
            }
        });
        input.Elements = [merge[usedOutput-1].outList];
        ReturnTasks(UsedTasks);
    }
}*/
#endregion