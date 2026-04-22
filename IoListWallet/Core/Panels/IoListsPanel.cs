using Avalonia;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Lists;
using System;
using System.Threading;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Core.Panels;
// A parent for IoPanels containing some SearchList.
// They have a format of a list of various elements, simple or composite.
// The have a top toolbar with page selector, and additional custom action buttons (implemented in SearchList's Children).
internal abstract class IoListsPanel : IoBookPanel {
    protected virtual string[] GetListCount() => [""];
    internal enum TabListPage : byte {
        Main
    }
    protected override string[] GetPageNames()
        => [""];
    internal IoListsPanel() : base() {
        MyList = new IoListBase[GetListCount().Length];
        Book.Swipe.OnRightEvent += (_, _) => ToggleFilter();
        Book.Swipe.OnMove += (_, _) => {
            foreach (var l in MyList)
                if (l.MyFilter.Swipe.IsDragging) {
                    Book.Swipe.CancelDrag(); // do not let me drag master book when dragging filter book of this page's IoList
                    return;
                }
        };
        Book.Swipe.Vertical = false;
        //for (int i = GetListCount(); 0 <= --i; MyList[i] = MakeList(i)) ;  
    }
    protected void ToggleFilter() {
        Book.Swipe.CancelDrag();
            MyList[MyList.Length - 1].ToggleFilter(this, new EventArgs());
            foreach (var l in MyList)
                if (l.Swipe.IsDragging) {
                    l.Swipe.CancelDrag(); // opening filter by dragging left, should cancel the drag on the lists
                    return;
                }
    }
    #region Save
    [Save]
    internal uint[] PageSize {
        get; set {
            field = new uint[value.Length];
            for (int i = 0; i < value.Length; ++i)
                MyList[i].SetPageSize(field[i] = value[i]);
            Dirty();
            /*var _field = new uint[value.Length];
            bool d = false;
            int i = Math.Min(field.Length, value.Length);
            while (0 <= --i)
                if (field[i] != value[i]) {
                    MyList[i].SetPageSize(_field[i] = value[i]);
                    d = true;
                }
            for (i = field.Length; i < value.Length; ++i) {
                MyList[i].SetPageSize(_field[i] = value[i]);
                d = true;
            }
            field = _field;
            if (d) Dirty();*/
        }
    } = [];
    [Save]
    internal int[] Page {
        get; set {
            field = new int[value.Length];
            for (int i = 0; i < value.Length; ++i)
                MyList[i].SetPage((0, field[i] = value[i]));
            Dirty();
            /*var _field = new int[value.Length];
            bool d = false;
            int i = Math.Min(field.Length, value.Length);
            while (0 <= --i)
                if (field[i] != value[i]) {
                    MyList[i].SetPage((0, _field[i] = value[i]));
                    d = true;
                }
            for (i = field.Length; i < value.Length; ++i) {
                MyList[i].SetPage((0, _field[i] = value[i]));
                d = true;
            }
            field = _field;
            if (d) Dirty();*/
        }
    } = [];
    [Save]
    internal IoFilter?[] MyFilter {
        get; set {
            field = new IoFilter?[value.Length];
            for (int i = 0; i < value.Length; ++i) 
                MyList[i].MyFilter.Restore(field[i] = value[i]);
            Dirty();
            /*var _field = new IoFilter?[value.Length];
            bool d = false;
            int i = Math.Min(field.Length, value.Length);
            while (0 <= --i)
                if (field[i] != value[i]) {
                    MyList[i].MyFilter.Restore(_field[i] = value[i]);
                    d = true;
                }
            for (i = field.Length; i < value.Length; ++i) {
                MyList[i].MyFilter.Restore(_field[i] = value[i]);
                d = true;
            }
            field = _field;
            if (d) Dirty();*/
        }
    } = [];
    internal override void Snapshot() {

        var f = new IoFilter?[MyList.Length];
        var p = new int[MyList.Length];
        var ps = new uint[MyList.Length];
        for (int i = 0; i < MyList.Length; ++i) {
            f[i] = MyList[i].MyFilter.Filters;
            p[i] = MyList[i].Swipe.Page.Y;
            ps[i] = MyList[i].pageSize;
        }
        MyFilter = f;
        Page = p;
        PageSize = ps;
    }
    #endregion

    protected IoListBase[] MyList;

    internal override void Init(Size available) {
        foreach (var item in MyList) {
            item.OnClickTool += OnClickTool;
            item.OnClickButton += OnClickButton;
            item.MyFilter.PrepareLayout(available);
        }
        base.Init(available);
    }
    internal override int RefreshesWhenOpened() => 0; // Make sure to follow this thorugh and put enqueueTask, or refresh bit everywhere this gets opened from!
    // If selecting an item on that list will close this panel
    protected virtual bool CloseOnClick(int fromList) => true;
    // If clicking on already selected item will close the panel (for easy back when selecting alone doesn't close)
    protected virtual bool CloseOnDoubleClick(int fromList) => true;

    internal override void Refresh(int job) {
        base.Refresh(job);
        foreach (var item in MyList) 
            if(((1 << item.MyIndex) & job) > 0)
                item.Refresh();
        
        //MyContent.Content ??= MyList;
        //Snapshot(); // TODO only if autosaving
    }
    internal override Size PrepareLayout(Size available) {
        //Size maxSize = new(120, 120);
        foreach (var item in MyList) { 
            //var s = 
                item.PrepareLayout(available);
            //maxSize = new(Math.Max(s.Width,maxSize.Width), Math.Max(s.Height,maxSize.Height));
        }
        var bookSize = Book.PrepareLayout(available);
        //maxSize = new(Math.Max(bookSize.Width, maxSize.Width), Math.Max(bookSize.Width, maxSize.Width));
        //var toolSize = base.PrepareLayout(available);
        return MinSize = base.PrepareLayout(available);//new(Math.Max(maxSize.Width, toolSize.Width), maxSize.Height + toolSize.Height);
    }
    internal override void Delete(bool refresh = true) {
        base.Delete(refresh);
        foreach (var l in MyList) 
            l.Delete();
    }
    internal override void RefreshData(int job, CancellationToken? cancel) {
        foreach (var item in MyList)
            if (((1 << item.MyIndex) & job) > 0)
                item.RefreshData(cancel); // Perform Search
    }
    internal virtual void Select(int fromList, IoToggleButton? T, object? o) {
        if (o == null)
            return;
        if (CloseOnDoubleClick(fromList) && o == Selected(fromList)) {
            Close();
            return;
        }
        PerformSelect(fromList, o);
        //AppMainMenu?.OnResponse(this, 0); // Let the main menu know my selection has changed, to let it update
    }
    protected virtual void PerformSelect(int fromList, object? o) { }
    protected virtual object? Selected(int fromList) => null;
    //protected virtual object MakeList<T>(int listIndex) => new IoList<T>(this, []);
    protected virtual void OnClickTool(object? s, ClickToolEventArgs e) { }
    protected virtual void OnClickButton(object? s, ClickButtonEventArgs e) {
        var b = e.ClickedButton ?? throw new("Failed IoListPanel.OnClickButton");
        Select(e.FromList, b.Button, b.Button.Tag);
        if (CloseOnClick(e.FromList))
            Close();
        //TrueParent?.OnResponse(this, 0);
    }
}