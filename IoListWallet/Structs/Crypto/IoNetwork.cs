using Avalonia.Media;
using System;
using System.Collections.Generic;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Structs.Crypto;
internal abstract class IoNetwork {
    internal class History {
        internal bool Dirty = true;
        [Save]
        internal Dictionary<string, IoTransaction> Tx { get; set { field = value; Dirty = true; } } = [];
        internal void AddTx(string key, IoTransaction tx) {
            Dirty = true;
            Tx[key] = tx;
        }
    }
    internal class AddressBookEntry {
        internal enum AddressBookRule : byte {
            Allow = 0,  // allows any value without quetions
            Warn = 1,   // warns you the value is exceeding the limit, asks you if you really want to spend
            Block = 2,  // blocks a transactionwhere the value exceeds the limit, until the transation or the rule is changed
            BlockOrWarnBelow = 3 // blocks over the limit, but also warns if the value is below limit (but not zero)
        }
        [Save] // Index on the list
        internal ushort Index { get; set { field = value; GetNetwork?.Dirty = true; } }
        [Save] // blockchain check
        internal bool IsValid { get; set { field = value; GetNetwork?.Dirty = true; } } = false;
        [Save] // User Name
        internal string Name { get; set { field = value; GetNetwork?.Dirty = true; } } = "";
        [Save] // User Color 0=Red, 1=Yellow, 2=Green, 3=Cyan, 4=Blue, 5=Purple, 6=White, 7=Grey
        internal byte Color { get; set { field = value; GetNetwork?.Dirty = true; } } = 6;
        [Save] // matching address
        internal string Address { get; set { field = value; GetNetwork?.Dirty = true; } } = "";
        [Save] 
        internal AddressBookRule RuleWithdraw { get; set { field = value; GetNetwork?.Dirty = true; } } = AddressBookRule.Allow;
        [Save]
        internal ulong MaxWithdrawValue { get; set { field = value; GetNetwork?.Dirty = true; } } = 0;
        [Save] 
        internal AddressBookRule RuleDeposit { get; set { field = value; GetNetwork?.Dirty = true; } } = AddressBookRule.Allow;
        [Save] 
        internal ulong MaxDepositValue { get; set { field = value; GetNetwork?.Dirty = true; } } = 0;

        internal IoNetwork? GetNetwork = null;
    }
    internal class TickerBookEntry {
        [Save] // Index on the list
        internal ushort Index { get; set { field = value; GetNetwork?.Dirty = true; } }
        [Save] // Token Unit Nickname
        internal string Unit { get; set { field = value; GetNetwork?.Dirty = true; } } = "";
        [Save] // Token Id
        internal string Id { get; set { field = value; GetNetwork?.Dirty = true; } } = "";
        [Save] // Decimal point location (how many decimal numbers after decimal point)
        internal byte Decimals { get; set { field = value; GetNetwork?.Dirty = true; } } = 0;
        [Save] // User Color 0=Red, 1=Yellow, 2=Green, 3=Cyan, 4=Blue, 5=Purple, 6=White, 7=Grey
        internal byte Color { get; set { field = value; GetNetwork?.Dirty = true; } } = 6;
        [Save] // Custom Description
        internal string Description { get; set { field = value; GetNetwork?.Dirty = true; } } = "";

        internal IoNetwork? GetNetwork = null;
    }
    internal bool Dirty = true;

    internal abstract Type WalletType();

    // TODO: Manually dirtied:
    [Save] // Custom address book
    internal Dictionary<int, AddressBookEntry> AddressBook { get; set { field = value; Dirty = true; } } = [];
    [Save] // Custom ticker book
    internal Dictionary<int, TickerBookEntry> TickerBook { get; set { field = value; Dirty = true; } } = [];

    // Autosave
    [Save] // Selected Wallet
    internal int Wallet { get; set { field = value; Dirty = true; } } = -1;
    [Save] // Selected Units
    internal byte Unit { get; set { field = value; Dirty = true; } } = 0;

    // Folders
    //[Save] // All created Wallets under this network. List of WalletId, Wallet = AppWal[WalId]
    internal Dictionary<int, IoWallet> Wallets = [];

    // carries from default constructor:
    internal string Blockchain = "";
    internal string Ticker = "";
    internal string MyCoinPrefix = "";
    internal object MyNetwork = 0;
    internal Type[] AddressTypes = [];
    internal bool Faucet = false;
    internal uint Blocks = 0;

    // RAM (filled on create, on load tx, and on add tx)
    internal Dictionary<(string, int), IoTxo> Txo = [];
    internal History? Tx = null;
    // Pointers:
    internal IoProfile? GetProfile = null;
    internal IoWallet? GetWallet => Wallets.TryGetValue(Wallet, out var w) ? w : null;
    internal IoAccount? GetAccount => GetWallet?.GetAccount;

    internal IoUnit[] Units = [new(0,"")];

    // Functions:
    internal abstract string GetNetwork();
    internal (int Wallet, AddressInfo Info)? GetAddressInfo(IoAddress Address) {
        foreach (var w in Wallets)
            foreach (var a in w.Value.Accounts)
                if (a.Value.Addresses.TryGetValue(Address.GetAddressString(), out var info))
                    return (w.Key, info);
        return null;
    }
    internal abstract bool IsValidAddress(string address);
}
