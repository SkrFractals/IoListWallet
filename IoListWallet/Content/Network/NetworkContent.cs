using IoListWallet.Core.Books;
using IoListWallet.Core.Panels;
using System;
using System.IO;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Network;
internal class NetworkContent : IoBookPanel {
    public NetworkContent() : base() { }

    // Serialized:
    //[Save]
    internal Type? MyNetwork => Networks.TryGetValue(Key, out var n) ? n : null;

    protected override string CustomTitle()
        => " - " + MyNetwork?.Name ?? "";
    internal enum SettingsNetworkPage : byte {
        Main = 0
    }
    internal override void Refresh(int job) {
        Book.PageNames = [""];
        base.Refresh(job);
    }
    private const string F = "Failed NetworkContent.", T = "T_NetworkContent.";

    protected override void FillPage(object? sender, IoBook.FillPageEventArgs e) {
        var s = e.Simulated ? "s." : "";
        switch ((SettingsNetworkPage)Book.Swipe.Page.X) {
            case SettingsNetworkPage.Main:

                // Address Book Editor
                void clickListAddressBook(object? s, EventArgs e) => OpenRefreshPanel(this, typeof(AddressBook), Key);
                
                var listAddressBookButton 
                    = MakeButton(s + "listAddressBookButton", T + "AddressBook", "Editor", clickListAddressBook)
                    ?? throw new(F + "listAddressBookButton");

                // Remove Network
                void clickRemoveNetwork(object? s, EventArgs e) {
                    if (MyNetwork == null)
                        return;
                    // Remove network:
                    GetNetworks?.Remove(MyNetwork);
                    Directory.Delete(Path.Combine(GetProfileDir(), MyNetwork.Name));
                    GetProfile?.Network = null;
                    // Update network select
                    RefreshPanel((typeof(NetworkSelect), ""), true);
                    // And close myself
                    Close();
                }
                var removeNetworkButton 
                    = MakeButton(s + "removeNetworkButton", T + "RemoveNetwork", "Remove",
                        clickRemoveNetwork, false, Avalonia.Media.FontWeight.Bold, L("AreYouSure"))
                    ?? throw new(F + "removeNetworkButton");
                
                Book.MakeRow(e, "0", "RemoveNetwork", [(removeNetworkButton.Button, 1)]);
                Book.MakeRow(e, "1", "AddressBookList", [(listAddressBookButton.Button, 1)]);

                break;
            
        }
        base.FillPage(sender, e);
    }
}