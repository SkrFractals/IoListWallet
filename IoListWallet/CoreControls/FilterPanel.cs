using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using HarfBuzzSharp;
using IoListWallet.ElementControls;
using IoListWallet.ElementControls.Basic;
using IoListWallet.MainControls.Panels;
using IoListWallet.Structs;
using System;
using System.Collections.Generic;
using static IoListWallet.App;
using static IoListWallet.AppSave;

namespace IoListWallet.CoreControls;
// This is the panel with filter settings that opens when you click "Filter" on any SearchList containing Panel
internal class FilterPanel<T> : IoBook {

    // VAR
    //internal IoPopup MyPopup;

    internal IoList<T> MyList;
    internal FilterPanel(IoList<T> myList) {
        MyList = myList;
        PageNames = ["Sort", "Search", "AddSearch"];
        FillPage += OnFillPage;
        OnBeforeTabRefresh += BeforeTabRefresh;
    }

    //[Range(1,64)]
    //static byte MaxSize = 4; // How many Search tabs allowed?
   
    internal IoFilter? Filters;
    internal string ResultsText = "";
    internal (TextBlock Text, Viewbox View)? Results;
    internal event EventHandler<EventArgs>? OnUpdate;

    private int Search = 0;
    private List<ushort> SortIndices = [];
    protected override void SwipeAway((int toX, int toY) NewPage) {
        // turn swipeaway in orthogonal directions, diagonal to hide the popup:
        if (MyList.MyPopup.Hor > 0 && MyList.Swipe.Page.X > CloseOnPage.MinX) NewPage = (CloseOnPage.MinX, NewPage.toY);
        if (MyList.MyPopup.Hor < 0 && MyList.Swipe.Page.X < CloseOnPage.MaxX) NewPage = (CloseOnPage.MaxX, NewPage.toY);
        if (MyList.MyPopup.Ver > 0 && MyList.Swipe.Page.Y > CloseOnPage.MinY) NewPage = (NewPage.toX, CloseOnPage.MinY);
        if (MyList.MyPopup.Ver < 0 && MyList.Swipe.Page.Y < CloseOnPage.MaxY) NewPage = (NewPage.toX, CloseOnPage.MaxY);
        base.SwipeAway(NewPage);
    }
    internal void OnFillPage(object? s, EventArgs e) {
        if (Filters == null)
            return;

        Search = Swipe.Page.X - 1;
        SortIndices.Clear();
        ushort Index = 0;
        var S = Search;
        ulong TakeBit(ulong mask) => mask & ((ulong)1 << S);
        ulong SetBit(ref ulong mask, ulong bit) => mask = (mask & ~((ulong)1 << S)) + (bit << S);

        void UpdateNumeric(IoNumericLeftRight? e, FilterType filter, Action<IoNumericLeftRight> lambda, ulong customDisabled = 0) {
            if (e != null) {
                lambda(e);
                if (TakeBit(filter.Disabled) == 0 && TakeBit(filter.Disabled) == 0)
                    OnUpdate?.Invoke(this, new EventArgs());
            }
        }
        void UpdateText(IoTextBox? e, FilterType filter, Action<IoTextBox> lambda) {
            if (e != null && e.Text != null) {
                lambda(e);
                if (TakeBit(filter.Disabled) == 0)
                    OnUpdate?.Invoke(this, new EventArgs());
            }
        }
        void UpdateCombo(IoComboBox? e, FilterType filter, Action<ushort> lambda) {
            if (e != null && e.SelectedItem != null) {
                lambda((ushort)e.SelectedIndex);
                if (TakeBit(filter.Disabled) == 0)
                    OnUpdate?.Invoke(this, new EventArgs());
            }
        }
        void UpdateDate(IoDatePicker? e, FilterType filter, Action<DateTime> lambda) {
            if (e != null && e.SelectedDate != null) {
                lambda(e.SelectedDate == null ? DateTime.Now : e.SelectedDate.Value.DateTime);
                if (TakeBit(filter.Disabled) == 0)
                    OnUpdate?.Invoke(this, new EventArgs());
            }
        }
        var Page = Swipe.Page.X;
        string PageS = Page.ToString(), F = "Failed FilterPanel.";

        if (Page > 0 && Page <= Filters.Size) {

            // Remove Search Button:
            var removeSearch = MakeButton("removeSearch." + PageS, "T_RemoveSearch", L("Remove"), DeleteSearch, false, FontWeight.Bold, L("AreYouSure"))
                ?? throw new(F + "RemoveSearch");
            //removeSearch.Button.IsEnabled = Filters.Size > 1;
            if (Filters.Size > 1)
                MakeRow("_RemoveSearch", "RemoveSearch", [(removeSearch.Button, 1)]);

            foreach (var filter in Filters.Filters) {
                if (filter == null)
                    throw new(F + "filter");
                var name = filter.Name;


                // Enable Field Button:
                void enablePressed(object? s, EventArgs e) {
                    var d = filter.Disabled;
                    SetBit(ref d, (ulong)((((IoToggleButton?)s)?.IsChecked ?? false) ? 0 : 1));
                    filter.Disabled = d;
                    if (dButtons.TryGetValue("enable." + PageS + "." + name, out var b) && b != null)
                        b.Value.Text.Text = Dis(TakeBit(filter.Disabled) > 0);
                    OnUpdate?.Invoke(this, new EventArgs());
                }
                var disabledF = TakeBit(filter.Disabled) > 0;
                var EnableF = MakeButton("enable." + PageS + "." + name, "T_Enable", Dis(disabledF), enablePressed, !disabledF, FontWeight.Normal, "")
                    ?? throw new(F + "Disabled " + name);

                if (filter is FilterText ft) {

                    // Regexp
                    void set(TextBox s) { 
                        ft.Regexp[S] = ((TextBox?)s)?.Text ?? "";
                        IoPanel.Dirty();
                    }
                    void update(object? s, EventArgs _)
                        => UpdateText((IoTextBox?)s, filter, set);
                    var regexp = MakeTextBox("regexp." + PageS + "." + name, "T_Regexp", ft.Regexp[S], "Regexp", update, update)
                        ?? throw new(F + "Regexp " + name);
                    MakeRow(name, name, [(EnableF.Button, 1), (regexp.Border, 2)]);

                } else if (filter is FilterNumber fn) {

                    // Number Min-Max
                    void setMin(IoNumericLeftRight s) { 
                        fn.Range[S] = new(s.Value, fn.Range[S].Max);
                        IoPanel.Dirty();
                    }
                    void setMax(IoNumericLeftRight s) { 
                        fn.Range[S] = new(fn.Range[S].Min, s.Value);
                        IoPanel.Dirty();
                    }
                    void updateMin(object? s, EventArgs _)
                        => UpdateNumeric((IoNumericLeftRight?)s, filter, setMin);
                    void updateMax(object? s, EventArgs _)
                        => UpdateNumeric((IoNumericLeftRight?)s, filter, setMax);
                    var Min = MakeNumeric("min." + PageS + "." + name, "T_FilterMinValue", fn.Range[S].Min, fn.MinMin, fn.MaxMin, 1, 10, "Min", "", "", updateMin)
                        ?? throw new(F + "Number.Min " + name);
                    var Max = MakeNumeric("max." + PageS + "." + name, "T_FilterMaxValue", fn.Range[S].Max, fn.MinMax, fn.MaxMax, 1, 10, "Max", "", "", updateMax)
                        ?? throw new(F + "Number.Max " + name);
                    var (_, DashV) = MakeTextBlock("dash." + PageS + "." + name, "", " - ")
                        ?? throw new(F + "Number.Dash " + name);
                    MakeRow(name, name, [(EnableF.Button, 4), (Min, 4), (DashV, 1), (Max, 4)]);

                } else if (filter is FilterEnum fe) {

                    // Enum Match
                    void set(ushort m) { 
                        fe.Rule[S] = new(m, fe.Rule[S].Rule);
                        IoPanel.Dirty();
                    }
                    void setR(ushort r) { 
                        fe.Rule[S] = new(fe.Rule[S].Match, (byte)r);
                            IoPanel.Dirty();
                        }
                    void update(object? s, EventArgs _)
                        => UpdateCombo((IoComboBox?)s, filter, set);
                    void updateR(object? s, EventArgs _)
                        => UpdateCombo((IoComboBox?)s, filter, setR);
                    var ToMatch = new ushort[fe.Max - fe.Min + 1];
                    for (ushort m = fe.Min; m <= fe.Max; ++m)
                        ToMatch[m - fe.Min] = m;
                    var EnumTemplate = new FuncDataTemplate<ushort>((index, _)
                        => new TextBlock { Text = ETypes.TryGetValue(fe.EnumType, out var type) ? GetEnumText((Enum)Enum.ToObject(type, index)) : "" }, false);
                    var compTemplate = new FuncDataTemplate<ushort>((index, _)
                        => new TextBlock { Text = GetEnumText((CompareRule)index) }, false);
                    var (EnumC, EnumV) = MakeComboBox("enum." + PageS + "." + name, "T_FilterEnum", ToMatch, fe.Rule[S].Match, EnumTemplate, update)
                        ?? throw new("Enum " + name);
                    var (RuleC, RuleV) = MakeComboBox("enumRule." + PageS + "." + name, "T_FilterEnumRule", Enum.GetValues<CompareRule>(), (CompareRule)fe.Rule[S].Rule, compTemplate, updateR)
                        ?? throw new(F + "EnumRule " + name);
                    MakeRow(name, name, [(EnableF.Button, 1), (EnumV, 2), (RuleV, 2)]);

                } else if (filter is FilterDate fd) {

                    // Date Min-Max
                    void setMin(DateTime t) {
                        fd.Range[S] = new(t, fd.Range[S].Max);
                        IoPanel.Dirty();
                    }
                    void setMax(DateTime t) { 
                        fd.Range[S] = new(fd.Range[S].Min, t);
                        IoPanel.Dirty();
                    }
                    void updateMin(object? s, DatePickerSelectedValueChangedEventArgs _)
                        => UpdateDate((IoDatePicker?)s, filter, setMin);
                    void updateMax(object? s, DatePickerSelectedValueChangedEventArgs _)
                        => UpdateDate((IoDatePicker?)s, filter, setMax);
                    var (_, MinV) = MakeDatePick("min." + PageS + "." + name, "T_FilterMinDate", fd.Range[S].Min, updateMin)
                        ?? throw new(F + "Date.Min " + name);
                    var (_, MaxV) = MakeDatePick("max." + PageS + "." + name, "T_FilterMaxDate", fd.Range[S].Max, updateMax)
                        ?? throw new(F + "Date.Max " + name);
                    var (_, dashV) = MakeTextBlock("dash." + PageS + "." + name, "", " - ")
                        ?? throw new(F + "Date.Dash " + name);
                    MakeRow(name, name, [(EnableF.Button, 3), (MinV, 2), (dashV, 1), (MaxV, 2)]);

                } else if (filter is FilterToken fa) {
                   
                    // Token Min-Max
                    void set(IoTextBox s) { 
                        fa.Cond[S] = new(s?.Text ?? "", fa.Cond[S].Min, fa.Cond[S].Max);
                        IoPanel.Dirty();
                    }
                    void update(object? s, EventArgs _)
                        => UpdateText((IoTextBox?)s, filter, set);
                    void setd(object? s, EventArgs _) {
                        var d = fa.DisabledValues;
                        SetBit(ref d, (ulong)((((IoCheckBox?)s)?.IsChecked ?? false) ? 0 : 1));
                        fa.DisabledValues = d;
                        if (dButtons.TryGetValue("d." + PageS + "." + name, out var b) && b != null)
                            b.Value.Text.Text = Dis(TakeBit(fa.DisabledValues) > 0);
                        OnUpdate?.Invoke(this, new EventArgs());
                        IoPanel.Dirty();
                    }
                    void setMin(IoNumericLeftRight s) { 
                        fa.Cond[S] = new(fa.Cond[S].Ticker, s.Value, fa.Cond[S].Max);
                        IoPanel.Dirty();
                    }
                    void setMax(IoNumericLeftRight s) {
                        fa.Cond[S] = new(fa.Cond[S].Ticker, fa.Cond[S].Min, s.Value);
                        IoPanel.Dirty();
                    }
                    void updateMin(object? s, EventArgs _)
                        => UpdateNumeric((IoNumericLeftRight?)s, filter, setMin, fa.DisabledValues);
                    void updateMax(object? s, EventArgs _)
                        => UpdateNumeric((IoNumericLeftRight?)s, filter, setMax, fa.DisabledValues);
                    var Regexp = MakeTextBox("regexpt." + PageS + "." + name, "T_FilterTicker", fa.Cond[S].Ticker, "Regexp", update)
                        ?? throw new(F + "Token.Regexp " + name);
                    var disabledT = TakeBit(fa.DisabledValues) > 0;
                    var EnableT = MakeButton("d." + PageS + "." + name, "T_FilterTokenValue", Dis(disabledT), setd, !disabledT, FontWeight.Normal, "")
                        ?? throw new(F + "Token.EnableValue" + name);
                    var Min = MakeNumeric("mint." + PageS + "." + name, "T_FilterMinTokens", fa.Cond[S].Min, fa.MinMin, fa.MaxMin, 1, 10, "Min", "", "", updateMin)
                       ?? throw new(F + "Number.Mint " + name);
                    var Max = MakeNumeric("maxt." + PageS + "." + name, "T_FilterMaxTokens", fa.Cond[S].Max, fa.MinMax, fa.MaxMax, 1, 10, "Max", "", "", updateMax)
                        ?? throw new(F + "Number.Maxt " + name);
                    var Dash = MakeTextBlock("dash." + PageS + "." + name, "", " - ") ?? throw new(F + "Token.Dash" + name);
                    MakeRow(name, name, [(EnableF.Button, 4), (Regexp.Border, 4), (EnableT.Button, 1), (Min, 4), (Dash.View, 1), (Max, 4)]);
                }
                SortIndices.Add(Index++);
            }
        } else foreach (var filter in Filters.Filters) SortIndices.Add(Index++);
        if (Page <= 0) {

            string Inc(bool inc) => L(inc ? "Increasing" : "Decreasing");
            //string ValInc2() => L(Filters?.Increasing2 ?? true ? "Increasing" : "Decreasing");
            // Sort:
            //var incLabel = MakeTextBlock("inc1label", "T_Increasing1", L("Increasing")) ?? throw new("Failed FilterListPanel.incLabel");
            void setInc(object? s, EventArgs e) {
                Filters?.Increasing = ((IoToggleButton?)s)?.IsChecked ?? false;
                if (dButtons.TryGetValue("inc1check", out var b) && b != null)
                    b.Value.Text.Text = Inc(Filters?.Increasing ?? true);
                //incLabel.Text.Text = (Filters.Increasing = ((IoToggleButton?)s)?.IsChecked ?? false) ? L("Increasing") : L("Decreasing");
                OnUpdate?.Invoke(this, new EventArgs());
            }
            void setSort1(object? s, EventArgs e) {
                var i = ((ComboBox?)s)?.SelectedIndex ?? 0;
                if (i >= 0) {
                    Filters?.SortIndex = SortIndices[i];
                    OnUpdate?.Invoke(this, new EventArgs());
                }
            }
            var inc1 = MakeButton("inc1check", "T_Increasing1", Inc(Filters.Increasing), setInc, Filters.Increasing, FontWeight.Normal, "") ?? throw new(F + "inc");
            var sort1 = MakeComboBox("sort1", "T_Sort1", SortIndices, Filters.SortIndex,
                new FuncDataTemplate<ushort>((mode, _) => new TextBlock { Text = Filters.Filters[mode].Name }, false), setSort1)
                ?? throw new(F + "sort1");
            MakeRow("_Sort", "FirstSort", [(inc1.Button, 1), (sort1.View, 3)]);

            // Sort2:
            //var incLabel2 = MakeTextBlock("inc2label", "T_Increasing1", L("Increasing")) ?? throw new("Failed FilterListPanel.incLabel2");
            void setInc2(object? s, EventArgs e) {
                Filters?.Increasing2 = ((IoToggleButton?)s)?.IsChecked ?? false;
                if (dButtons.TryGetValue("inc2check", out var b) && b != null)
                    b.Value.Text.Text = Inc(Filters?.Increasing ?? true);
                //incLabel2.Text.Text = (Filters.Increasing2 = ((IoToggleButton?)s)?.IsChecked ?? false) ? L("Increasing") : L("Decreasing");
                OnUpdate?.Invoke(this, new EventArgs());
            }
            void setSort2(object? s, EventArgs e) {
                var i = ((ComboBox?)s)?.SelectedIndex ?? 0;
                if (i >= 0) {
                    Filters?.SortIndex2 = SortIndices[i];
                    OnUpdate?.Invoke(this, new EventArgs());
                }
            }
            var inc2 = MakeButton("inc2check", "T_Increasing2", Inc(Filters.Increasing2), setInc2, Filters.Increasing2, FontWeight.Normal, "") ?? throw new(F + "inc");
            var sort2 = MakeComboBox("sort2", "T_Sort2", SortIndices, Filters.SortIndex2,
                new FuncDataTemplate<ushort>((mode, _) => new TextBlock { Text = Filters.Filters[mode].Name }, false), setSort2)
                ?? throw new(F + "sort2");
            MakeRow("_Sort2", "SecondSort", [(inc2.Button, 1), (sort2.View, 3)]);
        }
        Results = MakeTextBlock("results." + PageS, "T_Results", ResultsText) ?? throw new(F + "Results");
        MakeRow("_Results", "Results", [(Results.Value.View, 1)]);
    }

