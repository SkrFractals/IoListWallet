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
using IoListWallet.ElementControls.Basic;
using IoListWallet.MainControls;
using IoListWallet.MainControls.Panels;
using IoListWallet.IoPanels.Main;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static IoListWallet.AppSave;
using IoListWallet.Structs;
using IoListWallet.IoPanels.Profile;

namespace IoListWallet;

public partial class App : Application {

    private static bool appRunning = true;

    private DispatcherTimer timer = new() {
        Interval = TimeSpan.FromMilliseconds(100),
        IsEnabled = true
    };

    #region Start_End
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {

        // First of my code that is run:

        // DEBUG:
        // testing purposes before having the settings and settings.json:
        //SApp.DisplayMode = DisplayMode.SingleWindow;
        //AppSwipeTime = 0;

        Styles.Add(new Style(x => x.Is<TextBlock>()) {
            Setters =
        {
            new Setter(TextElement.FontFamilyProperty, new DynamicResourceExtension("ControlFontFamily")),
            new Setter(TextElement.FontSizeProperty, new DynamicResourceExtension("ControlFontSize"))
        }
        });
        Styles.Add(new Style(x => x.Is<TextBox>()) {
            Setters =
        {
            new Setter(TextElement.FontFamilyProperty, new DynamicResourceExtension("ControlFontFamily")),
            new Setter(TextElement.FontSizeProperty, new DynamicResourceExtension("ControlFontSize"))
        }
        });

        Styles.Add(new Style(x => x.Is<IoInfoTextBlock>()) {
            Setters =
        {
            new Setter(TextElement.FontFamilyProperty, new DynamicResourceExtension("InfoFontFamily")),
            new Setter(TextElement.FontSizeProperty, new DynamicResourceExtension("InfoFontSize"))
        }
        });
        // stop buttons from shrinking when clicked
        Styles.Add(new Style(x => x.OfType<ToggleButton>().Class(":pressed")) {
            Setters =
        {
            new Setter(Visual.RenderTransformProperty, null)
        }
        });
        // setup cached tuple converter
        //AppSave.tupleOptions.Converters.Add(new GeneralTupleConverter());
        // Load Locales:
        FindLocales();
        // Prepare IoPanel Type dictionaries
        foreach (var t in typeof(IoPanel).Assembly.GetTypes()) 
            if(!t.IsAbstract && typeof(IoPanel).IsAssignableFrom(t)) 
                PTypes[t.Name] = t;
        // Prepare FilterType Type dictionaries
        foreach (var t in typeof(FilterType).Assembly.GetTypes())
            if (!t.IsAbstract && typeof(FilterType).IsAssignableFrom(t))
                FTypes[t.Name] = t;
        // Prepare Enum dictionaries
        foreach (var t in typeof(IoPanel).Assembly.GetTypes())
            if (t.IsEnum && t.Namespace?.StartsWith("IoListWallet") == true)
                ETypes[t.Name] = t;
        // Load save files
        LoadProfiles();

        // Refresh Menu:
        if (AppMainMenu != null) {
            AppMainMenu.SetTitle();
            RefreshPanel(AppMainMenu);
            // Dock Menu:
            AppDockRoot = (IoDockRoot?)(AppMainMenu.ParentDock?.Parent);
            
        }
        base.OnFrameworkInitializationCompleted();
        if (!(GetProfile?.CorrectPassword ?? true))
            OpenRefreshPanel(AppMainMenu, typeof(BookProfile)); // open profile settings to login page if not logged in
        // Async updater:
        timer.Tick += TryRunQueue;
    }
    internal static void StartApp() {
        var profile = GetProfile ?? throw new("Failed StartApp.GetProfile");
        var current = Current ?? throw new("Failed StartApp.Current");
        current.Resources["InfoFontFamily"] = new FontFamily(profile.InfoFont);
        current.Resources["InfoFontSize"] = profile.InfoFontSize;
        current.Resources["ControlFontFamily"] = new FontFamily(profile.ControlFont);
        current.Resources["ControlFontSize"] = profile.ControlFontSize;
        // TODO verify if this actually detects the desktop kind (capable of windows or nay):
        //var key = IoPanel.MakeK(typeof(ListMainMenu), "");
        if (current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            // windows are allowed - use whatever selected DisplayMode
            if (profile.DisplayMode != DisplayMode.Fullscreen) {
                IoWindow NewWindow = new();
                AppMainMenu = new ListMainMenu((IoDockPanel)NewWindow.Dock.Dock.TopNonRoot);
                desktop.MainWindow = NewWindow;
            }
        } else { profile.DisplayMode = DisplayMode.Fullscreen; } // if non-desktop like an android, only allow Fullscreen
        if (GetProfile.DisplayMode == DisplayMode.Fullscreen) {
            IoMainPanel NewMainPanel = new();
            AppMainMenu = new ListMainMenu((IoDockPanel)NewMainPanel.Dock.TopNonRoot);
            // attach to top-level visual
            var host = new Window {
                Content = NewMainPanel,
                WindowState = WindowState.FullScreen,
                CanResize = false,
                ShowInTaskbar = true,
            };
            host.Show();
        }
    }
    internal static void AppClose() {
        if (appRunning && Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            appRunning = false;
            foreach (var p in GetProfile?.Pan.LoadedPanels ?? [])
                p.Value.Snapshot();
            SaveApp();
            SavePro();
            SaveAll(); //  save all dirtied networks and wallets and transactions
            SavePan();
            desktop.Shutdown();
        }
    }
    #endregion

