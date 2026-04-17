using IoListWallet.Core.Static;
using System.Collections.Generic;
namespace IoListWallet.Structs.Crypto;
internal abstract class IoTxo {
    // all user's txo should be generated this way, single we alraedy should also load all the user's tranasctions so we could also browse that history
    internal IoTxo((string outIn, int vout) txoId, IoAddress address) {
        var outputIn = AppSave.TryTransaction(txoId.outIn) ?? throw new("Txos without transactions not allowed");
        Network = AppSave.GetNetwork ?? throw new("Failed IoTxo.GetNetwork");
        Address = address; // what address does it sit in
        Network.Txo[Id = txoId] = this;
        outputIn.Outputs.Add(this);
        if(Network.GetAddressInfo(address) is (int, AddressInfo) info) {
            Owner = info.Wallet;
            InitInfo(info.Info);
        }
    }
    internal abstract void InitInfo(AddressInfo info);
    //internal void SetId() => Id = $"{TxId}:{Vout}";
    internal IoScript? ScriptPubKey = null;
    //internal string Network;
    internal int Owner = -1; // Private owner
    internal IoNetwork Network;
    internal IoKeyPath? KeyPath;
    internal IoAddress Address; // what address does it sit in
    internal (string TxId, int Vout) Id; // TxId of transaction this is an output of
    internal List<string> InputIn = []; // TxIds, multiple transaction can try to spend an Utxo, but only one can be confirmed
    internal bool Spent() {
        foreach (var i in InputIn) 
            if ((AppSave.TryTransaction(i)?.State ?? TxState.Building) == TxState.OnChain)
                return true;
        return false;
    }
    internal ulong Value = 0; // value of main coins (sats/locvelaces/etc...)
    internal List<IoTokenAttachment> Tokens = []; // attached tokens if possible
    internal IoTransaction? Script = null; // Pointer to a transaction with uploaded script used for this output. Null defaults to regular spending script 

    internal void Delete() {
        var tx = AppSave.TryTransaction(Id.TxId);
        tx?.Outputs.Remove(this);
        Network.Txo.Remove(Id);
    }
}