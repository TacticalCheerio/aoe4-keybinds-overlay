using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AoE4KeybindsOverlay.Models;
using AoE4KeybindsOverlay.Services.InputHook;
using AoE4KeybindsOverlay.Services.Keybinding;
using AoE4KeybindsOverlay.Services.MatchDetection;
using AoE4KeybindsOverlay.Services.Overlay;
using AoE4KeybindsOverlay.Services.Persistence;
using AoE4KeybindsOverlay.Services.Statistics;
using AoE4KeybindsOverlay.ViewModels;
using AoE4KeybindsOverlay.Views;

namespace AoE4KeybindsOverlay;

/// <summary>
/// Application entry point. Configures DI, loads data, shows the overlay, and manages system tray.
/// </summary>
public partial class App : Application
{
    private const string SettingsFileName = "user_settings.json";

    private IHost? _host;
    private TaskbarIcon? _trayIcon;
    private OverlayWindow? _overlay;

    private static readonly string Aoe4DocumentsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "my games", "Age of Empires IV");

    private static readonly string DefaultProfilesPath = Path.Combine(
        Aoe4DocumentsPath, "keyBindingProfiles");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(
                $"Unhandled UI exception:\n\n{args.Exception}",
                "AoE4 Keybinds Overlay - Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                MessageBox.Show(
                    $"Unhandled exception:\n\n{ex}",
                    "AoE4 Keybinds Overlay - Fatal Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<IInputHookService>(sp =>
                        new GlobalInputHookService(Current.Dispatcher));
                    services.AddSingleton<IKeybindingService, KeybindingService>();
                    services.AddSingleton<IStatisticsService, StatisticsService>();
                    services.AddSingleton<IPersistenceService, JsonPersistenceService>();
                    services.AddSingleton<IOverlayWindowService, OverlayWindowService>();
                    services.AddSingleton<IMatchDetectionService>(sp =>
                        new WarningsLogMatchDetectionService(Aoe4DocumentsPath, Current.Dispatcher));

                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<KeyboardViewModel>();
                    services.AddSingleton<MouseViewModel>();
                    services.AddSingleton<ActiveBindingsViewModel>();
                    services.AddSingleton<StatisticsViewModel>();
                    services.AddSingleton<SettingsViewModel>();

                    services.AddSingleton<OverlayWindow>();
                })
                .Build();

            await _host.StartAsync();

            // Load saved user settings
            var persistence = _host.Services.GetRequiredService<IPersistenceService>();
            var savedSettings = persistence.Load<UserSettings>(SettingsFileName);

            var stats = _host.Services.GetRequiredService<IStatisticsService>();
            await stats.LoadAsync();

            // Apply saved preferences to SettingsViewModel (before profile loading)
            var settingsVm = _host.Services.GetRequiredService<SettingsViewModel>();
            if (savedSettings is not null)
            {
                settingsVm.ApplyUserSettings(savedSettings);
            }

            var keybindings = _host.Services.GetRequiredService<IKeybindingService>();
            settingsVm.ProfilesDirectory = DefaultProfilesPath;
            if (Directory.Exists(DefaultProfilesPath))
            {
                keybindings.LoadProfilesDirectory(DefaultProfilesPath);

                // Populate available profiles list
                foreach (var name in keybindings.AvailableProfileNames)
                {
                    settingsVm.AvailableProfiles.Add(name);
                }

                // Load saved profile or fall back to first available
                var profileToLoad = savedSettings?.SelectedProfileName;
                if (profileToLoad is not null &&
                    keybindings.AvailableProfileNames.Contains(profileToLoad))
                {
                    var profilePath = Path.Combine(DefaultProfilesPath, profileToLoad + ".rkp");
                    if (File.Exists(profilePath))
                    {
                        keybindings.LoadProfile(profilePath);
                    }
                }
                else if (keybindings.AvailableProfileNames.Count > 0)
                {
                    var firstProfile = Directory.GetFiles(DefaultProfilesPath, "*.rkp").FirstOrDefault();
                    if (firstProfile is not null)
                    {
                        keybindings.LoadProfile(firstProfile);
                    }
                }
            }

            if (keybindings.ActiveProfile is not null)
            {
                stats.SetBindings(keybindings.GetAllBindings());
                settingsVm.SelectedProfile = keybindings.ActiveProfile.Name;
            }

            var inputHook = _host.Services.GetRequiredService<IInputHookService>();
            inputHook.Start();
            stats.StartSession();

            // Start match detection (watches AoE4's warnings.log)
            var matchDetection = _host.Services.GetRequiredService<IMatchDetectionService>();
            matchDetection.MatchStarted += (_, args) =>
            {
                stats.StartMatch(args.MatchTypeId, args.SessionId);
            };
            matchDetection.MatchEnded += (_, _) =>
            {
                stats.EndMatch();
            };
            matchDetection.Start();

            var layoutPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Data", "keyboard_layout_us.json");

            if (!File.Exists(layoutPath))
            {
                layoutPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..", "..", "..", "Data", "keyboard_layout_us.json");
            }

            var mainVm = _host.Services.GetRequiredService<MainViewModel>();
            if (File.Exists(layoutPath))
            {
                mainVm.Initialize(layoutPath, settingsVm.KeyboardFormFactor);
            }

            _overlay = _host.Services.GetRequiredService<OverlayWindow>();
            _overlay.DataContext = mainVm;

            // Restore saved window position and size
            if (savedSettings is not null)
            {
                if (savedSettings.WindowWidth.HasValue && savedSettings.WindowHeight.HasValue)
                {
                    _overlay.Width = savedSettings.WindowWidth.Value;
                    _overlay.Height = savedSettings.WindowHeight.Value;
                }
                if (savedSettings.WindowLeft.HasValue && savedSettings.WindowTop.HasValue)
                {
                    _overlay.Left = savedSettings.WindowLeft.Value;
                    _overlay.Top = savedSettings.WindowTop.Value;
                }
                if (savedSettings.IsLocked)
                {
                    _overlay.IsLocked = true;
                }
            }

            _overlay.Show();

            // System tray icon
            SetupTrayIcon();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Startup failed:\n\n{ex}",
                "AoE4 Keybinds Overlay - Startup Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void SetupTrayIcon()
    {
        var contextMenu = new ContextMenu();

        var showHideItem = new MenuItem { Header = "Show/Hide Overlay" };
        showHideItem.Click += (_, _) =>
        {
            if (_overlay is null) return;

            if (_overlay.Visibility == Visibility.Visible)
                _overlay.Hide();
            else
                _overlay.Show();
        };

        var lockUnlockItem = new MenuItem { Header = "Lock Position" };
        lockUnlockItem.Click += (_, _) =>
        {
            if (_overlay is null) return;

            _overlay.IsLocked = !_overlay.IsLocked;
            lockUnlockItem.Header = _overlay.IsLocked ? "Unlock Position" : "Lock Position";
        };

        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => Shutdown();

        contextMenu.Items.Add(showHideItem);
        contextMenu.Items.Add(lockUnlockItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(exitItem);

        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons", "app.ico");
        var icon = File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Application;

        _trayIcon = new TaskbarIcon
        {
            Icon = icon,
            ToolTipText = "AoE4 Keybinds Overlay",
            ContextMenu = contextMenu
        };

        // Double-click tray icon to toggle show/hide
        _trayIcon.TrayMouseDoubleClick += (_, _) =>
        {
            if (_overlay is null) return;

            if (_overlay.Visibility == Visibility.Visible)
                _overlay.Hide();
            else
                _overlay.Show();
        };
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();

        if (_host is not null)
        {
            // Save user settings
            try
            {
                var settingsVm = _host.Services.GetService<SettingsViewModel>();
                var persistence = _host.Services.GetService<IPersistenceService>();
                if (settingsVm is not null && persistence is not null)
                {
                    var userSettings = settingsVm.ToUserSettings();

                    // Save window position and size
                    if (_overlay is not null)
                    {
                        userSettings.WindowLeft = _overlay.Left;
                        userSettings.WindowTop = _overlay.Top;
                        userSettings.WindowWidth = _overlay.Width;
                        userSettings.WindowHeight = _overlay.Height;
                        userSettings.IsLocked = _overlay.IsLocked;
                    }

                    persistence.Save(SettingsFileName, userSettings);
                }
            }
            catch
            {
                // Best effort save
            }

            var stats = _host.Services.GetService<IStatisticsService>();
            if (stats is not null)
            {
                stats.EndSession();
                await stats.SaveAsync();
            }

            var matchDetection = _host.Services.GetService<IMatchDetectionService>();
            matchDetection?.Stop();
            if (stats is not null && stats.IsInMatch)
                stats.EndMatch();

            var inputHook = _host.Services.GetService<IInputHookService>();
            inputHook?.Stop();

            var overlayService = _host.Services.GetService<IOverlayWindowService>();
            overlayService?.StopTopmostEnforcement();

            _host.Dispose();
        }

        base.OnExit(e);
    }
}
