using System;
namespace IoListWallet.Structs.Crypto.Cardano;
internal sealed class Transaction_Cardano : IoTransaction {
    internal override void Sign(IoNetwork network) {
        throw new NotImplementedException();
    }
}