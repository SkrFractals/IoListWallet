using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IoListWallet.ConstructionHelpers;

namespace IoListWallet.ElementControls.Basic;
internal class IoNumericLeftRight : UserControl {

    internal event EventHandler<EventArgs>? ValueChanged;

    internal bool Trigger = true; // Disabled triggering value changed when script-changing the values
    internal ulong Value { get; set { field = value; NumberBox?.Text.Text = Value.ToString(); } }
    internal ulong Increment, Minimum, Maximum, Speed;
    internal bool Stretch;
    internal string Tooltip, Watermark, TextLeft, TextRight;
    internal (TextBlock Text, Viewbox View)? TextLeftBlock, TextRightBlock;

    private (IoTextBox Text, Viewbox View, Border Border)? NumberBox;
    private (TextBlock Text, Viewbox View, IoToggleButton Button)? Prev, Next;
    private Grid? grid;
    private ulong prevHeld = 0, nextHeld = 0, processHeld = 1;
    private bool prevReleaseAction = false, nextReleaseAction = false;

    internal IoNumericLeftRight(string tooltip, ulong value, ulong minimum, ulong maximum, ulong increment, ulong speed, string watermarkLocaleId = "", bool stretch = true, string textLeft = "", string textRight = "", EventHandler<EventArgs>? Response = null) {

        AttachedToVisualTree += OnAttached;
        ClipToBounds = false;
        Speed = speed;
        Minimum = minimum;
        Maximum = maximum;
        Increment = increment;
        Stretch = stretch;
        BigTextBlock(ref TextLeftBlock, "", TextLeft = textLeft);
            if(TextLeftBlock == null)
                throw new("Failed NumericLeftRight.TextLeft");
        BigTextBlock(ref TextRightBlock, "", TextRight = textRight);
        if(TextRightBlock == null)
            throw new("Failed NumericLeftRight.TextRight");
        if (Response != null)
            ValueChanged += Response;

        // TextBox
        BigTextBox(ref NumberBox, Tooltip = tooltip, (Value = value).ToString(), Watermark = watermarkLocaleId, NumberBoxEdited, NumberBoxEdited);
        if (NumberBox == null)
            throw new("Failed NumericLeftRight.NumberBox");
        // Previous
        BigButton(ref Prev, "", "◀");
        if (Prev == null)
            throw new("Failed NumericLeftRight.Prev");
        Prev.Value.Button.AddHandler(PointerPressedEvent, PrevPressed, RoutingStrategies.Tunnel);
        Prev.Value.Button.IsEnabled = Value > Minimum;
        // Next
        BigButton(ref Next, "", "▶");
        if (Next == null)
            throw new("Failed NumericLeftRight.Next");
        Next.Value.Button.AddHandler(PointerPressedEvent, NextPressed, RoutingStrategies.Tunnel);
        Next.Value.Button.IsEnabled = Value < Maximum;
        BuildGrid();
        Refresh();
    }
    internal void ChangeValue(int value) {
        NumberBox?.Text.Text = value.ToString();
        NumberBoxEdited(null, new EventArgs());
    }
    internal void SetEnabled(bool Enabled) {
        if (Prev == null || Next == null || NumberBox == null)
            return;
        Prev.Value.Button.IsEnabled = Next.Value.Button.IsEnabled = NumberBox.Value.Text.IsEnabled = Enabled;
    }
    internal void Refresh() {
        BigTextBox(ref NumberBox, Tooltip, Value.ToString(), Watermark);
        BigTextBlock(ref TextLeftBlock, "", TextLeft);
        BigTextBlock(ref TextRightBlock, "", TextRight);
        Prev?.Button.IsEnabled = Value > Minimum;
        Next?.Button.IsEnabled = Value < Maximum;
    }
    private void BuildGrid() {
        JustGrid(ref grid);
        if (grid == null)
            throw new("Failed NumericLeftRight.grid");
        if (Stretch) {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star))); // Left
            grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star))); // Prev
            grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(3, GridUnitType.Star))); // Number (Twice as wide)
            grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star))); // Next
            grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star))); // Right

        } else {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // Left
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // Prev
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // Number (Twice as wide)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // Next
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // Right
        }
        if (TextLeftBlock != null) {
            Grid.SetColumn(TextLeftBlock.Value.View, 0);
            AddChild(grid, TextLeftBlock.Value.View); //grid.Children.Add(TextLeftBlock.Value.View);
        }
        if (Prev != null) {
            Grid.SetColumn(Prev.Value.Button, 1);
            AddChild(grid, Prev.Value.Button); //grid.Children.Add(Prev.Value.Button);
        }
        if (NumberBox != null) {
            Grid.SetColumn(NumberBox.Value.Border, 2);
            AddChild(grid, NumberBox.Value.Border); //grid.Children.Add(NumberBox.Value.Border);
        }
        if (Next != null) {
            Grid.SetColumn(Next.Value.Button, 3);
            AddChild(grid, Next.Value.Button); //grid.Children.Add(Next.Value.Button);
        }
        if (TextRightBlock != null) {
            Grid.SetColumn(TextRightBlock.Value.View, 4);
            AddChild(grid, TextRightBlock.Value.View); //grid.Children.Add(TextRightBlock.Value.View);
        }
        FinishChildren(grid);
        Content ??= grid;
    }
    private void OnAttached(object? sender, VisualTreeAttachmentEventArgs e) {
        var top = TopLevel.GetTopLevel(this);
        if (top != null) {
            top.AddHandler(
                PointerReleasedEvent,
                GlobalPointerReleased,
                RoutingStrategies.Tunnel,
                true);
            top.AddHandler(
                PointerCaptureLostEvent,
                GlobalPointerLost,
                RoutingStrategies.Tunnel,
                true);
        }
    }
    private void NumberBoxEdited(object? s, EventArgs e) {
        var PrevValue = Value;
        if (NumberBox == null)
            return;
        NumberBox.Value.Text.Text = NumberBox.Value.Text.Text == null ? "" : Regex.Replace(NumberBox.Value.Text.Text, "[^0-9]", "");
        if (NumberBox.Value.Text.Text != "") {
            ulong.TryParse(NumberBox.Value.Text.Text, out var value);
            string ns = (Value = Math.Clamp(value, Minimum, Maximum)).ToString();
            if (ns != NumberBox.Value.Text.Text)
                NumberBox.Value.Text.Text = ns;
        } else Value = 0;

        Prev?.Button.IsEnabled = Value > Minimum;
        Next?.Button.IsEnabled = Value < Maximum;

        if (Value != PrevValue && Trigger)
            ValueChanged?.Invoke(this, new EventArgs());
    }
    private void PrevPressed(object? s, PointerPressedEventArgs e) {
        prevReleaseAction = true;
        PrevPress(prevHeld = ++processHeld);
    }
    private void NextPressed(object? s, PointerPressedEventArgs e) {
        nextReleaseAction = true;
        NextPress(nextHeld = ++processHeld);
    }
    private async void PrevPress(ulong Id) {
        int delay = (int)(1 + Speed / 4.0); // initial delay (ms)
        await Task.Delay(delay);
        while (prevHeld == Id) {
            delay = Math.Clamp((int)(delay * (.95f - Speed * .01f)), 200, 420);
            prevReleaseAction = false;
            Backward();
            await Task.Delay(delay);
        }
    }
    private async void NextPress(ulong Id) {
        int delay = (int)(1000 / (1+Speed/4.0)); // initial delay (ms)
        await Task.Delay(delay);
        while (nextHeld == Id) {
            delay = Math.Clamp((int)(delay * (.95f - Speed * .01f)), 200, 420);
            nextReleaseAction = false;
            Forward();
            await Task.Delay(delay);
        }
    }
    private async void GlobalPointerReleased(object? s, PointerReleasedEventArgs e) => Release();
    private async void GlobalPointerLost(object? s, PointerCaptureLostEventArgs e) => Release();
    private void Release() {
        if (nextReleaseAction)
            Forward();
        if (prevReleaseAction)
            Backward();
        prevHeld = nextHeld = 0;
        prevReleaseAction = nextReleaseAction = false;
    }
    private void Forward()
        => NumberBox?.Text.Text = Math.Min(Maximum, Value + Increment).ToString();
    private void Backward() 
        => NumberBox?.Text.Text = Math.Max(Minimum, Value - Increment).ToString();
}