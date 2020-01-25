using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;


namespace WpfApp1


{
    // ************************************************************************

    struct TradeOpItem
    {
        public readonly TradeOp Value;
        public TradeOpItem(TradeOp value) { this.Value = value; }

        public override string ToString() { return ToString(Value); }

        public static string ToString(TradeOp value)
        {
            switch (value)
            {
                case TradeOp.Buy:
                    return "Покупка";
                case TradeOp.Sell:
                    return "Продажа";
                case TradeOp.Upsize:
                    return "Наращивание";
                case TradeOp.Close:
                    return "Закрытие";
                case TradeOp.Pause:
                    return "Пауза";
                case TradeOp.Wait:
                    return "Ожидание";
                case TradeOp.Kill:
                    return "Снятие";
                case TradeOp.Cancel:
                    return "Отмена";
            }

            return value.ToString();
        }

        public static TradeOpItem[] GetItems()
        {
            Array values = Enum.GetValues(typeof(TradeOp));
            TradeOpItem[] items = new TradeOpItem[values.Length - 3];

            for (int i = 0; i < items.Length; i++)
                items[i] = new TradeOpItem((TradeOp)values.GetValue(i + 1));

            return items;
        }
    }

    // ************************************************************************

    struct BaseQuoteItem
    {
        public readonly BaseQuote Value;
        public BaseQuoteItem(BaseQuote value) { this.Value = value; }

        public override string ToString() { return ToString(Value, true); }

        public static string ToString(BaseQuote value) { return ToString(value, false); }

        public static string ToString(BaseQuote value, bool detailed)
        {
            switch (value)
            {
                case BaseQuote.Counter:
                    return detailed ? "относительно лучшей встречной котировки" : "Встречная";
                case BaseQuote.Similar:
                    return detailed ? "относительно лучшей попутной котировки" : "Попутная";
                case BaseQuote.Absolute:
                    return detailed ? "указанная мышью в стакане" : "Указанная";
                case BaseQuote.Position:
                    return detailed ? "относительно цены позиции" : "Цена поз.";
            }

            return value.ToString();
        }

        public static BaseQuoteItem[] GetItems()
        {
            Array values = Enum.GetValues(typeof(BaseQuote));
            BaseQuoteItem[] items = new BaseQuoteItem[values.Length - 1];

            for (int i = 0; i < items.Length; i++)
                items[i] = new BaseQuoteItem((BaseQuote)values.GetValue(i + 1));

            return items;
        }
    }

    // ************************************************************************

    struct QtyTypeItem
    {
        public readonly QtyType Value;
        public QtyTypeItem(QtyType value) { this.Value = value; }

        public override string ToString() { return ToString(Value); }

        public static string ToString(QtyType value)
        {
            switch (value)
            {
                case QtyType.Absolute:
                    return "абсолютное значение";
                case QtyType.WorkSize:
                    return "% от рабочего объема";
                case QtyType.Position:
                    return "% от объема позиции";
            }

            return value.ToString();
        }

        public static QtyTypeItem[] GetItems()
        {
            Array values = Enum.GetValues(typeof(QtyType));
            QtyTypeItem[] items = new QtyTypeItem[values.Length - 1];

            for (int i = 0; i < items.Length; i++)
                items[i] = new QtyTypeItem((QtyType)values.GetValue(i + 1));

            return items;
        }
    }

    // ************************************************************************

    struct ClusterBaseItem
    {
        public readonly ClusterBase Value;
        public ClusterBaseItem(ClusterBase value) { this.Value = value; }

        public override string ToString()
        {
            switch (Value)
            {
                case ClusterBase.Time: return "время (секунды)";
                case ClusterBase.Volume: return "объем";
                case ClusterBase.Range: return "диапазон цены";
                case ClusterBase.Ticks: return "кол-во сделок";
                case ClusterBase.Delta: return "модуль дельты";
            }

            return Value.ToString();
        }

        public static ClusterBaseItem[] GetItems()
        {
            Array values = Enum.GetValues(typeof(ClusterBase));
            ClusterBaseItem[] items = new ClusterBaseItem[values.Length - 1];

            for (int i = 0; i < items.Length; i++)
                items[i] = new ClusterBaseItem((ClusterBase)values.GetValue(i + 1));

            return items;
        }
    }

    // ************************************************************************

    struct ClusterViewItem
    {
        public readonly ClusterView Value;
        public ClusterViewItem(ClusterView value) { this.Value = value; }

