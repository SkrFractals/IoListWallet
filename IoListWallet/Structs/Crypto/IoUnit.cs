using static IoListWallet.Core.Static.AppSave;

namespace IoListWallet.Structs.Crypto;

internal class IoUnit(byte decimals = 0, string unit = "") {
    internal string U = unit;
    internal byte D = decimals;
    internal string ToString(ulong num, bool x = false) { // BTC decimal 8, mBTC decimal 5, ADA decimal 6...
        if (GetProfile is not IoProfile profile)
            return "";
        byte i = 0, s = (byte)(D % 3); if (s == 0) s = 3;
        string r = "";
        while (num > 0 || i <= D) {
            r = (x ? ++i <= D ? "x" : "X" : (num % 10).ToString()) + r;
            num /= 10;
            if (0 < --s) continue;
            r = (i == D ? profile.Decimal : i > D ? profile.Thousand : profile.Thousanth) + r;
            s = 3;
        }
        return U == "" ? r : r + " " + U;
    }
}
