using Avalonia;
using System;
using IoListWallet.Core.Books;

namespace IoListWallet.Core.Panels;
// Parent for any settings-type panel - the format is is a list of Label+[Controls] where you set various properties.
// It can have a top toolbar acting as "tabs"/"groups"

internal abstract class IoBookPanel : IoPanel {

    internal IoBook Book;

    protected virtual string[] GetPageNames() => [""];

    internal IoBookPanel() : base(){
        Book = new IoBook {
            PageNames = GetPageNames()
        };
        MyContent.Content ??= Book;
    }
    internal override void Init(Size available) {
        base.Init(available);
        Book.FillPage += FillPage;
        Book.OnBeforeTabRefresh += BeforeTabRefresh;
        Book.OnTabClick += OnTabClick;
        Book.OnClose += OnBookClose;
        MyContent.Content ??= Book;
        Book.PrepareLayout(available);
    }

    internal override Size PrepareLayout(Size available) {
        var bookSize = Book.MinSize;
        var toolSize = base.PrepareLayout(available);
        MinSize = new(Math.Max(bookSize.Width, toolSize.Width), bookSize.Height + toolSize.Height);
        //ParentDock?.RefreshMinSize();
        return MinSize;
    }
    internal override void Refresh(int job) {
        base.Refresh(job);
        Book.Refresh();
    }
    protected virtual void FillPage(object? sender, IoBook.FillPageEventArgs e) { /*UpdateWindow();*/ }
    protected virtual void OnBookClose(object? sender, EventArgs e) {
        Book.Swipe.Page = Book.Swipe.PrevPage = (0, 0);
        Close();
    }
    internal override IoPanel OnOpen(bool initial = false) {
        Book.Swipe.Page = Book.Swipe.PrevPage = (0, 0);
        return base.OnOpen(initial);
    }
    protected override string CustomTitle() {
        var e = App.L(GetPageNames()[Book.Swipe.Page.X]);
        return e == "" ? base.CustomTitle() : base.CustomTitle() + "-" + e;
    }
     
    protected virtual void BeforeTabRefresh(object? sender, EventArgs e) { }
    protected virtual void OnTabClick(object? sender, EventArgs args) { SetTitle(); }
}