using Avalonia.Controls;
using Avalonia.Media;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Panels;
using System;
using System.Collections.Generic;
using static IoListWallet.Core.Static.ConstructionHelpers;

namespace IoListWallet.Core.Lists;
internal class IoListButtons<T> : IoList<T> {
    internal int Columns = 1;

    private List<(IoTextBlock Text, Viewbox View, IoToggleButton Button)?[]> Elements = [];
    internal IoListButtons(IoListsPanel Owner, (string, string)[] Tools) : base(Owner, Tools) { }
    private const string F = "Failed IoListButtons.";
    protected override void MakeElements() {

        uint iElement = 0;
        (string From, string To)? g = null;

        void MakeGrouped(Grid grid) {
            // element with a group boundary label above it:
            var gt = new IoInfoTextBlock { Text = (g.Value.From == "" || g.Value.To == "" ? g.Value.From : g.Value.From + " - ") + g.Value.To, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
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

        foreach (var entry in SourceList) {
        
            if (entry.Key is (string From, string To)) {
                // if we got a group label, remember to put it at top of the next row
                g = (From, To);
                continue;
            }
            // each '-' at the start of the first text in Texts[], we will remove one extra button from this row:
            int iColumn = 0;
            while (entry.Texts[0][iColumn] == '-')
                ++iColumn;
            entry.Texts[0] = entry.Texts[0][iColumn..];
            iColumn = Columns - iColumn;
            // Prepare button row pointers:
            if (Swipe.Page.Y < 0) Swipe.Page.Y = 0;
            int Id = (int)(iElement + Swipe.Page.Y * pageSize);
            while (Elements.Count <= Id || iColumn > Elements[Id].Length) {
                var ea = new (IoTextBlock, Viewbox, IoToggleButton)?[iColumn];
                for (int i = 0; i < iColumn; ++i) ea[i] = null;
                if (Elements.Count <= Id)
                    Elements.Add(ea);
                else Elements[Id] = ea;
            }
            // Makes all the buttons in this row:
            for (int i = 0; i < iColumn; ++i) {
                (IoTextBlock Text, Viewbox View, IoToggleButton Button)? element = Elements[Id][i];
                // spawn buttons + selected one if present
                var s = Selected;
                int index = i;
                string text = entry.Texts[i * 2 + 1], tooltip = entry.Texts[i * 2];
                void Click(object? s, EventArgs e) 
                    => ClickButton(Id, index);
                //Button.Tag will carry the id
                if ((entry.Selected == 1) && i == 0) {
                    // selected button
                    BigButton(ref element, tooltip, text, Click, true, FontWeight.Bold, null, entry.Key);
                    //Selecteds.Add((element, Id));
                } else // regular button
                    BigButton(ref element, tooltip, text, Click, false, FontWeight.Normal, null, entry.Key);
                if (element == null)
                    break;
                Elements[Id][i] = element;
                // Selected==2 will disable the button
                element.Value.Button?.IsEnabled = entry.Selected < 2;
            }
            // Make Horizontal Button grid
            Grid? _grid = null;
            JustGrid(ref _grid);
            Grid grid = _grid ?? throw new("Failed SearchListButtonRows.Grid");
            // Link buttons:
            for (int i = 0; i < iColumn; ++i) {
                (IoTextBlock Text, Viewbox View, IoToggleButton Button)? element = Elements[Id][i];
                if (element == null)
                    break;
                grid.ColumnDefinitions.Add(new ColumnDefinition(i == 0 ? GridLength.Star : GridLength.Auto));
                grid.Children.Add(element.Value.Button);
                Grid.SetColumn(element.Value.Button, i);
            }
            // Wrap with a group boundary label and add to pageEntries, or just add to pageEntries.
            if (g != null) 
                MakeGrouped(grid);  // Make group label
            else 
                PageEntries.Add((new UserControl { Content = grid }, null)); // Just a row without a group label
            ++iElement;
        }
        // if we are left with one last boundary, make one last empty element, and attach the label to that
        if (g != null)
            MakeGrouped(new Grid());
    }
    protected virtual void ClickButton(int iElement, int index) { // iElement = row, index = Column
        ClickButton(Elements[iElement][index], index);
    }
}