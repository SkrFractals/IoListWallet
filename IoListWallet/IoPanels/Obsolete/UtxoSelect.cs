using Avalonia.Controls;
using Avalonia.Layout;
using IoListWallet.Structs;
using System.Collections.Generic;

namespace IoListWallet.IoPanels.Obsolete;
/*internal class UtxoSelectPanel : IoPanel
{
    internal UtxoSelectPanel()
    {

        // Example UTXOs
        List<Utxo> utxos = [
        new Utxo
            {
                Address = "bc1qaddress1",
                Value = 123,
                Tokens = [new TokenAttachment{ Id="T1", Value=10 }]
            },
            new Utxo
            {
                Address = "bc1qaddress2",
                Value = 456
            }
        ];

        // Create your list control
        var utxoListControl = new UtxoListControl(utxos);

        // Root layout (optional toolbar, etc.)
        var root = new DockPanel();

        // If you want a toolbar at top:
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4
        };
        toolbar.Children.Add(new Button { Content = "Refresh" });
        toolbar.Children.Add(new Button { Content = "Settings" });
        DockPanel.SetDock(toolbar, Dock.Top);

        root.Children.Add(toolbar);
        root.Children.Add(utxoListControl); // list fills remaining space

        Content = root;
    }
}*/