// =========================================================================
//   TradeManager.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =========================================================================

using System.Collections.Generic;
using System.Text;

using WpfApp1.Market.Internals;
using WpfApp1.Market.Manager.Internals;

namespace WpfApp1.Market.Manager
{
  sealed partial class TradeManager
  {
    // **********************************************************************

    ITrader trader;

    LinkedList<Transaction> tlist;
    bool tlistUpdated;
    StringBuilder tlistText;

    List<Transaction> stopOrders;

    // **********************************************************************

    public Position Position { get; private set; }

    // **********************************************************************

    public bool QueueUpdated
    {
      get { return tlistUpdated && !(tlistUpdated = false); }
    }

    public int QueueLength { get { return tlist.Count; } }

    // **********************************************************************

    public int FilledBalance { get; private set; }

    bool filledBalanceUpdated;

    public bool FilledBalanceUpdated
    {
      get { return filledBalanceUpdated && !(filledBalanceUpdated = false); }
    }

    // **********************************************************************

    public TradeManager()
    {
      trader = new NullTrader();

      Position = new Position(this);

      tlist = new LinkedList<Transaction>();
      tlistText = new StringBuilder(128);

      stopOrders = new List<Transaction>();
      MktProvider.LastPriceHandler += LastPriceHandler;

      tlistUpdated = true;
      filledBalanceUpdated = true;
    }

    // **********************************************************************

    public bool Attach(ITrader trader)
    {
      if(this.trader != trader)
      {
        this.trader.Deactivate();

        this.trader.TraderReplyHandler -= TraderReplyHandler;
        this.trader.OrderUpdateHandler -= OrderUpdateHandler;
        this.trader.OwnTradeHandler -= OwnTradeHandler;

        //MktProvider.Log.Put("Присоединение TradeManager к " + trader.Name);

        this.trader = trader;

        trader.TraderReplyHandler += TraderReplyHandler;
        trader.OrderUpdateHandler += OrderUpdateHandler;
        trader.OwnTradeHandler += OwnTradeHandler;

        trader.Activate(cfg.u.SecCode, cfg.u.ClassCode);

        return true;
      }
      else
        return trader.Activate(cfg.u.SecCode, cfg.u.ClassCode);
    }

    // **********************************************************************

    static void PutError(Transaction t, string error)
    {
      if(t == null)
        MktProvider.Receiver.PutMessage(new Message("Ошибка торговой системы:\n"
          + error.Trim()));
      else
        MktProvider.Receiver.PutMessage(new Message("Транзакция T" + t.TId + ", "
          + t.Operation + " " + t.Source.Quantity.ToString(cfg.BaseCulture)
          + "*" + Price.GetString(t.Price) + "\nотвергнута торговой системой:\n"
          + error.Trim()));

    }

    // **********************************************************************
  }
}
