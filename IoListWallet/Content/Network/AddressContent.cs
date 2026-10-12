using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using IoListWallet.Content.Abstract;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Books;
using System;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;
using static IoListWallet.Structs.Crypto.IoNetwork.AddressBookEntry;

namespace IoListWallet.Content.Network;
internal class AddressContent : IoBookPanel {
    internal override bool IsModal() => true;
    public AddressContent() : base() { }

    // Serialized:
    /*[Save]
    internal Type? MyNetwork { get; set { Dirty(); field = value; } } = null;
    [Save]
    internal ushort MyIndex { get; set { Dirty(); field = value; } } = 0;*/

    private string MyNetName => Key[(Key.IndexOf('.') + 1)..];
    internal ushort MyIndex => ushort.TryParse(Key[..Key.IndexOf('.')], out var i) ? i : (ushort)0;
    internal Type? MyNetwork => Networks.TryGetValue(MyNetName, out var Net) ? Net : null;
    

    internal enum BookAddressPage : byte {
        Main = 0
    }
    private const string F = "Failed AddressContent.", T = "T_AddressContent.";
    protected override string CustomTitle() 
        => " - " + TryNetwork(MyNetwork)?.AddressBook[MyIndex].Name ?? "";
    private string GetIsValid() {
        //string k = Key[..Key.IndexOf('.')];
        var Net = TryNetwork(MyNetwork) ?? throw new(F + "GetIsValid.TryNetwork");
        var thisAddress = Net.AddressBook[MyIndex].Address;
        bool IsValid = Net.IsValidAddress(thisAddress);
        if(IsValid)
            for (int i = Net.AddressBook.Count; 0 <= --i;)
                if (i != MyIndex && Net.AddressBook[i].Address == thisAddress)
                    IsValid = false; // is a duplicate
        return (Net.AddressBook[MyIndex].IsValid = IsValid) ? L("Success") + ": " + L("ValidAddress") : L("Error") + ": " + ("InvalidAddress");
    }
    internal override void Refresh(int job) {
        Book.PageNames = [""];
        base.Refresh(job);
    }
    protected override void FillPage(object? sender, IoBook.FillPageEventArgs e) {
        byte i = 0;
        string k() => (++i).ToString();
        var s = e.Simulated ? "s." : "";
        var network = TryNetwork(MyNetwork) ?? throw new(F + "FillPage.TryNetwork");
        switch ((BookAddressPage)Book.Swipe.Page.X) {
            case BookAddressPage.Main:
                if (!network.AddressBook.ContainsKey(MyIndex)) {
                    base.FillPage(sender, e);
                    return;
                }
                // Name
                void changeName(object? s, EventArgs e) {
                    var text = ((TextBox?)s)?.Text ?? "";
                    /*if (text == "")
                        text = "New";
                    var origText = text;
                    bool fail = true;
                    ushort count = 0;
                    while (fail) {
                        fail = false;
                        for (int i = network.AddressBook.Count; 0 <= --i;)
                            if (i != MyIndex && network.AddressBook[i].Name == text)
                                fail = true;
                        if (fail)
                            text = origText + (++count).ToString();
                    }*/
                    //ChangeKey(network.Blockchain + " - " + network.GetNetwork() + " - " + text);
                    network.AddressBook[MyIndex].Name = text;
                    _ = SaveNet();
                    RefreshAll(); // only to update matches elsewhere
                }
                var changeNameBox = MakeTextBox(s + "changeNameBox", this, T + "AddressName",
                    network.AddressBook[MyIndex].Name, "", changeName);
                changeNameBox.Text.Foreground = PaletteColors[network.AddressBook[MyIndex].Color];

                // Validator
                var validNameBlock = MakeTextBlock(s + "validNameBlock", this, "", GetIsValid());
               
                // Color
                void selectColor(object? s, EventArgs e) {
                    var c = network.AddressBook[MyIndex].Color = (byte)(((ComboBox?)s)?.SelectedIndex ?? 6);
                    changeNameBox.Text.Foreground = PaletteColors[c];
                    _ = SaveNet();
                    RefreshAll(); // only to update matches elsewhere
                }
                var selectColorBox = MakeComboBox(s + "selectColorBox", this, T + "SelectAddressColor",
                    Enum.GetValues<ColorPalette>(), (ColorPalette)network.AddressBook[MyIndex].Color,
                    new FuncDataTemplate<ColorPalette>((mode, _) => new IoTextBlockControl { Text = GetEnumText(mode) }, false), selectColor);
               
                // Address
                void changeAddress(object? s, EventArgs e) {
                    bool WasValid = network.AddressBook[MyIndex].IsValid;
                    network.AddressBook[MyIndex].Address = ((TextBox?)s)?.Text ?? "";
                    validNameBlock.Text.Text = GetIsValid();
                    _ = SaveNet();
                    if (WasValid || network.AddressBook[MyIndex].IsValid) // invalid to invalid should not do anything
                        RefreshAll(); // only to update matches elsewhere
                    else RefreshParent();
                }
                var changeAddressBox = MakeTextBox(s + "changeAddressBox", this, T + "CustomAddress",
                    network.AddressBook[MyIndex].Address, "", changeAddress);
                
                // Deposit Rules
                void selectDeposit(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].RuleDeposit = (AddressBookRule?)((ComboBox?)s)?.SelectedItem ?? AddressBookRule.Allow;
                    RefreshParent();
                    _ = SaveNet();
                }
                var selectDepositBox = MakeComboBox(s + "selectDepositBox", this, T + "RuleDeposit",
                    Enum.GetValues<AddressBookRule>(), network.AddressBook[MyIndex].RuleDeposit,
                    new FuncDataTemplate<AddressBookRule>((mode, _) => new IoTextBlockControl { Text = GetEnumText(mode) }, false), selectDeposit);
                void valueDeposit(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].MaxDepositValue = ulong.TryParse(((TextBox?)s)?.Text, out var d) ? d : 0;
                    RefreshParent();
                    _ = SaveNet();
                }
                var valueDepositBox = MakeTextBox(s + "valueDepositBox", this, T + "RuleDepositValue",
                    network.AddressBook[MyIndex].MaxDepositValue.ToString(), "Maximum", valueDeposit);
                
