using IoListWallet.Content.Abstract;
using IoListWallet.Core;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Lists;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;
using static IoListWallet.Structs.Crypto.IoNetwork;

namespace IoListWallet.IoPanels.Network;
internal class TickerBook : IoListPanel {
    internal bool initTicker = true;
    internal override bool IsModal() => true;
    
    public TickerBook() : base() {
        MakeList<TickerBookEntry>(new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "Unit" }, // Unit, how you call the token of the ticker
                new FilterText() { Name = "Id" }, // Token Id
                new FilterNumber() { Name = "Decimals", UpperMax = 19, LowerMax = 19, dRange = new(0, 19), Range = [new(0, 19)] }, // Decimals
                new FilterText() { Name = "Description" }, // Description
        ], SortIndex = 0, SortIndex2 = 1, Increasing = true, Increasing2 = true
        }, new Gen_Buttons(1), [("T_AddTicker", L("AddTicker"))]); // 0 = Add Ticker
    }
    private static string F = "Failed TickerBook.";
    internal override IoPanel OnOpen(bool initial = false) {
        var r = base.OnOpen(initial);
        var n = TryNetwork(MyNetwork) ?? throw new(F + "OnOpen.TryNetwork");
        if (n.TickerBook.Count <= 0 && initTicker) {
            initTicker = false;
            OnClickTool(null, new IoListBase.ClickToolEventArgs() { FromList = 0, Index = 0 });
        }
        return r;
    }
    internal Type? MyNetwork => Networks.TryGetValue(Key, out Type? Net) ? Net : null;
    protected override string CustomTitle()
        => " - " + MyNetwork?.Name ?? "";
    internal override string TrueParentType => MakeK(typeof(NetworkContent), Key);
    internal override void RefreshData(int job, CancellationToken? token) {
        MyList[0].RefreshTemplate(TryNetwork(MyNetwork)?.TickerBook.Values,
            (ticker) => new Gen_Buttons.GenData(["T_SelectTickerBook", ticker is TickerBookEntry t ? "| " + t.Unit + " - " + L("Decimals") + ": " + t.Decimals : ""]), // Select Ticker
            (ticker) => ticker is TickerBookEntry t ? [ // Search Data:
                t.Id,
                t.Unit,
                t.Decimals.ToString(),
                t.Description
            ] : []);
        base.RefreshData(job, token);
    }
    protected override void OnClickRow(object? s, IoListBase.ClickRowEventArgs e) {
        switch (e.Index) {
            case 0: // Edit Ticker
                var b = e.ClickedRow as WToggleButton ?? throw new(F + "OnClickButton");
                TickerBookEntry A = b.Button.Tag as TickerBookEntry ?? throw new(F + "Button.Tag is not an TickerBookEntry!");
                ushort MyIndex = A.Index;
                OpenRefreshPanel(this, typeof(TickerContent), MyIndex.ToString() + "." + Key);
                break;
        }
    }
    protected override void OnClickTool(object? s, IoListBase.ClickToolEventArgs e) {
        switch (e.Index) {
            case 0: // Add Ticker
                var n = TryNetwork(MyNetwork) ?? throw new(F + "OnClickTool.TryNetwork");
                ushort MyIndex = 0;
                while (n.TickerBook.ContainsKey(MyIndex))
                    ++MyIndex;
                n.TickerBook[MyIndex] = new() { Index = MyIndex };
                OpenRefreshPanel(this, typeof(TickerContent), MyIndex.ToString() + "." + Key);
                RefreshPanel(this, ~0, true);
                break;
        }
    }
}