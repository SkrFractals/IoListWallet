using IoListWallet.Content.Abstract;
using IoListWallet.Core.Books;
using System;
using static IoListWallet.App;

namespace IoListWallet.Content.Main;
internal class Tutorial : IoBookPanel {
    public Tutorial() : base() => Book.Swipe.OnRightEvent += (_, _) => { Book.Swipe.CancelDrag(); Close(); };
    /*internal enum TabTutorialPage : byte {
        TabTutorialIntro,
        TabTutorialStructure,
        TabTutorialProfiles,
        TabTutorialNetworks,
        TabTutorialWallets,
        TabTutorialAccounts,
        TabTutorialTransactions,
        TabTutorialBuild,
        TabTutorialDock
    }*/
    internal override bool IsModal() => true;
    protected override string[] GetPageNames() => Desktop != null
            ? ["Intro", "Structure", "Profiles", "Networks", "Wallets", "Accounts", "Transactions", "Build", "Dock"]
            : ["Intro", "Structure", "Profiles", "Networks", "Wallets", "Accounts", "Transactions", "Build"];

    private const string F = "Failed Tutorial.", T = "T_Tutorial.";
    protected override void FillPage(object? sender, IoBook.FillPageEventArgs e) {
        var s = e.Simulated ? "s." : "";
        var i = 0;
        string k() => (++i).ToString();
        void r(string t) => Book.MakeRow(e, k(), T + t, []);
        r(GetPageNames()[e.Page.X]);
        //switch ((TabTutorialPage)e.Page.X) { case TabTutorialPage.TabTutorialIntro:
        if (e.Page.X == 0) {
            r("Intro");
            var fail = MakeTextBlock(s + "tooltipsFail", null, "", "");
            void onFail(object? s, EventArgs e) => fail.Text.Text = L(T + "TooltipsFail");
            var fBut = MakeButton(s + "tooltipTest", this, T + "TooltipsSuccess", L("TutorialTooltip"), onFail);
            Book.MakeRow(e, k(), "", [(fail.View, 1)]);
            Book.MakeRow(e, k(), "", [(fBut.Button, 1)]);
        }
        // break;}
        base.FillPage(sender, e);
    }
}