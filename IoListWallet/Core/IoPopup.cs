using Avalonia;
using Avalonia.Controls;
using IoListWallet.Core.Books;
using System;
using static IoListWallet.Core.Static.AppSave;
using static IoListWallet.Core.Static.ConstructionHelpers;
using static IoListWallet.Core.IoSwipe;
using IoListWallet.Core.Static;

namespace IoListWallet.Core;

internal class IoPopup : IoUserControl {

    internal Control Core;
    internal IoBook Popup;
    internal bool Opened = false;

    protected Grid GridPopup, GridCore, Main;
    protected GridLength A, B, H;


    internal short Hor, Ver;

    internal double ratioW, ratioH, MinSplitWidth, MinSplitHeight;
    internal IoPopup(Control core, IoBook popup) : base() {
        Core = core;
        Popup = popup;
        ClipToBounds = false;
        Popup.Swipe.OnUpdateTransform += UpdateTransform;
        Main = MakeGrid("Main") ?? throw new("Failed IoListPanel.Main");
        GridPopup = MakeGrid("GridPopup") ?? throw new("Failed IoListPanel.GridPopup");
        GridCore = MakeGrid("GridList") ?? throw new("Failed IoListPanel.GridList");
        A = new GridLength(0, GridUnitType.Star);
        B = new GridLength(1, GridUnitType.Star);
        H = new GridLength(.5, GridUnitType.Star);
        GridPopup.ColumnDefinitions.Add(new ColumnDefinition(H));
        GridPopup.ColumnDefinitions.Add(new ColumnDefinition(H));
        GridPopup.RowDefinitions.Add(new RowDefinition(H));
        GridPopup.RowDefinitions.Add(new RowDefinition(H));
        GridCore.ColumnDefinitions.Add(new ColumnDefinition(A));
        GridCore.ColumnDefinitions.Add(new ColumnDefinition(B));
        GridCore.RowDefinitions.Add(new RowDefinition(A));
        GridCore.RowDefinitions.Add(new RowDefinition(B));
        Grid.SetColumnSpan(Core, 2);
        Grid.SetRowSpan(Core, 2);
        Grid.SetColumn(Core, 0);
        Grid.SetRow(Core, 0);
        Main.RowDefinitions.Add(new RowDefinition(B));
        Main.ColumnDefinitions.Add(new ColumnDefinition(B));
        AddChild(Main, GridPopup);
        AddChild(Main, GridCore);

        //AddChild(Main, t);

        FinishChildren(Main);
        AddChild(GridPopup, Popup);
        FinishChildren(GridPopup);
        AddChild(GridCore, Core);
        FinishChildren(GridCore);
        Grid.SetRow(Popup, 1);
        Grid.SetColumn(Popup, 1);
        Content = Main;
        Popup.OnClose += OnClose;
        Popup.IsVisible = false;

        // This will close the filter if the windw shrinks too much for it
        PropertyChanged += (s, e) =>
        {
            if (e.Property == BoundsProperty) {
                if (Opened && Popup.Swipe.Time >= 1 && (Hor > 0 ? Bounds.Width < MinSplitWidth : Bounds.Height < MinSplitHeight)) {
                    Popup.Sanity();
                    Popup.Swipe.StartSwipeForced((-Hor, -Ver), Hor != 0, Ver != 0);
                    Popup.Refresh();
                }
            }
        };
    }
    internal void OnClose(object? s, EventArgs e) {
        Opened = false;
        AfterTransform(0, 1, 0, 0);
        Popup.IsVisible = false;
        Popup.Swipe.Page = (0, 0);
    }
    private void UpdateTransform(object? s, UpdateTransformEventArgs e) {
        (short hor, short ver) = Opened ? (Hor, Ver) : ((short)0, (short)0);
        if (hor > 0) {
            AfterTransform(1 - ratioW, Math.Min(1, 1 - ratioW + ratioW * (
                Popup.Swipe.Page.X < 0
                    ? e.Current.X + 1
                    : Popup.Swipe.Page.X <= 0
                    ? Math.Max(0, e.Current.X)
                    : 0)), 1, 0);
        } else if (hor < 0) {
            // Please don't use this (left) one, unless it's only a isngle page book!
            AfterTransform(ratioW, Math.Min(1, ratioW + ratioW * (
               Popup.Swipe.Page.X > 0
                   ? e.Current.X - 1
                   : Popup.Swipe.Page.X >= 0
                   ? Math.Min(0, e.Current.X)
                   : 0)), -1, 0);
        } else if (ver > 0) {
            AfterTransform(1 - ratioH, Math.Min(1, 1 - ratioH + ratioH * (
              Popup.Swipe.Page.Y < 0
                  ? Math.Max(e.Current.Y + 1, 0)
                  : Popup.Swipe.Page.Y <= 0
                  ? /*Popup.Swipe.Time >= 1 || Popup.Swipe.isSnapBack ? e.Current.Y :*/ Math.Max(0, e.Current.Y) //e.Current.Y //Math.Max(0, e.Current.Y)
                  : 0)), 0, 1);
        } else if (ver < 0) {
            AfterTransform(ratioH, Math.Min(1, ratioH + ratioH * (
            Popup.Swipe.Page.Y > 0
                ? Math.Min(0, e.Current.Y - 1)
                : Popup.Swipe.Page.Y >= 0
                ? /*Popup.Swipe.Time >= 1 || Popup.Swipe.isSnapBack ? e.Current.Y :*/ Math.Min(0, e.Current.Y) //e.Current.Y // Math.Min(0, e.Current.Y)
                : 0)), 0, -1);

        } else AfterTransform(0, 1, 0, 0);
    }
    internal void AfterTransform(double o, double r, short hor, short ver, bool natural = true) {
        //if(natural)
        //    last = (r, hor, ver);
        r = Math.Clamp(r, 0, 1);
        GridCore.RowDefinitions[0] = new RowDefinition(new GridLength(ver > 1 ? 1 - r : r, GridUnitType.Star));
        GridCore.RowDefinitions[1] = new RowDefinition(new GridLength(ver > 1 ? r : 1 - r, GridUnitType.Star));
        GridCore.ColumnDefinitions[0] = new ColumnDefinition(new GridLength(hor > 1 ? 1 - r : r, GridUnitType.Star));
        GridCore.ColumnDefinitions[1] = new ColumnDefinition(new GridLength(hor > 1 ? r : 1 - r, GridUnitType.Star));
        Grid.SetColumnSpan(Core, /*(Hor = hor)*/hor == 0 || r >= 1 ? 2 : 1);
        Grid.SetRowSpan(Core, /*(Ver = ver)*/ver == 0 || r >= 1 ? 2 : 1);
        Grid.SetColumn(Core, hor >= 0 || r >= 1 ? 0 : 1);
        Grid.SetRow(Core, ver >= 0 || r >= 1 ? 0 : 1);
        if (hor != 0) {
            Grid.SetRow(Popup, 0);
            Grid.SetColumn(Popup, hor <= 0 ? 0 : 1);
            Grid.SetRowSpan(Popup, 2);
            Grid.SetColumnSpan(Popup, 1);
            Popup.SetToolbarOffset((r - o) / ratioW, 0);
        }
        if (ver != 0) {
            Grid.SetRow(Popup, ver <= 0 ? 0 : 1);
            Grid.SetColumn(Popup, 0);
            Grid.SetRowSpan(Popup, 1);
            Grid.SetColumnSpan(Popup, 2);
            Popup.SetToolbarOffset(0, (r - o) / ratioH);
        }
        //t.Text = "r:" + (r).ToString() + "\ndisp: " + Popup.Swipe.displaceNow.ToString();

        //(Bounds.Width / App.Available.Width >= Bounds.Height / App.Available.Height)
       

    }
    internal void Open(/*double ratio*/) {
        //if (Bounds.Width/* / App.Available.Width*/ <= Bounds.Height/* / App.Available.Height*/) {
        if (Bounds.Width < MinSplitWidth || (Bounds.Height >= MinSplitHeight && Bounds.Height > Bounds.Width)) {
            int d = GetDisplayMode == IMode.Fullscreen ? 0 : 2;
            Hor = 0;
            AfterTransform(d == 0 ? 1 - ratioH : ratioH, 1, 0, Ver = (short)(1 - d));
            Popup.Swipe.Page = (0, d - 1);
            Popup.Swipe.TransformBounds = (-1, -1, Popup.PageNames.Length, 2);
            Popup.CloseOnPage = (-1, -1, int.MaxValue, 1);
        } else {
            Hor = 1;
            Ver = 0;
            AfterTransform(1 - ratioW, 1, 1, 0);
            Popup.Swipe.TransformBounds = (-1, -1, Popup.PageNames.Length, 2);
            Popup.CloseOnPage = (-1, -1, int.MaxValue, 1);
            Popup.Swipe.Page = (-1, 0);
        }
        Opened = true;
        Popup.IsVisible = true;
        Popup.Swipe.SwipeTo((0, 0));
    }
}


