using IoListWallet.ElementControls;
using IoListWallet.ElementControls.Basic;
using IoListWallet.Structs;
using System.Collections.Generic;
using System.Threading;
using static IoListWallet.AppSave;

namespace IoListWallet.MainControls.Panels;
// A parent for IoPanels containing some SearchList.
// They have a format of a list of various elements, simple or composite.
// The have a top toolbar with page selector, and additional custom action buttons (implemented in SearchList's Children).
internal class IoListPanel<T> : IoPanel {

    internal IoListPanel() : base() { 
        MyList = MakeList();
        MyList.OnClickTool += OnClickTool;
        MyList.OnClickButton += OnClickButton;
        MyList.Skip = SkipEntries;
        MyList.Disable = DisableEntries;
    }

    #region Save
    [Save] internal uint PageSize { get; set { 
            if (field == value) 
                return; 
            MyList.SetPageSize(field = value);
            Dirty(); 
        } }
    [Save] internal int Page { get; set { 
            if (field == value) 
                return;
            MyList.SetPage((0, field = value));
            Dirty();
        } }
    [Save] internal IoFilter? MyFilter { get; set {
            if (field == value || value == null) 
                return; 
            MyList.MyFilter.Restore(field = value);
            Dirty(); 
        } }
    internal override void Snapshot() {
        MyFilter = MyList.MyFilter.Filters;
        Page = MyList.Swipe.Page.Y;
        PageSize = MyList.pageSize;
    }
    /*internal override void Restore() {
        MyList.MyFilter.Filters = MyFilter;
        MyList.Swipe.Page.Y = Page;
        MyList.pageSize = PageSize;
    }*/
    #endregion

    protected IoList<T> MyList;

    internal override bool RefreshesWhenOpened() => true; // Make sure to follow this thorugh and put enqueueTask, or refresh bit everywhere this gets opened from!

    protected virtual bool CloseOnClick() => true;

    protected virtual bool CloseOnDoubleClick() => true;

    internal override void Refresh() {
        base.Refresh();
        MyList.Refresh();
        Content ??= MyList;
        //Snapshot(); // TODO only if autosaving
    }

    protected override void PrepareLayout(IoWindow w) {
        MyList.MyPopup.PrepareLayout();
        base.PrepareLayout(w);
    }
    protected override void AfterLayout(IoWindow w) {
        base.AfterLayout(w);
        double X = w.Width, Y = w.Height;
        if (X < Y) {
            w.Width = w.MinWidth = 2 * X;
        } else {
            w.Height = w.MinHeight = 2 * Y;
        }
        MyList.MyPopup.AfterLayout();
    }

    internal override void RefreshData(CancellationToken? cancel) {
        MyList.RefreshData(cancel); // Perform Search
    }
    internal virtual bool SkipEntries(object? o) => false;
    internal virtual bool DisableEntries(object? o) => false;
    internal virtual void Select(IoToggleButton? T, object? o) {
        if (o == null)
            return;
        if (CloseOnDoubleClick() && o == Selected()) {
            Close();
            return;
        }
        PerformSelect(o);
        //AppMainMenu?.OnResponse(this, 0); // Let the main menu know my selection has changed, to let it update
    }
    protected virtual void PerformSelect(object? o) { }
    protected virtual object? Selected() => null;
    protected virtual IoList<T> MakeList() 
        => new(this, []);
    protected virtual void OnClickTool(object? s, IoList<T>.ClickToolEventArgs e) { }
    protected virtual void OnClickButton(object? s, IoList<T>.ClickButtonEventArgs e) {
        var b = e.ClickedButton ?? throw new("Failed IoListPanel.OnClickButton");
        Select(b.Button, b.Button.Tag);
        if (CloseOnClick())
            Close();
        TrueParent?.OnResponse(this, 0);
    }
}