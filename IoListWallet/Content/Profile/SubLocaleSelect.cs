using IoListWallet.Content.Abstract;
using IoListWallet.Core;
using IoListWallet.Core.Lists;
using System.Threading;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Content.Profile;
internal class SubLocaleSelect : IoListPanel {
    public SubLocaleSelect() : base() => MakeList<string>(new IoFilter() {
        Filters = [ // Search Formats:
            new FilterText() { Name = "Code" }, // Language Code 
            new FilterText() { Name = "Name" }, // Language Name 
        ], SortIndex = 1, SortIndex2 = 0, Increasing = true, Increasing2 = true
    }, new Gen_Buttons(1));
    internal override bool IsModal() => true;
    internal override string TrueParentType => MakeK(typeof(ProfileContent), "");
    internal override void RefreshData(int job, CancellationToken? token) {
        MyList[0].RefreshTemplate(SubLocaleNames.Keys,
            (object? code) => new Gen_Buttons.GenData(["T_ProfileContent.SelectSubLanguageEntry", code is string s ? SubLocaleNames[s] : "Error"]),
            (code) => code is string c ? [ // Search Data:
                c, // Language Code
                SubLocaleNames.TryGetValue(c, out var n) ? n : "" // Language Name
            ] : []);
        base.RefreshData(job, token);
    }
    protected override string? Selected(int fromList) => GetProfile?.SelectedSubLocale;
    protected override void PerformSelect(int fromList, object? tag) 
        => GetProfile?.SelectedSubLocale = (string?)tag ?? "";
    protected override bool CloseOnClick(int fromList) => true;
}