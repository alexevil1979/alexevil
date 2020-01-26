// =======================================================================
//    Position.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =======================================================================

using System;

namespace WpfApp1.Market.Manager.Internals
{
  sealed class Position
  {
    // **********************************************************************

    const string stopDescr = "AutoSL";
    const string takeDescr = "AutoTP";
    const string cancelDescr = "AutoCancel";

    TradeManager tmgr;

    int quantity;
    int pricesum;

    int stopLossPrice;

    Transaction stopLoss;
    Transaction takeProfit;

    bool stopActivated, stopDisposed;
    bool takeActivated, takeDisposed;

    // **********************************************************************

    public int Quantity { get { return quantity; } }
    public int AvgPrice { get; private set; }

    // **********************************************************************

    public Position(TradeManager tmgr) { this.tmgr = tmgr; }

    // **********************************************************************

    public void PutOwnTrade(OwnTrade trade)
    {
      // ------------------------------------------------------------

      int nq = this.quantity + trade.Quantity;

      if(Math.Sign(nq) != Math.Sign(this.quantity))
      {
        // ------------------------------------------------

        if(this.quantity != 0)
        {
         // MktProvider.TradeLog.AddClose(trade.DateTime, -this.quantity,
       //     trade.Price, trade.Price * this.quantity - this.pricesum);

          if(cfg.u.CancelOnClose)
            tmgr.ExecAction(cancelDescr, new OwnAction(TradeOp.Cancel));
        }

        if(nq != 0)
        //  MktProvider.TradeLog.AddOpen(trade.DateTime, nq, trade.Price);

        this.pricesum = trade.Price * nq;

        // ------------------------------------------------

        if(stopLoss != null)
        {
          tmgr.CancelAction(stopDescr, stopLoss);
          stopLoss = null;
        }

        if(takeProfit != null)
        {
          tmgr.CancelAction(takeDescr, takeProfit);
          takeProfit = null;
        }

        stopActivated = false;
        stopDisposed = false;
        takeActivated = false;
        takeDisposed = false;

        // ------------------------------------------------
      }
      else
      {
                if (Math.Sign(trade.Quantity) == Math.Sign(this.quantity))
                { String ddsd = ""; }
                //  MktProvider.TradeLog.AddOpen(trade.DateTime, trade.Quantity, trade.Price);
                else
                {  // MktProvider.TradeLog.AddClose(trade.DateTime, trade.Quantity, trade.Price);
                }
                    this.pricesum += trade.Price * trade.Quantity;
      }

      this.quantity = nq;

      // ------------------------------------------------------------

      if(quantity == 0)
        AvgPrice = 0;
      else
      {
        // ------------------------------------------------

        if(stopLoss != null)
        {
          if(stopLoss.State == Transaction.States.Disposed)
            stopDisposed = true;
          else
            tmgr.CancelAction(stopDescr, stopLoss);

          stopLoss = null;
        }

        if(takeProfit != null)
        {
          if(takeProfit.State == Transaction.States.Disposed)
          {
            takeDisposed = true;
            takeProfit = null;
          }
          else if(takeProfit.OId == trade.OId)
            takeActivated = true;
          else if(!takeActivated)
          {
            tmgr.CancelAction(takeDescr, takeProfit);
            takeProfit = null;
          }
        }

        // ------------------------------------------------

        AvgPrice = pricesum / quantity;

        // ------------------------------------------------

        if(cfg.u.AutoStopOffset != 0 && !(stopActivated || stopDisposed))
          if(quantity > 0)
          {
            if(!takeActivated)
              stopLossPrice = Price.Ceil(AvgPrice - cfg.u.AutoStopOffset);

            stopLoss = tmgr.ExecAction(stopDescr, new OwnAction(
              TradeOp.Sell, BaseQuote.Absolute, stopLossPrice,
              QtyType.Absolute, quantity, true, cfg.u.AutoStopSlippage,
              cfg.u.AutoStopTrailStart, cfg.u.AutoStopTrailOffset));
          }
          else
          {
            if(!takeActivated)
              stopLossPrice = Price.Floor(AvgPrice + cfg.u.AutoStopOffset);

            stopLoss = tmgr.ExecAction(stopDescr, new OwnAction(
              TradeOp.Buy, BaseQuote.Absolute, stopLossPrice,
              QtyType.Absolute, -quantity, true, cfg.u.AutoStopSlippage,
              cfg.u.AutoStopTrailStart, cfg.u.AutoStopTrailOffset));
          }

        // ------------------------------------------------

        if(cfg.u.AutoTakeOffset != 0 && !(takeActivated || takeDisposed || stopActivated))
          if(quantity > 0)
          {
            takeProfit = tmgr.ExecAction(takeDescr, new OwnAction(
              TradeOp.Sell, BaseQuote.Absolute,
              Price.Ceil(AvgPrice + cfg.u.AutoTakeOffset),
              QtyType.Absolute, quantity));
          }
          else
          {
            takeProfit = tmgr.ExecAction(takeDescr, new OwnAction(
              TradeOp.Buy, BaseQuote.Absolute,
              Price.Floor(AvgPrice - cfg.u.AutoTakeOffset),
              QtyType.Absolute, -quantity));
          }

        // ------------------------------------------------
      }

      // ------------------------------------------------------------

      MktProvider.Receiver.PutPosition(quantity, AvgPrice);
    }

    // **********************************************************************

    public void StopOrderActivated(Transaction t)
    {
      if(t == stopLoss)
      {
        stopActivated = true;

        if(takeProfit != null)
        {
          tmgr.CancelAction(stopDescr, takeProfit);
          takeProfit = null;
        }
      }
    }

    // **********************************************************************

    public void Clear()
    {
      quantity = 0;
      pricesum = 0;
      AvgPrice = 0;

     // MktProvider.TradeLog.Reset();
      MktProvider.Receiver.PutPosition(0, 0);
    }

    // **********************************************************************
  }
}
