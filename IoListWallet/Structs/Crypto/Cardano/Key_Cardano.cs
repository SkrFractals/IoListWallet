namespace IoListWallet.Structs.Crypto.Cardano;

// TODO implement proper
internal sealed class Key_Cardano : IoKey {
    string Key;
    internal Key_Cardano(string key) => Key = key;
    internal override string GetKey() => Key;
}
internal sealed class ExtKey_Cardano : IoExtKey {
    internal string ExtKey;
    internal override string GetExtKey() => ExtKey;
    internal ExtKey_Cardano(string key) => ExtKey = key;
}
internal sealed class KeyPathCardano : IoKeyPath {
    string KeyPath = ""; 
    internal override string GetKeyPath() => KeyPath;
}
internal sealed class Script_Cardano : IoScript {
    internal string Script;
    internal override string GetScript() => Script;
    internal Script_Cardano(string script) => Script = new(script);
}
internal sealed class PubKey_Cardano : IoPubKey {
    internal string PubKey;
    internal override string GetPubKey() => PubKey;
    internal PubKey_Cardano(string hex) => PubKey = hex;
}