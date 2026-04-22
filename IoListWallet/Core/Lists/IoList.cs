using Avalonia.Controls;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Panels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Core.Lists;

internal class ClickToolEventArgs {
    internal int FromList;
    internal int Index;
}
internal class ClickButtonEventArgs {
    internal int FromList;
    internal (IoTextBlock Text, Viewbox View, IoToggleButton Button)? ClickedButton;
    internal int Index;
}
// This is a core parent of SearchList elements, it querries and contructs a list representation of various elements
// It splits the list into pages that you can browse with numberics at the bottom, or swiping up/down.
// It has semi-automatic Search tab, that you can open within, which lets you double sort the elements, or filter any parts with complex conditions
// RefreshData is expensive call that filters and sorts the whole Query, it's only to be called before Refresh when the query or filter conditions changed, just flippign pages calls Refresh only
// Refresh builds the current page, it's called after RefreshData, or after swiping to a different page
internal class IoList<T>(IoPanel owner, (string, string)[] tools) : IoListBase(owner, tools) {

    private const string F = "Failed IoList.";

    /*[Save] internal IoPanel Owner { get; set { field = value; IoPanel.Dirty(); } }
    [Save] internal IoPopup MyPopup { get; set { field = value; IoPanel.Dirty(); } }
    [Save] internal FilterPanel<T> MyFilter { get; set { field = value; IoPanel.Dirty(); } }
    [Save] internal uint pageSize { get; set { field = value; IoPanel.Dirty(); } } = 10;
    [Save] public IoSwipe Swipe { get; set { field = value; IoPanel.Dirty(); } }*/

    //internal List<((IoTextBlock Text, Viewbox View, IoToggleButton Button)? Item, int Id)> Selecteds = [];

    internal IEnumerable<T>? ItemSource;

