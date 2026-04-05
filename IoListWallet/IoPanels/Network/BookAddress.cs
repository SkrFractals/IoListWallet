using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using IoListWallet.MainControls.Panels;
using System;
using static IoListWallet.App;
using static IoListWallet.AppSave;
using static IoListWallet.Structs.Crypto.IoNetwork.AddressBookEntry;

namespace IoListWallet.IoPanels.Network;
internal class BookAddress : IoBookPanel {
    public BookAddress() : base() { }

    // Serialized:
    [Save]
    internal Type? MyNetwork { get; set { Dirty(); field = value; } } = null;
    [Save]
    internal ushort MyIndex { get; set { Dirty(); field = value; } } = 0;

    internal enum BookAddressPage : byte {
        Main = 0
    }
    private const string F = "Failed SettingsAppPanel.";
    protected override string CustomTitle() 
        => " - " + TryNetwork(MyNetwork)?.AddressBook[MyIndex].Name ?? "";
    private string GetIsValid() {
        var Net = TryNetwork(MyNetwork) ?? throw new(F + "GetIsValid.TryNetwork");
        var thisAddress = Net.AddressBook[MyIndex].Address;
        bool IsValid = Net.IsValidAddress(thisAddress);
        if(IsValid)
            for (int i = Net.AddressBook.Count; 0 <= --i;)
                if (i != MyIndex && Net.AddressBook[i].Address == thisAddress)
                    IsValid = false;
        return L((IsValid = Net.AddressBook[MyIndex].IsValid) ? "ValidAddress" : "InvalidAddress");
    }
    internal override void Refresh() {
        Book.PageNames = ["Main"];
        base.Refresh();
    }
    protected override void FillPage(object? s, EventArgs e) {
       
        var network = TryNetwork(MyNetwork) ?? throw new(F + "FillPage.TryNetwork");
        var T = "T_BookAddress.";
        switch ((BookAddressPage)Book.Swipe.Page.X) {
            case BookAddressPage.Main:

                // Name
                void changeName(object? s, EventArgs e) {
                    var text = ((TextBox?)s)?.Text ?? "New";
                    if (text == "")
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
                    }
                    ChangeKey(network.Blockchain + " - " + network.GetNetwork() + " - " + text);
                    network.AddressBook[MyIndex].Name = text;
                    SaveNet();
                    RefreshAll(); // only to update matches elsewhere
                }
                var changeNameBox = MakeTextBox("changeNameBox", T + "AddressName",
                    network.AddressBook[MyIndex].Name, "", changeName)
                    ?? throw new(F + "changeNameBox");
                changeNameBox.Text.Foreground = AddressBookColors[network.AddressBook[MyIndex].Color];

                // Validator
                var validNameBlock = MakeTextBlock("validNameBlock", "", GetIsValid())
                    ?? throw new(F + "validNameBlock");
               
                // Color
                void selectColor(object? s, EventArgs e) {
                    var c = network.AddressBook[MyIndex].Color = (byte)(((ComboBox?)s)?.SelectedIndex ?? 6);
                    changeNameBox.Text.Foreground = AddressBookColors[c];
                    SaveNet();
                    RefreshAll(); // only to update matches elsewhere
                }
                var selectColorBox = MakeComboBox("selectColorBox", T + "SelectAddressColor",
                    Enum.GetValues<AddressBookColor>(), (AddressBookColor)network.AddressBook[MyIndex].Color,
                    new FuncDataTemplate<AddressBookColor>((mode, _) => new TextBlock { Text = GetEnumText(mode) }, false), selectColor)
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
                var changeAddressBox = MakeTextBox("changeAddressBox", T + "CustomAddress",
                    network.AddressBook[MyIndex].Address, "", changeAddress)
                    ?? throw new(F + "changeAddressBox");
                
                // Deposit Rules
                void selectDeposit(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].RuleDeposit = (AddressBookRule?)((ComboBox?)s)?.SelectedItem ?? AddressBookRule.Allow;
                    RefreshParent();
                    SaveNet();
                }
                var selectDepositBox = MakeComboBox("selectDepositBox", T + "RuleDeposit",
                    Enum.GetValues<AddressBookRule>(), network.AddressBook[MyIndex].RuleDeposit,
                    new FuncDataTemplate<AddressBookRule>((mode, _) => new TextBlock { Text = GetEnumText(mode) }, false), selectDeposit)
                    ?? throw new(F + "selectDepositBox");
                void valueDeposit(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].MaxDepositValue = ulong.TryParse(((TextBox?)s)?.Text, out var d) ? d : 0;
                    RefreshParent();
                    SaveNet();
                }
                var valueDepositBox = MakeTextBox("valueDepositBox", T + "RuleDepositValue",
                    network.AddressBook[MyIndex].MaxDepositValue.ToString(), "Maximum", valueDeposit)
                    ?? throw new(F + "valueDepositBox");
                
                // Withdraw Rules
                void selectWithdraw(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].RuleWithdraw = (AddressBookRule?)((ComboBox?)s)?.SelectedItem ?? AddressBookRule.Allow;
                    RefreshParent();
                    SaveNet();
                }
                var selectWithdrawBox = MakeComboBox("selectWithdrawBox", T + "RuleWithdraw",
                    Enum.GetValues<AddressBookRule>(), network.AddressBook[MyIndex].RuleWithdraw,
                    new FuncDataTemplate<AddressBookRule>((mode, _) => new TextBlock { Text = GetEnumText(mode) }, false), selectWithdraw)
                    ?? throw new(F + "selectWithdrawBox");
                void valueWithdraw(object? s, EventArgs e) {
                    network.AddressBook[MyIndex].MaxWithdrawValue = ulong.TryParse(((TextBox?)s)?.Text, out var d) ? d : 0;
                    RefreshParent();
                    SaveNet();
                }
                var valueWithdrawBox = MakeTextBox("valueWithdrawBox", T + "RuleWithdrawValue",
                    network.AddressBook[MyIndex].MaxWithdrawValue.ToString(), "Maximum", valueWithdraw)
                    ?? throw new(F + "valueWithdrawBox");
               
                // Remove Address
                void clickRemoveAddress(object? s, EventArgs e) {
                    // shake
                    network.AddressBook.RemoveAt(MyIndex);
                    for (int i = MyIndex; i < network.AddressBook.Count; ++i)
                        ((BookAddress)GetPanel((
                            typeof(BookAddress),
                            network.Blockchain + " - " + network.GetNetwork() + " - " + network.AddressBook[i].Name)
                        )).MyIndex = --network.AddressBook[i].Index;
                    RefreshPanel((typeof(ListAddress), network.Blockchain + " - " + network.GetNetwork()));
                    //EnqueueTask((GetPanel((typeof(ListAddress), Net.Blockchain + " - " + Net.GetNetwork())), 1)); // Refresh Parent Address Book List
                    Delete(); // totally delete this panel instead of just closing.
                    RefreshAll(); // only to update matches elsewhere
                }
                var removeAddressButton = MakeButton("removeAddressButton", T + "RemoveAddress", "Remove",
                        clickRemoveAddress, false, FontWeight.Bold, L("AreYouSure"))
                    ?? throw new(F + "removeAddressButton");
                
                Book.MakeRow("0", "RemoveAddress", [(removeAddressButton.Button, 1)]);
                Book.MakeRow("1", "AddressName", [(changeNameBox.Border, 1)]);
                Book.MakeRow("2", "", [(validNameBlock.View, 1)]);
                Book.MakeRow("3", "SelectAddressColor", [(selectColorBox.View, 1)]);
                Book.MakeRow("4", "CustomAddress", [(changeAddressBox.Border, 1)]);
                Book.MakeRow("5", "RuleDeposit", [(selectDepositBox.View, 1), (valueDepositBox.Border, 3)]);
                Book.MakeRow("6", "RuleWithdraw", [(selectWithdrawBox.View, 1), (valueWithdrawBox.Border, 3)]);

                break;
        }
        base.FillPage(s, e);
    }
    private void RefreshParent() => EnqueueTask((GetPanel((typeof(ListAddress), Key)), 1));
}