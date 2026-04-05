using System;
using IoListWallet.ElementControls;

namespace IoListWallet.MainControls.Panels;
// Parent for any settings-type panel - the format is is a list of Label+[Controls] where you set various properties.
// It can have a top toolbar acting as "tabs"/"groups"

internal class IoBookPanel : IoPanel {

    internal IoBook Book;

    protected virtual string[] GetPageNames() => ["Main"];

    internal IoBookPanel()  : base(){
        Book = new IoBook {
            PageNames = GetPageNames()
        };
        Book.FillPage += FillPage;
        Book.OnBeforeTabRefresh += BeforeTabRefresh;
        Book.OnTabClick += OnTabClick;
        Book.OnClose += OnBookClose;
        Content ??= Book;
    }

    protected override void PrepareLayout(IoWindow w) {
        Book.PrepareLayout();
        base.PrepareLayout(w);
    }
    protected override void AfterLayout(IoWindow w) { 
        Book.AfterLayout();
        base.AfterLayout(w);
    }

    internal override void Refresh() {
        base.Refresh();
        Book.Refresh();
    }
    protected virtual void FillPage(object? sender, EventArgs e) { /*UpdateWindow();*/ }
    protected virtual void OnBookClose(object? sender, EventArgs e) {
        Book.Swipe.Page = Book.Swipe.PrevPage = (0, 0);
        Close();
    }
    protected override IoPanel OnOpen() {
        Book.Swipe.Page = Book.Swipe.PrevPage = (0, 0);
        return base.OnOpen();
    }
    protected virtual void BeforeTabRefresh(object? sender, EventArgs e) { }
    protected virtual void OnTabClick(object? sender, EventArgs args) { }
}