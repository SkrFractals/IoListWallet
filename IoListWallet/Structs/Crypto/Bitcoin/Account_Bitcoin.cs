using IoListWallet.Core.Static;
using System;
namespace IoListWallet.Structs.Crypto.Bitcoin;
internal sealed class Account_Bitcoin : IoAccount {
    //internal Account_Bitcoin() { }
    internal override Address_Bitcoin GenerateAddress(Type AddressType) {
        var keyPath = GetNextExternalPath();
        var wallet = GetWallet ?? throw new("Failed Account_Bitcoin.GenerateAddress.GetWallet");
        Key_Bitcoin key = wallet.DeriveKey(keyPath) as Key_Bitcoin
            ?? throw new("Failed AccountBitcoin.GenerateAddress.Key");
        var BNetwork = AppSave.TryNetwork(wallet.MyNetwork)?.MyNetwork as NBitcoin.Network
            ?? throw new("Failed AccountBitcoin.GenerateAddress.Network");
        var script = wallet.GetScript(new PubKey_Bitcoin(key.Key.PubKey)) as Script_Bitcoin
            ?? throw new("Failed AccountBitcoin.GenerateAddress.Script");
        var destAddr = script.Script.GetDestinationAddress(BNetwork)
            ?? throw new("Failed AccountBitcoin.GenerateAddress.DestinationAddress");
        var wrappedAddr = (Address_Bitcoin)Activator.CreateInstance(AddressType, destAddr)!
            ?? throw new("Failed AccountBitcoin.GenerateAddress.Address_Bitcoin");
        Addresses[wrappedAddr.GetAddressString()] = new AddressInfo_Bitcoin(wrappedAddr, keyPath, script);
        return wrappedAddr;
    }
    // TODO: this seems to be onyl external segwit, i need it to be universal
    internal KeyPath_Bitcoin GetNextExternalPath() {
        var path = new KeyPath_Bitcoin([
        84 | 0x80000000,
        0 | 0x80000000,
        0 | 0x80000000,
        0,
        (ushort)ExternalIndex++
    ]);
        return path;
    }
}