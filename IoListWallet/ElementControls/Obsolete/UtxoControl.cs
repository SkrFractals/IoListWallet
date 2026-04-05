using Avalonia.Controls;
using Avalonia.Layout;
using IoListWallet.Structs.Crypto;

namespace IoListWallet.ElementControls.Obsolete;

internal class UtxoControl : UserControl
{
    internal UtxoControl(IoTxo utxo)
    {
        // Root vertical layout
        var root = new StackPanel
        {
            Spacing = 4
        };

        // Top row: Address left, Value right
        var topRow = new DockPanel();
        var valueText = new TextBlock { Text = utxo.Value.ToString() };
        DockPanel.SetDock(valueText, Dock.Right);

        var addressText = new TextBlock
        {
            Text = utxo.Address.GetAddressString(),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };

        topRow.Children.Add(valueText);
        topRow.Children.Add(addressText);

        // Bottom row: buttons left, token list right
        var bottomRow = new DockPanel();

        // Buttons panel
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4
        };
        var copyBtn = new Button { Content = "Copy" };
        var spendBtn = new Button { Content = "Spend" };
        buttons.Children.Add(copyBtn);
        buttons.Children.Add(spendBtn);

        // Token badges panel
        /*var tokensPanel = new WrapPanel { Spacing = 4 };
        foreach (var t in utxo.Tokens)
        {
            tokensPanel.Children.Add(new TextBlock { Text = $"{t.Id}({t.Value})" });
        }*/

        string tokens = "";
        foreach (var token in utxo.Tokens) {
            tokens = tokens + token.Value + " " + token.Id + "; ";
        }

        var tokensPanel = new TextBlock
        {
            Text = tokens,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        DockPanel.SetDock(tokensPanel, Dock.Right);

        bottomRow.Children.Add(tokensPanel);
        bottomRow.Children.Add(buttons);

        // Add rows to root
        root.Children.Add(topRow);
        root.Children.Add(bottomRow);

        Content = root;
    }
}