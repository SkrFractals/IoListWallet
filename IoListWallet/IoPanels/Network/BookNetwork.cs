using IoListWallet.MainControls.Panels;
using System;
using System.IO;
using static IoListWallet.App;
using static IoListWallet.AppSave;

namespace IoListWallet.IoPanels.Network;
internal class BookNetwork : IoBookPanel {
    public BookNetwork() : base() { }

    // Serialized:
    [Save]
    internal Type? MyNetwork { get; set; } = null;

    protected override string CustomTitle()
        => " - " + MyNetwork?.Name ?? "";
    internal enum SettingsNetworkPage : byte {
        Main = 0
    }
    internal override void Refresh() {
        Book.PageNames = ["Main"];
        base.Refresh();
    }
    protected override void FillPage(object? s, EventArgs e) {
        var T = "T_BookNetwork.";
        var F = "Failed BookNetwork.";

        switch ((SettingsNetworkPage)Book.Swipe.Page.X) {
            case SettingsNetworkPage.Main:

                // Address Book Editor
                void clickListAddressBook(object? s, EventArgs e) {
                    var NewPanel = (ListAddress?)OpenPanel(this, typeof(ListAddress), Key, false, false);
                    NewPanel?.MyNetwork = MyNetwork;
                    RefreshPanel(NewPanel);
                }
                var listAddressBookButton 
                    = MakeButton("listAddressBookButton", T + "AddressBook", "Editor", clickListAddressBook)
                    ?? throw new(F + "listAddressBookButton");

                // Remove Network
                void clickRemoveNetwork(object? s, EventArgs e) {
                    if (MyNetwork == null)
                        return;
                    // Remove network:
                    GetNetworks?.Remove(MyNetwork);
                    Directory.Delete(Path.Combine(GetProfileDir(), MyNetwork.Name));
                    GetProfile?.Network = null;
                    // Update network select
                    RefreshPanel((typeof(ListNetworkSelect), ""));
                    // And close myself
                    Close();
                }
                var removeNetworkButton 
                    = MakeButton("removeNetworkButton", T + "RemoveNetwork", "Remove",
                        clickRemoveNetwork, false, Avalonia.Media.FontWeight.Bold, L("AreYouSure"))
                    ?? throw new(F + "removeNetworkButton");
                
                Book.MakeRow("0", "RemoveNetwork", [(removeNetworkButton.Button, 1)]);
                Book.MakeRow("1", "AddressBookList", [(listAddressBookButton.Button, 1)]);


                // Button
                /*void clickLanguageBox(object? s, EventArgs e)
                    => EnqueueTask(((ListLanguagePanel)OpenPanel(this, typeof(ListLanguagePanel), "", false, false), 1));
                var languageBox = MakeButton("languageBox", "T_SettingsDisplay", Languages[AppLanguage], clickLanguageBox)
                    ?? throw new(F + "languageBox");
                MakeRow("Language", [(languageBox.Button, 1)]);*/

                // TextBox
                /*void clickDateFormat(object? s, EventArgs e)
                    => SetDateFormat(s, e);
                var dateFormat = MakeTextBox("dateFormat", "T_SettingsDateFormat",
                    AppDateFormat, "", clickDateFormat)
                    ?? throw new(F + "dateFormat");
                MakeRow("DateFormat", [(dateFormat.View, 1)]);*/

                // Combo
                /*void clickDateFormat(object? s, EventArgs e)
                     => SetDateFormat(s, e);
                var dateFormat = MakeComboBox("dateFormat", "T_SettingsDateFormat",
                    DateFormats, AppDateFormat,
                    new FuncDataTemplate<string>((mode, _) => new TextBlock { Text = mode }, false), clickDateFormat)
                    ?? throw new(F + "dateFormat");
                MakeRow("DateFormat", [(dateFormat.View, 1)]);
                dateFormat.Combo.IsEditable = true;*/

                // Numeric
                /*void clickInfoFontSize(object? s, EventArgs e)
                    => SetInfoFont(AppInfoFont, ((NumericLeftRight?)s)?.Value ?? DefaultInfoFontSize);
                var infoFontSize = MakeNumeric("infoFontSize", "T_SettingsInfoFontSize",
                    (int)AppInfoFontSize, 6, 120, 1, "", "", clickInfoFontSize)
                    ?? throw new(F + "infoFontSize");
                MakeRow("InfoFont", [(infoFontBox.Button, 1), (infoFontSize, 2)]);*/


                // Reset Settings
                /*void clickResetInt(object? s, EventArgs e) {
                    dateFormat.Combo.Text = DefaultDateFormat;
                    // Reset Fonts
                    controlFontBox.Text.Text = (AppControlFont = DefaultFont).ToString();
                    infoFontBox.Text.Text = (AppInfoFont = DefaultFont).ToString();
                    controlFontSize.Value = 18;
                    infoFontSize.Value = 12;
                    // Reset Language
                    AppLanguage = DefaultLanguage;
                    EnqueueTask((GetPanel(this, typeof(ListLanguagePanel))));
                    // Reset Display
                    displayBox.Combo.SelectedIndex = 0;
                }
                var resetIntButton = MakeButton("resetIntButton", "T_SettingsReset", "Reset", clickResetInt)
                    ?? throw new(F + "resetButton");
                MakeRow("ResetToDefault", [(resetIntButton.Button, 1)]);*/

                break;
            
        }
        base.FillPage(s, e);
    }
}