    #region PanelManager
    internal static IoPanel? FromK((Type type, string key)? k)
        => k == null ? null : GetPanel((k.Value.type, k.Value.key));
    internal static IoPanel? FromK(string? k) 
        => k == null ? null : GetPanel(k);
    internal static void Register(string key, IoPanel panel) {
        GetProfile?.Pan.LoadedPanels[key] = panel;
        IoPanel.Dirty();
    }
    // If you are putting OpenPanel or GetPanel into EnqueueTask, make sure to give them all Refresh arguments as false, otherwise it will refresh them twice!
    internal static Dictionary<string, Type> PTypes = [];
    internal static Dictionary<string, Type> FTypes = [];
    internal static Dictionary<string, Type> ETypes = [];
    internal static IoPanel? GetPanel(string key, bool NewRefresh = false) 
        => IoPanel.SplitK(key) is (Type, string) k ? GetPanel(k, NewRefresh) : null;
    internal static IoPanel GetPanel((Type t, string key) key, bool NewRefresh = false) {
        var fullkey = IoPanel.MakeK(key.t, key.key);
        if (!(GetProfile?.Pan.LoadedPanels.TryGetValue(fullkey, out var Panel) ?? false)) {
            Panel = (IoPanel)Activator.CreateInstance(key.t/*, key.key*/)!;
            Panel.Key = key.key;
            Register(fullkey, Panel);
            if (NewRefresh && !Panel.RefreshesWhenOpened())
                RefreshPanel(Panel);
        }
        return Panel;
    }
    // If you are putting OpenPanel or GetPanel into EnqueueTask, make sure to give them all Refresh arguments as false, otherwise it will refresh them twice!
    internal static IoPanel? OpenPanel(IoPanel? Parent, Type t, string key = "", bool OpenRefresh = false, bool NewRefresh = true) {
        if (Parent == null)
            return null;
        var Panel = GetPanel((t, key), NewRefresh && !OpenRefresh);
        Panel.Open(Parent);
        if(OpenRefresh)
            RefreshPanel(Panel);
        return Panel;
    }
    internal static IoPanel? OpenRefreshPanel(IoPanel? Parent, Type t, string key = "") 
        => EnqueueTask((OpenPanel(Parent, t, "", false, false) ?? null, 1));
    #endregion

    #region Locale
    internal static string L(string key) 
        => LoadedLocale.TryGetValue(key, out var value) ? value : (GetProfile?.DebugLocale ?? false) ? "[-" + key + "-]" : key;
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
    private static List<(IoPanel? Panel, ushort TaskId)> TaskQueue = []; // only put into this in main thread, what should be executed.
    
    internal static ushort AllocMaxTasks { get; private set; } = 0;// How many tasks are currently allocated?

    private List<TaskBin> tasks = [];					// All available tasks
    internal enum TaskState : byte {
        Free = 0,       // The task has not been started yet, or already finished and joined and ready to be started again
        Done = 1,       // The task if finished and ready to join without waiting
        Running = 2     // The task is running
    }
    // If you are putting OpenPanel or GetPanel into this, make sure to give them all Refresh arguments as false, otherwise it will refresh them twice!
    internal static IoPanel? EnqueueTask((IoPanel? Panel, ushort TaskId) func) {
        if (func.Panel is not IoPanel fPanel)
            return null;
        if (fPanel.IsAsync()) {
            if (func.TaskId == 1) // task number one is always full refresh, since we called to start it, let's erase the content, until it's reloaded
                fPanel.Content = null;
            TaskQueue.Add(func);  // Add task to queue
        } else {
            fPanel.AsyncTask(func.TaskId, null, null);
            fPanel.FinishAsyncTask(func.TaskId);
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
    internal static IoPanel? RefreshPanel(IoPanel? panel) => panel == null ? null : EnqueueTask((panel, 1));
    internal static IoPanel? RefreshPanel(string? key) => key == null ? null : EnqueueTask((GetPanel(key), 1));
    internal static IoPanel? RefreshPanel((Type type, string key)? key) => key == null ? null : EnqueueTask((GetPanel((key.Value.type, key.Value.key)), 1));
    internal static void RefreshAll() {
        var panels = GetProfile?.Pan.LoadedPanels.Values;
        if(panels != null)
            foreach (var panel in panels)
                RefreshPanel(panel);
    }
    // run this in looped thread
    private void TryRunQueue(object? sender, EventArgs e) {

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
        // 
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
        internal ushort Job = 0;
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
        internal void Start(IoPanel Panel, ushort Job/*, Action action*/) {
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