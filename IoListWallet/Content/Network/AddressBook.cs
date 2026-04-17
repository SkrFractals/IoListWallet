using IoListWallet.Core;
using IoListWallet.Core.Lists;
using IoListWallet.Core.Panels;
using IoListWallet.Structs.Crypto;
using System;
using System.Threading;
using static IoListWallet.App;
using static IoListWallet.Core.Static.AppSave;
using static IoListWallet.Structs.Crypto.IoNetwork;
using static IoListWallet.Structs.Crypto.IoNetwork.AddressBookEntry;

namespace IoListWallet.IoPanels.Network;
internal class AddressBook : IoListPanel {
    public AddressBook() : base() {
        (MyList[0] = new IoListButtons<AddressBookEntry>(this, [("T_AddAddress", L("AddAddress"))]) { MyIndex = 0 })
            .MyFilter.Filters = new IoFilter() {
            Filters = [ // Search Formats:
                new FilterText() { Name = "AddressName" }, // Address nickname how you call it
                new FilterText() { Name = "CustomAddress" }, // Actual address the nickname is attached to
                new FilterEnum() { // SelectAddressColor 
                    Name = "SelectAddressColor",
                    dMatch = 1 << 6,
                    Match = [1 << 6],
                    /*dRule = new(6, (byte)CompareRule.Equal),
                    Rule = [new(6, (byte)CompareRule.Equal)],
                    Max =  (ushort)Enum.GetValues(typeof(AddressBookColor)).Length,*/
                    EnumType = typeof(AddressBookColor).Name
                },
                new FilterEnum() { // RuleDepositR 
                    Name = "RuleDepositR",
                    EnumType = typeof(AddressBookRule).Name
                },
                new FilterNumber() { Name = "RuleDepositL", dRange = new(0, 1000000), Range = [new(0, 1000000)] },
                new FilterEnum() { // RuleWithdrawR 
                    Name = "RuleWithdrawR",
                    EnumType = typeof(AddressBookRule).Name
                },
                new FilterNumber() { Name = "RuleWithdrawL", dRange = new(0, 1000000), Range = [new(0, 1000000)] }
        ], SortIndex = 2, SortIndex2 = 0, Increasing = true, Increasing2 = true
        };
        //MyContent.Content = MyList[0].Content;
    }
    internal override IoPanel OnOpen(bool initial = false) {
        var n = TryNetwork(MyNetwork) ?? throw new("Failed AddressBook.OnClickTool.TryNetwork");
        if (n.AddressBook.Count <= 0)
            OnClickTool(null, new ClickToolEventArgs() { FromList = 0, Index = 0 });
        return base.OnOpen(initial);
    }

