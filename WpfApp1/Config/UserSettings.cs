using System.Windows;
using System.Windows.Input;

using WpfApp1.XMedia;

namespace WpfApp1
{
    public sealed class UserSettings35
    {
        // **********************************************************************
        // *                              QUIK & DDE                            *
        // **********************************************************************

        public string QuikFolder = @"C:\Program Files\QUIK";
        public bool AcceptAllTrades = false;

        public string DdeServerName = cfg.ProgName;

        // **********************************************************************
        // *                                 Счет                               *
        // **********************************************************************

        public string QuikAccount = "SPBFUTxxxxx";
        public string QuikClientCode = "";

        // **********************************************************************
        // *                              Инструмент                            *
        // **********************************************************************

        public string SecCode = "RIZ2";
        public string ClassCode = "SPBFUT";

        public int PriceRatio = 1;
        public int PriceStep = 1;

        public int Grid1Step = 100;
        public int Grid2Step = 500;

        public int FullVolume = 300;

        // **********************************************************************
        // *                                Стакан                              *
        // **********************************************************************

        public double VQuoteVolumeWidth = 70;
        public double VQuotePriceWidth = 46;

        // **********************************************************************
        // *                     График спреда и лента сделок                   *
        // **********************************************************************

        public double SpreadTickWidth = 5;
        public int SpreadsTickInterval = 500;

        public int TradesTickInterval = 750;

        public int TradeVolume1 = 1;
        public int TradeVolume2 = 10;
        public int TradeVolume3 = 20;
        public double TradeVolume3Div = 1;

        // **********************************************************************
        // *                               Поводырь                             *
        // **********************************************************************

        public GuideSource[] GuideSources = new GuideSource[] {
      new GuideSource("MICEXINDEXCF", "INDX", 0.01, 1, 1) };
        //new GuideSource("SBER", "EQBR", 0.01, 0.2, 1),
        //new GuideSource("GAZP", "EQNE", 0.01, 0.2, 1),
        //new GuideSource("GMKN", "EQBS", 1, 0.2, 1),
        //new GuideSource("LKOH", "EQBR", 0.1, 0.2, 1),
        //new GuideSource("ROSN", "EQNL", 0.01, 0.2, 0.5) };

        public double GuideTickWidth = 4;
        public double GuideTickHeight = 3;
        public int GuideTickInterval = 0;

        // **********************************************************************
        // *                              Настроение                            *
        // **********************************************************************

        public ToneSource[] ToneSources = new ToneSource[] {
      new ToneSource("SBER", "EQBR", 2000, 10000, false, false),
      new ToneSource("GAZP", "EQNE", 2000, 2000, false, false),
      new ToneSource("GMKN", "EQBS", 2000, 800, false, false),
      new ToneSource("LKOH", "EQBR", 2000, 1200, false, false),
      new ToneSource("ROSN", "EQNL", 2000, 1500, false, false) };

        public double ToneMeterHeight = 100;

        // **********************************************************************
        // *                               Кластеры                             *
        // **********************************************************************

        public int Clusters = 3;

        public ClusterBase ClusterBase = ClusterBase.Time;
        public int ClusterSize = 600;

        public ClusterBase ClusterLegend = ClusterBase.Volume | ClusterBase.Time;

        public ClusterView ClusterView = ClusterView.Summary;
        public int ClusterValueFilter = 0;

        public ClusterFill ClusterFill = ClusterFill.Double;
        public int ClusterOpacityDelta = 500;
        public int ClusterFillVolume1 = 1000;
        public int ClusterFillVolume2 = 1500;

        public double ClusterWidth = 51;

        // **********************************************************************
        // *                              Управление                            *
        // **********************************************************************