    internal void Restore(IoFilter? newFilters) {
        if (newFilters == null)
            return;
        Filters = newFilters;
        var c = Filters.Size;
        var more = c < (GetProfile?.MaxSearchTabs ?? DPro.MaxSearchTabs);
        PageNames = new string[more ? c + 2 : c + 1];
        PageNames[0] = "Sort";
        for (int i = PageNames.Length - 1; 0 < --i; PageNames[i] = "Search") ;
        if (more) 
            PageNames[PageNames.Length-1] = "AddSearch";
    }

    protected virtual void BeforeTabRefresh(object? sender, EventArgs e/*IoToggleButton? button*/) { 
        //base.BeforeTabRefresh(sender, e);
        Search = Swipe.Page.X - 1;
        if (Swipe.Page.X == PageNames.Length - 1) {
            var L = PageNames.Length;
            Filters?.SetSize((byte)(L - 1));
            if (PageNames.Length <= (GetProfile?.MaxSearchTabs ?? DPro.MaxSearchTabs)) {
                PageNames = new string[PageNames.Length + 1];
                PageNames[0] = "Sort";
                for (int i = L; 0 < --i; PageNames[i] = "Search") ;
                PageNames[L] = "AddSearch";
            } else PageNames[L - 1] = "Search";
        }
        //Book.PageNames = Book.PageNames;
        Swipe.TransformBounds.ExcMaxX = PageNames.Length;
    }
  
    private void DeleteSearch(object? s, EventArgs e) {
        var L = PageNames.Length - 2;
        if (L <= 0 || Filters == null)
            return;
        if (Filters.Size == (GetProfile?.MaxSearchTabs ?? DPro.MaxSearchTabs))
            ++L; // make sure to remove correctly when you are at capacity of Searches
        Filters.RemoveSearch((byte)Search); // shake the following searched down by 1 index:
        Filters.SetSize((byte)(L - 1));
        PageNames = new string[L + 1];
        PageNames[0] = "Sort";
        for (int i = L; 0 < --i; PageNames[i] = "Search") ;
        PageNames[L] = "AddSearch";
        // click on previous tab so that we are somewhere legal
        //OnTabClick(/*Book.dSButtons.TryGetValue("pageButton." + (Page - 1).ToString(), out var b) && b != null ? b.Value.Button : null*/ this, new ElementControls.IoBook.TabClickEventArgs() { Tab = });
        Swipe.TransformBounds.ExcMaxX = PageNames.Length;
        Swipe.StartSwipeForced((Swipe.Page.X - 1, Swipe.Page.Y), true, false);
    }
}