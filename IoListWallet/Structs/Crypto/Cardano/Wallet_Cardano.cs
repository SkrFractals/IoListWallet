using System;

namespace IoListWallet.Structs.Crypto.Cardano;

internal sealed class Wallet_Cardano : IoWallet {
    Signer_Cardano Signer;
    //ExtKey MasterKey;
    private static Signer_Cardano MakeSimpleSigner() {
        SignerList_Cardano snc = new () {
            RequiredSigners = 1,
            Timelock = null,
            //Signers = [new SignerParticipantCardanoWallet(this)]
        };
        return new Signer_Cardano {
            Payment = snc,
            Stake = snc,
            DRep = snc
        };
    }
    private void Init(string name, Type addressType, WalletType myType, Signer_Cardano? signer = null) {
        Signer = signer ?? MakeSimpleSigner();
        Name = name;
        MyType = myType;
        Tags = [];
        IconPath = "";
    }
    // Hot New
    internal static Wallet_Cardano GetWallet_Cardano(string name, Type addressType, int words) {
        var w = new Wallet_Cardano() { Name = name, AddressType = addressType };
        // TODO generate keys and mnemonics for the desired number of words
        w.Init(name, addressType, WalletType.Hot);
        throw new NotImplementedException();
    }
    // Hot Restore
    internal static Wallet_Cardano GetWallet_Cardano(string name, Type addressType, string[] words) {
        var w = new Wallet_Cardano() { Name = name, AddressType = addressType };
        // TODO restore from words
        w.Init(name, addressType, WalletType.Hot);
        throw new NotImplementedException();
    }
    // Multisig New
    internal static Wallet_Cardano GetWallet_Cardano(string name, Type addressType, Signer_Cardano signer) {
        var w = new Wallet_Cardano() { Name = name, AddressType = addressType };
        w.Init(name, addressType, WalletType.MultiSig, signer);
        throw new NotImplementedException();
    }
    // Readonly - can add readonly accounts - using Account PubKey, or just a single address (makes a pubkey-less single-address account)
    internal static Wallet_Cardano GetWallet_Cardano(string name, Type addressType) {
        var w = new Wallet_Cardano() { Name = name, AddressType = addressType };
        w.Init(name, addressType, WalletType.ReadOnly);
        throw new NotImplementedException();
    }
    // Import File
    internal static Wallet_Cardano GetWallet_Cardano(string filePath) {
        // TODO restore from file (any type)
        throw new NotImplementedException();
    }
    // Import QR
    internal static Wallet_Cardano GetWallet_Cardano(/*QR?*/) {
        // TODO restore from QR (any type)
        throw new NotImplementedException();
    }
    internal override IoSigner GetSigner() => Signer;
    internal override Key_Cardano DeriveKey(IoKeyPath path) {
        if (path is not KeyPathCardano keyPath)
            throw new("DeriveKey.NotCardanoKeyPath");
        throw new NotImplementedException();
    }
    internal override IoScript BuildAddress(IoKey ioKey) {
        throw new NotImplementedException();
    }
    internal override IoScript GetScript(IoPubKey pubKey) {
        throw new NotImplementedException();
    }
}