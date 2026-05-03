using Avalonia.Media;
using System;

namespace IoListWallet.Core.Basic;
internal class IoTextBlockControl : IoTextBlockBase {
    protected override FontFamily GetFontFamily() => Static.AppSave.GetProfile?.ControlFont ?? Static.AppSave.DPro.ControlFont;
    protected override double GetFontSize() => Static.AppSave.GetProfile?.ControlFontSize ?? Static.AppSave.DPro.ControlFontSize;
    protected override Type StyleKeyOverride => typeof(IoTextBlockBase);
}