using IoListWallet.Core;
using IoListWallet.Core.Books;
using IoListWallet.Core.Lists;
using System;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Content.Abstract;
// A parent for IoPanels containing some SearchList.
// They have a format of a list of various elements, simple or composite.
// The have a top toolbar with page selector, and additional custom action buttons (implemented in SearchList's Children).
internal abstract class IoListPanel : IoListsPanel {
    protected override void FillPage(object? s, IoBook.FillPageEventArgs e) {
        if (GetProfile == null)
            return;
        if (e.Simulated) {
            Book.MaxSize = new(Math.Max(MyList[0].MinSize.Width, Book.MaxSize.Width), Math.Max(MyList[0].MinSize.Height, Book.MaxSize.Height));
        } else {
            Book.MakeRow(e, "ListPage", "", [(MyList[0], 1)]);
            base.FillPage(s, e);
        }
    }
    protected IoList<T> MakeList<T>(IoFilter f, IoGen gen, (string, string)[]? tools = null, short maxSearch = -1, int index = 0, Func<object?, bool>? disable = null, Func<object?, bool>? skip = null, Func<object?, bool>? selected = null) {
        var l = new IoList<T>(this, tools ?? []) { MyIndex = index };
        (MyList[index] = l).MyFilter.Filters = f;
        if (maxSearch >= 0 && (l.MyFilter.MaxSearch = (byte)maxSearch) == 1)
            l.MyFilter.PageNames = ["Sort"];
        if (disable != null)
            l.Disable += disable;
        if (skip != null)
            l.Skip += skip;
        if (selected != null)
            l.Selected += selected;
        l.MyGen = gen;
        return l;
    }
}