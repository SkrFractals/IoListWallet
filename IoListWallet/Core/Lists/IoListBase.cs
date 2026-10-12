using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Books;
using System;
using System.Collections.Generic;
using System.Threading;
using static IoListWallet.Core.Static.AppSave;
using static IoListWallet.Core.Static.ConstructionHelpers;

namespace IoListWallet.Core.Lists;
internal abstract class IoListBase : IoUserControl {

    internal class ClickToolEventArgs {
        internal int FromList;
        internal int Index;
    }
    internal class ClickRowEventArgs {
        internal int FromList;
        internal object? ClickedRow = null;
        internal int Index = 0;
        internal int SubIndex = 0;
    }

    /*internal class IoEntry_ContentButtons(WContentButton?[] content) : IoEntry {
        internal WContentButton?[] Content = content;
    }*/
    internal IoPanel Owner { get; private set; }
    internal IoPopup MyPopup { get; private set; }
    internal IoSwipe Swipe { get; private set; }
    internal BookFilter MyFilter { get; private set; }
    public List<(object Key, byte State, IoGen.Data? GenData)> SourceList { get; protected set; } = []; // Final list to be build into the custom UserControls, after being filtered sorted
    internal List<(UserControl Entry, Grid? Parent)> PageEntries = []; // The UserElements built from the SourceList

    // Owner will set this up:
    internal IoGen? MyGen = null;
    internal Func<object?, IoGen.Data?>? ItemTemplate = null;
    internal Func<object?, string[]>? FilterCommandTemplate = null;
    internal Func<object?, bool>? Skip = Null;
    internal Func<object?, bool>? Disable = Null;
    internal Func<object?, bool>? Selected = Null;
    static bool Null(object? s) => false;

    //internal object? Selected = null;

    internal int MyIndex;

    protected List<Control> ToolbarList = [];
    protected List<WSToggleButton?> toolButton = [];
    protected (string Tooltip, string Text)[] Tools = [];
    internal uint pageSize = 10;

    private WSToggleButton? filterSelect;
    private IoNumericLeftRight? pageSelect = null, pageSizeSelect = null;
    private Grid? page = null, prevPage = null, grid = null, tGrid = null;
    private WrapPanel? toolbar = null;

    protected uint Results = 0, SourceCount = 0;
    internal List<IoEntry> Elements { get; private set; } = [];
    internal event EventHandler<ClickToolEventArgs>? OnClickTool;
    internal event EventHandler<ClickRowEventArgs>? OnClickButton;

    internal WSToggleButton? GetFilterSelect => filterSelect;

