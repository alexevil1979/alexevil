// =======================================================================
//    Emulator.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =======================================================================

using System;
using System.Collections.Generic;
using System.Threading;

namespace WpfApp1.Market.Internals
{
  sealed class Emulator : ITrader
  {
    // **********************************************************************

    sealed class Order
    {
      public int Id;
      public int Price;
      public int Quantity;
      public DateTime ExecAfter;
      public DateTime KillAfter;
      public int ExecutedPrice;
    }

    // ----------------------------------------------------------------------

    enum ReplyTypes { Unknown, OrderReply, OrderUpdate, Trade }

    // ----------------------------------------------------------------------

    struct ReplyData
    {
      public readonly ReplyTypes Type;

      public readonly int Id;
      public readonly string Error;

      public readonly int Active;
      public readonly int Filled;

      public readonly int Price;

      public ReplyData(int id, string error)
      {
        this.Type = ReplyTypes.OrderReply;

        this.Id = id;
        this.Error = error;
        this.Active = 0;
        this.Filled = 0;
        this.Price = 0;
      }

      public ReplyData(ReplyTypes type, int id, int active, int filled, int price)
      {
        this.Type = type;

        this.Id = id;
        this.Error = null;

        this.Active = active;
        this.Filled = filled;

        this.Price = price;
      }
    }

    // **********************************************************************

    const string deactivatedText = "Эмулятор остановлен";

    int lastId;

    bool isActive;

    Random rnd;

    List<Order> olist;
    Queue<ReplyData> replies;

    string secCode, classCode;

    // **********************************************************************

    public string Name { get { return "Emulator"; } }

    // **********************************************************************

    public event StatusUpdateHandler Disconnected;

    event StatusUpdateHandler ITrader.Connected { add { } remove { } }
    event StatusUpdateHandler ITrader.Broken { add { } remove { } }

    public event TraderReplyHandler TraderReplyHandler;
    public event OrderUpdateHandler OrderUpdateHandler;
    public event OwnTradeHandler OwnTradeHandler;

    // **********************************************************************

    public Emulator()
    {
      lastId = 1000;

      rnd = new Random();

      olist = new List<Order>();
      replies = new Queue<ReplyData>();
    }

    // **********************************************************************

    void IConnector.Setup() { }

    // **********************************************************************

    public bool Activate(string secCode, string classCode)
    {
      bool result;

      if(this.secCode != secCode || this.classCode != classCode)
      {
        Deactivate();

        this.secCode = secCode;
        this.classCode = classCode;

        result = true;
      }
      else
        result = false;

      if(!isActive)
      {
        isActive = true;

        MktProvider.LastPriceHandler += LastPriceHandler;

        Thread t = new Thread(Process);
        t.Name = Name;
        t.IsBackground = true;
        t.Start();

     //   MktProvider.TraderStatus.Emulated("Эмуляция исполнения заявок");
      }

      return result;
    }

    // **********************************************************************

    public void Deactivate()
    {
      if(isActive)
      {
        isActive = false;

        MktProvider.LastPriceHandler -= LastPriceHandler;

        if(Disconnected != null)
          Disconnected(deactivatedText);
      }
    }

    // **********************************************************************

