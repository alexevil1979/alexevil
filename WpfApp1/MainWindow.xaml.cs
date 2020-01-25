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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
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

            Tick newtik = new Tick();

            newtik.DateTime = DateTime.Now;
            newtik.IntPrice = RandomNumber(100,200);
            newtik.Volume = RandomNumber(5, 10);
            newtik.RawPrice=0.95;
            newtik.Op = 0;
            textb1.Text = newtik.DateTime.ToString()+"\n"+ newtik.IntPrice.ToString() + "\n" + newtik.Volume.ToString() + "\n" + newtik.RawPrice.ToString() + "\n" + newtik.Op.ToString();

          

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
                    buttonRec.Content = "Остановить";
                }
                else
                {
                    folder.IsEnabled = true;
                    buttonRec.Content = "Начать";
                }
            }
        }
    }
}
