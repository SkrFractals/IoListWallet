using NBitcoin;
namespace IoListWallet.Structs.Crypto.Bitcoin;
internal sealed class Key_Bitcoin : IoKey {
    internal Key Key;
    internal Key_Bitcoin(Key key) => Key = key;
    internal override Key GetKey() => Key;
}
internal sealed class ExtKey_Bitcoin : IoExtKey {
    internal ExtKey ExtKey;
    internal override ExtKey GetExtKey() => ExtKey;
    internal ExtKey_Bitcoin(byte[] seed) => ExtKey = ExtKey.CreateFromSeed(seed);
    internal ExtKey_Bitcoin(ExtKey key) => ExtKey = key;
}
internal sealed class KeyPath_Bitcoin : IoKeyPath {
    internal KeyPath KeyPath;
    internal override KeyPath GetKeyPath() => KeyPath;
    internal KeyPath_Bitcoin(string path) => KeyPath = new(path);
    internal KeyPath_Bitcoin(uint[] indices) => KeyPath = new(indices);
    internal KeyPath_Bitcoin(KeyPath keyPath) => KeyPath = keyPath;
}
internal sealed class Script_Bitcoin : IoScript {
    internal Script Script;
    internal override Script GetScript() => Script;
    internal Script_Bitcoin(string script) => Script = new(script);
    internal Script_Bitcoin(Script script) => Script = script;
}
internal sealed class PubKey_Bitcoin : IoPubKey {
    internal PubKey PubKey;
    internal override PubKey GetPubKey() => PubKey;
    internal PubKey_Bitcoin(string hex) => PubKey = new(hex);
    internal PubKey_Bitcoin(PubKey pubKey) => PubKey = pubKey;
}