    // Serialized:
    //[Save]
    //internal Type? MyNetwork { get; set; } = null;
    internal Type? MyNetwork => Networks.TryGetValue(Key, out Type? Net) ? Net : null;
    protected override string CustomTitle()
        => " - " + MyNetwork?.Name ?? "";
    internal override string TrueParentType => MakeK(typeof(NetworkContent), Key);
    protected override bool CloseOnClick(int fromList) => false;
    /*internal override bool OnResponse(IoPanel From, object Data) {
        if (base.OnResponse(From, Data))
            return true;
        if (From is CustomAddressPanel) { // Deeper NetworkAdd selected something and is letting me know:
            MainMenu?.OnResponse(this, 0);
            Close();
            return true;
        }
        return false;
    }*/
    protected override void OnClickTool(object? s, ClickToolEventArgs e) {
        switch (e.Index) {
            case 0: // Add Custom Address
                var n = TryNetwork(MyNetwork) ?? throw new("Failed AddressBook.OnClickTool.TryNetwork");
                ushort MyIndex = 0;
                while (n.AddressBook.ContainsKey(MyIndex))
                    ++MyIndex;
                n.AddressBook[MyIndex] = new() { Index = MyIndex };
                OpenRefreshPanel(this, typeof(AddressContent), MyIndex.ToString() + "." + Key);
                /*ushort MyIndex = (ushort)(n.AddressBook.Count);
                n.AddressBook.Add(new() { Index = MyIndex });
                var NewPanel = (BookAddress?)OpenPanel(this, typeof(BookAddress), n.Blockchain + " - " + n.GetNetwork() + " - New", false, false) 
                    ?? throw new("Failed ListAddress.OnClickTool.NewPanel");
                NewPanel.MyNetwork = MyNetwork;
                NewPanel.MyIndex = MyIndex;
                NewPanel.SetTitle();
                RefreshPanel(NewPanel, 1, true);*/
                break;
        }
    }
    internal override void RefreshData(int job, CancellationToken? token) {
        if (TryNetwork(MyNetwork) is not IoNetwork network || MyList[0] is not IoListButtons<AddressBookEntry> list)
            return;
        // testing data:
        /*network.AddressBook = [ 
            new AddressBookEntry(){ Index=0, Name = "C2", Address = "wb 1000-2000", Color = 2, MaxDepositValue=1000, MaxWithdrawValue=2000,RuleDeposit = AddressBookRule.Warn, RuleWithdraw = AddressBookRule.Block},
            new AddressBookEntry(){ Index=0, Name = "C4", Address = "bw 4000-5000", Color = 4, MaxDepositValue=4000, MaxWithdrawValue=5000,RuleDeposit = AddressBookRule.Block, RuleWithdraw = AddressBookRule.Warn},
            new AddressBookEntry(){ Index=0, Name = "C5", Address = "aa 3000-4000", Color = 5, MaxDepositValue=3000, MaxWithdrawValue=4000,RuleDeposit = AddressBookRule.Allow, RuleWithdraw = AddressBookRule.Allow},
            new AddressBookEntry(){ Index=0, Name = "C3", Address = "ww 8000-9000", Color = 3, MaxDepositValue=8000, MaxWithdrawValue=9000,RuleDeposit = AddressBookRule.Warn, RuleWithdraw = AddressBookRule.Warn},
            new AddressBookEntry(){ Index=0, Name = "C1", Address = "aw 100-200", Color = 1, MaxDepositValue=100, MaxWithdrawValue=200,RuleDeposit = AddressBookRule.Allow, RuleWithdraw = AddressBookRule.Warn},
            new AddressBookEntry(){ Index=0, Name = "C3", Address = "wa 6000-7000", Color = 3, MaxDepositValue=6000, MaxWithdrawValue=7000,RuleDeposit = AddressBookRule.Warn, RuleWithdraw = AddressBookRule.Allow},
            new AddressBookEntry(){ Index=0, Name = "C4", Address = "ab 5000-6000", Color = 4, MaxDepositValue=5000, MaxWithdrawValue=6000,RuleDeposit = AddressBookRule.Allow, RuleWithdraw = AddressBookRule.Block}
            ];*/
        list.ItemSource = (System.Collections.Generic.IEnumerable<AddressBookEntry>?)network.AddressBook;//.AsEnumerable();
        list.ItemTemplate = (address) => [ // Columns:
                "T_SelectAddressBook", address is AddressBookEntry a ? "| " + a.Name + " - " + a.Address : "" // Select Address
                ];
        list.FilterCommandTemplate = (address) => address is AddressBookEntry a ? [ // Search Data:
                a.Name,
                a.Address,
                a.Color.ToString(),
                ((byte)a.RuleDeposit).ToString(),
                a.MaxDepositValue.ToString(),
                ((byte)a.RuleWithdraw).ToString(),
                a.MaxWithdrawValue.ToString()
                ] : [];
        base.RefreshData(job, token);
    }
    protected override object? Selected(int fromList) => null;
    protected override void OnClickButton(object? s, ClickButtonEventArgs e) {
        var n = TryNetwork(MyNetwork) ?? throw new("Failed ListAddress.OnClickButton.TryNetwork");
        switch (e.Index) {
            case 0: // Main button - Edit Address
                //base.OnClick(index, T, o);
                var b = e.ClickedButton ?? throw new("Failed ListAddress.OnclickButton");
                AddressBookEntry A = b.Button.Tag as AddressBookEntry ?? throw new("EditAddressBook - Button.Tag is not an AddressBook!");
                ushort MyIndex = A.Index;
                OpenRefreshPanel(this, typeof(AddressContent), MyIndex.ToString() + "." + Key/*n.Blockchain + " - " + n.GetNetwork() + " - " + A.Name*/);
                /*NewPanel.MyNetwork = MyNetwork;
                NewPanel.MyIndex = MyIndex;
                NewPanel.SetTitle();
                RefreshPanel(NewPanel);*/
                break;
        }
    }
}