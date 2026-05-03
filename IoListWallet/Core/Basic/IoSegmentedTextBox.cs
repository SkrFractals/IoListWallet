using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using System;

// TODO display a white bar inbetween parts when the scroll doesn't touch that side

namespace IoListWallet.Core.Basic;
internal class IoSegmentedTextBox : UserControl {
    internal int MinFirst { get; set; } = 0;
    internal int MinLast { get; set; } = 0;

    internal string Text { get; set { field = value; Refresh(); } }

    private readonly TextBlock first = new() { Classes = { "info" } };
    private readonly TextBlock middle = new() { Classes = { "info" } };
    private readonly TextBlock last = new() { Classes = { "info" } };
    private readonly ScrollViewer scroll;
    private readonly Grid grid = new();

    internal IoSegmentedTextBox() {
        scroll = new ScrollViewer {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = middle
        };
        Refresh();
    }
    private void Refresh() {
        grid.Children.Clear();
        grid.ColumnDefinitions.Clear();

        var t = Text ?? "";
        if (t.Length <= MinFirst + MinLast) {
            first.Text = last.Text = "";
            middle.Text = t;
            return;
        }
        first.Text = t[..MinFirst];
        middle.Text = t.Substring(MinFirst, t.Length - MinFirst - MinLast);
        last.Text = t[^MinLast..];
        int col = 0, ranChars = 0;
        void ApplyColorPattern(TextBlock block, string text) {
            if (block.Inlines == null)
                return;
            block.Inlines.Clear();
            for (int i = 0; i < text.Length; i += 8) {
                var e = Math.Min(i + (8 - (ranChars % 8)), text.Length);
                var run = new Run(t = text[i..e]) {
                    Foreground = (ranChars / 8) % 2 == 0
                        ? Brushes.White
                        : Brushes.LightGray
                };
                block.Inlines.Add(run);
                ranChars += e - i;
            }
        }
        void AddCol(TextBlock T, GridLength L) {
            if (T.Text == "")
                return;
            ApplyColorPattern(T, T.Text ?? "");
            grid.ColumnDefinitions.Add(new ColumnDefinition(L));
            Grid.SetColumn(T, col++);
            grid.Children.Add(T);
        }
        AddCol(first, GridLength.Auto);
        AddCol(middle, GridLength.Star);
        AddCol(last, GridLength.Auto);
        Content = grid;
    }
}