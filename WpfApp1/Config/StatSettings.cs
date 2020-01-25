
using System.Windows.Media;
using WpfApp1.XMedia;

namespace WpfApp1
{
    public sealed class StatSettings
    {
        // **********************************************************************

        public XBrush BackBrush = new XBrush(Colors.White);

        public XPen HGridLine1Pen = new XPen(Colors.Silver, 2);
        public XPen HGridLine2Pen = new XPen(Colors.DarkGray, 3);

        public XPen VSplitter1Pen = new XPen(Colors.Black, 1);
        public XPen VSplitter2Pen = new XPen(Colors.Silver, 1);
        public XPen VDragLinePen = new XPen(Colors.Gray, 2);

        public double TextHMargin = 2;
        public double TextVMargin = 3;

        // **********************************************************************

        public XBrush QuoteTextBrush = new XBrush(Colors.Black);

        public XBrush FreeQuoteBrush = new XBrush(Colors.White);
        public XBrush SpreadQuoteBrush = new XBrush(Colors.White);
        public XBrush AskQuoteBrush = new XBrush(Colors.LightPink);
        public XBrush BidQuoteBrush = new XBrush(Colors.LightGreen);
        public XBrush BestAskQuoteBrush = new XBrush(Colors.Red);
        public XBrush BestBidQuoteBrush = new XBrush(Colors.LimeGreen);

        public XBrush VolumeFillBrush = new XBrush(Colors.Orange);
        public double VolumeFillMargin = 1;

        public XPen SelectorPen = new XPen(Colors.Gray, 2);

        //public string SpreadString = "\x2022 \x2022 \x2022";
        public string SpreadString = "\x25aa \x25aa \x25aa";

        // **********************************************************************

        public XBrush ClusterTextBrush = new XBrush(Colors.Black);

        public XBrush ClusterCellBrush = new XBrush(Colors.White);
        public XPen ClusterCellPen = new XPen(Colors.Gray, 1);
        public double ClusterHPadding = 2;
        public double ClusterVPadding = 1;

        public XBrush ClusterFillBrush1 = new XBrush(Colors.LightBlue);
        public XBrush ClusterFillBrush2 = new XBrush(Colors.Gold);
        public XBrush ClusterBBalanceBrush = new XBrush(Colors.LightGreen);
        public XBrush ClusterSBalanceBrush = new XBrush(Colors.LightPink);
        public XBrush ClusterBDeltaBrush = new XBrush(Colors.LimeGreen);
        public XBrush ClusterSDeltaBrush = new XBrush(Colors.Red);

        public XBrush ClusterOpenBrush = new XBrush(Colors.White);
        public XBrush ClusterUpBrush = new XBrush(Colors.LimeGreen);
        public XBrush ClusterDownBrush = new XBrush(Colors.Red);
        public XPen ClusterMarkPen = new XPen(Colors.Gray, 1);
        public double ClusterMarkXRatio = 0.25;
        public double ClusterMarkYRatio = 0.33;
        public double ClusterMarkPlace = 1;
        public double ClusterMarkShift = -0.5;

        public XBrush ClusterLegendBrush = new XBrush(Colors.Gainsboro);

        public double ClusterWidthRatio = 1.7;

        // **********************************************************************

        public XBrush TradeTextBrush = new XBrush(Colors.Black);
        public XBrush TradeBuyBrush = new XBrush(Colors.LimeGreen);
        public XBrush TradeSellBrush = new XBrush(Colors.Red);
        public XPen TradeArcPen = new XPen(Colors.Black, 0.5);
        public double TradeBallRadius = 3.5;

