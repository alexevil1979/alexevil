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
        int lastCount;



        public MainWindow()
        {
            InitializeComponent();

            folder.Text = cfg.u.RecorderFolder.Length > 0
              ? cfg.u.RecorderFolder : cfg.AsmPath.Remove(cfg.AsmPath.Length - 1);

            lastCount = -1;
            recorder = MktProvider.GetRecorder();
            Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            if (recorder.IsRecording)
            {
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
                  ticks);

                Refresh();
            }
            // создаем стакан
            Thread myThread = new Thread(new ThreadStart(Count));
            myThread.Start(); // запускаем поток на 100 стаканов 2 раза в секунду.
        }

        public  void Count()
        {
            Random rnd = new Random();
            int start = rnd.Next(500, 1000);
            int lq = crstak(start);
            for (int kk = 0; kk < 100; kk++)
            {
                lq = crstak(lq);
                crorder(lq);
                Thread.Sleep(500);
            }
        }

        public void crorder(int pric)
        { 
            long id = 123123;
            Random rnd = new Random();
            int qty = rnd.Next(1, 20);
            recorder.AddOrder(id,pric,qty, recorder);
        }
        public int crstak (int start)
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
            quotes[bid].Type = QuoteType.BestBid;
            Application.Current.Dispatcher.InvokeAsync(() => { textb2.Text = "" ; });

            foreach (var item in quotes)
            {

                Application.Current.Dispatcher.InvokeAsync(() => { textb2.Text = textb2.Text + "\n" + item.Price.ToString() + " " + item.Volume.ToString() + " " + item.Type.ToString(); ; });

           
            }
            //запись стакана
            recorder.AddStock(quotes, new Spread(quotes[ask].Price, quotes[bid].Price), recorder);
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

        private void ButtonFolder_Click(object sender, RoutedEventArgs e)
        {

        }
    }


}
