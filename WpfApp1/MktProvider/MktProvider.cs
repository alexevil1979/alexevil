// ==========================================================================
//    MktProvider.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;

using WpfApp1.History;



namespace WpfApp1.Market
{
  static class MktProvider
  {
    // **********************************************************************

    public static int AskPrice { get; private set; }
    public static int BidPrice { get; private set; }






    public static event LastPriceHandler LastPriceHandler;

    public static IDataReceiver Receiver { get; private set; }


    // **********************************************************************

    static bool isReplayMode = false;
    static bool isNullTrader = false;

    // **********************************************************************

    // **********************************************************************

 

  

    // **********************************************************************

    static MktProvider()
    {
    

    }

    // **********************************************************************

    static void StockConnected(string text)
    {

    }

    // **********************************************************************

    static void StockHandler(Quote[] quotes, Spread spread)
    {
      //StockStatus.DataReceived = DateTime.UtcNow;

      AskPrice = spread.Ask;
      BidPrice = spread.Bid;

      Receiver.PutStock(quotes, spread);
    }

    // **********************************************************************

    static void TickHandler(int skey, Tick tick)
    {
    //  TicksStatus.DataReceived = DateTime.UtcNow;

      if(skey == cfg.WorkSecKey)
      {
        tick.IntPrice = Price.GetInt(tick.RawPrice);

        if(LastPriceHandler != null)
          LastPriceHandler(tick.IntPrice);
      }

      Receiver.PutTick(skey, tick);
    }

    // **********************************************************************

    public static void SetReceiver(IDataReceiver receiver)
    {
      MktProvider.Receiver = receiver;
    }

    // **********************************************************************

    public static void Activate()
    {
   
    }

    // **********************************************************************

    public static void Deactivate()
    {
   
    }

    // **********************************************************************

    public static void GetSecList(Action<SecList> callback)
    {
   
    }

    // **********************************************************************

    public static Recorder GetRecorder()
    {
      Recorder r = Receiver as Recorder;
            if (r == null)
            {
                r = new Recorder(Receiver);
                Receiver = r;

               
            }


            return r;
    }

    // **********************************************************************

    public static void KillRecorder()
    {
      Recorder r = Receiver as Recorder;

      if(r != null)
      {
        r.Stop();
        Receiver = r.Receiver;

       
      }
    }

    // **********************************************************************

    public static void AttachPlayer(Player p)
    {
      p.StockHandler += StockHandler;
      p.TickHandler += TickHandler;
    }

    // **********************************************************************

    public static void DetachPlayer(Player p)
    {
      p.StockHandler -= StockHandler;
      p.TickHandler -= TickHandler;
    }

    // **********************************************************************

    public static void SetMode(bool replay, bool nullTader)
    {
      if(isReplayMode != replay)
        if(isReplayMode = replay)
          

      isNullTrader = nullTader;

      Activate();
    }

    // **********************************************************************
  }
}
