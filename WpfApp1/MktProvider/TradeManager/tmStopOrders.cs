// =========================================================================
//   tmStopOrders.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =========================================================================

using System;
using WpfApp1.Market.Manager.Internals;


namespace WpfApp1.Market.Manager
{
  partial class TradeManager
  {
    // **********************************************************************

    static int lastStopId = -1000;

    // **********************************************************************

    void PutStopOwnOrder(Transaction t)
    {
      int quantity;

      switch(t.Source.QtyType)
      {
        case QtyType.Absolute:
          quantity = t.Source.Quantity;
          break;

        case QtyType.WorkSize:
          quantity = cfg.u.WorkSize * t.Source.Quantity / 100;
          break;

        case QtyType.Position:
          quantity = FilledBalance * t.Source.Quantity / 100;
          break;

        default:
          quantity = 0;
          break;
      }

      switch(t.Operation)
      {
        case TradeOp.Buy:
          MktProvider.Receiver.PutOwnOrder(
            new OwnOrder(t.TId, t.Price, quantity, t));
          break;

        case TradeOp.Sell:
          MktProvider.Receiver.PutOwnOrder(
            new OwnOrder(t.TId, t.Price, -quantity, t));
          break;
      }
    }

    // **********************************************************************

    void AddStopOrder(Transaction t)
    {
      t.TId = --lastStopId;
      stopOrders.Add(t);

      PutStopOwnOrder(t);

      if(MktProvider.TicksStatus.DataReceived == DateTime.MinValue)
        MktProvider.Receiver.PutMessage(new Message("Внимание!\nДля работы"
          + " стоп-заявок требуется информация о сделках по рабочему инструменту,"
          + " которая в данный момент отсутствует."));
    }

    // **********************************************************************

    void RmvStopOrder(Transaction t)
    {
      if(stopOrders.Remove(t))
        MktProvider.Receiver.PutOwnOrder(new OwnOrder(t.TId, t.Price));
    }

    // **********************************************************************

    void ActivateStopOrder(Transaction t, OwnAction newAction)
    {
     // MktProvider.Log.Put("Стоп-заявка " + t.Descr + " "
   //     + t.TId + " @" + t.Price + " активирована");

      OwnOrder ownOrder = new OwnOrder(t.TId, t.Price);

      t.Source = newAction;
      t.Execute();
      ProcessTList();

      Position.StopOrderActivated(t);
      MktProvider.Receiver.PutOwnOrder(ownOrder);
    }

    // **********************************************************************

    void TryTrail(Transaction t, int price, int sign)
    {
      OwnAction a = t.Source;
      int newStopPrice;

      if((t.Trailing || (t.Trailing = sign * (t.Price - price) >= a.TrailStart))
        && sign * (t.Price - (newStopPrice = price + sign * a.TrailOffset)) > 0)
      {
        MktProvider.Receiver.PutOwnOrder(new OwnOrder(t.TId, t.Price));

        t.Price = newStopPrice;

        PutStopOwnOrder(t);

      //  MktProvider.Log.Put("Стоп-заявка " + t.Descr + " "
      //    + t.TId + " перемещена на " + t.Price + " по " + price);

        tlistUpdated = true;
      }
    }

    // **********************************************************************

    void LastPriceHandler(int price)
    {
      if(stopOrders.Count > 0)
        lock(tlist)
        {
          int i = 0;

          while(i < stopOrders.Count)
          {
            Transaction t = stopOrders[i];
            OwnAction a = t.Source;

            switch(t.Operation)
            {
              case TradeOp.Buy:

                if(price >= t.Price)
                {
                  ActivateStopOrder(t, new OwnAction(a.Operation, BaseQuote.Absolute,
                    Price.Floor(t.Price + a.Slippage), a.QtyType, a.Quantity));

                  stopOrders.RemoveAt(i);
                }
                else
                {
                  if(a.TrailStart > 0)
                    TryTrail(t, price, 1);

                  i++;
                }

                break;

              case TradeOp.Sell:

                if(price <= t.Price)
                {
                  ActivateStopOrder(t, new OwnAction(a.Operation, BaseQuote.Absolute,
                    Price.Ceil(t.Price - a.Slippage), a.QtyType, a.Quantity));

                  stopOrders.RemoveAt(i);
                }
                else
                {
                  if(a.TrailStart > 0)
                    TryTrail(t, price, -1);

                  i++;
                }

                break;

              default:
                stopOrders.RemoveAt(i);
                MktProvider.Receiver.PutOwnOrder(new OwnOrder(t.TId, t.Price));
                break;
            }
          }
        }
    }

    // **********************************************************************

    void QtyBaseUpdated(QtyType qtyBase)
    {
      foreach(Transaction t in stopOrders)
        if(t.Source.QtyType == qtyBase)
        {
          MktProvider.Receiver.PutOwnOrder(new OwnOrder(t.TId, t.Price));
          PutStopOwnOrder(t);
        }
    }

    // **********************************************************************
  }
}