    private const string F = "Failed IoListBase.";
    internal void Delete() { Swipe.Delete(); MyFilter.Delete(); }
    internal IoListBase(IoPanel Owner, (string, string)[] Tools) {

        this.Tools = Tools;
        grid = new Grid();
        MyFilter = new(this.Owner = Owner, this);
        MyFilter.OnUpdate += Update;
        Swipe = new(false, true);
        Swipe.AnimationFinished += (_, _) => {
            toolbar?.IsEnabled = true; // unlock all content for the animation
        };
        Swipe.TriggerSwipe += (object? s, IoSwipe.SwipeEventArgs e)
            => Dispatcher.UIThread.Post(()
            => pageSelect?.ChangeValue(e.ToPage.Y + 1));
        MyPopup = new(grid, MyFilter);

        MyFilter.OnClose += (_, _)
          => {
              if (GetFilterSelect?.Button is IoToggleButton b) {
                  b.IsEnabled = true;
                  b.IsChecked = false;
              }
          };
        MyFilter.Swipe.OnSwipeStarted += (object? s, IoSwipe.SwipeEventArgs e) => {
            if (GetFilterSelect?.Button is IoToggleButton b && MyFilter.OutOfBounds(e.ToPage)) {
                b.IsEnabled = false;
                b.IsChecked = false;
            }
        };
        Content = MyPopup;

    }
    internal override Size PrepareLayout(Size available) {

        MyFilter.PrepareLayout(available);

        var filterSize = MyFilter.MinSize;
        if (tGrid == null || page == null)
            return new(120, 120);
        
        var toolSize = Static.MeasureService.Measure(tGrid);
        var listSize = Static.MeasureService.Measure(page);
        
        listSize = new(Math.Max(listSize.Width, toolSize.Width), listSize.Height + toolSize.Height);

        //plusW = filterSize.Width + listSize.Width,
        //plusH = filterSize.Height + listSize.Height,
        MyPopup.MinSplitWidth = 2 * Math.Max(filterSize.Width, listSize.Width);
        MyPopup.MinSplitHeight = 2 * Math.Max(filterSize.Height, listSize.Height);
        double
            shrinkW = MyPopup.MinSplitWidth / available.Width,
            shrinkH = MyPopup.MinSplitHeight / available.Height;

             

            //var ver = Width / available.Width >= Height / available.Height;


        //MyPopup.ratioW = filterSize.Width / plusW;
        //MyPopup.ratioH = filterSize.Height / plusH;

        MyPopup.ratioW = .5;
        MyPopup.ratioH = .5;

        if (shrinkW > shrinkH) {
            if (MyPopup.Hor > 0) {
                
            }
            //MyPopup.OpenHor = 0;
            //MyPopup.OpenVer = 1;
            return MinSize = new(Math.Max(filterSize.Width, listSize.Width) + 12, MyPopup.MinSplitHeight + 12);
        } else {
            //MyPopup.Hor = 1;
            //MyPopup.Ver = 0;
            return MinSize = new(MyPopup.MinSplitWidth + 12, Math.Max(filterSize.Height, listSize.Height) + 12);
        }

        //return MinSize = shrinkW > shrinkH
        //    ? new(Math.Max(filterSize.Width, listSize.Width) + 12, plusH)
        //    : new(plusW + 12, Math.Max(filterSize.Height, listSize.Height));
    }
    protected virtual void Update(object? s, EventArgs e) {
        var p = MyFilter.Filters?.PageSize ?? 10;
        if (p != pageSize) {
            SetPageSize(p);
            Owner.UpdateMinSize();
        } else
            App.RefreshPanel(Owner, 1 << MyIndex, false);
    }
    internal abstract bool RefreshData(CancellationToken? cancel, uint startIndex = 0, uint stopIndex = 0);
    protected virtual void MakeElements() {
        if (MyGen == null)
            return;
        uint iElement = 0;
        (string From, string To)? g = null;
        foreach (var entry in SourceList) {
            if (entry.Key is (string From, string To)) {
                // if we got a group label, remember to put it at top of the next row
                g = (From, To);
                continue;
            }
            var grid = MyGen.MakeElement(this, iElement, entry);
            if (grid == null)
                continue;
            // Wrap with a group boundary label and add to pageEntries, or just add to pageEntries.
            if (g != null)
                MakeGrouped(g, grid);  // Make group label
            else
                PageEntries.Add((new UserControl { Content = grid }, null)); // Just a row without a group label
            g = null;
            ++iElement;
        }
        // if we are left with one last boundary, make one last empty element, and attach the label to that
        if (g != null)
            MakeGrouped(g, new Grid());
    }
    //protected virtual UserControl MakeElement() => new();
    internal void ClearUp() {
        foreach (var p in PageEntries) {
            p.Parent?.Children.Remove(p.Entry);
        }
    }
    internal void ClearContent() {
        foreach (var p in PageEntries) {
            ClearContent(p.Entry.Content);
            p.Entry.Content = null;
        }
        ClearUp();
    }
    private void ClearContent(object? IContent) {
        if (IContent is StackPanel sp) {
            foreach (var C in sp.Children)
                ClearContent(C);
            sp.Children.Clear();
        }
        if (IContent is Grid gr) {
            foreach (var C in gr.Children)
                ClearContent(C);
            gr.Children.Clear();
            gr.ColumnDefinitions.Clear();
            gr.RowDefinitions.Clear();
        }
    }
    internal void ToggleFilter(object? s, EventArgs e) {
        var swipe = MyFilter?.Swipe ?? throw new(F + "Swipe");
        if (swipe.Time < 1)
            return;
        GetFilterSelect?.Button.IsChecked = true;
        if (MyPopup.Opened) {
            // Close the filters via the toolbar button
            MyFilter.Sanity();
            MyFilter?.Swipe.StartSwipeForced((-MyPopup.Hor, -MyPopup.Ver), MyPopup.Hor != 0, MyPopup.Ver != 0);
            return;
        } else MyPopup.Open(/*Owner.ratio*/);
        MyFilter?.Refresh();
    }
    internal void Refresh() {

        //Selecteds = [];
        PageEntries = [];
        MakeElements();

        // Display the toolbar and page
        JustGrid(ref grid);
        if (grid == null)
            throw new(F + "grid");
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));


        // Toolbar
        ulong max = (Math.Max(1, (ulong)PageEntries.Count) - 1) / pageSize + 1;
        Swipe.TransformBounds.ExcMaxY = (int)max;

        MakeMyToolbar(max, Swipe.Page.Y);

        if (toolbar == null)
            throw new(F + "toolbar");

        // Toolbar grid:
        JustGrid(ref tGrid);
        if (tGrid == null)
            throw new(F + "tGrid");
        tGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        tGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        tGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        // open search filter
        JustButton(ref filterSelect, Owner, "T_SearchFilter", App.L("Filter"), ToggleFilter);
        if (filterSelect == null)
            throw new(F + "filterSelect");
        ToolbarList.Add(filterSelect.Button);
        Grid.SetColumn(filterSelect.Button, 1);

        AddChild(tGrid, toolbar);
        AddChild(tGrid, filterSelect.Button);
        FinishChildren(tGrid);

        // Page:
        SetupPage();

        // final grid content:
        Control First = tGrid,
            Second = Swipe;
        GridLength GridFirst = GridLength.Auto,
            GridSecond = GridLength.Star;
        if (GetDisplayMode == IMode.Fullscreen)
            (First, Second, GridFirst, GridSecond)
                = (Second, First, GridSecond, GridFirst);
        grid.RowDefinitions.Add(new RowDefinition(GridFirst));
        grid.RowDefinitions.Add(new RowDefinition(GridSecond));

        Grid.SetRow(First, 0);
        AddChild(grid, First);
        Grid.SetRow(Second, 1);
        AddChild(grid, Second);
        FinishChildren(grid);

        Swipe.ClipToBounds = true;

        // Report How many filtered resuts:
        MyFilter.ResultsText
           = Results.ToString() + " = " + SourceCount.ToString() + " - " + (SourceCount - Results).ToString();
        MyFilter?.Results?.Text?.Text = MyFilter.ResultsText;

        var t = MyFilter?.dTexts;
    }
    private void MakeMyToolbar(ulong max, int page) {
        JustWrapPanel(ref toolbar);
        if (toolbar == null)
            throw new(F + "toolbar");
        ToolbarList.Clear();
        SetupToolbar(max, page);
        OnSetupToolbar();
        foreach (var C in ToolbarList)
            AddChild(toolbar, C);//toolbar.Children.Add(C);
        FinishChildren(toolbar);
    }
    protected void SetupToolbar(ulong max, int page) {

        // page select
        void pageChanged(object? s, EventArgs e)
            => SetPage((Swipe.Page.X, ((int?)pageSelect?.Value ?? 1) - 1));
        JustNumeric(ref pageSelect, Owner, "T_Page", (ulong)(page + 1), 1, max, 1, 1, "Page", false, App.L("Page") + ": ", " / " + max.ToString() + " ", pageChanged);
        if (pageSelect == null)
            throw new(F + "pageSelect");
        if(max > 1) 
            ToolbarList.Add(pageSelect);

        // page size select
        /*void changePageSize(object? s, EventArgs e) {
            SetPageSize((uint?)pageSizeSelect?.Value ?? 10);
        }
        JustNumeric(ref pageSizeSelect, "T_PageLength", pageSize, 1, 100, 1, 1, "PageSize", false, App.L("PageSize") + ": ", "", changePageSize);
        if (pageSizeSelect == null)
            throw new(F + "pageSizeSelect");
        ToolbarList.Add(pageSizeSelect);*/

        // custom tool tabs
        while (toolButton.Count < Tools.Length)
            toolButton.Add(null);
        for (int i = 0; i < Tools.Length; ++i) {

            var t = toolButton[i] = MakeSmallButton("CustomTool." + i.ToString(), Owner, Tools[i].Tooltip, Tools[i].Text, ClickTool, false, Avalonia.Media.FontWeight.Normal, null, i)
                ?? throw new(F + "CustomTool");
            ToolbarList.Add(t.Button);
        }
    }
    internal virtual void RefreshTemplate<IT>(IEnumerable<IT>? source, Func<object?, IoGen.Data?> itemT, Func<object?, string[]> filterCommandT) {
        ItemTemplate = itemT;
        FilterCommandTemplate = filterCommandT;
    }
    internal virtual void OnSetupToolbar() { }
    internal void SetupPage() {

        // perform swipe animation?
        if (Swipe.PrevPage.Y != Swipe.Page.Y) {
            var _prev = prevPage; // swap the pages for the animation
            prevPage = Swipe.Time < 1 ? (page ?? null) : null;
            page = _prev;
        }
        // init grid:
        JustGrid(ref page);
        if (page == null)
            throw new(F + "page");
        page.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        // build the elements:
        int row = 0;
        for (uint i = (uint)(Swipe.Page.Y * pageSize); i < Math.Min((Swipe.Page.Y + 1) * pageSize, PageEntries.Count); ++i) {
            page.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            var e = PageEntries[(int)i];
            e.Parent?.Children.Remove(e.Entry);
            Grid.SetRow(e.Entry, row++);
            AddChild(page, e.Entry);//page.Children.Add(PageEntries[(int)i]);
            e.Parent?.Children.Remove(e.Entry);
            PageEntries[(int)i] = (e.Entry, page);
        }
        FinishChildren(page);
        if(tGrid != null)
            PrepareLayout(App.Available);
        Swipe.RefreshHost(prevPage, page);

    }
    internal void SetPage((int X, int Y) NewPage) {
        if (NewPage == Swipe.Page)
            return;
        if (Swipe.SwipeTo(NewPage))
            toolbar?.IsEnabled = true;
        pageSelect?.Value = (ulong)(NewPage.Y + 1);
        SetupPage();
        //Owner.UpdateMinSize();
    }
    internal void SetPageSize(uint NewPageSize) {
        if (NewPageSize == pageSize)
            return;
        prevPage = null; // ???
        pageSize = NewPageSize;
       
        Swipe.Page = Swipe.PrevPage = (0, 0); // do not swipe page size changes
        pageSizeSelect?.Value = NewPageSize;
        ulong max =  pageSize > 0 ? (Math.Max(1, (ulong)PageEntries.Count) - 1) / pageSize + 1 : 1;
        Swipe.TransformBounds.ExcMaxY = (int)max;
        MakeMyToolbar(max, Swipe.Page.Y);
        SetupPage();
    }
    internal virtual void ClickTool(object? s, EventArgs e) {
        OnClickTool?.Invoke(this, new ClickToolEventArgs() { FromList = MyIndex, Index = (int?)((Button?)s)?.Tag ?? -1 });
    }
    internal virtual void ClickRow(object? e, int index) {
        OnClickButton?.Invoke(this, new ClickRowEventArgs() { FromList = MyIndex, ClickedRow = e, Index = index });

    }
    protected void MakeGrouped((string From, string To)? g, Grid grid) {
        // element with a group boundary label above it:
        var gt = new IoTextBlockInfo { Text = (g.Value.From == "" || g.Value.To == "" ? g.Value.From : g.Value.From + " - ") + g.Value.To, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
        Grid? _groupGrid = null;
        JustGrid(ref _groupGrid);
        Grid groupGrid = _groupGrid ?? throw new(F + "Group.Grid");
        groupGrid.Children.Add(gt);
        groupGrid.Children.Add(grid);
        groupGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        groupGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        groupGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        Grid.SetRow(grid, 1);
        PageEntries.Add((new UserControl { Content = groupGrid }, null));
        g = null;
    }
}
