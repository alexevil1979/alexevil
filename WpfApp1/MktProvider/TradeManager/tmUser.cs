// =====================================================================
//    tmUser.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =====================================================================

using System.Collections.Generic;
using WpfApp1.Market.Manager.Internals;

namespace WpfApp1.Market.Manager
{
  partial class TradeManager
  {
    // **********************************************************************

    public Transaction ExecAction(string descr, OwnAction action)
    {
      Transaction nt = null;

      if(MktProvider.TraderStatus.CanTrade)
      {
     //   MktProvider.Log.Put(">> " + descr + ": Торговая операция "
       //   + (action.IsStop ? "*" : "") + action.Operation + " @"
       //   + action.Quote.ToString().Substring(0, 1) + action.Value + " "
       //  + action.QtyType.ToString().Substring(0, 1) + action.Quantity);

        lock(tlist)
        {
          if(action.Operation == TradeOp.Cancel)
          {
            if(action.Quote == BaseQuote.Absolute)
            {
              foreach(Transaction t in tlist)
                if(t.Price == action.Value)
                  t.Cancel();
            }
            else
            {
              foreach(Transaction t in tlist)
                t.Cancel();
            }
          }
          else
            tlist.AddLast(nt = new Transaction(descr, action));

          ProcessTList();
        }
      }

      return nt;
    }

    // **********************************************************************

    public void CancelAction(string descr, Transaction t)
    {
     // MktProvider.Log.Put(">> " + descr + ": Отмена операции " + t.Descr
    //    + " " + t.Source.Operation + " T" + t.TId + ", " + t.State);

      lock(tlist)
      {
        t.Cancel();
        ProcessTList();
      }
    }

    // **********************************************************************

    public void MoveOrders(IList<OwnOrder> orders, int price)
    {
      if(orders != null)
        lock(tlist)
        {
          foreach(OwnOrder order in orders)
          {
            Transaction t = order.Tag as Transaction;
            LinkedListNode<Transaction> node;

            if(t != null && t.State != Transaction.States.Disposed && (node = tlist.Find(t)) != null)
            {
            //  MktProvider.Log.Put(">> Перенос заявки " + t.Descr + " "
           //     + t.Source.Operation + " T" + t.TId + " на " + price);

              t.Cancel();

              if(t.TId > 0)
                tlist.AddAfter(node, new Transaction("MvOrder", new OwnAction(
                  t.Operation, BaseQuote.Absolute, price, QtyType.Absolute, t.Volume)));
              else if(t.TId < 0)
              {
                OwnAction a = t.Source;

                if(t.Trailing)
                  tlist.AddAfter(node, new Transaction("MvTrail", new OwnAction(
                    a.Operation, BaseQuote.Absolute, price, a.QtyType, a.Quantity,
                    a.IsStop, a.Slippage)));
                else
                  tlist.AddAfter(node, new Transaction("MvStop", new OwnAction(
                    a.Operation, BaseQuote.Absolute, price, a.QtyType, a.Quantity,
                    a.IsStop, a.Slippage, a.TrailStart, a.TrailOffset)));
              }
            }
          }

          ProcessTList();
        }
    }

    // **********************************************************************

    public void DropState()
    {
   //   MktProvider.Log.Put("Сброс состояния TradeManager");

      lock(tlist)
      {
        foreach(Transaction t in tlist)
          t.Dispose();

        Position.Clear();
        stopOrders.Clear();

        tlist.Clear();
        tlistUpdated = true;

        FilledBalance = 0;
        filledBalanceUpdated = true;
      }

      MktProvider.Receiver.PutOwnOrder(new OwnOrder());
    }

    // **********************************************************************

    public void WorkSizeUpdated()
    {
      lock(tlist) { QtyBaseUpdated(QtyType.WorkSize); }
    }

    // **********************************************************************
  }
}
