using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using IoListWallet.Core;
using IoListWallet.Core.Basic;
using IoListWallet.Core.Panels;
using IoListWallet.IoPanels.Main;
using IoListWallet.Structs.Crypto;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using static IoListWallet.Core.Static.AppSave;
using IoListWallet.Core.Dock;

namespace IoListWallet;

public partial class App : Application {

    internal static event EventHandler<EventArgs>? SwipeCaptured = null;

    // DEBUG:

    // Block MainMenu from spawning Tutorial, Login, Select Network+Wallet+Account...
    internal const bool DisableHandHolding = 
        true;
    //false;
    // Block saving any save files, so that you can test from a clean slate every time with worrying about save states
    internal const bool DisableSaving =
        true;
        //false;
    // OpenedWindows Loading is currently broken, so this needs to be off if you want to actaully do anything other than debugging OpenedWindows.
    internal const bool DisableSavingOpenedWindows =
        //true;
        false;

    internal static bool AppRunning { get; private set; } = true;
    internal static bool CanCloseMain = true;

    private DispatcherTimer timer = new() {
        Interval = TimeSpan.FromMilliseconds(20),
        IsEnabled = true
    };
    // Failed attempts at disabling selecting combo buttons:
    /*internal static void DontSelectToggles(object? s, EventArgs e) => ((IoComboBox?)s)?.SelectedItem = null;
    internal static ControlTheme ToggleComboTheme = new (typeof(ComboBoxItem)) {
        Setters = {  
            new Setter(InputElement.IsHitTestVisibleProperty, false),
            new Setter(InputElement.FocusableProperty, false)
        }
    };*/

    //internal event EventHandler<EventArgs>? PointerLost = null;
    internal static void FireSwipeCaptured(IoSwipe s) => SwipeCaptured?.Invoke(s, new EventArgs());
    
    internal static void UnpressNumerics(object? c){
        if (c is not Control)
            return;
        if (c is IoNumericLeftRight n) {
            n.Release(true);
            return;
        }
        if (c is ContentControl cc) {
            UnpressNumerics(cc.Content);
            return;
        }
        if (c is Panel p) 
            foreach (var ch in p.Children) 
                UnpressNumerics(ch);
        if (c is Viewbox v)
            UnpressNumerics(v.Child);
        return;
    }
 
