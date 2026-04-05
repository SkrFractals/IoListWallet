using IoListWallet.Structs.Crypto.Bitcoin;
using System;
using System.Collections.Generic;
internal sealed class Signer_Bitcoin : IoSigner {
    internal SignerList_Bitcoin Payment = new();
}
internal abstract class SignerNode_Bitcoin : IoSigner {
}
internal sealed class SignerList_Bitcoin : SignerNode_Bitcoin {
    //[Range(1,_)]
    internal int RequiredSigners = 1;
    internal DateTime? Timelock = DateTime.Now;
    internal List<IoSigner> Signers = [];
}
internal abstract class SignerParticipant_Bitcoin : SignerNode_Bitcoin {
    // OPTIONAL
    internal string ParticipantName = "";
    internal string ParticipantDescription = "";
    internal string ParticipantIconURL = "";
}
internal sealed class SignerParticipantBitcoinWallet : SignerParticipant_Bitcoin {
    /*PubKey PK;
    Wallet? Wallet;
    internal SignerParticipantCardanoWallet(PubKey PK) {
        this.PK = PK;
        Wallet = null;
    }
    internal SignerParticipantCardanoWallet(Wallet W) {
        Wallet = W;
        PK = W.PubKey;
    }*/
}
internal sealed class SignerParticipantAddress_Bitcoin : SignerParticipant_Bitcoin {
    internal Address_Bitcoin Addr;
    internal SignerParticipantAddress_Bitcoin(Address_Bitcoin Addr) {
        this.Addr = Addr;
    }
}
internal sealed class SignerParticipantCustomKeys_Bitcoin : SignerParticipant_Bitcoin {
    internal string PaymentCredential;
    internal SignerParticipantCustomKeys_Bitcoin(string PaymentCredential) {
        this.PaymentCredential = PaymentCredential;
    }
}