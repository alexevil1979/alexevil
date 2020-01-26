// ========================================================================
//    Loopbacks.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// ========================================================================

namespace WpfApp1.Market.Internals
{
  // ************************************************************************

  sealed class NullReceiver : IDataReceiver
  {
    void IDataReceiver.PutMessage(Message msg) { }
    void IDataReceiver.PutStock(Quote[] quotes, Spread spread) { }
    void IDataReceiver.PutTick(int skey, Tick tick) { }
    void IDataReceiver.PutOwnOrder(OwnOrder order) { }
    void IDataReceiver.PutPosition(int quantity, int price) { }
  }

  // ************************************************************************

  sealed class NullTrader : ITrader
  {
    public string Name { get { return "NullTrader"; } }
    public void Setup() { }

    public event StatusUpdateHandler Broken;

    event StatusUpdateHandler ITrader.Connected { add { } remove { } }
    event StatusUpdateHandler ITrader.Disconnected { add { } remove { } }
    event TraderReplyHandler ITrader.TraderReplyHandler { add { } remove { } }
    event OrderUpdateHandler ITrader.OrderUpdateHandler { add { } remove { } }
    event OwnTradeHandler ITrader.OwnTradeHandler { add { } remove { } }

    bool ITrader.Activate(string secCode, string classCode)
    {
      if(Broken != null)
        Broken("Блокировка исполнения заявок");

      return false;
    }

    void ITrader.Deactivate() { }

    string ITrader.SendBuyOrder(int price, int quantity, out int tid) { tid = 0; return Name; }
    string ITrader.SendSellOrder(int price, int quantity, out int tid) { tid = 0; return Name; }
    string ITrader.KillOrder(long oid) { return Name; }
  }

  // ************************************************************************
}
