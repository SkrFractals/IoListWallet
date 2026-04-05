namespace IoListWallet.Structs.Crypto.Cardano;
internal class AddressInfo_Cardano(IoAddress A) : AddressInfo(A) { }
internal abstract class Address_Cardano : IoAddress {
    string Addr;
    internal Address_Cardano(string addr) => Addr = addr;
    internal override string GetAddressString() => Addr;
    internal override object GetAddress() => Addr;
}
internal sealed class Address_CardanoByron(string addr) : Address_Cardano(addr) {
    // Old Byron address
}
internal sealed class Address_CardanoShelley(string addr) : Address_Cardano(addr) {
    // Normal address
}
internal sealed class Address_CardanoScript(string addr) : Address_Cardano(addr) {
    // Script
}
internal sealed class Address_CardanoHandle(string addr) : Address_Cardano(addr) {
    // Ada handle
}
internal sealed class Address_CardanoUnstoppableDomains(string addr) : Address_Cardano(addr) {
    // What is this?
}