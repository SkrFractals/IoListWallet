using IoListWallet.Core;
using IoListWallet.Core.Lists;
using IoListWallet.Core.Panels;
using System.Threading;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Profile;
internal class LocaleSelect : IoListPanel {
    public LocaleSelect() : base() {
        (MyList[0] = new IoListButtons<string>(this, []) { MyIndex = 0 })
            .MyFilter.Filters = new IoFilter() { 
            Filters = [ // Search Formats:
                new FilterText() { Name = "Code" }, // Language Code 
                new FilterText() { Name = "Name" }, // Language Name 
            ], SortIndex = 1, SortIndex2 = 0, Increasing = true, Increasing2 = true
        };
        //MyContent.Content = MyList[0].Content;
    }
    internal override bool IsModal() => true;
    internal override string TrueParentType => MakeK(typeof(ProfileContent), "");
    internal override void RefreshData(int job, CancellationToken? token) {
        if (MyList[0] is not IoListButtons<string> list)
            return;
        list.ItemSource = LocaleNames.Keys;
        list.ItemTemplate = (object? code) 
            => { var s = (string?)code; return [
                "T_ProfileContent.SelectLanguageEntry", s == null ? "ERROR" : LocaleNames[s]
                ]; };
        list.FilterCommandTemplate = (code) => code is string c ? [ // Search Data:
            c, // Language Code
            LocaleNames.TryGetValue(c, out var n) ? n : "" // Language Name
            ] : [];
        base.RefreshData(job, token);
    }
    protected override string? Selected(int fromList) => GetProfile?.Locale;
    protected override void PerformSelect(int fromList, object? tag) 
        => ((ProfileContent?)TrueParent)?.SetLocale((string?)tag);
}