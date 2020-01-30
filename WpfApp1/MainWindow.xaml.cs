using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WpfApp1.History;
using System.Windows.Shapes;
using WpfApp1.Market;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using WpfApp1.Market.History.Internals;
using WpfApp1.Windows;

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



namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

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

    sealed class NullReceiver : IDataReceiver
    {
        void IDataReceiver.PutMessage(Message msg) { }
        void IDataReceiver.PutStock(Quote[] quotes, Spread spread) { }
        void IDataReceiver.PutTick(int skey, Tick tick) { }
        void IDataReceiver.PutOwnOrder(OwnOrder order) { }
        void IDataReceiver.PutPosition(int quantity, int price) { }
    }
    public partial class MainWindow : Window
    {
        const string dtFmt = "HH:mm:ss.fff";

        Recorder recorder;
        Recorder recorder1;
        Recorder recorder2;
        int lastCount;
        int rkk=0;

        public Thread myThreadStock;
        public Thread myThreadOrders;
        public static MainWindow Instance { get; private set; } // тут будет форма
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            folder.Text = cfg.u.RecorderFolder.Length > 0
              ? cfg.u.RecorderFolder : cfg.AsmPath.Remove(cfg.AsmPath.Length - 1);

            lastCount = -1;

            recorder = MktProvider.GetRecorder();
            recorder1 = MktProvider.GetRecorder1();
            recorder2 = MktProvider.GetRecorder2();

            Refresh();
            players = new ObservableCollection<PlayerWrapper>();
            players.CollectionChanged += players_CollectionChanged;

            fileList.ItemsSource = players;

            Loaded += delegate
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background,
                  new Action(() => { ButtonFolder_Click(null, null); }));
            };
        }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (radio1.IsChecked== true) {
                rkk = 1;
            if (recorder.IsRecording)
            {

                if (writeStock.IsChecked == true)
                {
                    myThreadStock.Abort();
                }
                if (writeOrders.IsChecked == true)
                {
                    myThreadOrders.Abort();
                }
                recorder.Stop();
                Refresh();
            }
            else
            {
                lastCount = -1;

                HashSet<Security> ticks = new HashSet<Security>();

                if (writeTicks.IsChecked == true)
                    ticks.Add(new Security(cfg.u.SecCode, cfg.u.ClassCode));

                if (writeGuide.IsChecked == true)
                    foreach (GuideSource src in cfg.u.GuideSources)
                        ticks.Add(new Security(src.SecCode, src.ClassCode));

                if (writeTone.IsChecked == true)
                    foreach (ToneSource src in cfg.u.ToneSources)
                        ticks.Add(new Security(src.SecCode, src.ClassCode));

                recorder.Start(folder.Text,
                  writeStock.IsChecked == true,
                  writeOrders.IsChecked == true,
                  writeTrades.IsChecked == true,
                  writeMsgs.IsChecked == true,
                  ticks,0);

                Refresh();
                if (writeStock.IsChecked == true){
                    // создаем стакан
                    myThreadStock = new Thread(new ThreadStart(CountStock));
                myThreadStock.Start(); // запускаем поток на 100 стаканов 2 раза в секунду.
                }
                if (writeOrders.IsChecked == true)
                {
                    // создаем ордера
                    myThreadOrders = new Thread(new ThreadStart(CountOrders));
                    myThreadOrders.Start(); // запускаем поток на 100 стаканов 2 раза в секунду.
                }
            }
            }
            else
            {
                rkk = 2;
                if (recorder1.IsRecording)
                {

                    if (writeStock.IsChecked == true)
                    {
                        myThreadStock.Abort();
                    }
                    if (writeOrders.IsChecked == true)
                    {
                        myThreadOrders.Abort();
                    }
                    recorder1.Stop();
                    Refresh();
                }
                else
                {
                    lastCount = -1;

                    HashSet<Security> ticks = new HashSet<Security>();

                    if (writeTicks.IsChecked == true)
                        ticks.Add(new Security(cfg.u.SecCode, cfg.u.ClassCode));

                    if (writeGuide.IsChecked == true)
                        foreach (GuideSource src in cfg.u.GuideSources)
                            ticks.Add(new Security(src.SecCode, src.ClassCode));

                    if (writeTone.IsChecked == true)
                        foreach (ToneSource src in cfg.u.ToneSources)
                            ticks.Add(new Security(src.SecCode, src.ClassCode));

                    recorder1.Start(folder.Text,
                      writeStock.IsChecked == true,
                      writeOrders.IsChecked == true,
                      writeTrades.IsChecked == true,
                      writeMsgs.IsChecked == true,
                      ticks,1);

                    Refresh();
                    if (writeStock.IsChecked == true)
                    {
                        // создаем стакан
                        myThreadStock = new Thread(new ThreadStart(CountStock));
                        myThreadStock.Start(); // запускаем поток на 100 стаканов 2 раза в секунду.
                    }

                }

                if (recorder2.IsRecording)
                {

                    if (writeStock.IsChecked == true)
                    {
                        myThreadStock.Abort();
                    }
                    if (writeOrders.IsChecked == true)
                    {
                        myThreadOrders.Abort();
                    }
                    recorder2.Stop();
                    Refresh();
                }
                else
                {
                    lastCount = -1;

                    HashSet<Security> ticks = new HashSet<Security>();

                    if (writeTicks.IsChecked == true)
                        ticks.Add(new Security(cfg.u.SecCode, cfg.u.ClassCode));

                    if (writeGuide.IsChecked == true)
                        foreach (GuideSource src in cfg.u.GuideSources)
                            ticks.Add(new Security(src.SecCode, src.ClassCode));

                    if (writeTone.IsChecked == true)
                        foreach (ToneSource src in cfg.u.ToneSources)
                            ticks.Add(new Security(src.SecCode, src.ClassCode));

                    recorder2.Start(folder.Text,
                    writeStock.IsChecked == true,
                    writeOrders.IsChecked == true,
                    writeTrades.IsChecked == true,
                    writeMsgs.IsChecked == true,
                    ticks,2);
                    Refresh();
                    if (writeOrders.IsChecked == true)
                    {
                        // создаем ордера
                        myThreadOrders = new Thread(new ThreadStart(CountOrders));
                        myThreadOrders.Start(); // запускаем поток на 100 стаканов 2 раза в секунду.
                    }
                }



            }
            // создаем стакан

        }

        public  void CountStock()
        {
           int  rk = rkk;
            Random rnd = new Random();
            int start = rnd.Next(500, 1000);
            int lq = crstak(start, rk);
            for (int kk = 0; kk < 100; kk++)
            {
                lq = crstak(lq, rk);
                Thread.Sleep(500);
            }
        }

        public static void Res(String sss)
        {
            Application.Current.Dispatcher.InvokeAsync(() => { MainWindow.Instance.textb2.Text ="Стакан воспроизведение:\n"+ sss; });
        }
        public static void Res1(String sss)
        {
            Application.Current.Dispatcher.InvokeAsync(() => { MainWindow.Instance.textb3.Text = "Ордера воспроизведение:\n" + sss; });
        }

        public void CountOrders()
        {
            Random rnd = new Random();
            int rk = rkk;
            Application.Current.Dispatcher.InvokeAsync(() => { textb3.Text = "Запись Ордера"; });
            for (int kk = 0; kk < 100; kk++)
            {
                int start = rnd.Next(500, 1000);
                crorder(start,rk);
                Thread.Sleep(500);
            }

        }
        public void crorder(int pric, int rk)
        { 
            long id = 123123;
            Random rnd = new Random();
            int qty = rnd.Next(1, 20);
            if (rk == 1)
            {
                recorder.AddOrder(id, pric, qty, recorder);
            }else
            {
                recorder2.AddOrder(id, pric, qty, recorder2);
            }
            Application.Current.Dispatcher.InvokeAsync(() => { textb3.Text = "\n" + pric.ToString() + " " + qty.ToString() ; });



            }

            public int crstak (int start, int rk)
            {
            //создание стакана
            Random rnd = new Random();
            rnd = new Random();
            Quote[] quotes = new Quote[40];
            int del = rnd.Next(0, 3);
            int delta = 0;
            int lq = start;
            if (del == 1)
            {
                delta = 1;
            }
            else
            {
                delta = -1;
            }
            if (lq == 0)
            {
                lq = start;
            }
            else
            {
                lq = lq + delta;
            }
            int ask = -1, bid = -1;

            for (int i = 0; i < 40; i++)
            {
                int p = lq, av = 0, bv = 0, sc = 0;
                if (i < 20)
                {
                    //  rnd = new Random();
                    av = rnd.Next(10, 1000);
                    p = p - i;
                    ask = i;
                    quotes[i] = new Quote(p, av, QuoteType.Ask);


                }
                else
                {
                    bv = rnd.Next(10, 1000);
                    p = p - i;
                    bid = i;
                    quotes[i] = new Quote(p, bv, QuoteType.Bid);
                }

            }
            if (quotes[0].Price <= quotes[1].Price)
            {
                Trace.WriteLine("стакан перевернут");
            }
            quotes[ask].Type = QuoteType.BestAsk;
            quotes[ask+1].Type = QuoteType.BestBid;
            Application.Current.Dispatcher.InvokeAsync(() => { textb2.Text = "Запись Стакан" ; });

            foreach (var item in quotes)
            {

                Application.Current.Dispatcher.InvokeAsync(() => { textb2.Text = textb2.Text + "\n" + item.Price.ToString() + " " + item.Volume.ToString() + " " + item.Type.ToString(); ; });

           
            }
            //запись стакана
            if (rk == 1)
            {
                recorder.AddStock(quotes, new Spread(quotes[ask].Price, quotes[bid].Price), recorder);
            }
            else
            {
                recorder1.AddStock(quotes, new Spread(quotes[ask].Price, quotes[bid].Price), recorder1);
            }
            return lq;
            
        }

        public int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
        public void Refresh()
        {
            if (lastCount < recorder.WrittenCount)
            {
                lastCount = recorder.WrittenCount;

                lastDateTime.Text = recorder.LastDateTime == DateTime.MinValue
                  ? "-" : recorder.LastDateTime.ToLocalTime().ToString(dtFmt);

                count.Text = recorder.WrittenCount.ToString("N", cfg.BaseCulture);
                fileSize.Text = ((double)recorder.FileSize / 1024).ToString("N", cfg.BaseCulture);
            }

            if (radio1.IsChecked == true)
            {
                if (recorder.StatusUpdated)
                {
                    fileName.Content = recorder.FileName == null ? ". . ." : recorder.FileName;

                    status.Text = recorder.Status;

                    if (recorder.IsRecording)
                    {
                        folder.IsEnabled = false;

                        star.Content = "Остановить";
                        buttonRec.Content = "Остановить";
                    }
                    else
                    {
                        folder.IsEnabled = true;
                        star.Content = "Начать";
                        buttonRec.Content = "Начать";
                    }
                }
            }
            else
            {

                if (recorder1.StatusUpdated)
                {
                    fileName.Content = recorder1.FileName == null ? ". . ." : recorder1.FileName;

                    status.Text = recorder1.Status;

                    if (recorder1.IsRecording)
                    {
                        folder.IsEnabled = false;

                        star.Content = "Остановить";
                        buttonRec.Content = "Остановить";
                    }
                    else
                    {
                        folder.IsEnabled = true;
                        star.Content = "Начать";
                        buttonRec.Content = "Начать";
                    }
                }

            }
        }

        private void ButtonFolder_Click(object sender, RoutedEventArgs e)
        {

        }
        void StopPlay(object sender = null, RoutedEventArgs e = null)
        {
          //  foreach (PlayerWrapper pw in players)
            //    pw.Player.Stop();

          //  dateTimePointer.Text = null;

            buttonStop.IsEnabled = false;

            buttonPause.IsEnabled = false;
            buttonPause.IsChecked = false;

            Refresh();
        }
        ObservableCollection<PlayerWrapper> players;
        public event Action<UserSettings35> ConfigChecker;
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
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

                            dateYear.Content = localDateTime.Year.ToString();
                            dateMonth.Content = localDateTime.Month;
                            dateDay.Content = localDateTime.Day;
                            timeHour.Content = localDateTime.Hour;
                            timeMin.Content = localDateTime.Minute;
                            timeSec.Content = localDateTime.Second;

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
                          //  readTicks.IsChecked = ticks;
                            readOwns.IsChecked = orders || trades;
                        //    readMsgs.IsChecked = messages;

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
        }

        void SetActiveStreams(PlayerWrapper pw)
        {
            for (int i = 0; i < pw.Player.FileHeader.StreamsCount; i++)
            {
                Player.Stream s = pw.Player[i];

                switch (s.Header.Type)
                {
                    case StreamType.Stock: s.IsActive = readStock.IsChecked == true; break;
                  //  case StreamType.Ticks: s.IsActive = readTicks.IsChecked == true; break;
                    case StreamType.Orders: s.IsActive = readOwns.IsChecked == true; break;
                    case StreamType.Trades: s.IsActive = readOwns.IsChecked == true; break;
                  //  case StreamType.Messages: s.IsActive = readMsgs.IsChecked == true; break;
                }
            }
        }
        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            DateTime utcNow = DateTime.UtcNow;

            DateTime startDateTime = new DateTime(int.Parse(dateYear.Content.ToString()),
              int.Parse(dateMonth.Content.ToString()), int.Parse(dateDay.Content.ToString()), int.Parse(timeHour.Content.ToString()),
              int.Parse(timeMin.Content.ToString()), int.Parse(timeSec.Content.ToString()), DateTimeKind.Local)
              .ToUniversalTime();

            bool startPast = true;

            foreach (PlayerWrapper pw in players)
                if (pw.Player.FileHeader.BaseDateTime < startDateTime)
                {
                    if (startPast)
                    {
                        startPast = false;
                        pw.Player.Start(utcNow, startDateTime);
                    }
                }
                else
                    pw.Player.Start(utcNow, startDateTime);

            buttonStop.IsEnabled = true;

            buttonPause.IsEnabled = true;
            buttonPause.IsChecked = false;

            Refresh();
        }

      
    }


}