/*internal override Size PrepareLayout(Size available) {
    var BookSize = Popup.PrepareLayout(available);
    var CoreSize = MeasureService.Measure(Core);

    Size sizeHorizontal = new(BookSize.Width + CoreSize.Width, Math.Max(BookSize.Height, CoreSize.Height)),
        sizeVertical = new(Math.Max(BookSize.Width, CoreSize.Width), BookSize.Height + CoreSize.Height);

    var ratioHorizontal = Math.Log(sizeHorizontal.Width / sizeHorizontal.Height);
    var ratioVertical = Math.Log(sizeVertical.Width / sizeVertical.Height);
    var ratioAvailable = Math.Log(available.Width / available.Height);
    if (Math.Abs(ratioHorizontal - ratioAvailable) < Math.Abs(ratioVertical - ratioAvailable)) {
        Hor = 1;
        Ver = 0;
        return MinSize = sizeHorizontal;
    }
    Hor = 0;
    Ver = 1;
    return MinSize = sizeVertical;

    //if( (  )
    //AfterTransform(0, -1, -1, false);
    //Popup.PrepareLayout();
    //Popup.IsVisible = true;
}*/
/*internal void AfterLayout() {
    AfterTransform(last.r, last.hor, last.ver);
    Popup.AfterLayout();
    if (Opened)
        Popup.Refresh();
    else
        Popup.IsVisible = false;
}*/