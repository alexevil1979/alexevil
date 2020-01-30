// =====================================================================
//    Player.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;



using WpfApp1.Market.History.Internals;
using System.Collections.ObjectModel;


using System.Windows;
using WpfApp1;







namespace WpfApp1.Market.History
{
  sealed class Player
  {
    // **********************************************************************

    public sealed class Stream
    {
      public bool IsActive { get; set; }
      public readonly StreamHeader Header;

      public Stream(StreamHeader h)
      {
        this.Header = h;
        IsActive = true;
      }
    }

    // **********************************************************************

    struct HandledStream
    {
      public Stream Stream;
      public Action<Stream> Handler;
    }

    // **********************************************************************

    string _status;

    EventWaitHandle working, nosync;
    Thread pThread;

    FileStream fs;
    BinaryReader br;

    HandledStream[] hStreams;

    double currentTimeOffset;

    DateTime pauseDateTime;

    SortedDictionary<int, int> rawQuotes;

    // **********************************************************************

    public event StockHandler StockHandler;
    public event TickHandler TickHandler;

    // **********************************************************************

    public readonly string FilePath;
    public readonly string FileName;

    public DataFileHeader FileHeader { get; private set; }
    public Stream this[int i] { get { return hStreams[i].Stream; } }

    public bool IsPlaying { get; private set; }

    public long FileSize { get { return fs == null ? 0 : fs.Length; } }
    public long FilePosition { get { return fs == null ? 0 : fs.Position; } }

    public DateTime BaseDateTime { get; private set; }

    public DateTime CurrentDateTime
    {
      get
      {
        return FileHeader.BaseDateTime.Add(
          TimeSpan.FromMilliseconds(currentTimeOffset));
      }
    }

    // **********************************************************************

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

