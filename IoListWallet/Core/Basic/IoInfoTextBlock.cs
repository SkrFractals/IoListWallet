using Avalonia.Controls;
using System;

namespace IoListWallet.Core.Basic;
internal class IoInfoTextBlock : IoTextBlock {
    protected override TextBlock MyType() => new() { Classes = { "info" } };
    protected override TextBlock MyTypeS(int i) => new() { Classes = { "info", "shadow"} };
    protected override Type StyleKeyOverride => typeof(TextBlock);
 
}