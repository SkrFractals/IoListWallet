using System;
using NBitcoin;
namespace IoListWallet.Structs.Crypto.Bitcoin;
internal sealed class Wallet_Bitcoin : IoWallet {
    ExtKey? MasterKey = null; // BIP23 root
    private void Init(string name, Type addressType, WalletType myType) {
        Name = name;
        MyType = myType;
        Tags = [];
        IconPath = "";
    }
    // Hot New
    internal static Wallet_Bitcoin GetWallet_Bitcoin(string name, Type addressType, int words) {
        var w = new Wallet_Bitcoin() { Name = name, AddressType = addressType };
        // TODO generate keys and mnemonics for the desired number of words
        w.Init(name, addressType, WalletType.Hot);
        throw new NotImplementedException();
    }
    // Hot Restore
    internal static Wallet_Bitcoin GetWallet_Bitcoin(string name, Type addressType, string[] words) {
        var w = new Wallet_Bitcoin() { Name = name, AddressType = addressType };
        // TODO restore from words
        w.Init(name, addressType, WalletType.Hot);
        throw new NotImplementedException();
    }
    // Multisig New
    /*internal WalletBitcoin(string Name, SignerCardano Signer) {
        Init(Name, WalletType.MultiSig, Signer);
        throw new System.NotImplementedException();
    }*/
    // Readonly - can add readonly accounts - using Account PubKey, or just a single address (makes a pubkey-less single-address account)
    internal static Wallet_Bitcoin GetWallet_Bitcoin(string name, Type addressType) {
        var w = new Wallet_Bitcoin() { Name = name, AddressType = addressType };
        w.Init(name, addressType, WalletType.ReadOnly);
        throw new NotImplementedException();
    }
    // Import File
    internal static Wallet_Bitcoin GetWallet_Bitcoin(string filePath) {
        var w = new Wallet_Bitcoin();
        // TODO restore from file (any type)
        throw new NotImplementedException();
    }
    // Import QR
    internal static Wallet_Bitcoin GetWallet_Bitcoin(/*QR?*/) {
        var w = new Wallet_Bitcoin();
        // TODO restore from QR (any type)
        throw new NotImplementedException();
    }
    internal override Key_Bitcoin DeriveKey(IoKeyPath path) {
        if (path is not KeyPath_Bitcoin keyPath)
            throw new("DeriveKey.NotBitcoinKeyPath");
        if (MasterKey == null)
            throw new("DeriveKey.DoesNotContainMasterKey");
        ExtKey current = MasterKey;
        foreach (var index in keyPath.KeyPath.Indexes)
            current = current.Derive(index);
        return new Key_Bitcoin(current.PrivateKey);
    }
    internal override IoSigner GetSigner() {
        throw new NotImplementedException();
    }
    //  wrapped calls:
    internal override Script_Bitcoin GetScript(IoPubKey ioPubKey) {
        var pubKey = ioPubKey.GetPubKey() as PubKey ?? throw new("ScriptBitcoin.GetScript.PubKey");
        return new Script_Bitcoin(GetScript(pubKey));
    }
    internal override Script_Bitcoin BuildAddress(IoKey ioKey) {
        //var signer = GetSigner();
        //var key = DeriveKey(new KeyPathBitcoin("m/84'/0'/0'/0/0")); // TODO i might want to customize those zeroes!
        return new Script_Bitcoin(BuildScript((Key)ioKey.GetKey()));
    }

    // unwrapped pure calls:
    internal Script GetScript(PubKey pubKey) {
        return AddressType.Name switch {
            nameof(Address_BitcoinLegacy) => pubKey.Hash.ScriptPubKey,
            nameof(Address_BitcoinSegwit) => pubKey.WitHash.ScriptPubKey,
            nameof(Address_BitcoinTaproot) => pubKey.GetTaprootFullPubKey().ScriptPubKey,
            _ => throw new("Adress type not found in bitcoin: " + AddressType.Name)
        };
    }
    internal Script BuildScript(Key key) {
        //var signer = GetSigner();
        return GetScript(key.PubKey);
    }
}