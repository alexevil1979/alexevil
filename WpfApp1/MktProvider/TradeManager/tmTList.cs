// ======================================================================
//    tmTList.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// ======================================================================

using System;
using System.Collections.Generic;
using System.Threading;

using WpfApp1.Market.Manager.Internals;
using WpfApp1.ObjItems;

namespace WpfApp1.Market.Manager
{
  partial class TradeManager
  {
    // **********************************************************************

    const string strAskError = "{нет цены ask}";
    const string strBidError = "{нет цены bid}";
    const string strPosError = "{нет цены позиции}";
    const string strVolError = "{нулевой объем}";

    // ----------------------------------------------------------------------

    static void PutExecutionResult(Transaction t, bool show, string result)
    {
      if(result == null)
        result = "ok";
      else if(show)
        PutError(t, result);

     // MktProvider.Log.Put("Исполнение " + t.Operation + " @" + t.Price
      //  + " " + t.Volume + " (" + t.Descr + " " + (t.Source.IsStop ? "*" : "")
     //   + t.Source.Operation + " @" + t.Source.Quote.ToString().Substring(0, 1)
     //   + t.Source.Value + " " + t.Source.QtyType.ToString().Substring(0, 1)
    //    + t.Source.Quantity + "): T" + t.TId + ", " + result);
    }

    // ----------------------------------------------------------------------

    void ExecuteBuy(Transaction t)
    {
      t.Processed();
      t.Operation = TradeOp.Buy;

      switch(t.Source.Quote)
      {
        case BaseQuote.Absolute:
          t.Price = t.Source.Value;
          break;

        case BaseQuote.Counter:
          if(MktProvider.AskPrice == 0)
          {
            PutExecutionResult(t, false, strAskError);
            return;
          }

          t.Price = Price.Floor(MktProvider.AskPrice + t.Source.Value);
          break;

        case BaseQuote.Similar:
          if(MktProvider.BidPrice == 0)
          {
            PutExecutionResult(t, false, strBidError);
            return;
          }

          t.Price = Price.Floor(MktProvider.BidPrice + t.Source.Value);
          break;

        case BaseQuote.Position:
          if(Position.AvgPrice == 0)
          {
            PutExecutionResult(t, false, strPosError);
            return;
          }

          t.Price = Price.Floor(Position.AvgPrice + t.Source.Value);
          break;

        default:
          return;
      }

      if(t.Source.IsStop)
      {
        AddStopOrder(t);
        PutExecutionResult(t, false, null);
      }
      else
      {
        switch(t.Source.QtyType)
        {
          case QtyType.Absolute:
            t.Volume = t.Source.Quantity;
            break;

          case QtyType.WorkSize:
            t.Volume = cfg.u.WorkSize * t.Source.Quantity / 100;
            break;

          case QtyType.Position:
            t.Volume = Math.Abs(FilledBalance) * t.Source.Quantity / 100;
            break;
        }

        if(t.Volume == 0)
        {
          PutExecutionResult(t, false, strVolError);
          return;
        }

        PutExecutionResult(t, true, trader.SendBuyOrder(
          t.Price, t.Volume, out t.TId));
      }
    }

    // ----------------------------------------------------------------------

    void ExecuteSell(Transaction t)
    {
      t.Processed();
      t.Operation = TradeOp.Sell;

      switch(t.Source.Quote)
      {
        case BaseQuote.Absolute:
          t.Price = t.Source.Value;
          break;

        case BaseQuote.Counter:
          if(MktProvider.BidPrice == 0)
          {
            PutExecutionResult(t, false, strBidError);
            return;
          }

          t.Price = Price.Ceil(MktProvider.BidPrice - t.Source.Value);
          break;

        case BaseQuote.Similar:
          if(MktProvider.AskPrice == 0)
          {
            PutExecutionResult(t, false, strAskError);
            return;
          }

          t.Price = Price.Ceil(MktProvider.AskPrice - t.Source.Value);
          break;

        case BaseQuote.Position:
          if(Position.AvgPrice == 0)
          {
            PutExecutionResult(t, false, strPosError);
            return;
          }

          t.Price = Price.Floor(Position.AvgPrice - t.Source.Value);
          break;

        default:
          return;
      }

      if(t.Source.IsStop)
      {
        AddStopOrder(t);
        PutExecutionResult(t, false, null);
      }
      else
      {
        switch(t.Source.QtyType)
        {
          case QtyType.Absolute:
            t.Volume = t.Source.Quantity;
            break;

          case QtyType.WorkSize:
            t.Volume = cfg.u.WorkSize * t.Source.Quantity / 100;
            break;

          case QtyType.Position:
            t.Volume = Math.Abs(FilledBalance) * t.Source.Quantity / 100;
            break;
        }

        if(t.Volume == 0)
        {
          PutExecutionResult(t, false, strVolError);
          return;
        }

        PutExecutionResult(t, true, trader.SendSellOrder(
          t.Price, t.Volume, out t.TId));
      }
    }