        public override string ToString()
        {
            switch (Value)
            {
                case ClusterView.Summary: return "суммарный объем";
                case ClusterView.Separate: return "объемы по ask и bid";
                case ClusterView.Delta: return "разность ask и bid";
            }

            return Value.ToString();
        }

        public static ClusterViewItem[] GetItems()
        {
            Array values = Enum.GetValues(typeof(ClusterView));
            ClusterViewItem[] items = new ClusterViewItem[values.Length];

            for (int i = 0; i < items.Length; i++)
                items[i] = new ClusterViewItem((ClusterView)values.GetValue(i));

            return items;
        }
    }

    // ************************************************************************

    struct ClusterFillItem
    {
        public readonly ClusterFill Value;
        public ClusterFillItem(ClusterFill value) { this.Value = value; }

        public override string ToString()
        {
            switch (Value)
            {
                case ClusterFill.Double: return "относительно двойной шкалы";
                case ClusterFill.SingleDelta: return "с раскраской по дельте";
                case ClusterFill.SingleBalance: return "с балансом между ask и bid";
            }

            return Value.ToString();
        }

        public static ClusterFillItem[] GetItems()
        {
            Array values = Enum.GetValues(typeof(ClusterFill));
            ClusterFillItem[] items = new ClusterFillItem[values.Length];

            for (int i = 0; i < items.Length; i++)
                items[i] = new ClusterFillItem((ClusterFill)values.GetValue(i));

            return items;
        }
    }

    // ************************************************************************

    sealed class KbItem : INotifyPropertyChanged
    {
        // --------------------------------------------------------------

        public readonly Key Key;
        public readonly bool OnKeyDown;

        // --------------------------------------------------------------

        OwnAction action;
        int id;
        int priceRatio;

        // --------------------------------------------------------------

        public event PropertyChangedEventHandler PropertyChanged;

        // --------------------------------------------------------------

        public KbItem(Key key, bool onKeyDown, OwnAction action, int id, int priceRatio)
        {
            this.Key = key;
            this.OnKeyDown = onKeyDown;
            this.action = action;
            this.id = id;
            this.priceRatio = priceRatio;
        }

        // --------------------------------------------------------------

        public OwnAction Action
        {
            get { return action; }
            set
            {
                action = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Operation"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Quote"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Quantity"));
                }
            }
        }

        public int Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Id"));
                }
            }
        }

        public int PriceRatio
        {
            get { return priceRatio; }
            set
            {
                if (priceRatio != value)
                {
                    priceRatio = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        // --------------------------------------------------------------

        public static string OpText(OwnAction action)
        {
            if (action.IsStop)
                return "*" + TradeOpItem.ToString(action.Operation);
            else
                return TradeOpItem.ToString(action.Operation);
        }

        public static string QtyText(OwnAction action)
        {
            switch (action.QtyType)
            {
                case QtyType.Absolute:
                    return action.Quantity.ToString("N", cfg.BaseCulture);

                case QtyType.WorkSize:
                    return "Р." + action.Quantity.ToString("N", cfg.BaseCulture) + "%";

                case QtyType.Position:
                    return "П." + action.Quantity.ToString("N", cfg.BaseCulture) + "%";

                default:
                    if (action.Operation == TradeOp.Pause)
                        return action.Quantity.ToString("N", cfg.BaseCulture) + " сек.";
                    else
                        return null;
            }
        }

        // --------------------------------------------------------------

        public string KeyEvent { get { return Key + ", " + (OnKeyDown ? "нажатие" : "отпускание"); } }
        public string Operation { get { return OpText(action); } }
        public string Quantity { get { return QtyText(action); } }

        public string Quote
        {
            get
            {
                switch (action.Operation)
                {
                    case TradeOp.Buy:
                    case TradeOp.Sell:
                    case TradeOp.Upsize:
                    case TradeOp.Close:
                        return BaseQuoteItem.ToString(action.Quote);

                    default:
                        return null;
                }
            }
        }

        public string Value
        {
            get
            {
                switch (action.Operation)
                {
                    case TradeOp.Buy:
                    case TradeOp.Sell:
                    case TradeOp.Upsize:
                    case TradeOp.Close:
                        return (action.IsStop ? "*" : null)
                          + (action.Quote == BaseQuote.Absolute ? null
                          : Price.GetString(action.Value, PriceRatio));

                    default:
                        return null;
                }
            }
        }

        // --------------------------------------------------------------
    }

    // ************************************************************************
}

