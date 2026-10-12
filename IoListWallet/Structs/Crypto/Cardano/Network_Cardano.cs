using System;

namespace IoListWallet.Structs.Crypto.Cardano;
internal abstract class NetworkCardano : IoNetwork {
    internal NetworkCardano() : base() {
        AddressTypes = [
            typeof(Address_CardanoShelley),
            typeof(Address_CardanoScript),
            typeof(Address_CardanoHandle),
            typeof(Address_CardanoUnstoppableDomains),
            typeof(Address_CardanoByron)];
        Blockchain = "Cardano";
        Ticker = "ADA";
        Units = [new(6, "ADA"), new(3, "mADA"), new(0, "love"), new(9, "kADA"), new(12, "MADA")];
    }
    internal override string GetNetwork() => (string)MyNetwork;
    internal override bool IsValidAddress(string address) {
        return false;
        throw new System.NotImplementedException();
    }
    internal override Type WalletType() => typeof(Wallet_Cardano);
}
internal sealed class NetworkCardano_MainNet : NetworkCardano {
    public NetworkCardano_MainNet() : base() {
        Faucet = true;
        MyNetwork = "Mainnet";
    }
}
internal sealed class NetworkCardano_TestNet : NetworkCardano {
    public NetworkCardano_TestNet() : base() {
        Faucet = false;
        MyNetwork = "Testnet";
        MyCoinPrefix = "t";
    }
}