    #region Start_End
    internal static void SetButtonShadows(byte Radius) {
        Current?.Resources["ButtonShadow"] = new DropShadowEffect {
            BlurRadius = Radius,
            OffsetX = Radius,
            OffsetY = Radius
        };
    }
    internal static void SetTextShadows(int Radius) {
       /* Current?.Resources["shadow.1"] = Radius >= 1;
        Current?.Resources["shadow.2"] = Radius >= 2;
        Current?.Resources["shadow.3"] = Radius >= 3;*/
    }
    
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted() {

        // Shadow styles:
        Current?.Resources["ButtonShadow"] = new DropShadowEffect {
            BlurRadius = 0,
            OffsetX = 0,
            OffsetY = 0
        };
        Styles.Add(new Style(x => x.Is<Button>()) {
            Setters = {
                new Setter(Visual.EffectProperty, new DynamicResourceExtension("ButtonShadow"))
            }
        });

        /*Styles.Add(new Style(x => x.OfType<TextBlock>().Class("shadow.1")) {
            Setters = { new Setter(Visual.IsVisibleProperty, new DynamicResourceExtension("shadow.1")) } 
        });
        Styles.Add(new Style(x => x.OfType<TextBlock>().Class("shadow.2")) {
            Setters = { new Setter(Visual.IsVisibleProperty, new DynamicResourceExtension("shadow.2")) } 
        });
        Styles.Add(new Style(x => x.OfType<TextBlock>().Class("shadow.3")) {
            Setters = { new Setter(Visual.IsVisibleProperty, new DynamicResourceExtension("shadow.3")) }
        });*/



        /*shadowVisible = new Style(x => x.OfType<TextBlockControl>().Class("shadow")) {
            Setters =
    {
        new Setter(Visual.IsVisibleProperty, true)
    }
        };
        shadowVisibleI = new Style(x => x.OfType<TextBlockInfo>().Class("shadow")) {
            Setters =
   {
        new Setter(Visual.IsVisibleProperty, true)
    }
        };

        shadowHidden = new Style(x => x.OfType<TextBlockControl>().Class("shadow")) {
            Setters =
            {
        new Setter(Visual.IsVisibleProperty, false)
    }
        };
        shadowHiddenI = new Style(x => x.OfType<TextBlockInfo>().Class("shadow")) {
            Setters =
            {
        new Setter(Visual.IsVisibleProperty, false)
    }
        };*/
        /*DropShadowEffect[] shadow = [
            new() { BlurRadius = 1, OffsetX = 1, OffsetY = 1 },
            new() { BlurRadius = 2, OffsetX = 2, OffsetY = 2 },
            new() { BlurRadius = 4, OffsetX = 3, OffsetY = 3 }];
        Shadows.Add(new Setter(Visual.EffectProperty, null));
        for (int i = 0; i < 3; ++i) {
            var si = shadow[i];
            si.Color = Colors.Black;
            si.Opacity = 1.0;
            Shadows.Add(new Setter(Visual.EffectProperty, si));
        }
        Styles.Add(ShadowTexts.Level = new Style(x => x.Is<TextBlock>()) { Setters = {
            new Setter(TextElement.FontFamilyProperty, new DynamicResourceExtension("ControlFontFamily")),
            new Setter(TextElement.FontSizeProperty, new DynamicResourceExtension("ControlFontSize"))
            }});
        Styles.Add(ShadowButtons.Level = new Style(x => x.Is<Button>()) {
            Setters = { }
        });*/

        //ShadowTexts.Level.Add(new Style(x => x.OfType<TextBlock>()) { });
        //ShadowButtons.Level.Add(new Style(x => x.OfType<Button>()) { });
        //for (int i = 0; i < 3; ++i) {
        //    ShadowTexts.Level.Add(new Style(x => x.OfType<TextBlock>()) { Setters = { shadow[i] } });
        //    ShadowButtons.Level.Add(new Style(x => x.OfType<Button>()) { Setters = { shadow[i] } });
        //}
        //var shadowButNo = new Style(x => x.OfType<Button>()) { };

        // yuge Control Text
        Styles.Add(new Style(x => x.Is<TextBlock>().Class("control")) {
            Setters = {
            new Setter(TextElement.FontFamilyProperty, new DynamicResourceExtension("ControlFontFamily")),
            new Setter(TextElement.FontSizeProperty, new DynamicResourceExtension("ControlFontSize"))
            //new Setter(Visual.EffectProperty, new DynamicResourceExtension("TextShadow"))
        }
        });
        Styles.Add(new Style(x => x.Is<TextBox>()) {
            Setters = {
            new Setter(TextElement.FontFamilyProperty, new DynamicResourceExtension("ControlFontFamily")),
            new Setter(TextElement.FontSizeProperty, new DynamicResourceExtension("ControlFontSize"))
        }
        });

        // TODO figure out how to rotate tooltips and combobox selections corectly:
        // rotates tooltips just like that "PopupRoot" one. at 90 degrees: If taller (more lines) then they get show nearly perfectly, if, long lines, they get cut.
        /*Styles.Add(new Style(x => x.Is<ToolTip>()) {
            Setters = {
            new Setter(Visual.ClipToBoundsProperty, false),
            new Setter(Visual.RenderTransformOriginProperty, new RelativePoint(0.5,0.5,RelativeUnit.Relative)),
            new Setter(Visual.RenderTransformProperty, new DynamicResourceExtension("RotateTransform"))
        }
        });*/
        // no effect on tooltips, combobox selectors are still right side up, but instead of opening down from the box, they open upward
        /*Styles.Add(new Style(x => x.Is<Popup>()) {
            Setters = {
            new Setter(Visual.ClipToBoundsProperty, false),
            new Setter(Visual.RenderTransformOriginProperty, new RelativePoint(0.5,0.5,RelativeUnit.Relative)),
            new Setter(Visual.RenderTransformProperty, new DynamicResourceExtension("RotateTransform"))
        }
        });*/
        // this would also rotate tooltips (twice if alreadyrotating "ToolTip" style), and it crashes combobo selectors
        /*Styles.Add(new Style(x => x.Is<PopupRoot>()) {
            Setters = {
            new Setter(Visual.RenderTransformOriginProperty, new RelativePoint(0.5,0.5,RelativeUnit.Relative)),
            new Setter(Visual.RenderTransformProperty, new DynamicResourceExtension("RotateTransform"))
        }
        });*/

        // smol Info Text
        Styles.Add(new Style(x => x.Is<TextBlock>().Class("info")) {
            Setters = {
            new Setter(TextElement.FontFamilyProperty, new DynamicResourceExtension("InfoFontFamily")),
            new Setter(TextElement.FontSizeProperty, new DynamicResourceExtension("InfoFontSize"))
        }
        });
        // stop buttons from shrinking when clicked
        Styles.Add(new Style(x => x.OfType<ToggleButton>().Class(":pressed")) {
            Setters = {
            new Setter(Visual.RenderTransformProperty, null)
        }
        });

        // FlatComboBox
        // TODO figure out a way to make it more compact, there's a big gap in the contentPresenter
        var flatComboStyle = new Style(x => x.OfType<ComboBox>().Class("flat")) {
            Setters = {
        new Setter(ComboBox.BackgroundProperty, Brushes.Transparent),
        new Setter(ComboBox.BorderThicknessProperty, new Thickness(0)),
        new Setter(ComboBox.PaddingProperty, new Thickness(0))
            }
        };
        Styles.Add(flatComboStyle);

        // setup cached tuple converter
        //AppSave.tupleOptions.Converters.Add(new GeneralTupleConverter());
        // Load Locales:
        FindLocales();
        // Prepare IoPanel Type dictionaries
        foreach (var t in typeof(IoPanel).Assembly.GetTypes()) 
            if(!t.IsAbstract && typeof(IoPanel).IsAssignableFrom(t)) 
                PTypes[t.Name] = t;
        // Prepare FilterType/IoDock Type dictionaries
        foreach (var t in typeof(FilterType).Assembly.GetTypes())
            if (!t.IsAbstract && typeof(FilterType).IsAssignableFrom(t))
                ATypes[t.Name] = t;
        foreach (var t in typeof(IoDock).Assembly.GetTypes())
            if (!t.IsAbstract && typeof(IoDock).IsAssignableFrom(t))
                ATypes[t.Name] = t;
        // Prepare Enum dictionaries
        foreach (var t in typeof(IoPanel).Assembly.GetTypes())
            if (t.IsEnum && t.Namespace?.StartsWith("IoListWallet") == true)
                ETypes[t.Name] = t;
        // Load save files

        if (Current is Application a)
            if (a.ApplicationLifetime is ISingleViewApplicationLifetime s) 
                DPro.InterfaceMode = IMode.Fullscreen; // Only fullscreen allowed in SingleView, make it default
            else if (a.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                DPro.InterfaceMode = IMode.MultiWindow; // Any displaymode allowed, but MultiWindow is native for desktop, make it default
            } else throw new("This lifetime is not supported: " + a.ApplicationLifetime?.ToString()); // IControlledApplicationLifetime too?

        LoadProfiles();
        
        if (GetProfile == null)
            throw new("Any profile must be logged in! Something must have gone horribly wrong with the save files.");
        if (AppMainMenu == null)
            throw new("No MaainMenu was spawned!");

        base.OnFrameworkInitializationCompleted();
        
        // Async updater:
        timer.Tick += Tick;
        deltaTime = Stopwatch.StartNew();
    }
    internal static IClassicDesktopStyleApplicationLifetime? Desktop;
    internal static ISingleViewApplicationLifetime? SingleView;
    internal static Size Available { get; private set; }
    internal static Size AvailableRotated { get => ((GetProfile?.DisplayRotation ?? DPro.DisplayRotation) & 1) == 1 ? new(Available.Height, Available.Width) : Available; }
    internal static void SetAvailable() {
        Available = Current is not Application a ? new Size(120, 120)
            : a.ApplicationLifetime is ISingleViewApplicationLifetime s
                ? s.MainView?.Bounds is Rect r
                    ? new(r.Width, r.Height) : new Size(120, 120)
            : a.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow?.Screens.ScreenFromWindow(desktop.MainWindow) is Avalonia.Platform.Screen screen
                    ? new(screen.Bounds.Size.Width, screen.Bounds.Size.Height) : new Size(120, 120)
            : new Size(120, 120);
    }
    internal static void RefreshSizes() {
        if (GetProfile is not IoProfile profile)
            return;
        RefreshAll();
        foreach (var o in profile.Pan.LoadedPanels)
            o.Value.UpdateMinSize(true, true);
    }
    internal static void InitApp() {
        var current = Current ?? throw new("Failed StartApp.Current");
        Desktop = current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        SingleView = current.ApplicationLifetime as ISingleViewApplicationLifetime;
        FakeMainPanel();
        SetAvailable(); // setup screen size info
    }
    internal static void FakeMainPanel() {
        Desktop?.MainWindow = null;
        SingleView?.MainView = null;
        AppFakePanel?.Content = null;
        AppFakePanel = new();
        SingleView?.MainView = AppFakePanel;
        if (Desktop != null) {
            if (FakeWindow == null) {
                FakeWindow = new Window {
                    Width = 120,
                    Height = 120,
                    Content = AppFakePanel,
                    CanResize = false,
                    IsVisible = false,
                    WindowState = WindowState.Minimized,
                    ShowInTaskbar = false
                    //Width = 120 // i usedthis to confirm the extra window is realyl fakewindow
                };
                FakeWindow?.Show();
            } else FakeWindow.Content = AppFakePanel;
            Desktop.MainWindow = FakeWindow;
        }
    }
    internal static void InitProfile() {
        var profile = GetProfile ?? throw new("Failed StartApp.GetProfile");
        var current = Current ?? throw new("Failed StartApp.Current");
        current.Resources["InfoFontFamily"] = new FontFamily(profile.InfoFont);
        current.Resources["InfoFontSize"] = profile.InfoFontSize;
        current.Resources["ControlFontFamily"] = new FontFamily(profile.ControlFont);
        current.Resources["ControlFontSize"] = profile.ControlFontSize;
        current.Resources["RotateTransform"] = new RotateTransform(profile.DisplayRotation * 90);
    }
    internal static void StartApp() {
        var profile = GetProfile ?? throw new("Failed StartApp.GetProfile");
        var current = Current ?? throw new("Failed StartApp.Current");
        AppFakePanel?.Content = null;
        AppFakePanel = null;
        if (Desktop != null) {
            // windows are allowed - use whatever selected DisplayMode
            if (profile.InterfaceMode == IMode.Fullscreen) {
                // attach to top-level visual
                Desktop.MainWindow = DesktopFullscreen = new Window {
                    Content = AppMainPanel,
                    WindowState = WindowState.FullScreen,
                    CanResize = false,
                    ShowInTaskbar = true,
                };  
                DesktopFullscreen.Show();
            } else 
                Desktop.MainWindow = AppMainWindow;
        } else if (SingleView != null) 
            SingleView.MainView = AppMainPanel;
         
        else throw new("this ApplicationLifetime type is not supported yet: " + (current.ApplicationLifetime?.ToString() ?? ""));
    }
    internal static void AppClose() {
        if (AppRunning) {
            if(SingleView == null)
                AppRunning = false; // SingleView just Saves instead of exits
            foreach (var p in GetProfile?.Pan.LoadedPanels ?? [])
                p.Value.Snapshot();
            SaveApp();
            SavePro();
            SaveAll(); //  save all dirtied networks and wallets and transactions
            SavePan();
            if(GetDisplayMode == IMode.MultiWindow)
                SaveWin();
            Desktop?.Shutdown();
        }
    }
    #endregion

