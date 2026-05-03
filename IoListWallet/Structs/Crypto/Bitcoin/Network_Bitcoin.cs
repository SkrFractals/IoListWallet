using NBitcoin;
using System;

namespace IoListWallet.Structs.Crypto.Bitcoin;
internal abstract class NetworkBitcoin : IoNetwork {
    internal NetworkBitcoin() : base() {
        AddressTypes = [typeof(Address_BitcoinLegacy), typeof(Address_BitcoinSegwit), typeof(Address_BitcoinTaproot)];
        Blockchain = "Bitcoin";
        Ticker = "BTC";
        Units = [new(8, "BTC"), new(5, "BTC"), new(2, "μBTC"), new(0, "sat"), new(11, "kBTC")];
    }
    internal override string GetNetwork() => (MyNetwork as NBitcoin.Network)?.Name ?? "";
    internal override bool IsValidAddress(string address) {
        var N = MyNetwork as NBitcoin.Network ?? throw new("IsValidAddress.NotValidNetwork");
        try {
            BitcoinAddress.Create(address, N);
            return true;
        } catch { }
        return false;
    }
    internal override Type WalletType() => typeof(Wallet_Bitcoin);
}
internal sealed class NetworkBitcoin_MainNet : NetworkBitcoin {
    public NetworkBitcoin_MainNet() : base() {
        Faucet = true;
        MyNetwork = NBitcoin.Network.Main;
    }
}
internal sealed class NetworkBitcoin_TestNet : NetworkBitcoin {
    public NetworkBitcoin_TestNet() : base() {
        Faucet = false;
        MyNetwork = NBitcoin.Network.TestNet;
        MyCoinPrefix = "t";
    }
}
internal sealed class NetworkBitcoin_RegTest : NetworkBitcoin {
    public NetworkBitcoin_RegTest() : base() {
        Faucet = false;
        MyNetwork = NBitcoin.Network.RegTest;
        MyCoinPrefix = "t";
    }
}