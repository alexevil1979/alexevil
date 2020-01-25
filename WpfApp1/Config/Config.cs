using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace WpfApp1

{
    static class cfg
    {
        // **********************************************************************
        // *                        Constants & Properties                      *
        // **********************************************************************

        public const string ProgName = "QScalp";
        public static readonly string FullProgName;

        // **********************************************************************

        public const int TryConnectInterval = 1000;

        // **********************************************************************

        public static readonly TimeSpan RefreshInterval = new TimeSpan(0, 0, 0, 0, 15);
        public static readonly TimeSpan SbUpdateInterval = new TimeSpan(0, 0, 0, 0, 250);

        // **********************************************************************

        public static StatSettings s { get; private set; }
        public static UserSettings35 u { get; private set; }

        // **********************************************************************

        public static int WorkSecKey { get; private set; }

        public static string MainFormTitle { get; private set; }

        public static Typeface BaseFont { get; private set; }
        public static Typeface BoldFont { get; private set; }

        public static CultureInfo BaseCulture { get; private set; }
        public static NumberFormatInfo PriceFormat { get; private set; }

        public static double QuoteHeight { get; private set; }
        public static double TextTopOffset { get; private set; }
        public static double TextMinWidth { get; private set; }

        // **********************************************************************

        public const Key FKeySaveConf = Key.F2;
        public const Key FKeyLoadConf = Key.F3;
        public const Key FKeyCfgOrExit = Key.F4;
        public const Key FKeyTradeLog = Key.F5;
        public const Key FKeyEmulator = Key.F6;
        public const Key FKeyDropPos = Key.F7;
        public const Key FKeyClearGuide = Key.F8;
        public const Key FKeyClearLevels = Key.F9;
        public const Key FKeyShowMenu = Key.F10;
        public const Key FKeyRecord = Key.F11;
        public const Key FKeyReplay = Key.F12;

        // **********************************************************************

        public const string UserCfgFileExt = "cfg";
        public const string TradeLogFileExt = "csv";
        public const string HistoryFileExt = "qsh";
        public const string FlushScArg = "-FlushStaticConfig";

        // **********************************************************************

        public static readonly string AsmPath;

        public static readonly string ExecFile;
        public static readonly string UserCfgFile;
        public static readonly string StatCfgFile;
        public static readonly string TradeLogFile;


        // **********************************************************************
        // *                             Constructor                            *
        // **********************************************************************

        static cfg()
        {
            // ------------------------------------------------------------

            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            FullProgName = ProgName + " " + ver.Major.ToString() + "." + ver.Minor.ToString();

            // ------------------------------------------------------------

            ExecFile = Assembly.GetExecutingAssembly().Location;
            string fs = ExecFile.Remove(ExecFile.LastIndexOf('.') + 1);

            UserCfgFile = fs + UserCfgFileExt;
            StatCfgFile = fs + "sc";

            AsmPath = fs.Remove(fs.LastIndexOf('\\') + 1);
            TradeLogFile = fs + "trades." + TradeLogFileExt;

            // ------------------------------------------------------------

            BaseCulture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            BaseCulture.NumberFormat.NumberDecimalDigits = 0;

            PriceFormat = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();

            // ------------------------------------------------------------

#if DEBUG
            s = new StatSettings();
            u = new UserSettings35();
            Reinit();
#endif

            // ------------------------------------------------------------
        }


        // **********************************************************************
        // *                          Properties reinit                         *
        // **********************************************************************

        public static void Reinit()
        {
            WorkSecKey = Security.GetKey(cfg.u.SecCode, cfg.u.ClassCode);

            MainFormTitle = u.SecCode.Length > 0 ? u.SecCode + " - " + cfg.FullProgName : cfg.FullProgName;

            PriceFormat.NumberDecimalDigits = (int)Math.Log10(u.PriceRatio);

            BaseFont = new Typeface(
              new FontFamily(u.FontFamily),
              FontStyles.Normal,
              FontWeights.Medium,
              FontStretches.Normal);

            BoldFont = new Typeface(
              BaseFont.FontFamily,
              FontStyles.Normal,
              FontWeights.Bold,
              FontStretches.Normal);

            FormattedText ft = new FormattedText(
              "8", BaseCulture, FlowDirection.LeftToRight, BaseFont, u.FontSize, s.QuoteTextBrush);

            QuoteHeight = Math.Ceiling(ft.Extent + s.TextVMargin * 2);
            TextTopOffset = ft.Baseline - ft.Extent / 2;
            TextMinWidth = ft.MinWidth * 1.4;
        }


        // **********************************************************************
        // *                         Static config methods                      *
        // **********************************************************************

        public static void FlushStaticConfig()
        {
            try
            {
                using (Stream fs = new FileStream(StatCfgFile, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(StatSettings));
                    xs.Serialize(fs, new StatSettings());
                }

               // Program.ShowMessage("Файл статической конфигурации создан:\n"
               //   + StatCfgFile, MessageBoxImage.Asterisk);
            }
            catch (Exception e)
            {
               // Program.ShowMessage("Ошибка сохранения файла \'" + StatCfgFile
                //  + "\':\n" + e.Message, MessageBoxImage.Exclamation);
            }
        }

        // **********************************************************************

        public static void LoadStaticConfig()
        {
            if (File.Exists(StatCfgFile))
                try
                {
                    using (Stream fs = File.OpenRead(StatCfgFile))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(StatSettings));
                        s = (StatSettings)xs.Deserialize(fs);
                    }
                }
                catch (Exception e)
                {
                   // Program.ShowMessage("Ошибка загрузки файла \'" + StatCfgFile + "\':\n"
                   //   + e.Message + "\n\nУдалите его или создайте вновь.", MessageBoxImage.Exclamation);

                    s = new StatSettings();
                }
            else
                s = new StatSettings();
        }


        // **********************************************************************
        // *                        User config methods                         *
        // **********************************************************************

        public static void SaveUserConfig(string fn)
        {
            try
            {
                using (Stream fs = new FileStream(fn, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(UserSettings35));
                    xs.Serialize(fs, u);
                }
            }
            catch (Exception e)
            {
               // Program.ShowMessage("Ошибка сохранения конфигурационного файла:\n"
               //   + e.Message, MessageBoxImage.Exclamation);
            }
        }

        // **********************************************************************

        public static void LoadUserConfig(string fn)
        {
            try
            {
                using (Stream fs = File.OpenRead(fn))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(UserSettings35));
                    u = (UserSettings35)xs.Deserialize(fs);
                }

                Reinit();
            }
            catch (Exception e)
            {
                if (!(u == null && e is FileNotFoundException))
                   // Program.ShowMessage("Ошибка загрузки конфигурационного файла:\n" + e.Message
                   //   + "\nИспользованы исходные настройки.", MessageBoxImage.Exclamation);

                if (u == null)
                {
                    u = new UserSettings35();
                    Reinit();
                }
            }
        }

        // **********************************************************************
    }
}

