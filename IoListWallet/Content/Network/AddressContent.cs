using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Books;
using IoListWallet.Core.Panels;
using System;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;
using static IoListWallet.Structs.Crypto.IoNetwork.AddressBookEntry;

namespace IoListWallet.IoPanels.Network;
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
        string k = Key[..Key.IndexOf('.')];
        var Net = TryNetwork(MyNetwork) ?? throw new(F + "GetIsValid.TryNetwork");
        var thisAddress = Net.AddressBook[MyIndex].Address;
        bool IsValid = Net.IsValidAddress(thisAddress);
        if(IsValid)
            for (int i = Net.AddressBook.Count; 0 <= --i;)
                if (i != MyIndex && Net.AddressBook[i].Address == thisAddress)
                    IsValid = false;
        return (IsValid = Net.AddressBook[MyIndex].IsValid) ? L("Success") + ": " + L("ValidAddress") : L("Error") + ": " + ("InvalidAddress");
    }
    internal override void Refresh(int job) {
        Book.PageNames = [""];
        base.Refresh(job);
    }
    protected override void FillPage(object? sender, IoBook.FillPageEventArgs e) {
        var s = e.Simulated ? "s." : "";
        var network = TryNetwork(MyNetwork) ?? throw new(F + "FillPage.TryNetwork");
        switch ((BookAddressPage)Book.Swipe.Page.X) {
            case BookAddressPage.Main:

                // Name
                void changeName(object? s, EventArgs e) {
                    var text = ((TextBox?)s)?.Text ?? "New";
                    if (text == "")
                        text = "New";
                    /*var origText = text;
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
                    SaveNet();
                    RefreshAll(); // only to update matches elsewhere
                }
                var changeNameBox = MakeTextBox(s + "changeNameBox", T + "AddressName",
                    network.AddressBook[MyIndex].Name, "", changeName)
                    ?? throw new(F + "changeNameBox");
                changeNameBox.Text.Foreground = AddressBookColors[network.AddressBook[MyIndex].Color];

                // Validator
                var validNameBlock = MakeTextBlock(s + "validNameBlock", "", GetIsValid())
                    ?? throw new(F + "validNameBlock");
               
                // Color
                void selectColor(object? s, EventArgs e) {
                    var c = network.AddressBook[MyIndex].Color = (byte)(((ComboBox?)s)?.SelectedIndex ?? 6);
                    changeNameBox.Text.Foreground = AddressBookColors[c];
                    SaveNet();
                    RefreshAll(); // only to update matches elsewhere
                }
                var selectColorBox = MakeComboBox(s + "selectColorBox", T + "SelectAddressColor",
                    Enum.GetValues<AddressBookColor>(), (AddressBookColor)network.AddressBook[MyIndex].Color,
                    new FuncDataTemplate<AddressBookColor>((mode, _) => new IoTextBlock { Text = GetEnumText(mode) }, false), selectColor)
                    ?? throw new(F + "selectColorBox");
               
                // Address
                void changeAddress(object? s, EventArgs e) {
                    bool WasValid = network.AddressBook[MyIndex].IsValid;
                    network.AddressBook[MyIndex].Address = ((TextBox?)s)?.Text ?? "";
                    validNameBlock.Text.Text = GetIsValid();
                    SaveNet();
                    if (WasValid || network.AddressBook[MyIndex].IsValid) // invalid to invalid should not do anything
                        RefreshAll(); // only to update matches elsewhere
                    else RefreshParent();
                }
                var changeAddressBox = MakeTextBox(s + "changeAddressBox", T + "CustomAddress",
                    network.AddressBook[MyIndex].Address, "", changeAddress)
                    ?? throw new(F + "changeAddressBox");
                
                // Deposit Rules
                void selectDeposit(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].RuleDeposit = (AddressBookRule?)((ComboBox?)s)?.SelectedItem ?? AddressBookRule.Allow;
                    RefreshParent();
                    SaveNet();
                }
                var selectDepositBox = MakeComboBox(s + "selectDepositBox", T + "RuleDeposit",
                    Enum.GetValues<AddressBookRule>(), network.AddressBook[MyIndex].RuleDeposit,
                    new FuncDataTemplate<AddressBookRule>((mode, _) => new IoTextBlock { Text = GetEnumText(mode) }, false), selectDeposit)
                    ?? throw new(F + "selectDepositBox");
                void valueDeposit(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].MaxDepositValue = ulong.TryParse(((TextBox?)s)?.Text, out var d) ? d : 0;
                    RefreshParent();
                    SaveNet();
                }
                var valueDepositBox = MakeTextBox(s + "valueDepositBox", T + "RuleDepositValue",
                    network.AddressBook[MyIndex].MaxDepositValue.ToString(), "Maximum", valueDeposit)
                    ?? throw new(F + "valueDepositBox");
                
                // Withdraw Rules
                void selectWithdraw(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].RuleWithdraw = (AddressBookRule?)((ComboBox?)s)?.SelectedItem ?? AddressBookRule.Allow;
                    RefreshParent();
                    SaveNet();
                }
                var selectWithdrawBox = MakeComboBox(s + "selectWithdrawBox", T + "RuleWithdraw",
                    Enum.GetValues<AddressBookRule>(), network.AddressBook[MyIndex].RuleWithdraw,
                    new FuncDataTemplate<AddressBookRule>((mode, _) => new IoTextBlock { Text = GetEnumText(mode) }, false), selectWithdraw)
                    ?? throw new(F + "selectWithdrawBox");
                void valueWithdraw(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].MaxWithdrawValue = ulong.TryParse(((TextBox?)s)?.Text, out var d) ? d : 0;
                    RefreshParent();
                    SaveNet();
                }
                var valueWithdrawBox = MakeTextBox(s + "valueWithdrawBox", T + "RuleWithdrawValue",
                    network.AddressBook[MyIndex].MaxWithdrawValue.ToString(), "Maximum", valueWithdraw)
                    ?? throw new(F + "valueWithdrawBox");
               
                // Remove Address
                void clickRemoveAddress(object? s, EventArgs e) {
                    // shake
                    network.AddressBook.Remove(MyIndex);
                    RefreshPanel((typeof(AddressBook), MyNetName), true);
                    Delete(); // totally delete this panel instead of just closing.
                    RefreshAll(); // only to update matches elsewhere
                }
                var removeAddressButton = MakeButton(s + "removeAddressButton", T + "RemoveAddress", "Remove",
                        clickRemoveAddress, false, FontWeight.Bold, L("AreYouSure"))
                    ?? throw new(F + "removeAddressButton");
                
                Book.MakeRow(e, "0", "RemoveAddress", [(removeAddressButton.Button, 1)]);
                Book.MakeRow(e, "1", "AddressName", [(changeNameBox.Border, 1)]);
                Book.MakeRow(e, "2", "", [(validNameBlock.View, 1)]);
                Book.MakeRow(e, "3", "SelectAddressColor", [(selectColorBox.View, 1)]);
                Book.MakeRow(e, "4", "CustomAddress", [(changeAddressBox.Border, 1)]);
                Book.MakeRow(e, "5", "RuleDeposit", [(selectDepositBox.View, 1), (valueDepositBox.Border, 3)]);
                Book.MakeRow(e, "6", "RuleWithdraw", [(selectWithdrawBox.View, 1), (valueWithdrawBox.Border, 3)]);

                break;
        }
        base.FillPage(sender, e);
    }
    private void RefreshParent() {
        //if(ParentPanel != null && IoPanel.SplitK(ParentPanel) is (Type, string) k && k.Type == typeof(AddressBook))
        //RefreshPanel((typeof(AddressBook), MyNetName), true);
        RefreshPanel(ParentPanel, true);
    }
}