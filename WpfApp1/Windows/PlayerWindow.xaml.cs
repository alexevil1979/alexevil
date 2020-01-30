// ============================================================================
//  PlayerWindow.xaml.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// ============================================================================

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using WpfApp1.Market;
using WpfApp1.Market.History;
using WpfApp1.Market.History.Internals;


namespace WpfApp1.Windows
{
    sealed partial class PlayerWindow : Window
    {
        // ************************************************************************

        public const string DateTimeFmt = "dd/MM/yyyy HH:mm:ss.fff";

        // ************************************************************************

        sealed class PlayerWrapper : INotifyPropertyChanged
        {
            // --------------------------------------------------------------


            DateTime lastDateTime;

            public event PropertyChangedEventHandler PropertyChanged;
            public readonly Player Player;

            // --------------------------------------------------------------

            public PlayerWrapper(string fn) { Player = new Player(fn); }

            // --------------------------------------------------------------

            public string Info
            {
                get
                {
                    StringBuilder sb = new StringBuilder(128);
                    sb.AppendLine(Player.FilePath);

                    sb.Append("Записан: ");
                    sb.AppendLine(Player.FileHeader.BaseDateTime.ToLocalTime()
                      .ToString(PlayerWindow.DateTimeFmt));

                    sb.Append("Программа: ");
                    sb.AppendLine(Player.FileHeader.RecorderName);

                    sb.Append("Размер файла: ");
                    sb.Append((Player.FileSize / 1024).ToString("N", cfg.BaseCulture));
                    sb.AppendLine(" кб");

                    sb.AppendLine();
                    sb.Append("Данные:");

                    for (int i = 0; i < Player.FileHeader.StreamsCount; i++)
                    {
                        Player.Stream s = Player[i];

                        sb.AppendLine();
                        sb.Append("\x2219 ");

                        switch (s.Header.Type)
                        {
                            case StreamType.Stock:
                                sb.Append("Стакан");
                                break;
                            case StreamType.Ticks:
                                sb.Append("Тики сделок");
                                break;
                            case StreamType.Orders:
                                sb.Append("Свои заявки");
                                break;
                            case StreamType.Trades:
                                sb.Append("Свои сделки");
                                break;
                            case StreamType.Messages:
                                sb.Append("Сообщения");
                                break;
                            default:
                                sb.Append(s.Header.Type.ToString());
                                break;
                        }

                        if (s.Header.Type != StreamType.Messages)
                        {
                            sb.Append(" ");
                            sb.Append(s.Header.Security.ToString());

                            if (s.Header.Type == StreamType.Stock)
                            {
                                sb.Append(" (шаг ");
                                sb.Append((double)s.Header.PriceStep / s.Header.PriceRatio);
                                sb.Append(" пт)");
                            }
                        }
                    }

                    return sb.ToString();
                }
            }

            // --------------------------------------------------------------

            public string FileName { get { return Player.FileName; } }

            // --------------------------------------------------------------

            public double Progress
            {
                get
                {
                    long size = Player.FileSize;
                    return size > 0 ? (double)Player.FilePosition / size : 0;
                }
            }

            // --------------------------------------------------------------

            public string State
            {
                get
                {
                    return Player.IsPlaying ? Player.CurrentDateTime.ToLocalTime()
                      .ToString(PlayerWindow.DateTimeFmt) : Player.Status;
                }
            }

            // --------------------------------------------------------------

            public void Refresh()
            {
                if (PropertyChanged != null)
                {
                    DateTime lastDateTime = Player.CurrentDateTime;

                    if (Player.StatusUpdated || this.lastDateTime != lastDateTime)
                    {
                        this.lastDateTime = lastDateTime;

                        PropertyChanged(this, new PropertyChangedEventArgs("State"));
                        PropertyChanged(this, new PropertyChangedEventArgs("Progress"));
                    }
                }
            }

            // --------------------------------------------------------------
        }

        // **********************************************************************

        ObservableCollection<PlayerWrapper> players;

        // **********************************************************************

        public event Action<UserSettings35> ConfigChecker;

        // **********************************************************************

