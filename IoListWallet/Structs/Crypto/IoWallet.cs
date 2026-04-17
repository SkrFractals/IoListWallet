// P2WPKH: var script = DeriveKey().Key.pubKey.WitHash.ScriptPubKey;
// P2PKH: var script = DeriveKey().Key.pubKey.Hash.ScriptPubKey;
// Multisig: Script BuildMultisig(int required, PubKey[] pubKeys) => PayToMultiSigTemplate.Instance.GenerateScriptPubKey(required, pubKeys);
// P2WSH: var script = multisigScript.WitHash.ScriptPubKey;

using System;
using System.Collections.Generic;
using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Structs.Crypto;

internal abstract class IoWallet {
    internal enum WalletType {
        Hot,
        HW,
        MultiSig,
        CLI,
        ReadOnly
    }
    internal bool Dirty = true;

    // TODO: Manually dirtied:
    [Save]  // User Tags
    internal List<string> Tags { get; set { field = value; Dirty = true; } }
    [Save] // Created Account Indices
    internal Dictionary<int, IoAccount> Accounts { get; set { field = value; Dirty = true; } } = [];

    // Autosave
    [Save] // Network type (when importing, check if it matches the network!)
    internal Type NetType { get; set { field = value; Dirty = true; } }
    [Save] // User name
    internal string Name { get; set { field = value; Dirty = true; } }
    [Save] // User Icon file path
    internal string IconPath { get; set { field = value; Dirty = true; } }  
    [Save] // NetworkKeyId: App.AllNetworks[MyNetwork]
    internal Type MyNetwork { get; set { field = value; Dirty = true; } }  
    [Save]  // HOT / HW / ReadOnly / etc
    internal WalletType MyType { get; set { field = value; Dirty = true; } }    
    [Save] // Legacy/Segwit... Byron/Shelley....
    internal Type AddressType { get; set { field = value; Dirty = true; } }
    [Save]  // Seed
    internal byte[] Seed { get; set { field = value; Dirty = true; } }
    [Save]  // Selected Account in the Selected Wallet
    internal int Account { get; set { field = value; Dirty = true; } } = -1;

    // Pointers:
    internal IoProfile? GetProfile => GetNetwork?.GetProfile;
    internal IoNetwork? GetNetwork = null;
    internal IoAccount? GetAccount => Accounts.TryGetValue(Account, out var a) ? a : null;

internal abstract IoSigner GetSigner();
    internal abstract IoKey DeriveKey(IoKeyPath ioPath);
    internal abstract IoScript BuildAddress(IoKey ioKey);
    internal abstract IoScript GetScript(IoPubKey ioPubKey);
}