        public KeyBindings KeyDownBindings = new KeyBindings(
          new Key[] { Key.Up, Key.Down, Key.Right, Key.Left,
        Key.Delete, Key.Tab, Key.Escape, Key.B, Key.S },
          new OwnAction[][] {
        new OwnAction[] {
          new OwnAction(TradeOp.Buy, BaseQuote.Counter, 20, QtyType.WorkSize, 100) },
        new OwnAction[] {
          new OwnAction(TradeOp.Sell, BaseQuote.Counter, 20, QtyType.WorkSize, 100) },
        new OwnAction[] {
          new OwnAction(TradeOp.Upsize, BaseQuote.Similar, 10, QtyType.WorkSize, 100) },
        new OwnAction[] {
          new OwnAction(TradeOp.Close, BaseQuote.Similar, 10, QtyType.WorkSize, 100) },
        new OwnAction[] {
          new OwnAction(TradeOp.Cancel),
          new OwnAction(TradeOp.Wait),
          new OwnAction(TradeOp.Close, BaseQuote.Counter, 500, QtyType.Position, 100) },
        new OwnAction[] {
          new OwnAction(TradeOp.Cancel),
          new OwnAction(TradeOp.Close, BaseQuote.Counter, 500, QtyType.Position, 200) },
        new OwnAction[] {
          new OwnAction(TradeOp.Cancel) },
        new OwnAction[] {
          new OwnAction(TradeOp.Buy, BaseQuote.Absolute, 0, QtyType.WorkSize, 100) },
        new OwnAction[] {
          new OwnAction(TradeOp.Sell, BaseQuote.Absolute, 0, QtyType.WorkSize, 100) } });

        public KeyBindings KeyUpBindings = new KeyBindings(
          new Key[] { Key.Up, Key.Down },
          new OwnAction[][] {
        new OwnAction[] {
          new OwnAction(TradeOp.Cancel),
          new OwnAction(TradeOp.Wait),
          new OwnAction(TradeOp.Close, BaseQuote.Counter, 500, QtyType.Position, 100) },
        new OwnAction[] {
          new OwnAction(TradeOp.Cancel),
          new OwnAction(TradeOp.Wait),
          new OwnAction(TradeOp.Close, BaseQuote.Counter, 500, QtyType.Position, 100) } });

        // **********************************************************************

        public int WorkSize = 1;
        public int WorkSizeDelta = 1;

        public bool MouseEnabled = false;
        public int MouseSlippage = 100;

        public double ViewScale = 1;

        // **********************************************************************

        public Key KeyBlockKey = Key.RightShift;

        public Key KeyCenterSpread = Key.Enter;
        public Key KeyPageUp = Key.PageUp;
        public Key KeyPageDown = Key.PageDown;

        public Key KeyScaleUp = Key.OemPlus;
        public Key KeyScaleDown = Key.OemMinus;

        public Key KeyWorkSizeInc = Key.Add;
        public Key KeyWorkSizeDec = Key.Subtract;

        // **********************************************************************
        // *                        Автозакрытие позиции                        *
        // **********************************************************************

        public int AutoStopOffset = 0;
        public int AutoStopSlippage = 100;
        public int AutoStopTrailStart = 0;
        public int AutoStopTrailOffset = 0;
        public int AutoTakeOffset = 0;

        public bool CancelOnClose = false;

        // **********************************************************************
        // *                          Торговый журнал                           *
        // **********************************************************************

        public int SingleTradeTimeout = 4;
        public bool TradeLogFlush = false;

        public bool ShowTradeLog = false;
        public WindowBounds TradeLogBounds = new WindowBounds(100, 100, 0, 400);

        // **********************************************************************
        // *                         Эмулятор терминала                         *
        // **********************************************************************

        public bool TermEmulation = false;
        public int EmulatorDelayMin = 300;
        public int EmulatorDelayMax = 1000;
        public int EmulatorLimit = 10000;

        // **********************************************************************
        // *                                 Прочее                             *
        // **********************************************************************

        public string FontFamily = "Tahoma, Segoe UI, Microsoft Sans Serif";
        public double FontSize = 10.5;

        public bool WindowTopmost = false;
        public bool ConfirmExit = false;

        public WindowState WindowState = WindowState.Normal;
        public WindowBounds WindowBounds = new WindowBounds(50, 50, 650, 670);

        public bool ShowWorkLog = false;
        public WindowBounds WorkLogBounds = new WindowBounds(100, 100, 700, 300);

        public string RecorderFolder = "C:\\Users\\1\\source\\repos\\WpfApp1\\WpfApp1\\bin\\Debug";
        public WindowLocation RecorderLocation = new WindowLocation(150, 150);
        public WindowLocation PlayerLocation = new WindowLocation(150, 150);

        // **********************************************************************
        // *                               Clone()                              *
        // **********************************************************************

        public UserSettings35 Clone()
        {
            UserSettings35 u = (UserSettings35)MemberwiseClone();

            u.GuideSources = (GuideSource[])GuideSources.Clone();
            u.ToneSources = (ToneSource[])ToneSources.Clone();

            return u;
        }

        // **********************************************************************
    }
}