       // MktProvider.Log.Put("Player " + FileName + " <" + value + ">");
      }
    }

    // **********************************************************************

    public Player(string fn)
    {
      working = new EventWaitHandle(true, EventResetMode.ManualReset);
      nosync = new EventWaitHandle(false, EventResetMode.AutoReset);

      try
      {
        FilePath = fn;
        FileName = Path.GetFileName(fn);

        fs = new FileStream(fn, FileMode.Open, FileAccess.Read);

        ResetDataFile();

      //  MktProvider.AttachPlayer(this);

        Status = "Файл открыт";
      }
      catch(Exception e)
      {
        if(FileHeader == null)
          FileHeader = new DataFileHeader("?", "", DateTime.MinValue, 0);

        Status = e.Message;
      }
    }

    // **********************************************************************

    public void Dispose()
    {
     // MktProvider.DetachPlayer(this);

      Stop();

      if(fs != null)
        fs.Dispose();

      working.Dispose();
      nosync.Dispose();
    }

    // **********************************************************************

    public void Start(DateTime utcNow, DateTime startDateTime)
    {
      double startTimeOffset = (startDateTime - FileHeader.BaseDateTime).TotalMilliseconds;

      BaseDateTime = utcNow.Add(TimeSpan.FromMilliseconds(-startTimeOffset));

      if(startTimeOffset > currentTimeOffset)
        nosync.Set();
      else
        Stop();

      pauseDateTime = DateTime.MinValue;
      working.Set();

      if(pThread == null || !IsPlaying)
      {
        if(rawQuotes != null)
          ResetDataFile();

        IsPlaying = true;

        pThread = new Thread(PlayProc);
        pThread.IsBackground = true;
        pThread.Name = FileName;
        pThread.Start();
      }
    }

    // **********************************************************************

    public void Stop()
    {
      if(pThread != null)
      {
        IsPlaying = false;

        pauseDateTime = DateTime.MinValue;
        working.Set();

        nosync.Set();
        pThread.Join();
        pThread = null;
      }
    }

    // **********************************************************************

    public void Pause()
    {
      if(pauseDateTime == DateTime.MinValue)
      {
        working.Reset();
        pauseDateTime = DateTime.UtcNow;
      }
    }

    // **********************************************************************

    public void Continue()
    {
      if(pauseDateTime > DateTime.MinValue)
      {
        BaseDateTime += DateTime.UtcNow - pauseDateTime;

        pauseDateTime = DateTime.MinValue;
        working.Set();
      }
    }

    // **********************************************************************

    void ResetDataFile()
    {
      fs.Position = 0;
      br = new BinaryReader(new DeflateStream(fs, CompressionMode.Decompress, true));

      FileHeader = DataFile.ReadHeader(br);

      HandledStream[] hStreams = new HandledStream[FileHeader.StreamsCount];

      if(this.hStreams == null)
        this.hStreams = hStreams;

      for(int i = 0; i < hStreams.Length; i++)
      {
        HandledStream hs = new HandledStream();
        hs.Stream = new Stream(DataFile.ReadStreamHeader(br));

        switch(hs.Stream.Header.Type)
        {
          case StreamType.Stock:
            hs.Handler = StockReader;
            break;

          case StreamType.Ticks:
            hs.Handler = TickReader;
            break;

          case StreamType.Orders:
            hs.Handler = OrderReader;
            break;

          case StreamType.Trades:
            hs.Handler = TradeReader;
            break;

          case StreamType.Messages:
            hs.Handler = MessageReader;
            break;

          default:
            throw new FormatException("Неизвестный тип данных");
        }

        hStreams[i] = hs;
      }
    }

    // **********************************************************************

    void PlayProc()
    {
      try
      {
        // ----------------------------------------------------------

        if(FileHeader.StreamsCount == 0)
          throw new FormatException("Нет данных");

        rawQuotes = new SortedDictionary<int, int>();

        nosync.Reset();

        Status = "Воспроизведение";

        // ----------------------------------------------------------

        for(; ; )
        {
          HandledStream hs;

          if(hStreams.Length > 1)
            hs = hStreams[br.ReadByte()];
          else
            hs = hStreams[0];

          currentTimeOffset = br.ReadUInt32();

          int sleep = (int)(currentTimeOffset - (DateTime.UtcNow - BaseDateTime).TotalMilliseconds);

          if(sleep > 0)
            nosync.WaitOne(sleep);

          if(IsPlaying)
            working.WaitOne();
          else
            break;

          hs.Handler(hs.Stream);
        }

        // ----------------------------------------------------------

        Status = "Остановлен";
      }
      catch(EndOfStreamException)
      {
        Status = "Достигнут конец файла";
      }
      catch(Exception e)
      {
        Status = e.Message;
      }
      finally
      {
        IsPlaying = false;

        if(br != null)
          br.Dispose();
      }
    }

    // **********************************************************************

    void StockReader(Stream stream)
    {
      int n = br.ReadUInt16();
            String rrr = "";
      for(int i = 0; i < n; i++)
      {
        int p = br.ReadInt32();
        int v = br.ReadInt32();

        if(v == 0)
          rawQuotes.Remove(p);
        else if(rawQuotes.ContainsKey(p))
          rawQuotes[p] = v;
        else
          rawQuotes.Add(p, v);
      }

      if(stream != null && stream.IsActive)
      {
        Quote[] quotes = new Quote[rawQuotes.Count];

        int i = rawQuotes.Count - 1;

        Spread spread = new Spread();

        foreach(KeyValuePair<int, int> kvp in rawQuotes)
        {
          quotes[i].Price = kvp.Key;

          if(kvp.Value > 0)
          {
            quotes[i].Volume = kvp.Value;

            if(spread.Ask > 0)
              quotes[i].Type = QuoteType.Ask;
            else
            {
              quotes[i].Type = QuoteType.BestAsk;

              int j = i + 1;

              if(j < quotes.Length)
              {
                quotes[j].Type = QuoteType.BestBid;
                spread = new Spread(quotes[i].Price, quotes[j].Price);
              }
            }
          }
          else
          {
            quotes[i].Volume = -kvp.Value;
            quotes[i].Type = QuoteType.Bid;
          }
               rrr=  rrr+  " "+quotes[i].Price.ToString() + " " + quotes[i].Volume.ToString() + " " + quotes[i].Type.ToString()+ "\n";          
          
                    i--;
                  
                }
                MainWindow.Res(rrr);

                if (StockHandler != null)
          StockHandler(quotes, spread);
      }
    }

    // **********************************************************************

    void TickReader(Stream stream)
    {
      Tick t = new Tick();

      t.DateTime = new DateTime(br.ReadInt64());
      t.RawPrice = br.ReadDouble();
      t.Volume = br.ReadInt32();
      t.Op = (TradeOp)br.ReadByte();

      if(stream != null && stream.IsActive && TickHandler != null)
        TickHandler(stream.Header.Security.GetKey(), t);
    }

    // **********************************************************************

    void OrderReader(Stream stream)
    {
      long id = br.ReadInt64();
      int p = br.ReadInt32();
      int q = br.ReadInt32();
            String rrr = "";
            if (stream != null && stream.IsActive)
            {
                rrr = rrr + " " + p.ToString() + " " + q.ToString() + " " + id.ToString() + "\n";
                MainWindow.Res1(rrr);
            }
           
        
    }

    // **********************************************************************

    void TradeReader(Stream stream)
    {
      OwnTrade trade = new OwnTrade(br.ReadInt64(),
        new DateTime(br.ReadInt64()),
        br.ReadInt32(), br.ReadInt32());

     // if(stream != null && stream.IsActive)
      //  MktProvider.Manager.Position.PutOwnTrade(trade);
    }

    // **********************************************************************

    void MessageReader(Stream stream)
    {
      Message msg = new Message(new DateTime(br.ReadInt64()),
        (MessageType)br.ReadByte(), br.ReadString());

     // if(stream != null && stream.IsActive)
     //   MktProvider.Receiver.PutMessage(msg);
    }

    // **********************************************************************
  }
}
