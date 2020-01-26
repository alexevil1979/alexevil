// ==========================================================================
//    tmCallbacks.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// ==========================================================================

using WpfApp1.Market.Manager.Internals;

namespace WpfApp1.Market.Manager
{
  partial class TradeManager
  {
    // **********************************************************************

    void TraderReplyHandler(int tid, long oid, string error)
    {
    //  MktProvider.Log.Put("Ответ на транзакцию T" + tid
     //   + ": OID=" + oid + ", " + (error == null ? "ok" : error));

      lock(tlist)
        foreach(Transaction t in tlist)
          if(t.TId == tid)
          {
            if(error == null)
            {
              t.OId = oid;

              switch(t.Operation)
              {
                case TradeOp.Buy:
                  MktProvider.Receiver.PutOwnOrder(
                    new OwnOrder(oid, t.Price, t.Volume, t));
                  break;

                case TradeOp.Sell:
                  MktProvider.Receiver.PutOwnOrder(
                    new OwnOrder(oid, t.Price, -t.Volume, t));
                  break;
              }
            }
            else
            {
              PutError(t, error);
              t.Dispose();

              MktProvider.Receiver.PutOwnOrder(new OwnOrder(oid, t.Price));
            }

            ProcessTList();
            return;
          }
    }

    // **********************************************************************

    void OrderUpdateHandler(long oid, int active, int filled)
    {
      lock(tlist)
      {
        foreach(Transaction t in tlist)
          if(t.OId == oid)
          {
            FilledBalance += filled - t.Filled;
            t.Filled = filled;

         //   MktProvider.Log.Put("Заявка " + oid + ": активно " + active
          //    + ", исполнено " + filled + ", итог " + FilledBalance);

            if(active == 0)
            {
              t.Dispose();
              ProcessTList();

              MktProvider.Receiver.PutOwnOrder(new OwnOrder(oid, t.Price));
            }
            else
              MktProvider.Receiver.PutOwnOrder(new OwnOrder(oid, t.Price, active, t));

            filledBalanceUpdated = true;
            QtyBaseUpdated(QtyType.Position);

            return;
          }
      }
    }

    // **********************************************************************

    void OwnTradeHandler(OwnTrade trade)
    {
    //  MktProvider.Log.Put("Сделка OID=" + trade.OId + " " + trade.Quantity + "*"
     //   + trade.Price + " / P" + Position.Quantity + "*" + Position.AvgPrice);

      Position.PutOwnTrade(trade);
    }

    // **********************************************************************
  }
}
