using System;
using System.Collections.Generic;
namespace IoListWallet.Structs.Crypto.Cardano;
internal sealed class Signer_Cardano : IoSigner {
    internal SignerList_Cardano Payment = new();
    internal SignerList_Cardano Stake = new();
    internal SignerList_Cardano DRep = new();
}
internal abstract class SignerNode_Cardano : IoSigner { }
internal sealed class SignerList_Cardano : SignerNode_Cardano {
    //[Range(1,_)]
    internal int RequiredSigners;
    internal DateTime? Timelock;
    internal List<SignerNode_Cardano> Signers = [];
}
internal abstract class SignerParticipant_Cardano : SignerNode_Cardano {
    // OPTIONAL
    internal string ParticipantName = "";
    internal string ParticipantDescription = "";
    internal string ParticipantIconURL = "";
}
internal sealed class SignerParticipantWallet_Cardano : SignerParticipant_Cardano {
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
internal sealed class SignerParticipantAddress_Cardano : SignerParticipant_Cardano {
    internal Address_Cardano Addr;
    internal SignerParticipantAddress_Cardano(Address_Cardano Addr) {
        this.Addr = Addr;
    }
}
internal sealed class SignerParticipantCustomKeys_Cardano : SignerParticipant_Cardano {
    internal string PaymentCredential;
    internal string StakeCredential;
    internal string DRepCredential;
    internal SignerParticipantCustomKeys_Cardano(string PaymentCredential, string StakeCredential, string DRepCredential) {
        this.PaymentCredential = PaymentCredential;
        this.StakeCredential = StakeCredential;
        this.DRepCredential = DRepCredential;
    }
}