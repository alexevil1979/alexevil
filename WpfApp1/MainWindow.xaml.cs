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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
   
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

        public MainWindow()
        {
            InitializeComponent();

            folder.Text = cfg.u.RecorderFolder.Length > 0
              ? cfg.u.RecorderFolder : cfg.AsmPath.Remove(cfg.AsmPath.Length - 1);

            lastCount = -1;

            recorder = MktProvider.GetRecorder();
            recorder1 = MktProvider.GetRecorder1();
            recorder2 = MktProvider.GetRecorder2();

            Refresh();
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

        public void CountOrders()
        {
            Random rnd = new Random();
            int rk = rkk;
            Application.Current.Dispatcher.InvokeAsync(() => { textb3.Text = "Ордера"; });
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
                recorder2.AddOrder(id, pric, qty, recorder);
            }
            Application.Current.Dispatcher.InvokeAsync(() => { textb3.Text = textb3.Text + "\n" + pric.ToString() + " " + qty.ToString() ; });



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
            Application.Current.Dispatcher.InvokeAsync(() => { textb2.Text = "Стакан" ; });

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
                recorder1.AddStock(quotes, new Spread(quotes[ask].Price, quotes[bid].Price), recorder);
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

                
            }
            //else if (players.Count == 0)
          //      MktProvider.SetMode(true, readOwns.IsChecked == true);

            Focus();
        }
    }


}
