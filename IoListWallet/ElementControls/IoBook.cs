using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using IoListWallet.ElementControls.Basic;
using System;
using static IoListWallet.App;
using static IoListWallet.ConstructionHelpers;

namespace IoListWallet.ElementControls;
internal class IoBook : IoUserControl {

    internal event EventHandler<EventArgs>? FillPage, OnRefresh, OnBeforeTabRefresh, OnClose, OnTabClick;

    internal string[] PageNames = [""];
    internal IoSwipe Swipe;

    protected WrapPanel? MainPageSelect;
    protected int row;
    internal (int MinX, int MinY, int MaxX, int MaxY) 
        CloseOnPage = (-1, -1, int.MaxValue, 1);

    private TranslateTransform transformToolbar;
    private Grid main;
    private Grid? prevG, nowG;

    private static string F = "Failed IoSettings.";
    internal IoBook() : base() {
        transformToolbar = new TranslateTransform(0, 0);
        Content = main = MakeGrid("") ?? throw new(F + "new.main");
        Swipe = new(true, true) {
            TransformBounds = (0, -1, 1, 2),
            ClipToBounds = false
        };
        // unlock tab controls after swipe animation is finished
        Swipe.AnimationFinished += (_, _) => {
            MainPageSelect?.IsEnabled = true; // unlock all content for the animation
            if (OutOfBounds(Swipe.Page))
                OnClose?.Invoke(this, new EventArgs());
            Sanity();
        };
        // Request from Swiper to swipe will be met with clciking the tab it leads to
        Swipe.TriggerSwipe += (object? s, IoSwipe.SwipeEventArgs e)
            => Dispatcher.UIThread.Post(() => {
                if (e.ToPage.Y <= CloseOnPage.MinY || e.ToPage.Y >= CloseOnPage.MaxY 
                || e.ToPage.X <= CloseOnPage.MinX || e.ToPage.X >= CloseOnPage.MaxX)
                    SwipeAway(e.ToPage); 
                else TabClick(
                dSButtons.TryGetValue("pageButton." + e.ToPage.X.ToString(), out var b) && b != null
                    ? b.Value.Button : null,
                new EventArgs());
                    });
    }
    internal void Sanity() {
        if(Swipe.Page.X <= CloseOnPage.MinX)
            Swipe.Page.X = CloseOnPage.MinX + 1;
        if (Swipe.Page.Y <= CloseOnPage.MinY)
            Swipe.Page.Y = CloseOnPage.MinY + 1;
        if (Swipe.Page.X >= CloseOnPage.MaxX)
            Swipe.Page.X = CloseOnPage.MaxX - 1;
        if (Swipe.Page.Y >= CloseOnPage.MaxY)
            Swipe.Page.Y = CloseOnPage.MaxY - 1;
    }
    internal bool OutOfBounds((int X, int Y) NewPage) 
        => NewPage.X <= CloseOnPage.MinX
            || NewPage.Y <= CloseOnPage.MinY
            || NewPage.X >= CloseOnPage.MaxX
            || NewPage.Y >= CloseOnPage.MaxY;
    internal void SetToolbarOffset(double x, double y) {
        transformToolbar.X = x * Bounds.Width;
        transformToolbar.Y = y * Bounds.Height;
        MainPageSelect?.RenderTransform = transformToolbar;
    }
    protected virtual void SwipeAway((int toX, int toY) NewPage) {
        if (Swipe.SwipeTo(NewPage))
            MainPageSelect?.IsEnabled = false; // lock all content for the animation
        OnBeforeTabRefresh?.Invoke(this, new EventArgs());
        //Swipe.Time = 0;
        Refresh();
    }

