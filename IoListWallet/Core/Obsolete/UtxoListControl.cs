using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using IoListWallet.Structs.Crypto;
using System.Collections.Generic;

namespace IoListWallet.Core.Obsolete;

/*internal class UtxoListControl : UserControl
{
    internal UtxoListControl(List<IoTxo> list)
    {
        // Scrollable vertical list
        var stack = new StackPanel { Spacing = 8 };
        foreach (IoTxo utxo in list)
        {
            stack.Children.Add(new UtxoControl(utxo));
        }
        Content = new ScrollViewer
        {
            Content = stack,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
    }
}*/