    #region PanelManager
    internal static IoPanel? FromK((Type type, string key)? k)
        => k == null ? null : GetPanel((k.Value.type, k.Value.key), null, 0, false);
    internal static IoPanel? FromK(string? k) 
        => k == null ? null : GetPanel(k,0,false);
    internal static void Register(string key, IoPanel panel) {
        GetProfile?.Pan.LoadedPanels[key] = panel;
        IoPanel.Dirty();
    }
    // If you are putting OpenPanel or GetPanel into EnqueueTask, make sure to give them all Refresh arguments as false, otherwise it will refresh them twice!
    internal static Dictionary<string, Type> PTypes = []; // IoPanels
    internal static Dictionary<string, Type> ATypes = []; // AbstractTypes (FilterType/IoDock...)
    internal static Dictionary<string, Type> ETypes = []; // Enums
    internal static IoPanel? GetPanel(string key/*, bool NewRefresh = false*/, int page = 0, bool open = false) 
        => IoPanel.SplitK(key) is (Type, string) k ? GetPanel(k, null, page, open) : null;
    internal static IoPanel GetPanel((Type t, string key) key, IoPanel? parent = null, int page = 0, bool open = true) {
        var fullkey = IoPanel.MakeK(key.t, key.key);
        if (!(GetProfile?.Pan.LoadedPanels.TryGetValue(fullkey, out var Panel) ?? false)) {
            Panel = (IoPanel)Activator.CreateInstance(key.t/*, key.key*/)!;
            Panel.Key = key.key;
            Register(fullkey, Panel);
            if (open && Panel.RefreshesWhenOpened() == 0) {
                if (Panel.GetMyMainPanel() == null) {
                    Panel.Open(parent == null ? Panel.TrueParent! : parent);
                    if (parent == null && key.t != typeof(MainMenu))
                        Panel.Close();
                }
                RefreshPanel(Panel, ~0, true);
                Panel.UpdateMinSize(true);
            }
        } else if ((parent != null || key.t == typeof(MainMenu)) /*&& !Panel.IsVisible*/ && open) {
           
            Panel.Open(parent);

            Panel.UpdateMinSize(true);
        }
        if (Panel is IoBookPanel b && page > 0 && open)
            b.Book.Swipe.Page = b.Book.Swipe.PrevPage = (page, 0);
        return Panel;
    }
    // If you are putting OpenPanel or GetPanel into EnqueueTask, make sure to give them all Refresh arguments as false, otherwise it will refresh them twice!
    internal static IoPanel? OpenPanel(IoPanel? parent, Type t, string key = "", int page = 0) {
        if (parent == null && t != typeof(MainMenu))
            return null;

        var Panel = GetPanel((t, key), parent, page, true);
        if (!Panel.IsModal() && (parent?.IsModal() ?? false))
            throw new("OpenPanel attempted to open a non-modal window from a modal parent! Try to close the modal and open it from its non-modal parent instead.");
        //Panel.Open(parent);

        return Panel;
    }
    internal static IoPanel? OpenRefreshPanel(IoPanel? Parent, Type t, string key = "", int page = 0) 
        => EnqueueTask((OpenPanel(Parent, t, key, page) ?? null, ~0), true);
    #endregion

