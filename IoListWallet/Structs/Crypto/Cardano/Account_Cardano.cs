using System;
namespace IoListWallet.Structs.Crypto.Cardano;

internal sealed class Account_Cardano : IoAccount {
    internal override Address_Cardano GenerateAddress(Type AddressType) {
        //var keyPath = new KeyPathCardano(GetNextExternalPath()); // TODO implement GetNextExternalPath
        //KeyCardano key = Wallet.DeriveKey(keyPath) as KeyCardano ?? throw new("Failed AccountBitcoin.GenerateAddress.Key");
        throw new NotImplementedException();
        //var BNetwork = Wallet.MyNetwork.MyNetwork as Network ?? throw new("Failed AccountBitcoin.GenerateAddress.Network");
        //return key.Key.PubKey.WitHash.ScriptPubKey.GetDestinationAddress(BNetwork) ?? throw new("Failed AccountBitcoin.GenerateAddress.Address");
        //return (Address_Cardano)Activator.CreateInstance(AddressType, a)!;
    }
}