        public PlayerWindow()
        {
            InitializeComponent();

            players = new ObservableCollection<PlayerWrapper>();
            players.CollectionChanged += players_CollectionChanged;

            fileList.ItemsSource = players;

            Loaded += delegate
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background,
                  new Action(() => { buttonAdd_Click(null, null); }));
            };
        }

        // **********************************************************************

        protected override void OnClosed(System.EventArgs e)
        {
            foreach (PlayerWrapper pw in players)
                pw.Player.Dispose();

            MktProvider.SetMode(false, false);

            base.OnClosed(e);
        }

        // **********************************************************************

        public void Refresh()
        {
            if (buttonStop.IsEnabled)
            {
                bool chk = false;

                foreach (PlayerWrapper pw in players)
                {
                    pw.Refresh();
                    chk |= pw.Player.IsPlaying;
                }

                if (chk)
                {
                    if (buttonPause.IsChecked == true)
                        dateTimePointer.Text = "Пауза";
                    else
                    {
                        Player p = players[0].Player;
                        dateTimePointer.Text = DateTime.Now.Add(p.FileHeader.BaseDateTime
                          - p.BaseDateTime).ToString(DateTimeFmt);
                    }
                }
                else
                    StopPlay();
            }
            else
                foreach (PlayerWrapper pw in players)
                    pw.Refresh();
        }

        // **********************************************************************

        void StartPlay(object sender = null, RoutedEventArgs e = null)
        {
            DateTime utcNow = DateTime.UtcNow;

           // DateTime startDateTime = new DateTime((int)dateYear.Value,
          //    (int)dateMonth.Value, (int)dateDay.Value, (int)timeHour.Value,
           //   (int)timeMin.Value, (int)timeSec.Value, DateTimeKind.Local)
           //   .ToUniversalTime();

            bool startPast = true;

           // foreach (PlayerWrapper pw in players)
          //      if (pw.Player.FileHeader.BaseDateTime < startDateTime)
          //      {
          //          if (startPast)
          //          {
         //               startPast = false;
        //                pw.Player.Start(utcNow, startDateTime);
          //          }
         //       }
         //       else
           //         pw.Player.Start(utcNow, startDateTime);

            buttonStop.IsEnabled = true;

            buttonPause.IsEnabled = true;
            buttonPause.IsChecked = false;

            Refresh();
        }

        // **********************************************************************

        void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            if (buttonPause.IsChecked == true)
                foreach (PlayerWrapper pw in players)
                    pw.Player.Pause();
            else
                foreach (PlayerWrapper pw in players)
                    pw.Player.Continue();
        }

        // **********************************************************************

        void StopPlay(object sender = null, RoutedEventArgs e = null)
        {
            foreach (PlayerWrapper pw in players)
                pw.Player.Stop();

            dateTimePointer.Text = null;

            buttonStop.IsEnabled = false;

            buttonPause.IsEnabled = false;
            buttonPause.IsChecked = false;

            Refresh();
        }

        // **********************************************************************

        void SetActiveStreams(PlayerWrapper pw)
        {
            for (int i = 0; i < pw.Player.FileHeader.StreamsCount; i++)
            {
                Player.Stream s = pw.Player[i];

                switch (s.Header.Type)
                {
                    case StreamType.Stock: s.IsActive = readStock.IsChecked == true; break;
                    case StreamType.Ticks: s.IsActive = readTicks.IsChecked == true; break;
                    case StreamType.Orders: s.IsActive = readOwns.IsChecked == true; break;
                    case StreamType.Trades: s.IsActive = readOwns.IsChecked == true; break;
                    case StreamType.Messages: s.IsActive = readMsgs.IsChecked == true; break;
                }
            }
        }

        // **********************************************************************

        void StreamFlagChanged(object sender, RoutedEventArgs e)
        {
            foreach (PlayerWrapper pw in players)
                SetActiveStreams(pw);
        }

        // **********************************************************************

        void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fd = new System.Windows.Forms.OpenFileDialog();

            fd.Filter = "История торгов QScalp (*." + cfg.HistoryFileExt + ")|*." + cfg.HistoryFileExt;
            fd.RestoreDirectory = true;
            fd.InitialDirectory = cfg.u.RecorderFolder;
            fd.Title = "Добавить файлы для воспроизведения";
            fd.Multiselect = true;

            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StopPlay();

                foreach (string fn in fd.FileNames)
                {
                    bool unique = true;

                    foreach (PlayerWrapper epw in players)
                        if (epw.Player.FilePath == fn)
                        {
                            unique = false;
                            break;
                        }

                    if (unique)
                    {
                        PlayerWrapper pw = new PlayerWrapper(fn);

                        if (players.Count == 0)
                        {
                            DateTime localDateTime = pw.Player.FileHeader.BaseDateTime.ToLocalTime();

                       //     dateYear.Value = localDateTime.Year;
                       //     dateMonth.Value = localDateTime.Month;
                       //     dateDay.Value = localDateTime.Day;
                        //    timeHour.Value = localDateTime.Hour;
                          //  timeMin.Value = localDateTime.Minute;
                        //    timeSec.Value = localDateTime.Second;

                            bool stock = false;
                            bool ticks = false;
                            bool orders = false;
                            bool trades = false;
                            bool messages = false;

                            bool stockExist = false;

                            for (int i = 0; i < pw.Player.FileHeader.StreamsCount; i++)
                            {
                                Player.Stream s = pw.Player[i];

                                switch (s.Header.Type)
                                {
                                    case StreamType.Stock:
                                        stock = true;

                                        if (stockExist)
                                            s.IsActive = false;
                                        {
                                            stockExist = true;

                                            if ((cfg.u.SecCode != s.Header.Security.SecCode
                                              || cfg.u.ClassCode != s.Header.Security.ClassCode
                                              || cfg.u.PriceRatio != s.Header.PriceRatio
                                              || cfg.u.PriceStep != s.Header.PriceStep)
                                              && MessageBox.Show(this, "В добавляемом файле обнаружены данные биржевого стакана\n"
                                              + s.Header.Security + " (шаг " + ((double)s.Header.PriceStep / s.Header.PriceRatio)
                                              + " пт). Настроить привод на этот инструмент?", cfg.ProgName, MessageBoxButton.OKCancel,
                                              MessageBoxImage.Question) == MessageBoxResult.OK)
                                            {
                                                UserSettings35 old = cfg.u.Clone();

                                                cfg.u.SecCode = s.Header.Security.SecCode;
                                                cfg.u.ClassCode = s.Header.Security.ClassCode;
                                                cfg.u.PriceRatio = s.Header.PriceRatio;
                                                cfg.u.PriceStep = s.Header.PriceStep;

                                                cfg.Reinit();

                                                if (ConfigChecker != null)
                                                    ConfigChecker(old);
                                            }
                                        }

                                        break;

                                    case StreamType.Ticks: ticks = true; break;
                                    case StreamType.Orders: orders = true; break;
                                    case StreamType.Trades: trades = true; break;
                                    case StreamType.Messages: messages = true; break;
                                }
                            }

                            readStock.IsChecked = stock;
                            readTicks.IsChecked = ticks;
                            readOwns.IsChecked = orders || trades;
                            readMsgs.IsChecked = messages;

                            players.Add(pw);
                        }
                        else
                        {
                            SetActiveStreams(pw);
                            players.Add(pw);
                        }
                    }
                }
            }
            else if (players.Count == 0)
                MktProvider.SetMode(true, readOwns.IsChecked == true);

            Focus();
        }

        // **********************************************************************

        void buttonRmv_Click(object sender, RoutedEventArgs e)
        {
            PlayerWrapper pw;

            while ((pw = fileList.SelectedItem as PlayerWrapper) != null)
            {
                pw.Player.Dispose();
                players.RemoveAt(fileList.SelectedIndex);
            }
        }

        // **********************************************************************

        void ReadOwnsChanged(object sender, RoutedEventArgs e)
        {
            MktProvider.SetMode(true, readOwns.IsChecked == true);
            StreamFlagChanged(sender, e);
        }

        // **********************************************************************

        void players_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (players.Count > 0)
                buttonStart.IsEnabled = true;
            else
            {
                buttonStart.IsEnabled = false;
                buttonPause.IsEnabled = false;
            }
        }

        // **********************************************************************

        void fileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonRmv.IsEnabled = fileList.SelectedItem != null;
        }

        // **********************************************************************

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !e.IsRepeat)
            {
                if (buttonStop.IsEnabled)
                    StopPlay();
                else
                    this.Close();

                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        // **********************************************************************
    }
}
