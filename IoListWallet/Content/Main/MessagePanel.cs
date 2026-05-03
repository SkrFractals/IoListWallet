using IoListWallet.Content.Abstract;
using IoListWallet.Core.Books;

namespace IoListWallet.IoPanels.Main;
internal class MessagePanel : IoBookPanel {
    // Book.Swipe.CancelDrag(); not necessary since it deletes itself on close
    public MessagePanel() : base() => Book.Swipe.OnRightEvent += (_, _) => Close();
    internal enum TabNullPage : byte { Main }
    internal override bool IsModal() => true;
    internal override bool DeleteWhenClosed() => true;
    protected override string[] GetPageNames() => [""];

    private const string F = "Failed Message.", T = "T_Message.";
    protected override void FillPage(object? sender, IoBook.FillPageEventArgs e) {
        var s = e.Simulated ? "s." : "";
        Book.MakeRow(e, "0", "", [(MakeTextBlock(s + "null", this, "", App.L(Key))?.View!, 1)]);
        base.FillPage(sender, e);
    }
}