    #region Locale
    internal static string L(string key) 
        => SubLocale.TryGetValue(key, out var subvalue) 
        ? subvalue 
        : Locale.TryGetValue(key, out var value) 
        ? value 
        : (GetProfile?.DebugLocale ?? false) 
        ? "[-" + key + "-]" 
        : key;
    internal static string GetEnumText(Enum value) {
        /*var field = value.GetType().GetField(value.ToString());
        var attr = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .FirstOrDefault() as DescriptionAttribute;

        return attr?.Description ?? value.ToString();*/
        return L(value.ToString());
    }
    #endregion

    #region Threading
    private static object taskLock = new();
    private static ushort threadsRunning;
    private static List<(IoPanel? Panel, int TaskId)> TaskQueue = []; // only put into this in main thread, what should be executed.
    
    internal static ushort AllocMaxTasks { get; private set; } = 0;// How many tasks are currently allocated?

    private List<TaskBin> tasks = [];					// All available tasks
    internal enum TaskState : byte {
        Free = 0,       // The task has not been started yet, or already finished and joined and ready to be started again
        Done = 1,       // The task if finished and ready to join without waiting
        Running = 2     // The task is running
    }
    // If you are putting OpenPanel or GetPanel into this, make sure to give them all Refresh arguments as false, otherwise it will refresh them twice!
    internal static IoPanel? EnqueueTask((IoPanel? Panel, int TaskMask) func, bool forceSync = true) {
        if (func.Panel is not IoPanel fPanel)
            return null;
        int a = fPanel.IsAsync();
        var syncJob = forceSync ? func.TaskMask : func.TaskMask & ~a;
        fPanel.AsyncTask(syncJob, null, null);
        fPanel.FinishAsyncTask(syncJob);
        if ((func.TaskMask &= a) > 0 && !forceSync) {
            //if (fPanel.WillRefreshScreen(TaskMask)) // erase the content, until it's reloaded
            //    fPanel.Content = null;
            TaskQueue.Add(func);  // Add task to queue
        }
        return fPanel;
    }
    internal static ushort RequestFreeTasks(ushort requested, ushort left) {
        ushort r = 0;
        lock (taskLock) {
            threadsRunning += r = (ushort)Math.Max(1, Math.Min(requested, AllocMaxTasks - threadsRunning - left));
        }
        return requested;
    }
    internal static void ReturnTasks(ushort returned) {
        lock (taskLock) {
            threadsRunning -= returned;
        }
    }
    internal static IoPanel? RefreshPanel(IoPanel? panel, int mask, bool forceSync) => panel == null ? null : EnqueueTask((panel, mask), forceSync);
    internal static IoPanel? RefreshPanel(string? key, bool forceSync) => key == null ? null : EnqueueTask((GetPanel(key, 0, true), 1), forceSync);
    internal static IoPanel? RefreshPanel((Type type, string key)? key, bool forceSync) => key == null ? null : EnqueueTask((GetPanel((key.Value.type, key.Value.key), null, 0, true), ~0), forceSync);
    internal static void RefreshAll() {
        var panels = GetProfile?.Pan.LoadedPanels.Values;
        if(panels != null)
            foreach (var panel in panels)
                RefreshPanel(panel, ~0, false);
    }
    // run this in looped thread
    Stopwatch? deltaTime = null;
    long elapsed = 0;
    IoWindow? drag = null;
    IoPanel? LastHighest = null;
    internal static double TipTimer = -1;
    internal static object? TipSource = null;
    private void Tick(object? sender, EventArgs e) {
        var newElapsed = deltaTime?.ElapsedMilliseconds ?? 100;
        var dt = (newElapsed - elapsed) * 0.001f;//MathF.Exp((elapsed - preElapsed) * .01f);
        if (TipTimer > 0)
            if ((TipTimer -= dt) <= 0)
                if (TipSource != null) {
                    ToolTip.SetIsOpen((Control)TipSource, true);
                    TipSource = null;
                }
        elapsed = newElapsed;
        bool IsOwnedBy(WindowBase? who, WindowBase? by) {
            do if (who == by) return true;
            while ((who = who?.Owner) != null);
            return false;
        }
        (IoPanel? Pan, IoWindow? Win) Highest = (null, null);
        void NewHighest(IoPanel ip, (IoPanel? Pan, IoWindow? Win) New) {
            if (New.Win != null) {
                if (Highest.Pan == null || IsOwnedBy(New.Win, Highest.Win)) {
                    Highest.Pan?.rects.Update(0, new(0, 0), new(0, 0));
                    Highest = New;
                } else New.Pan?.rects.Update(0, new(0, 0), new(0, 0));
            } else if(Highest.Pan != null && ip == Highest.Pan) {
                Highest.Pan.rects.Update(0, new(0, 0), new(0, 0));
                Highest = (null, null);
            }
        }
        IBrush? UpdateDock(IoDock d, bool dragging, IoWindow? w, IoWindow? drag, PixelPoint dS/*, bool inside*/) {
            if (d is IoDockPanels p && p.Panel.Count > p.Selected && FromK(p.Panel[p.Selected]) is IoPanel ip)
                NewHighest(ip, ip.UpdateOverlay(
                    FromK(drag?.MyMainPanel.Dock.Original)?.GetTitleColor ?? DPro.TitleColor,
                    dragging || drag != null ? 1000000 : ip.IsCursorOver ? dt * (GetProfile?.UnMaskSpeed ?? DPro.UnMaskSpeed) : dt, 
                    // if dragging or modally disabled - overlay 1 (Hide Content), if cursor over - UnMask (negative alpha), else Mask (MaskOpacity)
                    dragging ? 1 : ip.IsCursorOver && drag == null ? -(GetProfile?.UnMaskTime ?? DPro.UnMaskTime) : .01 * (GetProfile?.MaskOpacity ?? DPro.MaskOpacity),
                    drag == null || IsOwnedBy(w, drag) ? null : w, // TODO drag must be above w! not just not equal!
                    dS));
            if (d is IoDockSplit s) {
                UpdateDock(s.First, dragging, w, drag, dS);
                UpdateDock(s.Second, dragging, w, drag, dS);
            }
            return null;
        }
        if (GetProfile is IoProfile p && p.InterfaceMode != IMode.Fullscreen) {
            if (!(drag?.isDockDragging ?? true)) {
                LastHighest?.DockInto(drag);
                LastHighest = null;
                drag = null;
            }
            foreach (var w in p.Win.OpenedWindows)
                if (w.Value.IsVisible && w.Value.isDockDragging && w.Value.WindowState != WindowState.Maximized)
                    drag = w.Value;
            var dS = drag != null ? drag.PointToScreen(new Point(drag.Width / 2, drag.Height / 2)) : new(0, 0);
            foreach (var w in p.Win.OpenedWindows) if(w.Value.IsVisible)
                UpdateDock(w.Value.MyMainPanel.Dock.TopNonRoot, w.Value.isDockDragging, w.Value, drag == w.Value ? null : drag, dS);
            if (Highest.Pan != null && drag != null)
                (LastHighest = Highest.Pan).UpdateOverlay2(dt, drag.MyMainPanel.Dock.TopNonRoot is IoDockPanels);
            else LastHighest = null;
        } else if (AppDockRoot?.TopNonRoot is IoDock d)
            UpdateDock(d, false, null, null, new(0,0));

        bool finished = true;
        for (var t = (int)AllocMaxTasks; 0 <= --t;) { // Try all the tasks
            var task = tasks[t];
            if (task.State == TaskState.Done) {
                // Task is Done - finish it
                task.parent?.FinishAsyncTask(task.Job);
                task.Join();
                task.State = TaskState.Free;
                --threadsRunning;
            }
            if (task.State == TaskState.Free) {
                if ((GetProfile?.MaxTasks ?? MaxTasks) == AllocMaxTasks) // if task is free and i'm not cleaning the tasks to change their allocation:
                    RunQueue(task); // try to start somethign from the queue
            } else finished = false;
        }
        if (finished) {
            lock (taskLock) {
                while ((GetProfile?.MaxTasks ?? MaxTasks) < AllocMaxTasks)
                    tasks.RemoveAt(--AllocMaxTasks);
                while ((GetProfile?.MaxTasks ?? MaxTasks) > AllocMaxTasks) {
                    tasks.Add(new TaskBin());
                    ++AllocMaxTasks;
                }
            }
        }
    }
    private void RunQueue(TaskBin task) {
        int i = TaskQueue.Count;
        while (0 <= --i) {
            int j = i;
            while (0 <= --j)
                if (TaskQueue[i].Panel == TaskQueue[j].Panel && (TaskQueue[i].TaskId & TaskQueue[j].TaskId) == TaskQueue[i].TaskId) {
                    TaskQueue.RemoveAt(i); // remove those queued tasks that are duplicate or partial tasks of another one in the queue.
                    //int k = i;while (++k < QueueLength) TaskQueue[k - 1] = TaskQueue[k];--QueueLength;
                }
        }
        if (TaskQueue.Count == 0)
            return;
        // TODO also prefer those that are visible first!
        for (i = 0; i < TaskQueue.Count; ++i) {
            bool free = true;
            foreach (var t in tasks) if (t != task && t.State != TaskState.Free && t.parent == TaskQueue[i].Panel && (~TaskQueue[i].TaskId & t.Job) > 0) {
                free = false; // There is already a running job that we must wait for to finish, because it's doing something that this new task wouldn't do
                break;
            }
            if (free) {
                foreach (var t in tasks) if (t.parent == TaskQueue[i].Panel && (TaskQueue[i].TaskId & t.Job) == t.Job) {
                    free = false; // There is a runnign task that will do a subset of stuff that this new task will do,
                    t.cancellationTokenSource.Cancel(); // so we could jsut cancel it, because this new task will refresh all of that anyway
                }
                if (free) {
                    lock (taskLock) {
                        if (threadsRunning < AllocMaxTasks) {
                            // No conflicting tasks for this queued task, so let's start it!
                            var (panel, job) = TaskQueue[i];

                            ++threadsRunning;

                            task.Start(panel, job);
                            TaskQueue.RemoveAt(i);
                        }
                    }
                    return;
                }
            }
        }
    }

    internal class TaskBin {

        private Task? task = null;          // Parallel Tasks
        internal TaskState State = TaskState.Free;   // States of Tasks
        internal int Job = 0;
        internal IoPanel? parent = null;
        internal CancellationTokenSource cancellationTokenSource;
        internal CancellationToken cancel;
        //internal ushort TaskIndex;   // The task index
        
        internal TaskBin() { cancel = (cancellationTokenSource = new CancellationTokenSource()).Token; }
        internal bool IsStillRunning() {
            return State == TaskState.Done ? Join() : State != TaskState.Free;
        }
        internal bool Join() {
            Stop(); // Stop the thread
            State = TaskState.Free; // Mark it as free to be started again
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Start(IoPanel Panel, int Job/*, Action action*/) {
            Stop();
            cancel = (cancellationTokenSource = new CancellationTokenSource()).Token;
            parent = Panel;
            this.Job = Job;
            State = TaskState.Running;
            //task = Task.Run(action);
            task = Task.Run(() => Panel.AsyncTask(Job, cancel, this));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Stop() {
            if (task == null)
                return;
            task.Wait();
            parent = null;
            task = null;
        }
    }
    #endregion
}