    internal override bool RefreshData(CancellationToken? cancel, uint startIndex = 0, uint stopIndex = 0) {

        // Unlink parentage of list entries to the previously opened page:
        ClearContent();

        Results = 0;
        if (ItemSource == null)
            return true;
        // Filter and Sort:
        List<(ushort TaskIndex, uint Start, uint End)> taskData = [];
        List<object?> ItemList = ItemSource as List<object?> ?? [..ItemSource];
        SourceCount = (uint)ItemList.Count;
        if (stopIndex == 0)
            stopIndex = (ushort)ItemList.Count;
        uint totalSize = stopIndex - startIndex;
        var profile = GetProfile;
        if (profile == null)
            return false;
        // Get maximum available tasks:
        ushort UsedTasks = ItemList.Count <= (1 << (int)profile.DefaultToSingle)
            ? (ushort)0 : RequestFreeTasks(ushort.MaxValue, 1), // request remainingFreeTasks-1 (let it keep one slot or other processes)
            taskCount = (ushort)Math.Max(1, totalSize / 256),
        SplitTasks = (ushort)(UsedTasks + 1);
        if (SplitTasks > taskCount) {
            // We don't actaully need as many tasks as available, so return some to be free:
            ReturnTasks((ushort)(SplitTasks - taskCount));
            UsedTasks = (ushort)((SplitTasks = taskCount) - 1);
        }
        // Prepare tasks:
        uint taskSize = (uint)Math.Ceiling((float)totalSize / SplitTasks);
        for (ushort t = 0; t < SplitTasks; ++t)
            taskData.Add((t, startIndex + t * taskSize, (uint)Math.Min(Math.Min(stopIndex, ItemList.Count), startIndex + (t + 1) * taskSize)));
        uint[] skipped = new uint[SplitTasks];
        //ParallelOptions po = new() { MaxDegreeOfParallelism = UsedTasks };
        Bucket bucket = new(SplitTasks, SortType.Linear, SortType.Linear);
        var filter = MyFilter.Filters;

        // Call helpers:
        string[] GetTexts(object? key)
            => (ItemTemplate == null ? [key?.ToString() ?? ""] : ItemTemplate(key)) ?? [""];
        // only the bucketintro sort required a more complex add
        Action<ushort, (object Key, IComparable Compare2, IComparable Compare, GroupBorder? Group, byte Selected, string[] Texts)> insert
            = profile.SortAlgo == SortAlgo.P_BucketIntro ? bucket.InsertThreaded : bucket.InsertThreadedSimple;

        if (filter?.Filters != null) {
            void PerformFilter(ushort taskIndex, object key, string[] texts, bool skip) {
                int i;
                if ((FilterCommandTemplate == null ? texts : FilterCommandTemplate(key)) is not string[] data
                    || (i = data.Length) <= 0
                    || i != filter.Filters.Count
                ) return; // There must be filter data each filter
                ulong isMatch = ((ulong)1 << filter.Size) - 1; // prepare initial bitmask for matching conditions
                (IComparable Compare, GroupBorder? Group) SortValue = (0, null);
                IComparable SortValue2 = 0;
                // go through all the filterable fields:
                while (0 <= --i) {
                    // process all OR conditions, and return sortable value and group
                    (ulong match, IComparable sort, GroupBorder? border) = filter.Filters[i].Process(data[i]);
                    isMatch &= match;
                    if (match > 0) {
                        // remember the sort values if we are actaully sorting by this one specifically
                        if (filter.SortIndex == i)
                            SortValue = (sort, border);
                        if (filter.SortIndex2 == i)
                            SortValue2 = sort;
                    }
                }
                // If it passed the filter conditions, add it to the SourceList
                if (isMatch > 0)
                    insert(taskIndex, (key, SortValue.Compare, SortValue2, SortValue.Group, (byte)(skip ? 2 : Selected != null && Selected(key)/*key.Equals(Selected)*/  ? 1 : 0), texts));
            }
            void WithFilter(int taskIndex) {
                // Each task goes through it's slice of data, runs virtual Skip condition
                var dataT = taskData[taskIndex]; // fetch task data
                bucket.Elements[dataT.TaskIndex] = []; // initialize bucket
                for (uint i = dataT.Start; i < dataT.End; ++i) {
                    if (ItemList[(int)i] == null)
                        continue;//null are not even counted
                    if (Owner.Cancel(cancel))
                        return; // cancellation request to stop completely
                    if (ItemList[(int)i] is not object key || (Skip != null && Skip(key))) {
                        ++skipped[dataT.TaskIndex];
                        continue; // remember how many we skipped
                    }
                    PerformFilter(dataT.TaskIndex, key, GetTexts(key), Disable != null && Disable(key)); // feeds it to the filter.
                }
            }
            // Depending what kinds of values we are sorting, we will let the sorter know how to bucket/compare it
            var SortFilter = filter.Filters[filter.SortIndex];
            if (SortFilter is FilterText)
                bucket.SortType = SortType.Alphabetic;
            else if (SortFilter is FilterNumber || SortFilter is FilterToken)
                bucket.SortType = SortType.Logarithmic;
            // Perform parallel parsing of ItemSource -> SourceList. It performs virtual skips + search conditions + sorting.
            Task[] tasks = new Task[UsedTasks]; // N-1 extra tasks, one of these will be performed synchrtonously in this thread.
            for (int t = 1; t <= UsedTasks; ++t) {
                var localT = t; // parallel taskData[1..UsedTasks]
                tasks[t - 1] = Task.Run(() => WithFilter(localT));
            }
            WithFilter(0); // this thread will take taskData[0]
            Task.WaitAll(tasks);
            ReturnTasks(UsedTasks);
            if (Owner.Cancel(cancel))
                return true;
            //if (!IgnoreSort.Contains(filter.SortIndex)) {

            // Sorts the Bucket into a LinkedList of buckets, each with Elements.Length = 0-1.
            // Uses a user-selected Sorting algoritm, but mostly regular Sort for non-gigantic lists
            if (bucket.Count <= (1 << (int)profile.DefaultToSingle) || bucket.Elements.Length <= 1)
                IntroSort.Sort(bucket, filter.Increasing, filter.Increasing2);
            else switch (profile.SortAlgo) {
                    case SortAlgo.P_IntroMerge:
                        ParallelIntroMergeSort.Sort(bucket, filter.Increasing, filter.Increasing2, cancel);
                        break;
                    case SortAlgo.S_Intro:
                        IntroSort.Sort(bucket, filter.Increasing, filter.Increasing2);
                        break;
                    case SortAlgo.P_BucketIntro:
                        bucket.CollapseMinMax();
                        ParallelBucketIntroSort.Sort(bucket, filter.Increasing, filter.Increasing2, cancel);
                        break;
                }
            //}
        } else {
            void WithoutFilter(int taskIndex) {
                // Each task goes through it's slice of data, runs virtual Skip condition
                var dataT = taskData[taskIndex]; // fetch task data
                bucket.Elements[dataT.TaskIndex] = []; // initialize bucket
                for (uint i = dataT.Start; i < dataT.End; ++i) {
                    if (ItemList[(int)i] == null)
                        continue;//null are not even counted
                    if (Owner.Cancel(cancel))
                        return; // cancellation request to stop completely
                    if (ItemList[(int)i] is not object key || (Skip != null && Skip(key))) {
                        ++skipped[taskIndex];
                        continue; // remember how many we skipped
                    }
                    // feeds it directly to to the SourceList, skipping the filter
                    insert(dataT.TaskIndex, (key, 0, 0, null, (byte)(Disable != null && Disable(key) ? 2 : Selected != null && key.Equals(Selected) ? 1 : 0), GetTexts(key)));
                }
            }
            // Perform parallel parsing of ItemSource -> SourceList. It performs virtual skips only (no search conditions + sorting).
            Task[] tasks = new Task[UsedTasks];
            for (int t = 1; t <= UsedTasks; ++t) {
                var localT = t; // parallel taskData[1..UsedTasks]
                tasks[t - 1] = Task.Run(() => WithoutFilter(localT));
            }
            WithoutFilter(0); // this thread will take taskData[0]
            Task.WaitAll(tasks);
            ReturnTasks(UsedTasks);
        }

        if (filter != null) {
            // Add grouping labels (like new days when filtered by DateTime, etc)
            void AddGroup(string Prev, string Now) {
                if (Prev != "" || Now != "")
                    SourceList.Add(((Prev == "" ? "" : Prev + " ▲", Now == "" ? "" : "▼ " + Now), 2, []));
            }
            SourceList = [];
            if (Owner.Cancel(cancel) || bucket == null || bucket.Elements.Length <= 0)
                return true;
            string CurrentGroup;
            var TodayEpoch = 0; // TODO make this actually reflect true current epoch/day!
            var todayDate = DateTime.Now;
            string GetToday(bool increasing) => increasing ? "" : filter.Filters[filter.SortIndex].Name switch {
                nameof(FilterEpoch) => TodayEpoch.ToString() != CurrentGroup ? TodayEpoch.ToString() : "" ?? "",
                nameof(FilterDate) => todayDate.Date.ToString() != CurrentGroup ? todayDate.ToString() : "" ?? "",
                _ => todayDate.Date.ToShortDateString() != CurrentGroup ? todayDate.Date.ToShortDateString() : "" ?? ""
            };
            if (bucket.ResultArray != null) {
                // Array version
                if (bucket.ResultArray.Length > 0) {
                    CurrentGroup = bucket.ResultArray[0].Group?.Text ?? "";
                    AddGroup(CurrentGroup == "" ? "" : GetToday(true), CurrentGroup); // initial group
                    foreach (var To in bucket.Elements[0]) {
                        if (To.Group != null && To.Group.Text != CurrentGroup) // group boundary detected, add a label
                            AddGroup(CurrentGroup, CurrentGroup = To.Group?.Text ?? "");
                        SourceList.Add((To.Key, To.Selected, To.Texts));
                    }
                    AddGroup(CurrentGroup, CurrentGroup == "" ? "" : GetToday(false)); // final group

                    // parallel is just not worth it, the groups are not making the indexing predictable, so it would need to get converted back into a list again
                    /*var startGroup = CurrentGroup = bucket.ResultArray[0].Group?.Text ?? "";
                    AddGroup(GetToday(true), CurrentGroup); // initial group
                    void ProcessSourceItem(int taskIndex, Bucket b, string iCurrentGroup) {
                        if (taskIndex == 0) {
                            // Make 
                        }
                        for (int i = taskIndex * chunkSize; i < (taskIndex+1) * chunkSize; ++i) {
                            var To = b.ResultArray[i];
                            if (To.Group != null && To.Group.Text != iCurrentGroup) // group boundary detected, add a label
                                AddGroup(iCurrentGroup, iCurrentGroup = To.Group?.Text ?? "");
                            SourceList.Add((To.Key, To.Selected, To.Texts));
                        }
                    }
                    SplitTasks = RequestFreeTasks(ushort.MaxValue, 1);
                    var L = bucket.ResultArray.Length;
                    ushort desiredTasks = (ushort)(L / (1 << Math.Max(DefaultToSingle, 12)));
                    if (desiredTasks < SplitTasks)
                        ReturnTasks((ushort)(SplitTasks - (SplitTasks = desiredTasks)));
                    var chunkSize = L / (SplitTasks + 1);
                    Task[] tasks = new Task[SplitTasks];
                    if(SplitTasks > 0)
                        tasks[0] = Task.Run(() => ProcessSourceItem(0, bucket, startGroup));
                    for (int t = 1; t < SplitTasks; ++t) {
                        var localT = t; // parallel taskData[1..UsedTasks]
                        tasks[t] = Task.Run(() => ProcessSourceItem(localT, bucket, bucket.ResultArray[localT * chunkSize - 1].Group?.Text ?? ""));
                    }
                    ProcessSourceItem(SplitTasks, bucket, SplitTasks > 0 ? bucket.ResultArray[SplitTasks * chunkSize - 1].Group?.Text ?? "" : startGroup); // this thread will take taskData[0]
                    Task.WaitAll(tasks);
                    ReturnTasks(SplitTasks);
                    AddGroup(CurrentGroup, GetToday(false)); // final group*/
                }

            } else if (bucket.Elements.Length > 0 && bucket.Elements[0].Count > 0) {
                // Bucket version
                CurrentGroup = bucket.Elements[0][0].Group?.Text ?? "";
                AddGroup(CurrentGroup == "" ? "" : GetToday(true), CurrentGroup); // initial group
                void ProcessSourceItem(Bucket b) {
                    foreach (var To in b.Elements[0]) {
                        if (To.Group != null && To.Group.Text != CurrentGroup) // group boundary detected, add a label
                            AddGroup(CurrentGroup, CurrentGroup = To.Group?.Text ?? "");
                        SourceList.Add((To.Key, To.Selected, To.Texts));
                    }
                }
                for (Bucket? b = bucket; b != null; b = b.Next)
                    ProcessSourceItem(b);
                AddGroup(CurrentGroup, CurrentGroup == "" ? "" : GetToday(false)); // final group
            }
        }
        if (Owner.Cancel(cancel))
            return true;

        foreach (var s in skipped)
            SourceCount -= s; // subtract skipped from cource count (they don't count at all, as they cannot be filtered in)
        Results = bucket.TotalCount();  // Bucket has been concatenated, so just ask for count
        return false;
    }
    internal override void RefreshTemplate<IT>(
    IEnumerable<IT>? source,
    Func<object?, string[]> itemT,
    Func<object?, string[]> filterCommandT) {
        base.RefreshTemplate(source, itemT, filterCommandT);
        ItemSource = source is IEnumerable<T> typed ? typed : null;
    }
}