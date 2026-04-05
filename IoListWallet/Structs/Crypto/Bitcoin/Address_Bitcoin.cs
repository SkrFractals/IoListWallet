using NBitcoin;
namespace IoListWallet.Structs.Crypto.Bitcoin;
internal abstract class Address_Bitcoin : IoAddress {
    BitcoinAddress Addr;
    internal Address_Bitcoin(BitcoinAddress addr) => Addr = addr;
    internal override string GetAddressString() => Addr.ToString();
    internal override BitcoinAddress GetAddress() => Addr;
}
internal class AddressInfo_Bitcoin(IoAddress A, IoKeyPath P, IoScript S) : AddressInfo(A) {
    internal IoKeyPath KeyPath = P;
    internal IoScript Script = S;
}
internal sealed class Address_BitcoinLegacy(BitcoinAddress addr) : Address_Bitcoin(addr) { }
internal sealed class Address_BitcoinSegwit(BitcoinAddress addr) : Address_Bitcoin(addr) { }
internal sealed class Address_BitcoinTaproot(BitcoinAddress addr) : Address_Bitcoin(addr) { }
