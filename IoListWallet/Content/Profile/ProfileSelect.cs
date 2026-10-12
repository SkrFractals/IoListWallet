using IoListWallet.Content.Abstract;
using IoListWallet.Core;
using IoListWallet.Core.Lists;
using IoListWallet.Content.Main;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Content.Profile;
internal class ProfileSelect : IoListPanel {
    //private const string F = "Failed ListProfileSelect.", T = "T_ListProfileSelect";
    public ProfileSelect() : base() => MakeList<int>(new IoFilter() {
        Filters = [ // Search Formats:
            new FilterText() { Name = "Name" } // Name 
        ], SortIndex = 0, SortIndex2 = 0, Increasing = true, Increasing2 = true
    }, new Gen_Buttons(1), [("T_AddProfile", L("Add") + " " + L("Profile"))], -1, 0, null, null, IsSelected);
    
    internal override string TrueParentType => MakeK(typeof(MainMenu), "");
    internal override void RefreshData(int job, CancellationToken? token) {
        MyList[0].RefreshTemplate(GetProfiles.Keys,
            // Columns:Select Profile
            (o) => new Gen_Buttons.GenData(["T_SelectProfile", IsProfile((int?)o ?? -1, out var p) && p != null ? p.Name == "" ? L("NoName") : p.Name : ""]),
            // Search Data: Name
            (o) => IsProfile((int?)o ?? -1, out var p) && p != null ? [L(p.Name)] : []); 
        base.RefreshData(job, token);
    }
    protected bool IsSelected(object? o) => o is int i && SApp.Profile == i;
    protected override Type? Selected(int fromList) => GetProfile?.Network;
    protected override void PerformSelect(int fromList, object? o) => LoadProfile((int?)o ?? -1);
    protected override void OnClickRow(object? sender, IoListBase.ClickRowEventArgs e) => base.OnClickRow(sender, e);
    protected override void OnClickTool(object? s, IoListBase.ClickToolEventArgs e) {
        //switch (e.Index) {
        //    case 0: // Add Profile
        ushort profileId = 0;
        while (SApp.Profiles.ContainsKey(profileId))
           ++profileId;
        LoadProfile(profileId);
        OpenRefreshPanel(AppMainMenu, typeof(ProfileContent));
        //        break;
        //}
    }
}