    internal void PrepareLayout() {
        main = MakeGrid("") ?? throw new(F + "AfterLayout.main2");
        main.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        RefreshToolbar();
        main.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // TODO figure out why this can't be Star, when other's don't have a problem with it...
        var p = Swipe.Page;
        Swipe.ClearHost(null, null);
        //var i = 0;//var i = new Random().Next(PageNames.Length);
        for (var i = 0; i < PageNames.Length; ++i) {
            Swipe.Page = (i, 0);
            var n = MakeGrid("page." + i.ToString())
              ?? throw new(F + "page " + i.ToString());
            n.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Auto)));
            n.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(3, GridUnitType.Star)));
            row = 0;
            FillPage?.Invoke(this, new EventArgs());
            FinishChildren(n);
            Grid.SetRow(n, 1);
            AddChild(main, n);
        }
        FinishChildren(main);
        Swipe.Page = p;
    }
    internal void AfterLayout() {
        main = MakeGrid("") ?? throw new(F + "AfterLayout.main");
        main.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        RefreshToolbar();
        AddChild(main, Swipe);
        main.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        FinishChildren(main);
        Swipe.RefreshHost(null, nowG);
        Grid.SetRow(Swipe, 1);
    }

    internal void Refresh() {
        // Content Grid:
        main = MakeGrid("") ?? throw new(F + "Refresh.main");
        main.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        //Swipe.ClipToBounds = false;
        // make toolbar:
        RefreshToolbar();
        AddChild(main, Swipe);
        FinishChildren(main);

        // Page we are swapping from
        prevG = Swipe.Time < 1 && !Swipe.isSnapBack /*&& Swipe.Page.X >= 0*/
            && (Swipe.PrevPage.X != Swipe.Page.X || Swipe.PrevPage.Y == 0)
            ? GetPage(Swipe.PrevPage.X) : null;
        
        // Make Page Grid:
        nowG = Swipe.Page.X >= 0 && Swipe.Page.Y == 0 ? ( MakeGrid("page." + Swipe.Page.X.ToString()) 
            ?? throw new(F + "page " + Swipe.Page.X.ToString())) : null;

        main.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        if (nowG != null) {
            nowG.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Auto)));
            nowG.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(3, GridUnitType.Star)));

            // fill the content page
            row = 0;
            FillPage?.Invoke(this, new EventArgs());
            FinishChildren(nowG);
        }
        FinishChildren(main);
        Swipe.TransformBounds.ExcMaxX = PageNames.Length;
        Grid.SetRow(Swipe, 1);

        // Refresh calls
        Swipe.RefreshHost(prevG, nowG);
        OnRefresh?.Invoke(this, new EventArgs());
    }
    internal void MakeRow(string labelTextLocaleId, string labelTextLocaleText, (Control Control, double Size)[] controls, IImmutableSolidColorBrush? brush = null) {
        if (!dGrids.TryGetValue("page." + Swipe.Page.X.ToString(), out var g) || g == null)
            throw new(F + "MakeRow.Grid");
        // Label:
        g.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        bool isLabel = labelTextLocaleText != "" && labelTextLocaleText[0] != '-';

        if (isLabel) {
            var (Text, View) = MakeTextBlock("label." + Swipe.Page.X.ToString() + "." + labelTextLocaleId, "", L(labelTextLocaleText) + ": ")
                ?? throw new(F + "MakeRow.label " + labelTextLocaleId);
            Text.Foreground = brush ?? Brushes.White;
            Grid.SetRow(View, row);
            Grid.SetColumn(View, 0);
            AddChild(g, View);
        }

        // Grid2
        var gRow = MakeGrid("row." + Swipe.Page.X.ToString() + "." + labelTextLocaleId) 
            ?? throw new(F + "MakeRow.gRow");
        gRow.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // WAS STAR - this one seems to be causing it, nevermind
        int collumn = 0;
        foreach (var control in controls) {
            gRow.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(control.Size, GridUnitType.Star)));
            Grid.SetColumn(control.Control, collumn++);
            AddChild(gRow, control.Control);
        }

        // Without a label, we will span both columns
        Grid.SetRow(gRow, row);
        if (isLabel) {
            Grid.SetColumn(gRow, 1);
            Grid.SetColumnSpan(gRow, 1);
        } else {
            Grid.SetColumn(gRow, 0);
            Grid.SetColumnSpan(gRow, 2);
        }

        // finish the row
        FinishChildren(gRow);
        AddChild(g, gRow);
        ++row;
    }
    protected virtual void RefreshToolbar() {
        if (PageNames.Length <= 1 || main == null)
            return;
        MainPageSelect = MakeToolbar("MainPageSelect") 
            ?? throw new(F + "mainPageSelect");
        //MainPageSelect.ClipToBounds = false;
        for (int i = 0; i < PageNames.Length; ++i) {
            var p = PageNames[i];
            var selected = Swipe.Page.X == i;
            void clickedTab(object? s, EventArgs e)
                => TabClick(s, new EventArgs() { });
            var pageButton 
                = MakeSmallButton("pageButton." + i.ToString(), "T_SettingsAppTab." + p, L(p),
                clickedTab, selected, selected ? FontWeight.Bold : FontWeight.Normal)
                ?? throw new(F + "mainPageSelect.Button");
            pageButton.Button.Tag = i;
            AddChild(MainPageSelect, pageButton.Button);
        }
        main.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        FinishChildren(MainPageSelect);
        AddChild(main, MainPageSelect);
    }
    protected virtual void TabClick(object? sender, EventArgs args) {
        OnTabClick?.Invoke(this, args);
        BeforeTabRefresh((IoToggleButton?)sender);
        Refresh();
    }
    protected virtual void BeforeTabRefresh(IoToggleButton? button) {
        if (Swipe.SwipeTo(((int)(button?.Tag ?? 0), Swipe.Page.Y)))
            MainPageSelect?.IsEnabled = false; // lock all content for the animation
        OnBeforeTabRefresh?.Invoke(this, new EventArgs());
    }
    internal Grid? GetPage(int Page)
        => Page >= 0 ? ((dGrids.TryGetValue("page." + Page.ToString(), out var p) ? p : null)
            ?? throw new(F + "GetPage " + Page.ToString())) : null;
    internal (TextBlock Text, IoToggleButton Button)? GetPageButton(int Page)
      => Page >= 0 ? ((dSButtons.TryGetValue("pageButton." + Page.ToString(), out var p) ? p : null)
          ?? throw new(F + "GetPageButton " + Page.ToString())) : null;
}

