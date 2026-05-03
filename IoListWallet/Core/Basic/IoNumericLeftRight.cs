using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IoListWallet.Core.Static.ConstructionHelpers;

namespace IoListWallet.Core.Basic;
internal class IoNumericLeftRight : UserControl {

    internal event EventHandler<EventArgs>? ValueChanged;

    internal bool Trigger = true; // Disabled triggering value changed when script-changing the values
    internal ulong Value { get; set { field = value; NumberBox?.Text.Text = Value.ToString(); } }
    internal ulong Increment, Minimum, Maximum, Speed;
    internal bool Stretch;
    internal string Tooltip, Watermark, TextLeft, TextRight;
    internal WTextBlock? TextLeftBlock, TextRightBlock;

    internal WTextBox? NumberBox;
    private WToggleButton? Prev, Next;
    private Grid? grid;
    internal IoUserControl? Owner;
    internal ulong prevHeld { get; private set; } = 0;
    internal ulong nextHeld { get; private set; } = 0;
    private ulong processHeld = 1;
    internal bool prevReleaseAction { get; private set; } = false;
    internal bool nextReleaseAction { get; private set; } = false;

    private EventHandler<EventArgs>? response = null;

    internal IoNumericLeftRight(IoUserControl? owner, string tooltip, ulong value, ulong minimum, ulong maximum, ulong increment, ulong speed, string watermarkLocaleId = "", bool stretch = true, string textLeft = "", string textRight = "", EventHandler<EventArgs>? newResponse = null) {

        AttachedToVisualTree += OnAttached;
        ClipToBounds = false;
        Speed = speed;
        Minimum = minimum;
        Maximum = maximum;
        Increment = increment;
        Stretch = stretch;
        BigTextBlock(ref TextLeftBlock, null, "", TextLeft = textLeft);
            if(TextLeftBlock == null)
                throw new("Failed NumericLeftRight.TextLeft");
        BigTextBlock(ref TextRightBlock, null, "", TextRight = textRight);
        if(TextRightBlock == null)
            throw new("Failed NumericLeftRight.TextRight");
        SetResponse(newResponse);

        // TextBox
        BigTextBox(ref NumberBox, Owner = owner, Tooltip = tooltip, (Value = value).ToString(), Watermark = watermarkLocaleId, NumberBoxEdited, NumberBoxEdited);
        if (NumberBox == null)
            throw new("Failed NumericLeftRight.NumberBox");
        // Previous
        BigButton(ref Prev, null, "", "◀");
        if (Prev == null)
            throw new("Failed NumericLeftRight.Prev");
        Prev.Button.AddHandler(PointerPressedEvent, PrevPressed, RoutingStrategies.Tunnel);
        Prev.Button.IsEnabled = Value > Minimum;
        // Next
        BigButton(ref Next, null, "", "▶");
        if (Next == null)
            throw new("Failed NumericLeftRight.Next");
        Next.Button.AddHandler(PointerPressedEvent, NextPressed, RoutingStrategies.Tunnel);
        Next.Button.IsEnabled = Value < Maximum;
        BuildGrid();
        Refresh();
    }
    internal void SetResponse(EventHandler<EventArgs>? newResponse) {
        if (newResponse != null) {
            if (response != null)
                ValueChanged -= response;

            ValueChanged += response = newResponse;
        }
    }
    internal void ChangeValue(int value) {
        NumberBox?.Text.Text = value.ToString();
        NumberBoxEdited(null, new EventArgs());
    }
    internal void SetEnabled(bool Enabled) {
        if (Prev == null || Next == null || NumberBox == null)
            return;
        Prev.Button.IsEnabled = Next.Button.IsEnabled = NumberBox.Text.Enabled = Enabled;
    }
    internal void Refresh() {
        BigTextBox(ref NumberBox, Owner, Tooltip, Value.ToString(), Watermark);
        BigTextBlock(ref TextLeftBlock, null, "", TextLeft);
        BigTextBlock(ref TextRightBlock, null, "", TextRight);
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
            Grid.SetColumn(TextLeftBlock.View, 0);
            AddChild(grid, TextLeftBlock.View); //grid.Children.Add(TextLeftBlock.Value.View);
        }
        if (Prev != null) {
            Grid.SetColumn(Prev.Button, 1);
            AddChild(grid, Prev.Button); //grid.Children.Add(Prev.Value.Button);
        }
        if (NumberBox != null) {
            Grid.SetColumn(NumberBox.Border, 2);
            AddChild(grid, NumberBox.Border); //grid.Children.Add(NumberBox.Value.Border);
        }
        if (Next != null) {
            Grid.SetColumn(Next.Button, 3);
            AddChild(grid, Next.Button); //grid.Children.Add(Next.Value.Button);
        }
        if (TextRightBlock != null) {
            Grid.SetColumn(TextRightBlock.View, 4);
            AddChild(grid, TextRightBlock.View); //grid.Children.Add(TextRightBlock.Value.View);
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
            top.AddHandler(
                PointerMovedEvent,
                GlobalPointerMoved,
                RoutingStrategies.Tunnel,
                true);
        }
    }
    private void NumberBoxEdited(object? s, EventArgs e) {
        var PrevValue = Value;
        if (NumberBox == null)
            return;
        NumberBox.Text.Text = NumberBox.Text.Text == null ? "" : Regex.Replace(NumberBox.Text.Text, "[^0-9]", "");
        if (NumberBox.Text.Text != "") {
            ulong.TryParse(NumberBox.Text.Text, out var value);
            string ns = (Value = Math.Clamp(value, Minimum, Maximum)).ToString();
            if (ns != NumberBox.Text.Text)
                NumberBox.Text.Text = ns;
        } else Value = 0;

        Prev?.Button.IsEnabled = Value > Minimum;
        Next?.Button.IsEnabled = Value < Maximum;

        if (Value != PrevValue && Trigger)
            ValueChanged?.Invoke(this, new EventArgs());
    }
    private int _activePointerId = -1;
    private void PrevPressed(object? s, PointerPressedEventArgs e) {
        prevReleaseAction = true;
        _activePointerId = e.Pointer.Id;
        PrevPress(prevHeld = ++processHeld);
    }
    private void NextPressed(object? s, PointerPressedEventArgs e) {
        nextReleaseAction = true;
        _activePointerId = e.Pointer.Id;
        NextPress(nextHeld = ++processHeld);
    }
    private async void PrevPress(ulong Id) {
        int delay = (int)(1000 / (1 + Speed / 4.0)); // initial delay (ms)
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
    private void GlobalPointerMoved(object? sender, PointerEventArgs e) {
        if ((prevHeld >0 || nextHeld > 0) && e.Pointer.Id == _activePointerId) {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
                nextReleaseAction = prevReleaseAction = false;
                Release();
            }
        }
    }
    private void GlobalPointerReleased(object? s, PointerReleasedEventArgs e) => Release();
    private void GlobalPointerLost(object? s, PointerCaptureLostEventArgs e) => Release();
    internal void Release(bool cancel = false) {
        if(cancel)
            nextReleaseAction = prevReleaseAction = false;
        _activePointerId = -1;
        if (nextReleaseAction)
            Forward();
        if (prevReleaseAction)
            Backward();
        prevHeld = nextHeld = 0;
        prevReleaseAction = nextReleaseAction = false;
    }
    private void Forward()
        => ChangeValue((int)Math.Min(Maximum, Value + Increment));
    private void Backward() 
        => ChangeValue((int)Math.Max(Minimum, Value - Increment));
}