using System;
using System.Collections.Generic;
using static IoListWallet.AppSave;

namespace IoListWallet.Structs.Crypto;

internal class IoApp {
    internal bool Dirty = true;

    #region APP - Profile:
    [Save] // Name of this profile
    internal int Profile { get; set { field = value; Dirty = true; } } = -1;
    #endregion

    #region PRO
    //[Save] // Selected Network KeyId (like Bitcoin testnet)
    internal Dictionary<int, IoProfile> Profiles = [];
    #endregion

    internal IoProfile? GetProfile => Profiles.TryGetValue(Profile, out var p) ? p : null;
    internal IoNetwork? GetNetwork => GetProfile?.GetNetwork;
    internal IoWallet? GetWallet => GetNetwork?.GetWallet;
    internal IoAccount? GetAccount => GetWallet?.GetAccount;
}