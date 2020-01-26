// =======================================================================
//    Recorder.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =======================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using WpfApp1.History.Internals;

namespace WpfApp1.History
{
  sealed class Recorder : IDataReceiver
  {
    // **********************************************************************

    const string FileNameFormat = "TradeHistory {0:yyyy-MM-dd@HHmmss}.{1}";
    const int DiffListBaseSize = 100;

    string _status;

    object pLock;

    bool writeStock;
    bool writeOrders;
    bool writeTrades;
    bool writeMessages;

    FileStream fs;
    BinaryWriter bw;

    DateTime baseDateTime;
    int streamsCount;

    int stockId;
    int orderId;
    int tradeId;
    int msgId;

    Dictionary<int, int> tickIds;

    Quote[] lastQuotes;
    List<Quote> diffQuotes;

    long lastFileSize;

    // **********************************************************************

    public readonly IDataReceiver Receiver;

    public string FileName { get; private set; }
    public DateTime LastDateTime { get; private set; }
    public int WrittenCount { get; private set; }

    public long FileSize
    {
      get
      {
        try { if(fs != null) lastFileSize = fs.Length; }
        catch { }

        return lastFileSize;
      }
    }

    // **********************************************************************

    public bool IsRecording { get; private set; }
    public bool StatusUpdated { get; private set; }

    public string Status
    {
      get
      {
        StatusUpdated = false;
        return _status;
      }
      private set
      {
        StatusUpdated = true;
        _status = value;

       // MktProvider.Log.Put("Recorder <" + value + ">");
      }
    }

        public static object IDataReceiver { get; internal set; }

        // **********************************************************************

        public Recorder(IDataReceiver receiver)
    {
      this.Receiver = receiver;

      pLock = new object();
      tickIds = new Dictionary<int, int>();

      diffQuotes = new List<Quote>(DiffListBaseSize);

      Status = "Ожидание начала записи...";
           
    }

    

        // **********************************************************************

        void WriteRecHeader(int id)
    {
      WrittenCount++;
      LastDateTime = DateTime.UtcNow;

      if(streamsCount > 1)
        bw.Write((byte)id);

      bw.Write((uint)(LastDateTime - baseDateTime).TotalMilliseconds);
    }

    // **********************************************************************

    void SetError(string text) { Stop(); Status = text; }

        // **********************************************************************



        public void AddStock(Quote[] quotes, Spread spread, Recorder fff)
        {
                
      
            
            fff.PutStock( quotes,  spread);
        }
        public void AddOrder(long id,int price, int qty,Recorder fff)
        {

        

            fff.PutOwnOrder(id, price,qty);
        }

