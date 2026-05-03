using Avalonia.Media;
using System;

namespace IoListWallet.Core.Basic;
internal class IoTextBlockInfo : IoTextBlockBase {
    protected override FontFamily GetFontFamily() => Static.AppSave.GetProfile?.InfoFont ?? Static.AppSave.DPro.InfoFont;
    protected override double GetFontSize() => Static.AppSave.GetProfile?.InfoFontSize ?? Static.AppSave.DPro.InfoFontSize;
    protected override Type StyleKeyOverride => typeof(IoTextBlockBase);
}