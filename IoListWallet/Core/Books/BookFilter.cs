using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Lists;
using IoListWallet.Core.Panels;
using IoListWallet.Core.Static;
using System;
using System.Collections.Generic;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Core.Books;
// This is the panel with filter settings that opens when you click "Filter" on any SearchList containing Panel
internal class BookFilter : IoBook {

    // VAR
    //internal IoPopup MyPopup;

    internal IoListBase MyList;
    internal byte MaxSearch = 64;
    internal BookFilter(IoListBase myList) {
        MyList = myList;
        PageNames = GetMaxSearch() > 1 ? ["Sort", "Search", "AddSearch"] : ["Sort"];
        FillPage += OnFillPage;
        OnBeforeTabRefresh += BeforeTabRefresh;
    }

    //[Range(1,64)]
    //static byte MaxSize = 4; // How many Search tabs allowed?
   
    internal IoFilter? Filters;
    internal string ResultsText = "";
    internal (IoTextBlock Text, Viewbox View)? Results;
    internal event EventHandler<EventArgs>? OnUpdate;

    //private static = new((index, _) => new IoToggleButton(new IoTextBlock { Text = ETypes.TryGetValue(fe.EnumType, out var type) ? GetEnumText((Enum)Enum.ToObject(type, index)) : "" }), false);
    private const string F = "Failed BookFilter.";

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
    protected void OnFillPage(object? sender, FillPageEventArgs e) {
        if (Filters == null)
            return;
        var s = e.Simulated ? "s." : "";
        var onlyone = GetMaxSearch() <= 1;
        Search = onlyone ? 0 : e.Page.X - 1;
        SortIndices.Clear();
        ushort Index = 0;
        var S = Search;
        ulong TakeBit(ulong mask) => GetBit(mask, S);// ulong TakeBit(ulong mask) => mask & ((ulong)1 << S);
        ulong SetBit(ref ulong mask, ulong bit, int I) => mask = (mask & ~((ulong)1 << I)) + (bit << I);

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
        /*void UpdateCombo(IoComboBox? e, FilterType filter, Action<ushort> lambda) {
            if (e != null && e.SelectedItem != null) {
                lambda((ushort)e.SelectedIndex);
                if (TakeBit(filter.Disabled) == 0)
                    OnUpdate?.Invoke(this, new EventArgs());
            }
        }*/
        void UpdateDate(IoDatePicker? e, FilterType filter, Action<DateTime> lambda) {
            if (e != null && e.SelectedDate != null) {
                lambda(e.SelectedDate == null ? DateTime.Now : e.SelectedDate.Value.DateTime);
                if (TakeBit(filter.Disabled) == 0)
                    OnUpdate?.Invoke(this, new EventArgs());
            }
        }
        var Page = e.Page.X;
        string PageS = Page.ToString();

        if ((Page > 0 && Page <= Filters.Size) || onlyone) {

            // Remove Search Button:
            var removeSearch = MakeButton(s + "removeSearch." + PageS, "T_RemoveSearch", L("Remove"), DeleteSearch, false, FontWeight.Bold, L("AreYouSure"))
                ?? throw new(F + "RemoveSearch");
            //removeSearch.Button.IsEnabled = Filters.Size > 1;
            if (Filters.Size > 1 || (e.Simulated && MaxSearch > 1))
                MakeRow(e, "_RemoveSearch", "RemoveSearch", [(removeSearch.Button, 1)]);

            foreach (var filter in Filters.Filters) {
                if (filter == null)
                    throw new(F + "filter");
                var name = filter.Name;

                // Enable Field Button:
                void enablePressed(object? s, EventArgs e) {
                    var d = filter.Disabled;
                    SetBit(ref d, (ulong)((((IoToggleButton?)s)?.IsChecked ?? false) ? 0 : 1), S);
                    filter.Disabled = d;
                    if (dButtons.TryGetValue("enable." + PageS + "." + name, out var b) && b != null)
                        b.Value.Text.Text = Dis(TakeBit(filter.Disabled) > 0);
                    OnUpdate?.Invoke(this, new EventArgs());
                }
                var disabledF = TakeBit(filter.Disabled) > 0;
                var EnableF = MakeButton(s + "enable." + PageS + "." + name, "T_Enable", Dis(disabledF), enablePressed, !disabledF, FontWeight.Normal, "")
                    ?? throw new(F + "Disabled " + name);

                if (filter is FilterText ft) {

                    // Regexp
                    void set(TextBox s) { 
                        ft.Regexp[S] = ((TextBox?)s)?.Text ?? "";
                        IoPanel.Dirty();
                    }
                    void update(object? s, EventArgs _)
                        => UpdateText((IoTextBox?)s, filter, set);
                    var regexp = MakeTextBox(s + "regexp." + PageS + "." + name, "T_Regexp", ft.Regexp[S], "Regexp", update, update)
                        ?? throw new(F + "Regexp " + name);
                    MakeRow(e, name, name, [(EnableF.Button, 1), (regexp.Border, 2)]);

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
                    var Min = MakeNumeric(s + "min." + PageS + "." + name, "T_FilterMinValue", fn.Range[S].Min, fn.MinMin, fn.MaxMin, 1, 10, "Min", "", "", updateMin)
                        ?? throw new(F + "Number.Min " + name);
                    var Max = MakeNumeric(s + "max." + PageS + "." + name, "T_FilterMaxValue", fn.Range[S].Max, fn.MinMax, fn.MaxMax, 1, 10, "Max", "", "", updateMax)
                        ?? throw new(F + "Number.Max " + name);
                    var (_, DashV) = MakeTextBlock(s + "dash." + PageS + "." + name, "", " - ")
                        ?? throw new(F + "Number.Dash " + name);
                    MakeRow(e, name, name, [(EnableF.Button, 4), (Min, 4), (DashV, 1), (Max, 4)]);

                } else if (filter is FilterEnum fe && ETypes.TryGetValue(fe.EnumType, out var isEnumType) && isEnumType is Type enumType) {

                    // Enum Match
                    byte i = 0; // Prepare the enum values we want to toggle (fe.Mask can disable the presence of some entries)
                    List<byte?> ToMatch = [];
                    for (var m = fe.Mask & (((ulong)1 << Enum.GetValues(enumType).Length) - 1); m > 0; m >>= 1, ++i) 
                        if ((m & 1) == 1) 
                            ToMatch.Add(i);
                    var key = s + "toggleEnum." + PageS + "." + name + "."; // key of the toggle combobox for the dictionary
                    void onToggleEnum(object? s, EventArgs e) {
                        if (s is not IoToggleButton b)
                            return;
                        if (b.Tag is byte bit) {
                            var m = fe.Match[S];
                            SetBit(ref m, (ulong)((b.IsChecked ?? true) ? 1 : 0), bit);
                            fe.Match[S] = m;
                            if (TakeBit(filter.Disabled) == 0)
                                OnUpdate?.Invoke(this, new EventArgs());
                            return;
                        }
                        if (dCombos.TryGetValue(key, out var d) && d?.Combo is ComboBox c) //if(b.FindAncestorOfType<IoComboBox>() is IoComboBox c)
                            c.IsDropDownOpen ^= true; // clicked the null button: toggle the dropdown
                    }
                    var toggleEnum = MakeBitmaskBox(key, "T_" + key, ToMatch, 
                        new FuncDataTemplate<byte?>((bit, _) => GetBitmaskButtons(key, bit, fe.Match[S], enumType, onToggleEnum)))
                        ?? throw new(F + key);
                    MakeRow(e, name, name, [(EnableF.Button, 1), (toggleEnum.View, 2)]);

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
                    var (_, MinV) = MakeDatePick(s + "min." + PageS + "." + name, "T_FilterMinDate", fd.Range[S].Min, updateMin)
                        ?? throw new(F + "Date.Min " + name);
                    var (_, MaxV) = MakeDatePick(s + "max." + PageS + "." + name, "T_FilterMaxDate", fd.Range[S].Max, updateMax)
                        ?? throw new(F + "Date.Max " + name);
                    var (_, dashV) = MakeTextBlock(s + "dash." + PageS + "." + name, "", " - ")
                        ?? throw new(F + "Date.Dash " + name);
                    MakeRow(e, name, name, [(EnableF.Button, 3), (MinV, 2), (dashV, 1), (MaxV, 2)]);

                } else if (filter is FilterToken fa) {
                   
                    // Token Min-Max
                    void set(IoTextBox s) { 
                        fa.Cond[S] = new(s?.Text ?? "", fa.Cond[S].Min, fa.Cond[S].Max);
                        IoPanel.Dirty();
                    }
                    void update(object? s, EventArgs _)
                        => UpdateText((IoTextBox?)s, filter, set);
                    void setd(object? s, EventArgs _) {
                        // Toggle disabled bit (each OR seach has one bit)
                        var d = fa.DisabledValues;
                        SetBit(ref d, (ulong)((((IoCheckBox?)s)?.IsChecked ?? false) ? 0 : 1), S);
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
                    var Regexp = MakeTextBox(s + "regexpt." + PageS + "." + name, "T_FilterTicker", fa.Cond[S].Ticker, "Regexp", update)
                        ?? throw new(F + "Token.Regexp " + name);
                    var disabledT = TakeBit(fa.DisabledValues) > 0;
                    var EnableT = MakeButton(s + "d." + PageS + "." + name, "T_FilterTokenValue", Dis(disabledT), setd, !disabledT, FontWeight.Normal, "")
                        ?? throw new(F + "Token.EnableValue" + name);
                    var Min = MakeNumeric(s + "mint." + PageS + "." + name, "T_FilterMinTokens", fa.Cond[S].Min, fa.MinMin, fa.MaxMin, 1, 10, "Min", "", "", updateMin)
                       ?? throw new(F + "Number.Mint " + name);
                    var Max = MakeNumeric(s + "maxt." + PageS + "." + name, "T_FilterMaxTokens", fa.Cond[S].Max, fa.MinMax, fa.MaxMax, 1, 10, "Max", "", "", updateMax)
                        ?? throw new(F + "Number.Maxt " + name);
                    var Dash = MakeTextBlock(s + "dash." + PageS + "." + name, "", " - ") ?? throw new(F + "Token.Dash" + name);
                    MakeRow(e, name, name, [(EnableF.Button, 4), (Regexp.Border, 4), (EnableT.Button, 1), (Min, 4), (Dash.View, 1), (Max, 4)]);
                }
                SortIndices.Add(Index++);
            }
        } else foreach (var filter in Filters.Filters) SortIndices.Add(Index++);
        if (Page <= 0) {

            string Inc(bool inc) => L(inc ? "Increasing" : "Decreasing");
            // Sort:
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
            var inc1 = MakeButton(s + "inc1check", "T_Increasing1", Inc(Filters.Increasing), setInc, Filters.Increasing, FontWeight.Normal, "") ?? throw new(F + "inc");
            var sort1 = MakeComboBox(s + "sort1", "T_Sort1", SortIndices, Filters.SortIndex,
                new FuncDataTemplate<ushort>((mode, _) => new IoTextBlock { Text = Filters.Filters[mode].Name }, false), setSort1)
                ?? throw new(F + "sort1");
            MakeRow(e, "_Sort", SortIndices.Count > 1 ? "FirstSort" : "Sort", [(inc1.Button, 1), (sort1.View, 3)]);

            // Sort2: Only allow secdondary sort, if there are actaully more than one ways to sort, otherwise it's meaningless
            if (SortIndices.Count > 1) {
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
                var inc2 = MakeButton(s + "inc2check", "T_Increasing2", Inc(Filters.Increasing2), setInc2, Filters.Increasing2, FontWeight.Normal, "") ?? throw new(F + "inc");
                var sort2 = MakeComboBox(s + "sort2", "T_Sort2", SortIndices, Filters.SortIndex2,
                    new FuncDataTemplate<ushort>((mode, _) => new IoTextBlock { Text = Filters.Filters[mode].Name }, false), setSort2)
                    ?? throw new(F + "sort2");
                MakeRow(e, "_Sort2", "SecondSort", [(inc2.Button, 1), (sort2.View, 3)]);
            }


            void changePageSize(object? s, EventArgs e) {
                Filters?.PageSize = (uint?)((IoNumericLeftRight?)s)?.Value ?? 10;
                OnUpdate?.Invoke(this, new EventArgs());
            }
            var pageSize = MakeNumeric(s + "pageSize", "T_PageLength", Filters?.PageSize ?? 10, 1, 100, 1, 1, "PageSize", "", "", changePageSize)
                ?? throw new(F + "pageSize");
            MakeRow(e, "_PageSize", "PageSize", [(pageSize, 1)]);

        }
        Results = MakeTextBlock(s + "results." + PageS, "T_Results", ResultsText) ?? throw new(F + "Results");
        MakeRow(e, "_Results", "Results", [(Results.Value.View, 1)]);
    }
    private ushort GetMaxSearch() => Math.Min(MaxSearch, (GetProfile?.MaxSearchTabs ?? DPro.MaxSearchTabs));
    internal void Restore(IoFilter? newFilters) {
        if (newFilters == null)
            return;
        //Filters = newFilters;
        Filters ??= new();
        Filters.Filters = newFilters.Filters;
        Filters.Size = newFilters.Size;
        Filters.SortIndex = newFilters.SortIndex;
        Filters.SortIndex2 = newFilters.SortIndex2;
        Filters.Increasing = newFilters.Increasing;
        Filters.Increasing2 = newFilters.Increasing2;

        var c = Filters.Size;
        var m = GetMaxSearch();
        var more = c < m;
        PageNames = new string[m <= 1 ? 1 : more ? c + 2 : c + 1];
        PageNames[0] = "Sort";
        for (int i = PageNames.Length - 1; 0 < --i; PageNames[i] = "Search") ;
        if (more) 
            PageNames[PageNames.Length-1] = "AddSearch";
    }
    protected virtual void BeforeTabRefresh(object? sender, EventArgs e) {
        //base.BeforeTabRefresh(sender, e);
        if (Swipe.Page.X <= 0 || Swipe.Page.Y != 0)
            return;
        Search = Swipe.Page.X - 1;
        if (Swipe.Page.X == PageNames.Length - 1) {
            var L = PageNames.Length;
            Filters?.SetSize((byte)(L - 1));
            if (PageNames.Length <= GetMaxSearch()) {
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
        if (Filters.Size == GetMaxSearch())
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