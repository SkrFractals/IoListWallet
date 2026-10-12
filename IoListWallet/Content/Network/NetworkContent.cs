using IoListWallet.Content.Abstract;
using IoListWallet.Core.Books;
using System;
using System.IO;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Content.Network;
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
        byte i = 0;
        string k() => (++i).ToString();
        var s = e.Simulated ? "s." : "";
        switch ((SettingsNetworkPage)Book.Swipe.Page.X) {
            case SettingsNetworkPage.Main:
                
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
                var networkDeleteButton
                    = MakeButton(s + "networkDeleteButton", this, T + "NetworkDeleteButton", L("Delete"),
                        clickRemoveNetwork, false, Avalonia.Media.FontWeight.Bold, L("AreYouSure"));

                // Open Netowrk Save Location Button
                void clickNetworkSave(object? s, EventArgs e) => OpenDirectory(GetNetworkDir());
                var networkSaveButton = MakeButton(s + "networkSaveButton", this, T + "NetworkSaveButton",
                    L("Open"), clickNetworkSave);

                // Address Book Editor
                void clickListAddressBook(object? s, EventArgs e) => OpenRefreshPanel(this, typeof(AddressBook), Key);
                var listAddressBookButton
                    = MakeButton(s + "listAddressBookButton", this, T + "AddressBook", "Editor", clickListAddressBook);

                // Address Book Editor
                void clickListTickerBook(object? s, EventArgs e) => OpenRefreshPanel(this, typeof(TickerBook), Key);
                var listTickerBookButton
                    = MakeButton(s + "listTickerBookButton", this, T + "TickerBook", "Editor", clickListTickerBook);

                Book.MakeRow(e, k(), "NetworkSave", [(networkDeleteButton.Button, 1), (networkSaveButton.Button,1)]);
                Book.MakeRow(e, k(), "AddressBookList", [(listAddressBookButton.Button, 1)]);
                Book.MakeRow(e, k(), "TickerBookList", [(listTickerBookButton.Button, 1)]);

                break;
            
        }
        base.FillPage(sender, e);
    }
}