        public void Start(string folder, bool writeStock, bool writeOrders,
      bool writeTrades, bool writeMessages, HashSet<Security> ticks)
    {
            
            try
      {
        lock(pLock)
        {
          // ---------------------------------------------------

          if(IsRecording)
          {
            Status = "Запись уже ведется";
            return;
          }

          // ---------------------------------------------------

          streamsCount = 0;

          if(writeStock)
            streamsCount++;

          if(writeOrders)
            streamsCount++;

          if(writeTrades)
            streamsCount++;

          if(writeMessages)
            streamsCount++;

          streamsCount += ticks.Count;

          if(streamsCount == 0)
            throw new Exception("Не заданы потоки для записи");

          // ---------------------------------------------------

          WrittenCount = 0;
          LastDateTime = DateTime.MinValue;

          // ---------------------------------------------------

          baseDateTime = DateTime.UtcNow;

          FileName = string.Format(FileNameFormat, baseDateTime.ToLocalTime(), cfg.HistoryFileExt);

          fs = new FileStream(folder + "\\" + FileName, FileMode.Create, FileAccess.Write);
          bw = new BinaryWriter(new DeflateStream(fs, CompressionMode.Compress));

          DataFile.WriteHeader(bw, new DataFileHeader(
            cfg.FullProgName, string.Empty, baseDateTime, streamsCount));

          // ---------------------------------------------------

          streamsCount = 0;

          StreamHeader sh = new StreamHeader(StreamType.Stock,
            new Security(cfg.u.SecCode, cfg.u.ClassCode),
            cfg.u.PriceRatio, cfg.u.PriceStep);

          if(writeStock)
          {
            stockId = streamsCount++;
            DataFile.WriteStreamHeader(bw, sh);

            lastQuotes = new Quote[0];
            this.writeStock = true;
          }

          if(writeOrders)
          {
            orderId = streamsCount++;
            sh.Type = StreamType.Orders;
            DataFile.WriteStreamHeader(bw, sh);

            this.writeOrders = true;
          }

          if(writeTrades)
          {
            tradeId = streamsCount++;
            sh.Type = StreamType.Trades;
            DataFile.WriteStreamHeader(bw, sh);

            this.writeTrades = true;
          }

          if(writeMessages)
          {
            msgId = streamsCount++;
            sh.Type = StreamType.Messages;
            DataFile.WriteStreamHeader(bw, sh);

            this.writeMessages = true;
          }

          if(ticks.Count > 0)
          {
            tickIds = new Dictionary<int, int>(ticks.Count);
            sh = new StreamHeader(StreamType.Ticks);

            foreach(Security s in ticks)
            {
              tickIds.Add(s.GetKey(), streamsCount++);

              sh.Security = s;
              DataFile.WriteStreamHeader(bw, sh);
            }
          }

          // ---------------------------------------------------
        }

        Status = "Запись...";
        IsRecording = true;
      }
      catch(Exception e)
      {
        SetError(e.Message);
      }
    }

    // **********************************************************************

    public void Stop()
    {
      lock(pLock)
      {
        writeStock = false;
        writeOrders = false;
        writeTrades = false;
        writeMessages = false;

        tickIds.Clear();

        if(fs != null)
          fs = null;

        if(bw != null)
        {
          bw.Dispose();
          bw = null;
        }

        if(IsRecording)
        {
          Status = "Запись остановлена";
          IsRecording = false;
        }
      }
    }

        void PutOwnOrder(long id, int price,int qty)
        {
            OwnOrder order = new OwnOrder(id,price, qty, "tag");
         
            if (writeOrders)
                try
                {
                    lock (pLock)
                    {
                        WriteRecHeader(orderId);

                        bw.Write(order.Id);
                        bw.Write(order.IsActive ? order.Price : -order.Price);
                        bw.Write(order.Quantity);
                    }
                }
                catch (Exception e)
                {
                    SetError(e.Message);
                }

          //  Receiver.PutOwnOrder(order);
        }

