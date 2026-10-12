using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using IoListWallet.Core.Static;
using IoListWallet.Structs.Crypto;
using System;
using System.Globalization;

namespace IoListWallet.Core.Basic;

internal abstract class IoTextBlockBase : Control {

    protected abstract FontFamily GetFontFamily();
    protected abstract double GetFontSize();

    // Const
    const int N = 32; // maximum sample size
    const double nudge = 2, di = 1.0f / N;

    private void InvalidateText() {
        updateText = updateTexts = true;
        InvalidateVisual();
        InvalidateMeasure();
    }

    // Properties:
    internal double PaddingMultiplier { get; set { 
            if (field != (field = value)) InvalidateText(); } } = 1;
    internal FontWeight FontWeight { get; set { 
            if (field != (field = value)) { InvalidateText(); InvalidateMeasure(); } } } = FontWeight.Normal;
    internal TextAlignment TextAlignment { get; set { 
            if (field != (field = value)) InvalidateText(); } } = TextAlignment.Center;
    internal string Text { get; set { 
            if (field != (field = value)) InvalidateText(); } } = ""; // maybe a binding would be nice?
    internal IoTooltips Tooltip;

    // Styled Properties:
    public static readonly StyledProperty<IBrush?> ForegroundProperty
        = TextBlock.ForegroundProperty.AddOwner<IoTextBlockBase>();
    public IBrush? Foreground {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }
    public static readonly StyledProperty<int> ShadowQualityProperty
        = AvaloniaProperty.Register<IoTextBlockBase, int>(nameof(ShadowQuality), 0);
    public int ShadowQuality {
        get => GetValue(ShadowQualityProperty);
        set => SetValue(ShadowQualityProperty, value);
    }

    // Cache:
    Point[][] trans = new Point[3][];
    Point[] xy = new Point[N];
    double[] R = new double[N], norms = new double[3];
    double[][] renorms = new double[3][];
    FormattedText[][] texts = new FormattedText[3][];
    FormattedText? formatText = null;
    IBrush[][] brushes = new IBrush[3][];

    // Var:
    private bool updateQ = true, updateText = true, updateTexts = true;
    private double pad = 0, lastRange = -1, lastDiv = -1, lastFontSize = -1; // n = 1;
    private int _N = 0;
    private FontFamily? lastFontFamily = null;
    
