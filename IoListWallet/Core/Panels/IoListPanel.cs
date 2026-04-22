using IoListWallet.Core.Books;
using IoListWallet.Core.Lists;
using IoListWallet.Structs.Crypto;
using System;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Core.Panels;
// A parent for IoPanels containing some SearchList.
// They have a format of a list of various elements, simple or composite.
// The have a top toolbar with page selector, and additional custom action buttons (implemented in SearchList's Children).
internal abstract class IoListPanel : IoListsPanel {

    protected override void FillPage(object? s, IoBook.FillPageEventArgs e) {
        if (GetProfile is not IoProfile profile)
            return;
        if (e.Simulated) {
            Book.MaxSize = new(Math.Max(MyList[0].MinSize.Width, Book.MaxSize.Width), Math.Max(MyList[0].MinSize.Height, Book.MaxSize.Height));
        } else {
            Book.MakeRow(e, "ListPage", "", [(MyList[0], 1)]);
            base.FillPage(s, e);
        }
    }
    protected IoListButtons<T> MakeList<T>(IoFilter f, (string, string)[]? tools = null, int columns = 1, short maxSearch = -1, int index = 0, Func<object?, bool>? disable = null, Func<object?, bool>? skip = null, Func<object?, bool>? selected = null) {
        var l = new IoListButtons<T>(this, tools == null ? [] : tools) { MyIndex = index };
        (MyList[index] = l).MyFilter.Filters = f;
        if (maxSearch >= 0 && (l.MyFilter.MaxSearch = (byte)maxSearch) == 1)
            l.MyFilter.PageNames = ["Sort"];
        if (disable != null)
            l.Disable += disable;
        if (skip != null)
            l.Skip += skip;
        if (selected != null)
            l.Selected += selected;
        l.Columns = columns; // 0 = Select Network, 1 = Edit Network
        return l;
    }
}