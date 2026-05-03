using IoListWallet.Content.Abstract;
using IoListWallet.Core;
using IoListWallet.Core.Lists;
using System.Threading;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Profile;
internal class LocaleSelect : IoListPanel {
    public LocaleSelect() : base() => MakeList<string>(new IoFilter() {
        Filters = [ // Search Formats:
            new FilterText() { Name = "Code" }, // Language Code 
            new FilterText() { Name = "Name" }, // Language Name 
        ], SortIndex = 1, SortIndex2 = 0, Increasing = true, Increasing2 = true
    }, new Gen_Buttons(1));
    internal override bool IsModal() => true;
    internal override string TrueParentType => MakeK(typeof(ProfileContent), "");
    internal override void RefreshData(int job, CancellationToken? token) {
        MyList[0].RefreshTemplate(LocaleNames.Keys,
            (object? code) => new Gen_Buttons.GenData(["T_ProfileContent.SelectLanguageEntry", code is string s ? LocaleNames[s] : "Error"]),
            (code) => code is string c ? [ // Search Data:
                c, // Language Code
                LocaleNames.TryGetValue(c, out var n) ? n : "" // Language Name
            ] : []);
        base.RefreshData(job, token);
    }
    protected override string? Selected(int fromList) => GetProfile?.SelectedLocale;
    protected override void PerformSelect(int fromList, object? tag)
        => GetProfile?.SelectedLocale = (string?)tag ?? "";
    protected override bool CloseOnClick(int fromList) => true;
}