                // Withdraw Rules
                void selectWithdraw(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].RuleWithdraw = (AddressBookRule?)((ComboBox?)s)?.SelectedItem ?? AddressBookRule.Allow;
                    RefreshParent();
                    _ = SaveNet();
                }
                var selectWithdrawBox = MakeComboBox(s + "selectWithdrawBox", this, T + "RuleWithdraw",
                    Enum.GetValues<AddressBookRule>(), network.AddressBook[MyIndex].RuleWithdraw,
                    new FuncDataTemplate<AddressBookRule>((mode, _) => new IoTextBlockControl { Text = GetEnumText(mode) }, false), selectWithdraw);
                void valueWithdraw(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].MaxWithdrawValue = ulong.TryParse(((TextBox?)s)?.Text, out var d) ? d : 0;
                    RefreshParent();
                    _ = SaveNet();
                }
                var valueWithdrawBox = MakeTextBox(s + "valueWithdrawBox", this, T + "RuleWithdrawValue",
                    network.AddressBook[MyIndex].MaxWithdrawValue.ToString(), "Maximum", valueWithdraw);
               
                // Remove Address
                void clickRemoveAddress(object? s, EventArgs e) {
                    // shake
                    _ = network.AddressBook.Remove(MyIndex);
                    _ = RefreshPanel((typeof(AddressBook), MyNetName), true);
                    Delete(); // totally delete this panel instead of just closing.
                    RefreshAll(); // only to update matches elsewhere
                }
                var removeAddressButton = MakeButton(s + "removeAddressButton", this, T + "RemoveAddress", "Remove",
                    clickRemoveAddress, false, FontWeight.Bold, L("AreYouSure"));
                
                Book.MakeRow(e, k(), "RemoveAddress", [(removeAddressButton.Button, 1)]);
                Book.MakeRow(e, k(), "AddressName", [(changeNameBox.Border, 1)]);
                Book.MakeRow(e, k(), "", [(validNameBlock.View, 1)]);
                Book.MakeRow(e, k(), "SelectAddressColor", [(selectColorBox.View, 1)]);
                Book.MakeRow(e, k(), "CustomAddress", [(changeAddressBox.Border, 1)]);
                Book.MakeRow(e, k(), "RuleDeposit", [(selectDepositBox.View, 1), (valueDepositBox.Border, 3)]);
                Book.MakeRow(e, k(), "RuleWithdraw", [(selectWithdrawBox.View, 1), (valueWithdrawBox.Border, 3)]);

                break;
        }
        base.FillPage(sender, e);
    }
    private void RefreshParent() {
        //if(ParentPanel != null && IoPanel.SplitK(ParentPanel) is (Type, string) k && k.Type == typeof(AddressBook))
        //RefreshPanel((typeof(AddressBook), MyNetName), true);
        _ = RefreshPanel(ParentPanel, true);
    }
}