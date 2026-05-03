using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Static;
using System;
using static IoListWallet.App;
using static IoListWallet.Core.Static.ConstructionHelpers;

namespace IoListWallet.Core.Books;
internal class IoBook : IoUserControl {

    internal class FillPageEventArgs : EventArgs {
        internal (int X, int Y) Page;
        internal bool Simulated = false;
    }

    internal event EventHandler<EventArgs>? OnRefresh, OnBeforeTabRefresh, OnClose, OnTabClick;

    internal event EventHandler<FillPageEventArgs>? FillPage;

    internal IoPanel Owner;

    internal string[] PageNames = [""];
    internal IoSwipe Swipe;
    internal Size MaxSize;

    protected WrapPanel? MainPageSelect;
    protected int row;
    internal (int MinX, int MinY, int MaxX, int MaxY) 
        CloseOnPage = (-1, -1, int.MaxValue, 1);

    private TranslateTransform transformToolbar;
    private Grid main;
    private Grid? prevG, nowG;

    private const string F = "Failed IoBook.";
    internal IoBook(IoPanel owner) : base() {
        Owner = owner;
        transformToolbar = new TranslateTransform(0, 0);
        Content = main = MakeGrid("");
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
                    ? b.Button : null,
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
    static void ApplyTemplatesRecursive(Control c) {
        c.ApplyTemplate();
        if (c is Panel p)
            foreach (var chs in p.Children)
                if (chs is Control cc)
                    ApplyTemplatesRecursive(cc);
        if(c is Viewbox v && v.Child is Control ch)
            ApplyTemplatesRecursive(ch);
        if (c is ContentControl b && b.Content is Control con)
            ApplyTemplatesRecursive(con);
    }
    internal void Delete() => Swipe.Delete();
    internal override Size PrepareLayout(Size available) {

        RefreshToolbar(false); // contents of this function:
        Size toolSize;
        if (MainPageSelect != null) {
            toolSize = MeasureService.Measure(MainPageSelect);
            available = new(available.Width, available.Height - toolSize.Height);
            MaxSize = new(toolSize.Width, 120);
        } else { MaxSize = new(0, 120); toolSize = new(0, 0); }
        //var p = Swipe.Page;
        for (var i = 0; i < PageNames.Length; ++i) {
            //Swipe.Page = (i, 0);
            var k = "s.page." + i.ToString();
            var n = MakeGrid(k);
            n.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Auto)));
            n.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(3, GridUnitType.Star)));
            row = 0;
            FillPage?.Invoke(this, new FillPageEventArgs() { Page = (i, 0), Simulated = true });
            FinishChildren(n); // attaches the elements we made in the FillPage invoke to the grid
            Grid.SetRow(n, 0);
            if (n.Children.Count > 0) {
                var s = MeasureService.Measure(n);
                MaxSize = new(Math.Max(s.Width, MaxSize.Width), Math.Max(s.Height, MaxSize.Height));
            }
            n.Children.Clear();
        }
        //Swipe.Page = p;
        return MinSize = new(MaxSize.Width + 12, MaxSize.Height + toolSize.Height);
    }

    internal void Refresh() {
        // Content Grid:
        main = MakeGrid("");
        main.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        // make toolbar:
        RefreshToolbar();
        main.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        AddChild(main, Swipe);
        FinishChildren(main);

        // Page we are swapping from
        prevG = Swipe.Time < 1 && !Swipe.isSnapBack /*&& Swipe.Page.X >= 0*/
            && (Swipe.PrevPage.X != Swipe.Page.X || Swipe.PrevPage.Y == 0)
            ? GetPage(Swipe.PrevPage.X) : null;
        
        // Make Page Grid:
        nowG = Swipe.Page.X >= 0 && Swipe.Page.Y == 0 ? ( MakeGrid("page." + Swipe.Page.X.ToString())) : null;

        main.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        if (nowG != null) {
            nowG.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Auto)));
            nowG.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(3, GridUnitType.Star)));

            // fill the content page
            row = 0;
            FillPage?.Invoke(this, new FillPageEventArgs() { Page = Swipe.Page });
            FinishChildren(nowG);
        }
        FinishChildren(main);
        Swipe.TransformBounds.ExcMaxX = PageNames.Length;
        Grid.SetRow(Swipe, 1);

        // Refresh calls
        Swipe.RefreshHost(prevG, nowG);
        OnRefresh?.Invoke(this, new EventArgs());
    }
    /// <summary>
    /// Add a row item to a page Swipe.Page.X
    /// </summary>
    /// <param name="entryKey">item dictionary key, make sure this is unique per item on a single page</param>
    /// <param name="labelTextLocaleText">text on the left, localized, and with ": " attached, empty string will merge the columns for the main content</param>
    /// <param name="controls">top layers of controls to attach, with size weights</param>
    /// <param name="labelBrush">Color of he label text</param>
    internal void MakeRow(FillPageEventArgs e, string entryKey, string labelTextLocaleText, (Control Control, double Size)[] controls, IImmutableSolidColorBrush? labelBrush = null, bool show = true) {
        if (!show)
            return;
        // entryKey must be unique and must have a format: s + "string";
        var s = e.Simulated ? "s." : "";
        if (!dGrids.TryGetValue(s + "page." + e.Page.X.ToString(), out var g) || g == null)
            throw new(F + "MakeRow.Grid");
        // Label:
        g.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        bool isLabel = labelTextLocaleText != "" && labelTextLocaleText[0] != '-';

        if (isLabel) {
            var Text = MakeTextBlock(s + "label." + e.Page.X.ToString() + "." + entryKey, Owner, "", L(labelTextLocaleText) + ": ");
            Text.Text.Foreground = labelBrush ?? Brushes.White;
            Text.Text.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            Text.Text.TextAlignment = TextAlignment.Left;
            Grid.SetRow(Text.View, row);
            Grid.SetColumn(Text.View, 0);
            AddChild(g, Text.View);
        }
        // Grid2
        var gRow = MakeGrid(s + "row." + e.Page.X.ToString() + "." + entryKey);
        gRow.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        int collumn = 0;
        //gRow.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
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
    // Regenerates the toolbar. real=false in size measurement simulations so it dones't get actaully attached
    protected virtual void RefreshToolbar(bool real = true) {
        if (PageNames.Length <= 1 || main == null)
            return;
        MainPageSelect = MakeToolbar("MainPageSelect");
        //MainPageSelect.ClipToBounds = false;
        for (int i = 0; i < PageNames.Length; ++i) {
            var p = PageNames[i];
            var selected = Swipe.Page.X == i;
            void clickedTab(object? s, EventArgs e)
                => TabClick(s, new EventArgs() { });
            var pageButton = MakeSmallButton("pageButton." + i.ToString(), this, "T_SettingsAppTab." + p, L(p),
                clickedTab, selected, selected ? FontWeight.Bold : FontWeight.Normal, null, i);
            AddChild(MainPageSelect, pageButton.Button);
        }
        if(real)
            AddChild(main, MainPageSelect);
        FinishChildren(MainPageSelect);
        
    }
    protected virtual void TabClick(object? sender, EventArgs args) {
        
        BeforeTabRefresh((IoToggleButton?)sender);

        // NEW moved this line down here from above the BeforeTabRefresh, hopefully it will onyl fix the Overlay tab text
        OnTabClick?.Invoke(this, args);

        Refresh();
    }
    protected virtual void BeforeTabRefresh(IoToggleButton? button) {
        if (Swipe.SwipeTo(((int?)button?.Tag ?? 0, Swipe.Page.Y)))
            MainPageSelect?.IsEnabled = false; // lock all content for the animation
        OnBeforeTabRefresh?.Invoke(this, new EventArgs());
    }
    internal Grid? GetPage(int Page)
        => Page >= 0 ? ((dGrids.TryGetValue("page." + Page.ToString(), out var p) ? p : null)
            ?? throw new(F + "GetPage " + Page.ToString())) : null;
    internal WSToggleButton? GetPageButton(int Page)
      => Page >= 0 ? ((dSButtons.TryGetValue("pageButton." + Page.ToString(), out var p) ? p : null)
          ?? throw new(F + "GetPageButton " + Page.ToString())) : null;
}

