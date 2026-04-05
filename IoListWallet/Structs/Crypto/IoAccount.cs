using System;
using System.Collections.Generic;
using static IoListWallet.AppSave;
namespace IoListWallet.Structs.Crypto;
internal abstract class IoAccount {
    [Save] 
    internal string Name { get; set { field = value; GetWallet?.Dirty = true; } }
    [Save]
    internal bool SingleAddr { get; set { field = value; GetWallet?.Dirty = true; } }
    [Save]
    internal int ExternalIndex { get; set { field = value; GetWallet?.Dirty = true; } } = 0;
    [Save]
    internal int InternalIndex { get; set { field = value; GetWallet?.Dirty = true; } } = 0;
    
    // RAM (filled on create, on load tx, and on add tx)
    internal Dictionary<string, AddressInfo> Addresses = [];// Known addresses of this account
    internal Dictionary<(string TxId, int Vout), IoTxo> Utxos = [];        // Available utxos that are spendable

    internal IoProfile? GetProfile => GetWallet?.GetProfile;
    internal IoNetwork? GetNetwork => GetWallet?.GetNetwork;
    internal IoWallet? GetWallet = null;

    internal abstract IoAddress GenerateAddress(Type AdressType);



}