    private static double S(double f) => f * f;
    static IoTextBlockBase() {
        ShadowQualityProperty.Changed.AddClassHandler<IoTextBlockBase>((x, e) => {
            x.updateQ = true;
            x.InvalidateArrange();
            x.InvalidateMeasure();
            x.InvalidateVisual();
        });
        ForegroundProperty.Changed.AddClassHandler<IoTextBlockBase>((x, e) => {
            x.updateText = true;
            x.InvalidateVisual();
        });
    }
    public IoTextBlockBase() : base() {
        Tooltip = new(this);
        // Precalculate general unchanging thigs:
        double fi = 0, a = App.rand.NextDouble() * double.Tau; // random starting angle
        for (int i = 0; i < N; ++i) {
            a += 2.3999632297286531f; // golden ratio increments
            R[i] = fi += di; // fi will go from 0..1 as i = 0.._N
            xy[i] = new(Math.Cos(a), Math.Sin(a)); // normalized golden coords
        }
        for (int q = 0; q <= 2; ++q) {
            int q3 = q + 3, i = 1 << q3;
            texts[q] = new FormattedText[i];
            brushes[q] = new IBrush[i];
            var _di = di * (norms[q] = (double)N / i);
            renorms[q] = new double[i];
            trans[q] = new Point[i];
            for (double o = 1; 0 <= --i; renorms[q][i] = Math.Sqrt(norms[q] * R[i]))
                brushes[q][i] = new SolidColorBrush(Colors.Black, 1 - S(o -= _di));
        }
        UpdateFont();
    }
    private void UpdateFont() {
        var newFamily = GetFontFamily();
        if (lastFontFamily != newFamily) {
            InvalidateText();
            lastFontFamily = newFamily;
        }
        var newSize = GetFontSize();
        if (lastFontSize != newSize) {
            InvalidateText();
            lastFontSize = newSize;
        }
    }
    protected override Size ArrangeOverride(Size finalSize) {

        if (AppSave.GetProfile == null || ShadowQuality < 0) {
            _N = 0;
            return finalSize;
        }
        UpdateFont();
        _N = Text == "·" ? 0 : 8 << ShadowQuality; // Selected sample size, chosen from options: q.0/1/2 -> _N.8/16/32, 0 = off
        double div = 1 / (1 + Math.Sqrt(Math.Min(finalSize.Width, finalSize.Height)) * nudge); // / sqrt(bounds)[readjust]
        double range = Math.Clamp(lastFontSize * div, .5, 2.0); // * font [base] , clamp(.5,2) [sanity]
        if (Math.Abs(range - lastRange) > 0.001 || Math.Abs(div - lastDiv) > 0.001 || updateQ) {
            updateQ = false;
            updateTexts = true;
            lastRange = range;
            lastDiv = div;
            var t = trans[ShadowQuality];
            var r = renorms[ShadowQuality];
            div += .5; // add a small starting distance, so that it would not just get drawn totally behind the foreground
            for (int i = 0; i < _N; ++i) {
                var xi = xy[i];
                var scale = range * r[i] + div; // R = 0..N->0..1, * n=N/_N [renormalize R], then Sqrt for dense outside, and scale by range
                t[i] = new(scale * xi.X, scale * xi.Y);
            }
        }
        UpdateText();
        UpdateTexts();
        return finalSize; // only change in font, font size will remake the texts:
    }
    protected void UpdateTexts() {
        if (!updateTexts || Text == "" || lastFontSize <= 0 || lastFontFamily == null)
            return;
        updateTexts = false;
        if (Text == "·") {
            _N = 0;
            return;
        }
        for (int i = 0; i < _N; ++i)
            texts[ShadowQuality][i] = new(
              Text,
              CultureInfo.CurrentCulture,
              FlowDirection.LeftToRight,
              new Typeface(GetFontFamily(), FontStyle.Normal, FontWeight),
              GetFontSize(),
              brushes[ShadowQuality][i]) {
                TextAlignment = TextAlignment
            };
    }
    protected void UpdateText() { 
        if (!updateText || Text == "" || lastFontSize <= 0 || lastFontFamily == null)
            return;
        updateText = false;
        formatText = new(
            Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(lastFontFamily, FontStyle.Normal, FontWeight),
            lastFontSize,
            Foreground ?? Brushes.White) {
            TextAlignment = TextAlignment
        };
    }
    protected override Size MeasureOverride(Size availableSize) {
        UpdateFont();
        UpdateText();
        if (formatText == null)
            return new Size(0, 0);
        pad = GetFontSize() * 0.333;
        return new Size(formatText.Width + pad, formatText.Height + pad);
    }
    public override void Render(DrawingContext context) {
        base.Render(context);
        UpdateText();
        UpdateTexts();
        if (formatText == null)
            return;
        var origin = new Point(
            //(Bounds.Width - formatText.Width) / 2,
            TextAlignment switch { 
                TextAlignment.Left => pad * PaddingMultiplier,
                TextAlignment.Right => Bounds.Width - pad * PaddingMultiplier,
                _ => (Bounds.Width - formatText.Width - pad) / 2 },
            (Bounds.Height - formatText.Height) / 2
        );

        double layoutWidth = Bounds.Width;
        if (!double.IsFinite(layoutWidth) || layoutWidth <= 0)
            layoutWidth = double.MaxValue; // fallback
        if (_N == 8 << ShadowQuality)
            for (int i = 0; i < _N; i++) {
                var t = texts[ShadowQuality][i];
                if (t != null) {
                    t.MaxTextWidth = layoutWidth;
                    context.DrawText(t, origin + trans[ShadowQuality][i]);
                }
            }
        formatText.MaxTextWidth = layoutWidth;
        context.DrawText(formatText, origin);
    }
    protected override Type StyleKeyOverride => typeof(IoTextBlockBase);
}
internal class WTextBlock(IoTextBlockBase text, Viewbox view) {
    internal IoTextBlockBase Text = text;
    internal Viewbox View = view;
}