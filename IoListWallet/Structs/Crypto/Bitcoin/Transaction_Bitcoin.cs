using NBitcoin;
namespace IoListWallet.Structs.Crypto.Bitcoin;
internal sealed class Transaction_Bitcoin : IoTransaction {
    internal override void Sign(IoNetwork MyNetwork) {
        var network = MyNetwork.MyNetwork as NBitcoin.Network ?? throw new("Failed Transaction_Bitcoin.Sign.TryNetwork");
        var builder = network.CreateTransactionBuilder();
        foreach (var inputKey in Inputs) {
            var input = AppSave.TryTxo(inputKey) ?? throw new("Failed Transaction_Bitcoin.Sign.TryTxo");
            if (input.ScriptPubKey is not Script_Bitcoin spk)
                continue;
            builder.AddCoins(new Coin(uint256.Parse(input.Id.TxId), (ushort)input.Id.Vout, Money.Satoshis(input.Value), spk.Script));
            if (input.Owner >= 0 && input.KeyPath != null) {
                var k = (Key_Bitcoin?)AppSave.TryWallet(input.Owner)?.DeriveKey(input.KeyPath) 
                    ?? throw new("Failed Transaction_Bitcoin.Sign.TryWallet/DeriveKey");
                builder.AddKeys(k.Key);
            }
        }
        foreach (var output in Outputs)
            builder.Send((BitcoinAddress)output.Address.GetAddress(), Money.Satoshis(output.Value));
        var tx = builder.BuildTransaction(true);
        Witnesses = tx.ToHex();
    }
}