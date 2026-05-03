using IoListWallet.Core.Basic;

namespace IoListWallet.Core.Lists;

internal class IoEntry { }
internal class IoEntry_Buttons(WToggleButton?[] content) : IoEntry {
    internal WToggleButton?[] Content = content;
}