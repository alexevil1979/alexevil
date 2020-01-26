using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace WpfApp1

{
    // ************************************************************************

    public enum TradeOp { None, Buy, Sell, Upsize, Close, Pause, Wait, Kill, Cancel, Downsize, Reverse }
    public enum BaseQuote { None, Counter, Similar, Absolute, Position }
    public enum QtyType { None, Absolute, WorkSize, Position }
    public enum QuoteType { Unknown, Free, Spread, Ask, Bid, BestAsk, BestBid }
    public enum ClusterView { Summary, Separate, Delta }
    public enum ClusterFill { Double, SingleDelta, SingleBalance }

    [Flags]
    public enum ClusterBase
    {
        None = 0x00, Time = 0x01, Volume = 0x02,
        Range = 0x04, Ticks = 0x08, Delta = 0x10
    }

    public enum MessageType { None, Info, Warning, Error }

    // ************************************************************************

    public sealed class OwnAction
    {
        public readonly TradeOp Operation;

        public readonly BaseQuote Quote;
        public readonly int Value;

        public readonly QtyType QtyType;
        public readonly int Quantity;

        public readonly bool IsStop;
        public readonly int Slippage;

        public readonly int TrailStart;
        public readonly int TrailOffset;

        public OwnAction(TradeOp operation,
          BaseQuote quote = BaseQuote.None,
          int value = 0,
          QtyType qtyType = QtyType.None,
          int quantity = 0,
          bool isStop = false,
          int slippage = 0,
          int trailStart = 0,
          int trailOffset = 0)
        {
            this.Operation = operation;
            this.Quote = quote;
            this.Value = value;
            this.QtyType = qtyType;
            this.Quantity = quantity;
            this.IsStop = isStop;
            this.Slippage = slippage;
            this.TrailStart = trailStart;
            this.TrailOffset = trailOffset;
        }
    }

    // ************************************************************************

    public struct GuideSource
    {
        [XmlAttributeAttribute]
        public string SecCode;

        [XmlAttributeAttribute]
        public string ClassCode;

        [XmlAttributeAttribute]
        public double PriceStep;

        [XmlAttributeAttribute]
        public double Wnew;

        [XmlAttributeAttribute]
        public double Wsrc;

        public string StrSecCode { get { return SecCode; } }
        public string StrClassCode { get { return ClassCode; } }
        public string StrPriceStep { get { return PriceStep.ToString(); } }
        public string StrWnew { get { return Wnew.ToString("N2"); } }
        public string StrWsrc { get { return Wsrc.ToString("N2"); } }

        public GuideSource(string secCode, string classCode, double priceStep, double wnew, double wsrc)
        {
            this.SecCode = secCode;
            this.ClassCode = classCode;
            this.PriceStep = priceStep;
            this.Wnew = wnew;
            this.Wsrc = wsrc;
        }
    }

    // ************************************************************************

    public struct ToneSource
    {
        [XmlAttributeAttribute]
        public string SecCode;

        [XmlAttributeAttribute]
        public string ClassCode;

        [XmlAttributeAttribute]
        public int Interval;

        [XmlAttributeAttribute]
        public int FillVolume;

        [XmlAttributeAttribute]
        public bool Balance;

        [XmlAttributeAttribute]
        public bool Inverse;

        public string StrSecCode { get { return SecCode; } }
        public string StrClassCode { get { return ClassCode; } }
        public string StrInterval { get { return Interval.ToString("N", cfg.BaseCulture); } }
        public string StrFillVolume { get { return FillVolume.ToString("N", cfg.BaseCulture); } }
        public string StrBalance { get { return Balance ? "\x221A" : string.Empty; } }
        public string StrInverse { get { return Inverse ? "\x221A" : string.Empty; } }

        public ToneSource(string secCode, string classCode,
          int interval, int fillVolume, bool balance, bool inverse)
        {
            this.SecCode = secCode;
            this.ClassCode = classCode;
            this.Interval = interval;
            this.FillVolume = fillVolume;
            this.Balance = balance;
            this.Inverse = inverse;
        }
    }

    // ************************************************************************

    public struct WindowBounds
    {
        [XmlAttributeAttribute]
        public double Left;

        [XmlAttributeAttribute]
        public double Top;

        [XmlAttributeAttribute]
        public double Width;

        [XmlAttributeAttribute]
        public double Height;

        public WindowBounds(double left, double top, double width, double height)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
        }

        public void Apply(Window w)
        {
            w.Left = Left;
            w.Top = Top;
            w.Width = Width;
            w.Height = Height;
        }

        public void Read(Window w)
        {
            Left = w.Left;
            Top = w.Top;
            Width = w.Width;
            Height = w.Height;
        }
    }

    // ************************************************************************

    public struct WindowLocation
    {
        [XmlAttributeAttribute]
        public double Left;

        [XmlAttributeAttribute]
        public double Top;

        public WindowLocation(double left, double top)
        {
            this.Left = left;
            this.Top = top;
        }

        public void Apply(Window w)
        {
            w.Left = Left;
            w.Top = Top;
        }

        public void Read(Window w)
        {
            Left = w.Left;
            Top = w.Top;
        }
    }

    // ************************************************************************

    struct Security
    {
        public string SecCode;
        public string ClassCode;

        public Security(string secCode, string classCode)
        {
            this.SecCode = secCode;
            this.ClassCode = classCode;
        }

        public override string ToString()
        {
            if (ClassCode == null)
                return SecCode == null ? "{none}" : SecCode;
            else
                return SecCode + "." + ClassCode;
        }

        public int GetKey()
        {
            return GetKey(SecCode == null ? string.Empty : SecCode,
              ClassCode == null ? string.Empty : ClassCode);
        }

        public static int GetKey(string secCode, string classCode)
        {
            return secCode.GetHashCode() ^ ~classCode.GetHashCode();
        }
    }

    // ************************************************************************
    // *                             Data types                               *
    // ************************************************************************

    struct Message
    {
        public readonly DateTime DateTime;
        public readonly string Text;

        public Message(string text)
        {
            this.DateTime = DateTime.Now;
            this.Text = text;
        }

        public Message(DateTime dateTime, MessageType type, string text)
        {
            this.DateTime = dateTime;
            this.Text = text;
        }
    }

    // ************************************************************************

    struct Quote
    {
        public int Price;
        public int Volume;
        public QuoteType Type;

        public Quote(int price, int volume, QuoteType type)
        {
            this.Price = price;
            this.Volume = volume;
            this.Type = type;
        }
    }

    // ************************************************************************

    struct Spread
    {
        public readonly int Ask;
        public readonly int Bid;

        public Spread(int ask, int bid)
        {
            this.Ask = ask;
            this.Bid = bid;
        }
    }

    // ************************************************************************

    struct Tick
    {
        public int IntPrice;
        public double RawPrice;
        public int Volume;
        public TradeOp Op;
        public DateTime DateTime;
        //public DateTime Received;
    }

    // ************************************************************************

    struct OwnOrder
    {
        public readonly long Id;
        public readonly int Price;

        public readonly bool IsActive;
        public readonly int Quantity;

        public readonly object Tag;

        public OwnOrder(long id, int price)
        {
            this.Id = id;
            this.Price = price;
            this.IsActive = false;
            this.Quantity = 0;
            this.Tag = null;
        }

        public OwnOrder(long id, int price, int quantity, object tag)
        {
            this.Id = id;
            this.Price = price;
            this.IsActive = true;
            this.Quantity = quantity;
            this.Tag = tag;
        }
    }

    // ************************************************************************

    struct OwnTrade
    {
        public readonly long OId;
        public readonly DateTime DateTime;
        public readonly int Price;
        public readonly int Quantity;

        public OwnTrade(long oid, DateTime dateTime, int price, int quantity)
        {
            this.OId = oid;
            this.DateTime = dateTime;
            this.Price = price;
            this.Quantity = quantity;
        }
    }

    // ************************************************************************
    // *                     Data Receiver Interface                          *
    // ************************************************************************

    interface IDataReceiver
    {
        void PutMessage(Message msg);
        void PutStock(Quote[] quotes, Spread spread);
        void PutTick(int skey, Tick tick);
        void PutOwnOrder(OwnOrder order);
        void PutPosition(int quantity, int price);
    }

    // ************************************************************************
}

