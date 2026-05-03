using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace IoListWallet.Core;

internal class IoFakeWindow : Window {
    internal class SnapPreviewOverlay : Control {
        internal Point RectPos = new(0, 0);
        internal Size RectSize = new(0, 0);
        internal Color TitleColor;
        internal double RectOpacity;
        public override void Render(DrawingContext context) {
            base.Render(context);
            if (RectSize.Width > 0 && RectSize.Height > 0)
                context.DrawRectangle(new SolidColorBrush(TitleColor, RectOpacity),
                    new Pen(new SolidColorBrush(Colors.White, RectOpacity), 8, DashStyle.Dash, PenLineCap.Round),
                    new Rect(RectPos, RectSize), 8);
        }
    }
    internal const double FullOpacity = 0.5;
    internal double TargetOpacity, TransientOpacity;
    internal PixelPoint TargetPosition, TransientPosition;
    internal double TargetWidth, TargetHeight, TransientWidth, TransientHeight;
    internal SnapPreviewOverlay r { get; private set; } = new SnapPreviewOverlay() {
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
    };
    private bool changed = true;
    internal Panel p = new();
    public IoFakeWindow() : base() => Content = p;
    internal void SetColor(Color c) => changed |= r.TitleColor != (r.TitleColor = c);
    internal void Tick(double deltaTime) {
        if (!IsVisible)
            return;
        double dto = deltaTime * 2, dt = Math.Max(Width,Height) * dto;
        changed |= TransientOpacity != (TransientOpacity = Math.Clamp(TargetOpacity, TransientOpacity - dto, TransientOpacity + dto));
        changed |= TransientWidth != (TransientWidth = Math.Clamp(TargetWidth, TransientWidth - dt, TransientWidth + dt));
        changed |= TransientHeight != (TransientHeight = Math.Clamp(TargetHeight, TransientHeight - dt, TransientHeight + dt));
        changed |= TransientPosition != (TransientPosition = new(
            (int)Math.Clamp(TargetPosition.X, TransientPosition.X - dt, TransientPosition.X + dt),
            (int)Math.Clamp(TargetPosition.Y, TransientPosition.Y - dt, TransientPosition.Y + dt)));
        var p = TransientPosition - Position;
        Point nPos = new(p.X + 12, p.Y + 12);
        Size nSize = new(TransientWidth - TransientPosition.X - 24, TransientHeight - TransientPosition.Y - 24);
        if (TargetOpacity <= 0 && TransientOpacity <= 0) {
            IsVisible = false;
            //WindowState = WindowState.Minimized;
        } else if (changed || nSize != r.RectSize || nPos != r.RectPos) {
            changed = false;
            r.RectOpacity = TransientOpacity;
            r.RectPos = nPos;
            r.RectSize = nSize;
            r.InvalidateVisual();
        }
    }
}

/*internal string debug {
            get;
            set {
                if (field != value) {
                    field = value;
                    t = new(
              value,
              CultureInfo.CurrentCulture,
              FlowDirection.LeftToRight,
              new Typeface(new FontFamily("Segoe UI"), FontStyle.Normal, FontWeight.Normal),
              10,
              Brushes.Blue) {
                        TextAlignment = TextAlignment.Left
                    };
                }
            }
        } = "";
        private FormattedText? t = null;*///if(t != null)context.DrawText(t, new(200, 200));