/*using Avalonia.Controls;
using Avalonia.Media;
using IoListWallet.Content.Abstract;
using IoListWallet.Core.Basic;
using System;
using System.Collections.Generic;
using static IoListWallet.Core.Static.ConstructionHelpers;

namespace IoListWallet.Core.Lists;




internal class IoListButtons<T> : IoList<T> {
    internal int Columns = 1;

    internal IoListButtons(IoListsPanel Owner, (string, string)[] Tools) : base(Owner, Tools) { }
    private const string F = "Failed IoListButtons.";
    protected override void MakeElements() {
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
                MakeGrouped(grid);  // Make group label
            else
                PageEntries.Add((new UserControl { Content = grid }, null)); // Just a row without a group label
            ++iElement;
        }
        // if we are left with one last boundary, make one last empty element, and attach the label to that
        if (g != null)
            MakeGrouped(new Grid());
    }
}*/