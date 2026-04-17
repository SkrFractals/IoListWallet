using Avalonia.Controls;
using IoListWallet.Core.Books;
using IoListWallet.Core.Panels;
using static IoListWallet.App;

namespace IoListWallet.IoPanels.Main;
internal class Tutorial : IoBookPanel {
    public Tutorial() : base() => Book.Swipe.OnRightEvent += (_, _) => { Book.Swipe.CancelDrag(); Close(); };
    internal enum TabTutorialPage : byte {
        TabTutorialIntro,
        TabTutorialProfiles,
        TabTutorialNetworks,
        TabTutorialWallets,
        TabTutorialAccounts,
        TabTutorialTransactions,
        TabTutorialBuild
    }
    internal override bool IsModal() => true;
    protected override string[] GetPageNames() 
        => ["Intro", "Profiles", "Networks", "Wallets", "Accounts", "Transactions", "Build"];

    private const string F = "Failed Tutorial.", T = "T_Tutorial.";
    protected override void FillPage(object? sender, IoBook.FillPageEventArgs e) {
        var s = e.Simulated ? "s." : "";
        switch ((TabTutorialPage)e.Page.X) {
            case TabTutorialPage.TabTutorialIntro:
                Book.MakeRow(e, "0", "", [(Make(s + "intro", "T_TutorialIntro"), 1)]);
                break;
            case TabTutorialPage.TabTutorialProfiles:
                Book.MakeRow(e, "0", "", [(Make(s + "profile", "T_TutorialProfiles"), 1)]);
                break;
            case TabTutorialPage.TabTutorialNetworks:
                Book.MakeRow(e, "0", "", [(Make(s + "networks", "T_TutorialNetworks"), 1)]);
                break;
            case TabTutorialPage.TabTutorialWallets:
                Book.MakeRow(e, "0", "", [(Make(s + "walets", "T_TutorialWallets"), 1)]);
                break;
            case TabTutorialPage.TabTutorialAccounts:
                Book.MakeRow(e, "0", "", [(Make(s + "accounts", "T_TutorialAccounts"), 1)]);
                break;
            case TabTutorialPage.TabTutorialTransactions:
                Book.MakeRow(e, "0", "", [(Make(s + "transactions", "T_TutorialTransactions"), 1)]);
                break;
            case TabTutorialPage.TabTutorialBuild:
                Book.MakeRow(e, "0", "", [(Make(s + "build", "T_TutorialBuild"), 1)]);
                break;
        }
        base.FillPage(sender, e);
    }
    private Viewbox Make(string id, string text) {
        var o = MakeTextBlock(id, "", L(text));
        o?.Text.TextAlignment = Avalonia.Media.TextAlignment.Left;
        return o?.View!;
    }
}