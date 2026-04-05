internal abstract class IoAddress {
    internal abstract string GetAddressString();
    internal abstract object GetAddress();
}
internal abstract class AddressInfo(IoAddress A) {
    internal IoAddress Address = A;
}