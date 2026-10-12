using Avalonia.Media;
using IoListWallet.Core;
using IoListWallet.Core.Lists;
using IoListWallet.Content.Profile;
using System.Threading;

namespace IoListWallet.Content.Abstract;
internal abstract class FontSelect : IoListPanel {
    protected abstract string EntryKey();
    public FontSelect() : base() => MakeList<FontFamily>(new IoFilter() {
        Filters = [ // Search Formats:
            new FilterText() { Name = "Name" } // Name: Font Name
        ], SortIndex = 0, SortIndex2 = 0, Increasing = true, Increasing2 = true
    }, new Gen_Buttons(1), [], 1);
    
    internal override bool IsModal() => true;
    internal override string TrueParentType => MakeK(typeof(ProfileContent), "");
    protected override bool CloseOnDoubleClick(int fromList) => true;
    internal override void RefreshData(int job, CancellationToken? token) {
        MyList[0].RefreshTemplate(
            (System.Collections.Generic.IEnumerable<FontFamily>?)FontManager.Current.SystemFonts, // Takes set of fonts
            (object? font) => new Gen_Buttons.GenData([
                EntryKey(), ((FontFamily?)font)?.ToString() ?? "ERROR" // prints them as a list
            ]),
            (font) => [   // Search Data:
                ((FontFamily?)font)?.ToString() ?? "" // Search: Name
            ]);
        base.RefreshData(job, token);
    }
}