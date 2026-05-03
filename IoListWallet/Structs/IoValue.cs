using System;
using System.Collections.Generic;

namespace IoListWallet.Structs;

public class IoValue {
    public ulong Value;
    // Are all item values empty?
    public virtual bool IsEmpty() => Value <= 0;
    /// <summary>
    /// How well does B fill my values? each item filled adds a fill ratio (0-1), and any extra gets added to overfill (max 1 per item)
    /// You want to find utxos that fill as much as possible, but without too much overfill, to get good automatic inputs
    /// </summary>
    /// <param name="B">What utxo are we texting to fill this value?</param>
    /// <returns>(successfull fill ratio, extra overfill)</returns>
    public virtual (double, double) Fills(IoValue B) {
        double overfill = (double)Value / B.Value, fill = Math.Min(1.0, overfill); if ((overfill -= fill) > 1) overfill = 1;
        return (fill, overfill);
    }
    /// <summary>
    /// Subtracts B from this IoValue (stop anythign at zero, doens't overflow into negatives)
    /// </summary>
    /// <param name="B">What to subtract from me</param>
    /// <returns>true if nothing remains in my values</returns>
    public virtual bool Subtract(IoValue B) {
        if (Value <= B.Value) {
            Value = 0; return true;
        }
        Value -= B.Value;
        return false;
    }
    /// <summary>
    /// Adds B to this IoValue
    /// </summary>
    /// <param name="B">what value to add</param>
    public virtual void Add(IoValue B) => Value += B.Value;
}

public class IoValueTokens : IoValue {
    public Dictionary<string, ulong> Tokens = [];
    public override bool IsEmpty() => base.IsEmpty() && Tokens.Count <= 0;
    public override (double, double) Fills(IoValue B) {
        if (B is not IoValueTokens BT)
            return (0, 0);
        var (fill, overfill) = base.Fills(B);
        foreach (var t in Tokens) {
            if (BT.Tokens.TryGetValue(t.Key, out var ft)) {
                double d = (double)t.Value / ft, 
                    f = Math.Min(1.0, d);
                fill += f; 
                overfill += Math.Min(1.0, f - 1);
            }
        }
        return (fill, overfill);
    }
    public override bool Subtract(IoValue B) {
        if (B is not IoValueTokens BT)
            return false;
        var remaining = Tokens.Count; // how many token keys remaining?
        foreach (var t in BT.Tokens) {
            var found = Tokens.TryGetValue(t.Key, out var ft);
            if (found) {
                if (ft <= t.Value) {
                    --remaining; // Completely subtracted this token to nothing:
                    Tokens.Remove(t.Key);
                } else Tokens[t.Key] = ft - t.Value; // partial subtract
            }
        }
        return base.Subtract(B) && remaining <= 0;
    }
    public override void Add(IoValue B) {
        if (B is not IoValueTokens BT)
            return;
        base.Add(B);
        foreach (var t in BT.Tokens)
            Tokens[t.Key] = Tokens.TryGetValue(t.Key, out var ft) ? ft + t.Value : t.Value;
    }
}