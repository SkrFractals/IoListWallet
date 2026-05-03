using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using IoListWallet.Content.Abstract;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Books;
using System;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.IoPanels.Network;
internal class TickerContent : IoBookPanel {
    internal override bool IsModal() => true;
    public TickerContent() : base() { }

    private string MyNetName => Key[(Key.IndexOf('.') + 1)..];
    internal ushort MyIndex => ushort.TryParse(Key[..Key.IndexOf('.')], out var i) ? i : (ushort)0;
    internal Type? MyNetwork => Networks.TryGetValue(MyNetName, out var Net) ? Net : null;
    
    internal enum BookTickerPage : byte {
        Main = 0
    }
    private const string F = "Failed TickerContent.", T = "T_TickerContent.";
    protected override string CustomTitle() 
        => " - " + TryNetwork(MyNetwork)?.TickerBook[MyIndex].Unit ?? "";
    /*private string GetIsValid() {
        string k = Key[..Key.IndexOf('.')];
        var Net = TryNetwork(MyNetwork) ?? throw new(F + "GetIsValid.TryNetwork");
        var thisTicker = Net.TickerBook[MyIndex].Id;
        bool IsValid = Net.IsValidToken(thisTicker);
        if(IsValid)
            for (int i = Net.TickerBook.Count; 0 <= --i;)
                if (i != MyIndex && Net.TickerBook[i].Id == thisTicker)
                    IsValid = false;
        return (Net.TickerBook[MyIndex].IsValid = IsValid) ? L("Success") + ": " + L("ValidTicker") : L("Error") + ": " + ("InvalidTicker");
    }*/
    internal override void Refresh(int job) {
        Book.PageNames = [""];
        base.Refresh(job);
    }
    protected override void FillPage(object? sender, IoBook.FillPageEventArgs e) {
        byte i = 0;
        string k() => (++i).ToString();
        var s = e.Simulated ? "s." : "";
        var network = TryNetwork(MyNetwork) ?? throw new(F + "FillPage.TryNetwork");
        switch ((BookTickerPage)Book.Swipe.Page.X) {
            case BookTickerPage.Main:
                if (!network.TickerBook.ContainsKey(MyIndex)) {
                    base.FillPage(sender, e);
                    return;
                }
                // TODO maybe add some filter to only refresh panels that might possibly have tickers in them, and not ALL of them.

                //changeNameBox.Text.Foreground = TickerBookColors[network.TickerBook[MyIndex].Color];

                // Validator
                //var validNameBlock = MakeTextBlock(s + "validNameBlock", this, "", GetIsValid())
                //    ?? throw new(F + "validNameBlock");

                // Remove Ticker
                void clickRemoveAddress(object? s, EventArgs e) {
                    // shake
                    network.TickerBook.Remove(MyIndex);
                    RefreshPanel((typeof(TickerBook), MyNetName), true);
                    Delete(); // totally delete this panel instead of just closing.
                    RefreshAll(); // only to update matches elsewhere
                }
                var removeTickerButton = MakeButton(s + "removeTickerButton", this, T + "RemoveTicker", "Remove",
                        clickRemoveAddress, false, FontWeight.Bold, L("AreYouSure"));

                // Id
                void changeId(object? s, EventArgs e) {
                    //bool WasValid = network.TickerBook[MyIndex].IsValid;
                    network.TickerBook[MyIndex].Id = ((TextBox?)s)?.Text ?? "";
                    //validNameBlock.Text.Text = GetIsValid();
                    //SaveNet();
                    //if (WasValid || network.TickerBook[MyIndex].IsValid) // invalid to invalid should not do anything
                    RefreshAll(); // only to update matches elsewhere
                    //else RefreshParent();
                }
                var idBox = MakeTextBox(s + "idBox", this, T + "Id",
                    network.TickerBook[MyIndex].Id, "", changeId);

                // Unit
                void changeUnit(object? s, EventArgs e) {
                    network.TickerBook[MyIndex].Unit = ((TextBox?)s)?.Text ?? "";
                    //SaveNet();
                    RefreshAll();
                }
                var unitBox = MakeTextBox(s + "unitBox", this, T + "Unit",
                    network.TickerBook[MyIndex].Unit, "", changeUnit);
                unitBox.Text.Foreground = PaletteColors[network.TickerBook[MyIndex].Color];

                void changeDecimals(object? s, EventArgs e)
                   => network.TickerBook[MyIndex].Decimals = (byte)(((IoNumericLeftRight?)s)?.Value ?? 0);
                var selectDecimals = MakeNumeric(s + "selectDecimals", this, T + "SelectDecimals",
                    network.TickerBook[MyIndex].Decimals, 0, 19, 1, 10, "SelectDecimals", L("SelectDecimals"), "", changeDecimals);

                // Color
                void selectColor(object? s, EventArgs e) {
                    var c = network.TickerBook[MyIndex].Color = (byte)(((ComboBox?)s)?.SelectedIndex ?? 6);
                    unitBox.Text.Foreground = PaletteColors[c];
                    //SaveNet();
                    RefreshAll(); // only to update matches elsewhere
                }
                var selectColorBox = MakeComboBox(s + "selectColorBox", this, T + "SelectTickerColor",
                    Enum.GetValues<ColorPalette>(), (ColorPalette)network.TickerBook[MyIndex].Color,
                    new FuncDataTemplate<ColorPalette>((mode, _) => new IoTextBlockControl { Text = GetEnumText(mode) }, false), selectColor);

                // Description
                void changeDescription(object? s, EventArgs e) {
                    network.TickerBook[MyIndex].Description = ((TextBox?)s)?.Text ?? "";
                    //SaveNet();
                    RefreshAll();
                }
                var descriptionBox = MakeTextBox(s + "descriptionBox", this, T + "Description",
                    network.TickerBook[MyIndex].Description, "", changeDescription);

                Book.MakeRow(e, k(), "RemoveAddress", [(removeTickerButton.Button, 1)]);
                Book.MakeRow(e, k(), "Id", [(idBox.Border, 1)]);
                Book.MakeRow(e, k(), "Unit", [(unitBox.Border, 1)]);
                Book.MakeRow(e, k(), "Decimals", [(selectDecimals, 1)]);
                Book.MakeRow(e, k(), "Color", [(selectColorBox.View, 1)]);
                Book.MakeRow(e, k(), "Description", [(descriptionBox.Border, 1)]);

                break;
        }
        base.FillPage(sender, e);
    }
    private void RefreshParent() => RefreshPanel(ParentPanel, true);
}