    void Process()
    {
      // ------------------------------------------------------------

      while(isActive)
      {
        // ------------------------------------------------

        if(olist.Count > 0 && MktProvider.AskPrice > 0)
          lock(olist)
          {
            int i = 0;

            while(i < olist.Count)
            {
              Order o = olist[i];

              if(o.ExecutedPrice > 0)
              {
                lock(replies)
                {
                  replies.Enqueue(new ReplyData(ReplyTypes.OrderUpdate, o.Id, 0, o.Quantity, 0));
                  replies.Enqueue(new ReplyData(ReplyTypes.Trade, o.Id, 0, o.Quantity, o.ExecutedPrice));
                }

                olist.RemoveAt(i);
                continue;
              }

              DateTime now = DateTime.UtcNow;

              if(o.ExecAfter < now
                && ((o.Quantity > 0 && o.Price >= MktProvider.AskPrice)
                || (o.Quantity < 0 && o.Price <= MktProvider.BidPrice)))
              {
                lock(replies)
                {
                  replies.Enqueue(new ReplyData(ReplyTypes.OrderUpdate, o.Id, 0, o.Quantity, 0));
                  replies.Enqueue(new ReplyData(ReplyTypes.Trade, o.Id, 0, o.Quantity,
                    o.Quantity > 0 ? MktProvider.AskPrice : MktProvider.BidPrice));
                }

                olist.RemoveAt(i);
                continue;
              }

              if(o.KillAfter < now)
              {
                lock(replies)
                  replies.Enqueue(new ReplyData(ReplyTypes.OrderUpdate, o.Id, 0, 0, 0));

                olist.RemoveAt(i);
                continue;
              }

              i++;
            }
          }

        // ------------------------------------------------

        while(replies.Count > 0)
        {
          ReplyData rd;

          lock(replies)
            rd = replies.Dequeue();

          switch(rd.Type)
          {
            case ReplyTypes.OrderReply:
              if(TraderReplyHandler != null)
                TraderReplyHandler(rd.Id, rd.Id, rd.Error);
              break;

            case ReplyTypes.OrderUpdate:
              if(OrderUpdateHandler != null)
                OrderUpdateHandler(rd.Id, rd.Active, rd.Filled);
              break;

            case ReplyTypes.Trade:
              if(OwnTradeHandler != null)
                OwnTradeHandler(new OwnTrade(rd.Id, DateTime.Now, rd.Price, rd.Filled));
              break;
          }
        }

        // ------------------------------------------------

        Thread.Sleep(cfg.s.EmulatorTickInterval);
      }

      // ------------------------------------------------------------ ?

      lock(olist)
        olist.Clear();

      lock(replies)
        replies.Clear();

      // ------------------------------------------------------------
    }

    // **********************************************************************

    void LastPriceHandler(int price)
    {
      if(olist.Count > 0)
        lock(olist)
          for(int i = 0; i < olist.Count; i++)
          {
            Order o = olist[i];

            if(o.ExecAfter < DateTime.UtcNow
              && o.ExecutedPrice == 0
              && ((o.Quantity > 0 && o.Price >= price)
              || (o.Quantity < 0 && o.Price <= price)))
            {
              o.ExecutedPrice = price;
            }
          }
    }

    // **********************************************************************

    string SendOrder(int price, int quantity, out int tid)
    {
      if(isActive)
      {
        tid = ++this.lastId;

        lock(olist)
        {
          int pLong = MktProvider.Manager.FilledBalance;
          int pShort = MktProvider.Manager.FilledBalance;

          if(quantity > 0)
            pLong += quantity;
          else
            pShort += quantity;

          for(int i = 0; i < olist.Count; i++)
            if(olist[i].Quantity > 0)
              pLong += olist[i].Quantity;
            else
              pShort += olist[i].Quantity;

          if(pLong > cfg.u.EmulatorLimit || -pShort > cfg.u.EmulatorLimit)
            lock(replies)
              replies.Enqueue(new ReplyData(tid, "Максимальный размер позиции = "
                + cfg.u.EmulatorLimit.ToString("N", cfg.BaseCulture)));
          else
          {
            Order order = new Order();
            order.Id = tid;
            order.Price = price;
            order.Quantity = quantity;
            order.ExecAfter = DateTime.UtcNow.Add(new TimeSpan(0, 0, 0, 0,
              rnd.Next(cfg.u.EmulatorDelayMin, cfg.u.EmulatorDelayMax)));
            order.KillAfter = DateTime.MaxValue;

            olist.Add(order);

            lock(replies)
            {
              replies.Enqueue(new ReplyData(tid, null));
              replies.Enqueue(new ReplyData(ReplyTypes.OrderUpdate, order.Id, order.Quantity, 0, 0));
            }
          }
        }

        return null;
      }
      else
      {
        tid = 0;
        return deactivatedText;
      }
    }

    // **********************************************************************

    public string SendBuyOrder(int price, int quantity, out int tid)
    {
      return SendOrder(price, quantity, out tid);
    }

    // **********************************************************************

    public string SendSellOrder(int price, int quantity, out int tid)
    {
      return SendOrder(price, -quantity, out tid);
    }

    // **********************************************************************

    public string KillOrder(long oid)
    {
      if(isActive)
      {
        lock(olist)
          for(int i = 0; i < olist.Count; i++)
            if(olist[i].Id == oid)
            {
              olist[i].KillAfter = DateTime.UtcNow.Add(
                new TimeSpan(0, 0, 0, 0, cfg.u.EmulatorDelayMin));

              return null;
            }

        return "Снимаемая заявка №" + oid + " не найдена.";
      }
      else
        return deactivatedText;
    }

    // **********************************************************************
  }
}
