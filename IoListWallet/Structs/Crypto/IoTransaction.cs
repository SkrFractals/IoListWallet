using IoListWallet.MainControls.Panels;
using NBitcoin;
using System.Collections.Generic;
using static IoListWallet.AppSave;

namespace IoListWallet.Structs.Crypto;
internal enum TxState {
    Building = 0,   // Open to adding more inputs/outputs/certificates, not built yet
    Signing = 1,    // Is successfully built and taking signatures (can either be still missing them or have all of them and be ready to broadcast), changing the tx reverts it to Building state
    OnChain = 2     // Blockchain has accepted the broadcasted transaction, can no longer be edited or cancelled
}
internal abstract class IoTransaction {
    /*internal IoTransaction(Type? Network, string Id, TxState State = TxState.Building) {
        this.Network = Network;
        this.Id = Id;
        this.State = State;
    }*/
    // Not save: restore when loading panels
    //internal (Type Type, string Key)? Panel = null; // what Transation Viewer/Editor IoPanel is viewing this transaction? 
    //internal Type? Network = null;
    [Save] internal string Id { get; set { field = value; IoPanel.Dirty(); } } // TxId
    [Save] internal TxState State { get; set { field = value; IoPanel.Dirty(); } }
    [Save] internal List<(string TxId, int Vout)> Inputs { get; set { field = value; IoPanel.Dirty(); } } = []; // (txId, Vout)
    [Save] internal List<IoTxo> Outputs { get; set { field = value; IoPanel.Dirty(); } } = [];
    [Save] internal string Certificates { get; set { field = value; IoPanel.Dirty(); } } = "{}"; // Json with certificates, like Withdrawing rewards, minting/butning tokens, delegating, voting...
    [Save] internal string Witnesses { get; set { field = value; IoPanel.Dirty(); } } = ""; // are the signatures?
    [Save] internal bool IsBroadcasting { get; set { field = value; IoPanel.Dirty(); } } = false; // If true, it can still allow to be edited and re-signed and re-broadcast (showing original)
    [Save] internal string ErrorMessage { get; set { field = value; IoPanel.Dirty(); } } = ""; // If broadcasting fails, it reports here
    [Save] internal string Name { get; set { field = value; IoPanel.Dirty(); } } = ""; // Custom name for building, downloaded tx do not have a name
    [Save] internal List<string> Signatures { get; set { field = value; IoPanel.Dirty(); } } = []; // signatures of the transaction by all required parties
    internal abstract void Sign(IoNetwork MyNetwork);
    // TODO: let the viewer panel do this, also tx not on chain yet are not in Network.Tx yet
    /*
    internal void Delete() {
        Panel?.Delete(); // Close and remove my Transaction viewer panel (there could be a Delete button in the panel that calls this)
        if (State == TxState.OnChain || AppSave.TryNetwork(Network) is not IoNetwork network)
            return; // Cannot delete the actual tx data below if it's on chain
        network.Tx.Tx.Remove(Id);
        network.Tx.Dirty = true;
        for (int i = Inputs.Count; 0 <= --i; RemoveInput(Inputs[i])) ;
        for (int i = Outputs.Count; 0 <= --i; RemoveOutput(Outputs[i])) ;
    }*/
    internal string AddInput(IoTxo i) {
        if (State == TxState.OnChain)
            return "OnChain"; // Cannot delete the below if it's on chain
        if (i.Spent())
            return "AlredySpent"; // cannot double spend an input that is alraedy spent on chain
        IoPanel.Dirty(); // changed transaction build - that is saved in Panels file
        Inputs.Add(i.Id);
        i.InputIn.Add(Id);
        return "";
    }
    internal string RemoveInput((string TxId, int Vout) i) {
        if (State == TxState.OnChain)
            return "OnChain"; // Cannot delete the below if it's on chain        
        if (TryTransaction(i.TxId) is not IoTransaction t)
            return "NoTxFound";
        IoPanel.Dirty(); // changed transaction build - that is saved in Panels file
        Inputs.Remove(i);
        t?.Outputs[i.Vout].InputIn.Remove(Id);
        return "";
    }
    internal string RemoveOutput(IoTxo output) {
        if (Outputs[output.Id.Vout] != output)
            return "NotMine";
        if (State == TxState.OnChain)
            return "OnChain"; // Cannot delete the below if it's on chain
        IoPanel.Dirty(); // changed transaction build - that is saved in Panels file
        output.Delete();
        // remove from the Vout list, adn shake down
        List<IoTxo> newOut = [];
        for (int i = 0; i < output.Id.Vout; ++i)
            newOut.Add(Outputs[i]);
        for (int i = output.Id.Vout; ++i < Outputs.Count; newOut.Add(Outputs[i]))
            --Outputs[i].Id.Vout;
        Outputs = newOut;
        return "";
    }
}