using Avalonia.Controls;
using Avalonia.Media;
using IoListWallet.Core.Basic;
using System;
using static IoListWallet.Core.Lists.IoListBase;
using static IoListWallet.Core.Static.ConstructionHelpers;

namespace IoListWallet.Core.Lists;

internal abstract class IoGen {
    internal abstract class Data{
    }
    internal abstract Grid? MakeElement(IoListBase owner, uint iElement, (object Key, byte State, Data? ElementData) entry);
}

internal sealed class Gen_Buttons(int columns) : IoGen {
    internal class GenData(string[] texts) : Data {
        internal string[] Texts = texts;
    }

    internal int Columns = columns;
    internal override Grid? MakeElement(IoListBase owner, uint iElement, (object Key, byte State, Data? ElementData) entry) {
        // each '-' at the start of the first text in Texts[], we will remove one extra button from this row:
        int iColumn = 0;
        if (entry.ElementData is not GenData data || data.Texts.Length <= 0)
            return null;
        while (data.Texts[0].Length > iColumn && data.Texts[0][iColumn] == '-')
            ++iColumn;
        data.Texts[0] = data.Texts[0][iColumn..];
        iColumn = Columns - iColumn;
        // Prepare button row pointers:
        if (owner.Swipe.Page.Y < 0) owner.Swipe.Page.Y = 0;
        int Id = (int)(iElement + owner.Swipe.Page.Y * owner.pageSize);
       
        WToggleButton?[] c;
        while (owner.Elements.Count <= Id || iColumn > ((IoEntry_Buttons)owner.Elements[Id]).Content.Length) {
            IoEntry_Buttons ea = new(new WToggleButton?[iColumn]);
            for (int i = 0; i < iColumn; ++i) ea.Content[i] = null;
            if (owner.Elements.Count <= Id)
                owner.Elements.Add(ea);
            else owner.Elements[Id] = ea;
        }
        if (owner.Elements.Count <= Id)
            return null;
        c = ((IoEntry_Buttons)owner.Elements[Id]).Content;
        // Makes all the buttons in this row:
        for (int i = 0; i < iColumn; ++i) {
            WToggleButton? element = ((IoEntry_Buttons)owner.Elements[Id]).Content[i];
            // spawn buttons + selected one if present
            var s = owner.Selected;
            int index = i;
            string text = data.Texts[i * 2 + 1], tooltip = data.Texts[i * 2];
            void Click(object? s, EventArgs e) => owner.ClickRow(c[index], index);
            //Button.Tag will carry the id
            if ((entry.State == 1) && i == 0) {
                // selected button
                BigButton(ref element, owner, tooltip, text, Click, true, FontWeight.Bold, null, entry.Key);
                //Selecteds.Add((element, Id));
            } else // regular button
                BigButton(ref element, owner, tooltip, text, Click, false, FontWeight.Normal, null, entry.Key);
            if (element == null)
                break;
            c[i] = element;
            // Selected==2 will disable the button
            element.Button?.IsEnabled = entry.State < 2;
        }
        // Make Horizontal Button grid
        Grid? _grid = null;
        JustGrid(ref _grid);
        Grid grid = _grid ?? throw new("Failed SearchListButtonRows.Grid");
        // Link buttons:
        for (int i = 0; i < iColumn; ++i) {
            WToggleButton? element = c[i];
            if (element == null)
                break;
            grid.ColumnDefinitions.Add(new ColumnDefinition(i == 0 ? GridLength.Star : GridLength.Auto));
            grid.Children.Add(element.Button);
            Grid.SetColumn(element.Button, i);
        }
        return grid;
    }
}