        // **********************************************************************
        void PutStock(Quote[] quotes, Spread spread)
        {
            if (writeStock)
                lock (pLock)
                {
                    diffQuotes.Clear();

                    int i = 0;

                    foreach (Quote lq in lastQuotes)
                    {
                        while (i < quotes.Length && lq.Price < quotes[i].Price)
                            diffQuotes.Add(quotes[i++]);

                        if (i < quotes.Length && quotes[i].Price == lq.Price)
                        {
                            if (lq.Type != quotes[i].Type || lq.Volume != quotes[i].Volume)
                                diffQuotes.Add(quotes[i]);

                            i++;
                        }
                        else
                            diffQuotes.Add(new Quote(lq.Price, 0, QuoteType.Unknown));
                    }

                    while (i < quotes.Length)
                        diffQuotes.Add(quotes[i++]);

                    if (diffQuotes.Count > 0)
                        try
                        {
                            if (diffQuotes.Count > ushort.MaxValue)
                                throw new OverflowException("Превышение допустимой глубины стакана");

                            WriteRecHeader(stockId);

                            bw.Write((ushort)diffQuotes.Count);

                            foreach (Quote q in diffQuotes)
                            {
                                bw.Write(q.Price);

                                switch (q.Type)
                                {
                                    case QuoteType.Ask:
                                    case QuoteType.BestAsk:
                                        bw.Write(q.Volume);
                                        break;

                                    case QuoteType.Bid:
                                    case QuoteType.BestBid:
                                        bw.Write(-q.Volume);
                                        break;

                                    default:
                                        bw.Write(0);
                                        break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            SetError(e.Message);
                        }

                    lastQuotes = quotes;
                }

            //eceiver.PutStock(quotes, spread);
        }

        void IDataReceiver.PutStock(Quote[] quotes, Spread spread)
    {
      if(writeStock)
        lock(pLock)
        {
          diffQuotes.Clear();

          int i = 0;

          foreach(Quote lq in lastQuotes)
          {
            while(i < quotes.Length && lq.Price < quotes[i].Price)
              diffQuotes.Add(quotes[i++]);

            if(i < quotes.Length && quotes[i].Price == lq.Price)
            {
              if(lq.Type != quotes[i].Type || lq.Volume != quotes[i].Volume)
                diffQuotes.Add(quotes[i]);

              i++;
            }
            else
              diffQuotes.Add(new Quote(lq.Price, 0, QuoteType.Unknown));
          }

          while(i < quotes.Length)
            diffQuotes.Add(quotes[i++]);

          if(diffQuotes.Count > 0)
            try
            {
              if(diffQuotes.Count > ushort.MaxValue)
                throw new OverflowException("Превышение допустимой глубины стакана");

              WriteRecHeader(stockId);

              bw.Write((ushort)diffQuotes.Count);

              foreach(Quote q in diffQuotes)
              {
                bw.Write(q.Price);

                switch(q.Type)
                {
                  case QuoteType.Ask:
                  case QuoteType.BestAsk:
                    bw.Write(q.Volume);
                    break;

                  case QuoteType.Bid:
                  case QuoteType.BestBid:
                    bw.Write(-q.Volume);
                    break;

                  default:
                    bw.Write(0);
                    break;
                }
              }
            }
            catch(Exception e)
            {
              SetError(e.Message);
            }

          lastQuotes = quotes;
        }

      Receiver.PutStock(quotes, spread);
    }

    // **********************************************************************

    void IDataReceiver.PutTick(int skey, Tick tick)
    {
      if(tickIds.Count > 0)
        lock(pLock)
        {
          int id;

          if(tickIds.TryGetValue(skey, out id))
            try
            {
              WriteRecHeader(id);

              bw.Write(tick.DateTime.Ticks);
              bw.Write(tick.RawPrice);
              bw.Write(tick.Volume);
              bw.Write((byte)tick.Op);
            }
            catch(Exception e)
            {
              SetError(e.Message);
            }
        }

      Receiver.PutTick(skey, tick);
    }

    // **********************************************************************

    void IDataReceiver.PutOwnOrder(OwnOrder order)
    {
      if(writeOrders)
        try
        {
          lock(pLock)
          {
            WriteRecHeader(orderId);

            bw.Write(order.Id);
            bw.Write(order.IsActive ? order.Price : -order.Price);
            bw.Write(order.Quantity);
          }
        }
        catch(Exception e)
        {
          SetError(e.Message);
        }

      Receiver.PutOwnOrder(order);
    }

    // **********************************************************************

    void IDataReceiver.PutPosition(int quantity, int price)
    {
      Receiver.PutPosition(quantity, price);
    }

    // **********************************************************************

    void IDataReceiver.PutMessage(Message msg)
    {
      if(writeMessages)
        try
        {
          lock(pLock)
          {
            WriteRecHeader(msgId);

            bw.Write(msg.DateTime.Ticks);
            bw.Write((byte)MessageType.Error);
            bw.Write(msg.Text);
          }
        }
        catch(Exception e)
        {
          SetError(e.Message);
        }

      Receiver.PutMessage(msg);
    }

    // **********************************************************************

    public void PutOwnTrade(OwnTrade trade)
    {
      if(writeTrades)
        try
        {
          lock(pLock)
          {
            WriteRecHeader(tradeId);

            bw.Write(trade.OId);
            bw.Write(trade.DateTime.Ticks);
            bw.Write(trade.Price);
            bw.Write(trade.Quantity);
          }
        }
        catch(Exception e)
        {
          SetError(e.Message);
        }
    }

    // **********************************************************************
  }
}