    // **********************************************************************

    void KillOrder(long oid)
    {
      string error = trader.KillOrder(oid);

    //  MktProvider.Log.Put("Снятие заявки " + oid
    //    + ": " + (error == null ? "ok" : error));

      if(error != null)
        PutError(null, error);
    }

    // **********************************************************************

    void ProcessTList()
    {
      LinkedListNode<Transaction> next = tlist.First;
      LinkedListNode<Transaction> curr;

      while(next != null)
      {
        curr = next;
        next = next.Next;

        switch(curr.Value.State)
        {
          // --------------------------------------------------------

          case Transaction.States.Execute:
            switch(curr.Value.Source.Operation)
            {
              // -----------------------------

              case TradeOp.Buy:
                ExecuteBuy(curr.Value);

                if(curr.Value.TId == 0)
                {
                  tlist.Remove(curr);
                  curr.Value.Dispose();
                }

                break;

              // -----------------------------

              case TradeOp.Sell:
                ExecuteSell(curr.Value);

                if(curr.Value.TId == 0)
                {
                  tlist.Remove(curr);
                  curr.Value.Dispose();
                }

                break;

              // -----------------------------

              case TradeOp.Upsize:
                if(FilledBalance > 0)
                  ExecuteBuy(curr.Value);
                else if(FilledBalance < 0)
                  ExecuteSell(curr.Value);

                if(curr.Value.TId == 0)
                {
                  tlist.Remove(curr);
                  curr.Value.Dispose();
                }

                break;

              // -----------------------------

              case TradeOp.Close:
                if(FilledBalance > 0)
                  ExecuteSell(curr.Value);
                else if(FilledBalance < 0)
                  ExecuteBuy(curr.Value);

                if(curr.Value.TId == 0)
                {
                  tlist.Remove(curr);
                  curr.Value.Dispose();
                }

                break;

              // -----------------------------

              case TradeOp.Pause:
                if(curr.Value.Timer == null)
                  curr.Value.Timer = new Timer(obj =>
                  {
                    Transaction t = (Transaction)obj;

                    lock(tlist)
                    {
                      t.Dispose();
                      ProcessTList();
                    }
                  }, curr.Value, curr.Value.Source.Quantity * 1000, Timeout.Infinite);

                next = null;

                break;

              // -----------------------------

              case TradeOp.Wait:
                if(curr == tlist.First)
                {
                  tlist.Remove(curr);
                  curr.Value.Dispose();
                }
                else
                  next = null;

                break;

              // -----------------------------

              case TradeOp.Kill:
                foreach(Transaction t in tlist)
                  if(t == curr.Value)
                    break;
                  else
                    t.Cancel();

                tlist.Remove(curr);
                curr.Value.Dispose();

                next = tlist.First;

                break;

              // -----------------------------

              default:
                tlist.Remove(curr);
                curr.Value.Dispose();
                break;

              // -----------------------------
            }
            break;

          // --------------------------------------------------------

          case Transaction.States.Cancel:
            if(curr.Value.TId == 0)
            {
              tlist.Remove(curr);
              curr.Value.Dispose();
            }
            else if(curr.Value.OId > 0)
            {
              curr.Value.Processed();
              KillOrder(curr.Value.OId);
            }
            else if(curr.Value.TId < 0)
            {
              RmvStopOrder(curr.Value);
              tlist.Remove(curr);
              curr.Value.Dispose();
            }
            break;

          // --------------------------------------------------------

          case Transaction.States.Passive:
            break;

          // --------------------------------------------------------

          case Transaction.States.Disposed:
            tlist.Remove(curr);
            break;

          // --------------------------------------------------------
        }
      }

      tlistUpdated = true;
    }

    // **********************************************************************

    public string GetQueueText()
    {
      tlistText.Length = 0;

      if(tlist.Count > 0)
        lock(tlist)
        {
          tlistText.Append("Очередь операций:");

          foreach(Transaction t in tlist)
          {
            tlistText.AppendLine();
            tlistText.Append(KbItem.OpText(t.Source));

            if(t.Price > 0)
            {
              tlistText.Append(" @");
              tlistText.Append(Price.GetRaw(t.Price));
            }

            string qtyText = KbItem.QtyText(t.Source);
            if(qtyText != null)
            {
              tlistText.Append(" ");
              tlistText.Append(qtyText);
            }

            switch(t.State)
            {
              case Transaction.States.Execute:
                tlistText.Append(" ...");
                break;
              case Transaction.States.Cancel:
                tlistText.Append(" - отмена");
                break;
              case Transaction.States.Disposed:
                tlistText.Append(" (отменено)");
                break;
            }
          }
        }
      else
        tlistText.Append("Очередь операций пуста");

      return tlistText.ToString();
    }

    // **********************************************************************
  }
}
