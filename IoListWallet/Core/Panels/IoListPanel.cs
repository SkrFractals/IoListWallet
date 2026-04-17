using IoListWallet.Core.Books;
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
}