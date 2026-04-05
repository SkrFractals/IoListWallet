namespace IoListWallet.Structs.Crypto.Bitcoin;
internal class Txo_Bitcoin((string TxId, int Vout) txoId, IoAddress address) : IoTxo(txoId, address) {
    internal override void InitInfo(AddressInfo info) {
        var infoBitcoin = info as AddressInfo_Bitcoin ?? throw new("Failed Get AddressInfo_Bitcoin");
        KeyPath = infoBitcoin.KeyPath;
        ScriptPubKey = infoBitcoin.Script;
    }
}