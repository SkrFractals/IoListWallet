using IoListWallet.ElementControls;
using IoListWallet.MainControls.Panels;
using IoListWallet.Structs;
using System.Threading;
using static IoListWallet.AppSave;

namespace IoListWallet.IoPanels.Profile;
internal class ListLanguage : IoListPanel<string> {
    public ListLanguage() : base() {
        MyList.MyFilter.Filters = new IoFilter() { 
            Filters = [ // Search Formats:
                new FilterText() { Name = "Code" }, // Language Code 
                new FilterText() { Name = "Name" }, // Language Name 
            ], SortIndex = 1, SortIndex2 = 0, Increasing = true, Increasing2 = true
        };
    }
    internal override bool IsModal() => true;
    internal override string TrueParentType => MakeK(typeof(BookProfile), "");
    internal override void RefreshData(CancellationToken? token) {
        MyList.ItemSource = Languages.Keys;
        MyList.ItemTemplate = (object? code) 
            => { var s = (string?)code; return [
                "T_SettingsApp.SelectLanguageEntry", s == null ? "ERROR" : Languages[s]
                ]; };
        MyList.FilterCommandTemplate = (code) => code is string c ? [ // Search Data:
            c, // Language Code
            Languages.TryGetValue(c, out var n) ? n : "" // Language Name
            ] : [];
        base.RefreshData(token);
    }
    protected override IoList<string> MakeList() 
        => new IoListButtons<string>(this, []);
    protected override string? Selected() => GetProfile?.Language;
    protected override void PerformSelect(object? tag) 
        => ((BookProfile?)TrueParent)?.SetLanguage((string?)tag);
}