using DC1AP.Logging;
using DC1AP.Models;
using DC1AP.Utils;
using ReactiveUI.Avalonia;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Timers;
using Color = Avalonia.Media.Color;
using LogEventLevel = Serilog.Events.LogEventLevel;

namespace DC1AP.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private string _host;
        private string _slot;
        private string _password;
        private bool _isPaneOpen;
        private ObservableCollection<LogListItem> _logList = new ObservableCollection<LogListItem>();
        private string _clientVersion;
        private string _archipelagoVersion;
        private Color _backgroundColor;
        private Color _textColor;
        private Color _buttonColor;
        private Color _buttonTextColor;
        private string _commandText;
        private string _selectedLogLevel;
        private bool _connectButtonEnabled;
        private ObservableCollection<LogListItem> _hintList = new ObservableCollection<LogListItem>();
        private ObservableCollection<LogListItem> _itemList = new ObservableCollection<LogListItem>();
        private bool _autoscrollEnabled = true;
        private bool _unstuckButtonEnabled;
        private readonly System.Timers.Timer _processingTimer;
        private readonly object _processingLock = new();
        private bool _isProcessingQueue = false;
        private const int MAX_BATCH_SIZE = 25; // Process messages in batches
        private const int TIMER_INTERVAL = 100; // Process queue every 100ms
        private readonly ConcurrentQueue<LogListItem> _messageQueue = new();
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
        }
        public ObservableCollection<string> LogEventLevels { get; private set; } = Enum.GetNames(typeof(LogEventLevel)).ToObservableCollection();
        public string SelectedLogLevel
        {
            get
            {
                var minLevel = LoggerConfig.GetMinimumLevel();
                return LogEventLevels.Single(x => x.ToLower() == LoggerConfig.GetMinimumLevel().ToString().ToLower());
            }
            set
            {
                if (_selectedLogLevel != value)
                {
                    LoggerConfig.SetLogLevel((Serilog.Events.LogEventLevel)Enum.Parse<LogEventLevel>(value));
                    this.RaisePropertyChanged();
                }
            }
        }
        public event EventHandler<ConnectClickedEventArgs> ConnectClicked;
        public event EventHandler<ArchipelagoCommandEventArgs> CommandReceived;
        public event EventHandler UnstuckClicked;
        public bool UnstuckButtonEnabled
        {
            get => this._unstuckButtonEnabled;
            set => this.RaiseAndSetIfChanged(ref _unstuckButtonEnabled, value);
        }
        public bool ConnectButtonEnabled
        {
            get => this._connectButtonEnabled;
            set => this.RaiseAndSetIfChanged(ref _connectButtonEnabled, value);
        }
        public bool AutoscrollEnabled
        {
            get => this._autoscrollEnabled;
            set => this.RaiseAndSetIfChanged(ref _autoscrollEnabled, value);
        }
        public ReactiveCommand<Unit, Unit> ConnectClickedCommand { get; }
        public ReactiveCommand<Unit, Unit> UnstuckClickedCommand { get; }
        public ReactiveCommand<Unit, Unit> TogglePaneCommand { get; }
        public ReactiveCommand<Unit, Unit> CommandSentCommand { get; }

        public ObservableCollection<LogListItem> LogList
        {
            get => _logList;
            set => this.RaiseAndSetIfChanged(ref this._logList, value);
        }

        public ObservableCollection<LogListItem> HintList
        {
            get => _hintList; 
            set => this.RaiseAndSetIfChanged(ref this._hintList, value);
        }

        public ObservableCollection<LogListItem> ItemList
        {
            get => _itemList;
            set => this.RaiseAndSetIfChanged(ref this._itemList, value);
        }

        public string Host
        {
            get => _host;
            set => this.RaiseAndSetIfChanged(ref this._host, value);
        }

        public string Slot
        {
            get => _slot;
            set => this.RaiseAndSetIfChanged(ref this._slot, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref this._password, value);
        }

        public string CommandText
        {
            get => _commandText;
            set => this.RaiseAndSetIfChanged(ref this._commandText, value);
        }

        public string ClientVersion
        {
            get => _clientVersion;
            set => this.RaiseAndSetIfChanged(ref _clientVersion, value);
        }

        public string ArchipelagoVersion
        {
            get => _archipelagoVersion;
            set => this.RaiseAndSetIfChanged(ref this._archipelagoVersion, value);
        }

        public MainWindowViewModel(string archipelagoVersion = "0.6.2")
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                throw new InvalidOperationException("ViewModel must be created on the UI thread");
            }
            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
            ConnectClickedCommand = ReactiveCommand.Create(HandleConnect);
            CommandSentCommand = ReactiveCommand.Create(HandleCommandSent);
            TogglePaneCommand = ReactiveCommand.Create(HandleTogglePane);
            UnstuckClickedCommand = ReactiveCommand.Create(HandleUnstuck);
            ClientVersion = "0.4.2";
            ArchipelagoVersion = archipelagoVersion;

            _processingTimer = new Timer(TIMER_INTERVAL);
            _processingTimer.Elapsed += ProcessMessageQueue;
            _processingTimer.AutoReset = true;
            _processingTimer.Start();

            LoggerConfig.Initialize((e, l) => WriteLine(e, l), (a, l) => WriteLine(a, l));
        }

        //Parameterless constructor required for XAML design time compiler
        public MainWindowViewModel() : this("0.6.4")
        {}

        private void HandleTogglePane()
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                IsPaneOpen = !IsPaneOpen;
            });
        }

        private void HandleCommandSent()
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                CommandReceived?.Invoke(this, new ArchipelagoCommandEventArgs
                {
                    Command = CommandText
                });
                CommandText = string.Empty;
            });
        }

        private void HandleUnstuck()
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                UnstuckClicked?.Invoke(this, EventArgs.Empty);
            });
        }

        private void HandleConnect()
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                ConnectClicked?.Invoke(this, new ConnectClickedEventArgs
                {
                    Host = Host,
                    Slot = Slot,
                    Password = Password
                });
            });
        }

        public void WriteLine(string output, LogEventLevel level)
        {
            _messageQueue.Enqueue(new LogListItem(output, GetColorForLogLevel(level)));
        }

        public void WriteLine(APMessageModel output, LogEventLevel level)
        {
            _messageQueue.Enqueue(new LogListItem(output));
        }

        private Color GetColorForLogLevel(LogEventLevel level)
        {
            Color logColor;
            switch (level)
            {
                case LogEventLevel.Error:
                    logColor = Color.FromRgb(255, 0, 0);
                    break;
                case LogEventLevel.Warning:
                    logColor = Color.FromRgb(255, 255, 0);
                    break;
                case LogEventLevel.Information:
                default:
                    logColor = Color.FromRgb(255, 255, 255);
                    break;
                case LogEventLevel.Debug:
                case LogEventLevel.Verbose:
                    logColor = Color.FromRgb(173, 216, 230);
                    break;
            }
            return logColor;
        }

        private void ProcessMessageQueue(object sender, ElapsedEventArgs e)
        {
            // Prevent multiple concurrent processing
            if (_isProcessingQueue)
                return;


            lock (_processingLock)
            {
                try
                {
                    _isProcessingQueue = true;

                    List<LogListItem> textBatch = new();
                    int processedCount = 0;

                    while (_messageQueue.TryDequeue(out var item) && processedCount < MAX_BATCH_SIZE)
                    {
                        textBatch.Add(item);
                        processedCount++;
                    }

                    if (textBatch.Count > 0)
                    {
                        RxApp.MainThreadScheduler.Schedule(() =>
                        {
                            if (textBatch.Count > 0)
                            {
                                foreach (var item in textBatch)
                                {
                                    LogList.Add(item);
                                }

                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing message queue: {ex.Message}");
                }
                finally
                {
                    _isProcessingQueue = false;
                }
            }
        }

        public override void Dispose()
        {
            _processingTimer?.Stop();
            _processingTimer?.Dispose();
            base.Dispose();
        }
    }
}
