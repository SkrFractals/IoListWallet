using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using IoListWallet.ElementControls.Basic;
using IoListWallet.MainControls.Panels;
using IoListWallet.IoPanels.Main;
using IoListWallet.Structs.Crypto;
using static IoListWallet.App;
using static IoListWallet.AppSave;
using static IoListWallet.ConstructionHelpers;
using IoListWallet.ElementControls;

namespace IoListWallet.MainControls;
internal class IoMainPanel : IoUserControl {
    [Save]
    internal IoDockRoot Dock { get; set { field = value; IoPanel.Dirty(); } }

    [Save]
    internal int MyWindowK { get; set { 
            field = value;
            //MyWindow = 
            //MyWindow.Activated += (_, _) => Overlay.Value.Text.IsVisible = false;
            //MyWindow.Deactivated += (_, _) => Overlay.Value.Text.IsVisible = true;
            IoPanel.Dirty(); } } = -1; // It can lack window, if app is in fullscreen mode

    //internal IoWindow? MyWindow = null;
    //internal IoWindow? MyWindow;
    private IoToggleButton? home, back, pin, pinned;
    private ContentControl dockHost;
    private Grid toolbar;
    private Grid grid;
    internal (TextBlock Text, Viewbox View)? Overlay { get; set; }
    // Reference to parent panel
   

    internal IoMainPanel() {
        Dock = new(this);

        // Unfocused Title Overlay
        Overlay = MakeTextBlock("overlay", "", ""/*MyWindow?.Title ?? ""*/) ?? throw new("Failed IoMainPanel.overlay");
        Overlay.Value.Text.Opacity = 0.4;
        Overlay.Value.Text.IsHitTestVisible = false;
        Overlay.Value.View.IsHitTestVisible = false;
        Overlay.Value.Text.IsVisible = true;
        //MyWindow = Window;
        
        grid ??= new Grid();
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        dockHost = new ContentControl();
        //dockHost.Content = Dock;
        Grid.SetRow(dockHost, 0);
        grid.Children.Add(dockHost);
        toolbar ??= new Grid {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnDefinitions =
    {
        new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
        new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
        new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
        new ColumnDefinition(new GridLength(1, GridUnitType.Star))
    }
        };
        //grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        static IoToggleButton MakeButton(string text) {
            var t = new TextBlock() { Text = text };
            return new IoToggleButton(t) {
                Content = t, HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Text = t.Text
            };
        }
        void cancelToggle(object? o, RoutedEventArgs e) => ((IoToggleButton?)o).IsChecked = false;
        
      
        pin = MakeButton(L("Pin"));
        pin.Click += (_, _) => Pin_Click();
        pin.AreYouSure = ""; // make it togglable in color

        pinned = MakeButton(L("Pinned"));
        pinned.Click += (s, e) => cancelToggle(s, e);
        pinned.Click += (_, _) => Pinned_Click();

        //pinned.Click += (_, _) => 
        SetTip(pinned, L("T_Pinned"));

        home = MakeButton(L("Home"));
        home.Click += (s, e) => cancelToggle(s, e);
        home.Click += (_, _) => Home_Click();
        SetTip(pinned, L("T_Home"));

        back = MakeButton(L("Back"));
        back.Click += (s, e) => cancelToggle(s, e);
        back.Click += (_, _) => Back_Click();

        // Put each in a column
        Grid.SetColumn(pin, 0);
        Grid.SetColumn(pinned, 1);
        Grid.SetColumn(home, 2);
        Grid.SetColumn(back, 3);

        // Add buttons to the grid
        toolbar.Children.Add(pin);
        toolbar.Children.Add(pinned);
        toolbar.Children.Add(home);
        toolbar.Children.Add(back);

        Grid.SetRow(toolbar, 1);
        
        grid.Children.Add(Overlay.Value.View);
        grid.Children.Add(toolbar);

        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        Update();
        Content ??= grid;
    }
    internal bool Update(bool total = false) {

        bool hasToolbar = grid.Children.Contains(toolbar);
        
        /*if (hasToolbar && AppDisplayMode == DisplayMode.MultiWindow) {
            grid.Children.Remove(toolbar);
            grid.RowDefinitions.RemoveAt(1);
        }
        if (!hasToolbar && AppDisplayMode != DisplayMode.MultiWindow) {
            grid.Children.Add(toolbar);
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }*/
        dockHost.Content = null;
        dockHost.Content = DockRenderer.Build(Dock);
        var display = GetProfile?.DisplayMode ?? DisplayMode.Fullscreen;

        if (pin == null || pinned == null || home == null || back == null || display == DisplayMode.MultiWindow)
            return false;
        if (Dock.TopNonRoot is not IoDockPanel MyDockPanel || FromK(MyDockPanel.Panel) is not IoPanel MyPanel)
            return pin.IsEnabled = pinned.IsEnabled = home.IsEnabled = back.IsEnabled = false;
        bool isMenu = MyPanel is ListMainMenu;
        
        string sBack = isMenu || display == DisplayMode.MultiWindow ? "Close" : "Back";
        bool ispinned = GetProfile?.Pan.PinnedPanels.Contains(MyPanel.GetK) ?? false;
        string sPin = ispinned ? "Unpin" : "Pin";

        pin.IsEnabled = pinned.IsEnabled = !MyPanel.IsModal();
        pin.FontWeight = ispinned ? FontWeight.Bold : FontWeight.Regular;
        pin.Content = L(sPin);
        SetTip(pin, L("T_" + sPin));
       
        home.IsEnabled = !isMenu;
        home.Content = L("Home");

        back.IsEnabled = true;
        back.Content = L(sBack);
        SetTip(back, L("T_" + sBack));

        return back.IsEnabled = true;
    }

    internal void Pin_Click() {
        if (pin == null)
            throw new("Failed IoMainPanel.Pin_Click");
        // toggle pinned, MyPanel should always be IoDockPanel, because Docking is only allowed in MultiWindow mode
        if (Dock.TopNonRoot is not IoDockPanel MyDockPanel)
            return;
        var MyPanelK = MyDockPanel.Panel;
        if (FromK(MyPanelK) is not IoPanel MyPanel || MyPanel.IsModal() || GetProfile?.Pan is not IoProfile.SaveFilePan Pan)
            return; // modal panels don't allow getting pinned
        if (Pan.PinnedPanels.Remove(MyPanelK ?? IoPanel.MakeK(GetType(),""))) {
            pin.Content = L("Pin");
        } else {
            Pan.PinnedPanels.Add(MyPanelK ?? IoPanel.MakeK(GetType(), ""));
            pin.Content = L("Unpin");
        }
        Pan.Dirty = true;
    }
    internal void Pinned_Click() {
        // modal panels don't allow going to a different panel
        if (Dock.TopNonRoot is IoDockPanel MyDockPanel && FromK(MyDockPanel.Panel) is IoPanel MyPanel && !MyPanel.IsModal())
            OpenRefreshPanel(MyPanel, typeof(ListPinned));
    }
    internal void Home_Click() {
        if(FromK((Dock.TopNonRoot as IoDockPanel)?.Panel) is IoPanel P)
            OpenPanel(P, typeof(ListMainMenu));
    }

    internal void Back_Click() {
        if (Dock.TopNonRoot is IoDockPanel MyDockPanel && FromK(MyDockPanel.Panel) is IoPanel MyPanel) {
            if (MyPanel is ListMainMenu)
                AppClose();
            else
                FromK(MyDockPanel?.Panel)?.Close();
        }
    }
}