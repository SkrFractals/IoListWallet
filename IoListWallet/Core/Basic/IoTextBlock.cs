using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace IoListWallet.Core.Basic;
internal class IoTextBlock : Panel {
    protected virtual TextBlock MyType() => Main = new TextBlockControl() { Classes = { "control" } };
    protected virtual TextBlock MyTypeS(int i) => Shade = new TextBlockControl() { Classes = { "control", "shadow" } };
    internal string Text { get; set => Main.Text = Shade.Text /*= Shade2.Text = Shade3.Text = Shade4.Text*/ = field = value; } = "";
    internal FontWeight FontWeight { get; set => Main.FontWeight = Shade.FontWeight /*= Shade2.FontWeight = Shade3.FontWeight = Shade4.FontWeight*/ = field = value; } = FontWeight.Normal;
    internal InlineCollection? Inlines { get; set => Main.Inlines = field = value; }
    internal IBrush? Foreground { get; set => Main.Foreground = field = value; }
    internal TextBlock Main, Shade, Shade2, Shade3, Shade4;

    internal TextAlignment TextAlignment { get; set => Main.TextAlignment = Shade.TextAlignment /*= Shade2.TextAlignment = Shade3.TextAlignment = Shade4.TextAlignment*/ = field = value; }
    new internal VerticalAlignment VerticalAlignment { get; set => base.VerticalAlignment = Main.VerticalAlignment = Shade.VerticalAlignment /*= Shade2.VerticalAlignment = Shade3.VerticalAlignment = Shade4.VerticalAlignment*/ = field = value; }

    protected override Type StyleKeyOverride => typeof(TextBlock);

    public IoTextBlock() : base() {

        Children.Add(Shade = MyTypeS(1));
        /*Children.Add(Shade2 = MyTypeS(2));
        Children.Add(Shade3 = MyTypeS(3));
        Children.Add(Shade4 = MyTypeS(4));*/
        Children.Add(Main = MyType());
        Shade.RenderTransform = new TranslateTransform(-1, -1);
        /*Shade2.RenderTransform = new TranslateTransform(-0.5, -0.5);
        Shade3.RenderTransform = new TranslateTransform(0.5, -0.5);
        Shade4.RenderTransform = new TranslateTransform(-0.5, 0.5);*/
        /*Shade4.Foreground = Shade3.Foreground = Shade2.Foreground = */Shade.Foreground = Brushes.Black;

        /*Shade.Bind(IsVisibleProperty,
            ShadowSettings.TextShadowRadius.Select(r => r >= 1));

        Shade2.Bind(IsVisibleProperty,
            ShadowSettings.TextShadowRadius.Select(r => r >= 2));

        Shade3.Bind(IsVisibleProperty,
            ShadowSettings.TextShadowRadius.Select(r => r >= 3));

        Shade4.Bind(IsVisibleProperty,
            ShadowSettings.TextShadowRadius.Select(r => r >= 4));*/


    }
}