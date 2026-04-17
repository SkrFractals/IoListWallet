using IoListWallet.Core;
using IoListWallet.Core.Lists;
using IoListWallet.Core.Panels;
using IoListWallet.IoPanels.Main;
using System;
using System.Collections.Generic;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Profile;
internal class ProfileSelect : IoListPanel {
    //private const string F = "Failed ListProfileSelect.", T = "T_ListProfileSelect";
    public ProfileSelect() : base() {
        (MyList[0] = new IoListButtons<int>(this, [("T_AddProfile", L("AddProfile"))]) { MyIndex = 0 })
            .MyFilter.Filters = new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Name" } // Name 
            ], SortIndex = 0, SortIndex2 = 0, Increasing = true, Increasing2 = true
        };
        MyList[0].Selected = IsSelected;
        //MyContent.Content = MyList[0].Content;
    }
    internal override string TrueParentType => MakeK(typeof(MainMenu), "");
    internal override void RefreshData(int job, CancellationToken? token) {
        if (MyList[0] is not IoListButtons<int> list)
            return;
        //((IoListButtons<int>)MyList).Columns = 2; // 0 = Select Network, 1 = Edit Network
        list.ItemSource = GetProfiles.Keys;
        list.ItemTemplate = (o) => [ // Columns:
            "T_SelectProfile", IsProfile((int?)o ?? -1, out var p) && p != null ? p.Name == "" ? L("NoName") : p.Name : "", // Select Profile
            //"T_EditProfile", L("EditProfile") // Edit Profile
            ];
        list.FilterCommandTemplate = (o) => IsProfile((int?)o ?? -1, out var p) && p != null ? [ // Search Data:
            L(p.Name) // Name
            ] : [];

        base.RefreshData(job, token);
    }
    protected bool IsSelected(object? o) => o is int i && SApp.Profile == i;
    protected override Type? Selected(int fromList) => GetProfile?.Network;
    protected override void PerformSelect(int fromList, object? o) {
        LoadProfile((int?)o ?? -1);
        //RefreshPanel(this);
    }
    protected override void OnClickButton(object? sender, ClickButtonEventArgs e) { 
        switch (e.Index) {
            case 0: // Select Profile
                base.OnClickButton(sender, e);
                break;
            /*case 1: // Edit Profile
                var b = e.ClickedButton ?? throw new(F + "OnClickButton");
                PerformSelect(b.Button.Tag);
                // TODO open profile edit
                break;*/
        }
    }
    protected override void OnClickTool(object? s, ClickToolEventArgs e) {
        switch (e.Index) {
            case 0: // Add Profile
                ushort profileId = 0;
                while (SApp.Profiles.ContainsKey(profileId))
                    ++profileId;
                LoadProfile(profileId);
                //var main = RefreshPanel((typeof(ListMainMenu), ""));
                OpenRefreshPanel(AppMainMenu, typeof(ProfileContent));
                break;
        }
    }
}