        public XBrush ToneBkgBrush = new XBrush(Color.FromArgb(180, 240, 240, 240));
        public XPen ToneBorderPen = new XPen(Colors.Silver, 1);
        public XBrush ToneBullBrush = new XBrush(Colors.LimeGreen);
        public XBrush ToneBullOvrBrush = new XBrush(Colors.LightGreen);
        public XBrush ToneBearBrush = new XBrush(Colors.Red);
        public XBrush ToneBearOvrBrush = new XBrush(Colors.LightPink);
        public XBrush ToneBalanceBrush = new XBrush(Colors.Gainsboro - Color.FromArgb(96, 0, 0, 0));
        public XPen ToneBalancePen = new XPen(Colors.Gray, 1);
        public double ToneBalanceXRatio = 0.80;
        public double ToneBalanceYRatio = 0.25;
        public double ToneMeterPlaceRatio = 1.6;
        public double ToneOverloadMargin = 2;

        public XPen AskGraphPen = new XPen(Colors.Red, 1.5);
        public XPen BidGraphPen = new XPen(Colors.LimeGreen, 1.5);
        public XPen GuideGraphPen = new XPen(Colors.Black, 1.5);

        // **********************************************************************

        public XBrush InfoTextBrush = new XBrush(Colors.Black);

        public XBrush PositionBrush = new XBrush(Colors.Silver);
        public XPen PositionPen = new XPen(Colors.Silver, 1);

        public XBrush ResultPBrush = new XBrush(Colors.PaleGreen);
        public XBrush ResultLBrush = new XBrush(Colors.Salmon);
        public XPen ResultPen = new XPen(Colors.Gray, 1);

        public XBrush OrderBrush = new XBrush(Colors.Aquamarine - Color.FromArgb(127, 0, 0, 0));
        public XPen OrderPen = new XPen(Colors.Cyan, 1);

        public XBrush StopOrderBrush = new XBrush(Colors.Orange - Color.FromArgb(127, 0, 0, 0));
        public XPen StopOrderPen = new XPen(Colors.Orange, 1);

        public XBrush OrderOutlineBrush = new XBrush(Colors.Transparent);
        public XPen OrderOutlinePen = new XPen(Colors.Gray, 2);

        public XBrush OrderSelectedBrush = new XBrush(Colors.Transparent);
        public XPen OrderSelectedPen = new XPen(Colors.Black, 1);

        public double VInfoWidthRatio = 2.7;

        // **********************************************************************

        public XBrush HlMouseBrush = new XBrush(Colors.DimGray);
        public XPen HlMousePen = new XPen(Colors.Transparent, 0);

        public XBrush HlLevelBrush = new XBrush(Color.FromRgb(144, 144, 144));
        public XPen HlLevelPen = new XPen(Colors.Transparent, 0);

        // **********************************************************************

        public XBrush MsgrBodyBrush = new XBrush(Colors.Black);
        public XPen MsgrBodyPen = new XPen(Colors.Transparent, 0);
        public XBrush MsgrLockBrush = new XBrush(Colors.Black);
        public XPen MsgrLockPen = new XPen(Colors.Orange, 4);

        public XBrush MsgrHeaderBrush = new XBrush(Colors.White);
        public XBrush MsgrErrorBrush = new XBrush(Colors.Red);

        public double MsgrTextMargin = 10;
        public double MsgrFontSizeRatio = 1.5;

        public int MsgrShowTime = 5000;
        public double MsgrFadeStep = 0.1;
        public int MsgrFadeTick = 50;

        // **********************************************************************

        public double CenteringStart = 0.2;
        public double CenteringMin = 3;
        public double CenteringDiv = 13;
        public double CenteringDisable = 5;

        public double GuideCenteringStart = 0.05;
        public double GuideCenteringMin = 2;
        public double GuideCenteringDiv = 15;
        public double GuideCenteringShift = 0.25;

        // **********************************************************************

        public double ViewScaleDelta = 0.1;
        public double ViewScaleMin = 0.3;
        public double ViewScaleMax = 3;

        public double LostFocusOpacity = 1;
        public double ManualScrollSize = 0.25;

        // **********************************************************************

        public int DataRcvTimeout = 3000;
        public int EmulatorTickInterval = 50;
        public int ToneDecreaseInterval = 50;

        public int WorkLogDepth = 200;
        public int StatusLogDepth = 5;

        // **********************************************************************
    }
}
