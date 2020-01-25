// =====================================================================
//    IFaces.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =====================================================================

using System;
using System.Collections.Generic;

namespace WpfApp1.Market
{
  // ************************************************************************

  delegate void StatusUpdateHandler(string text);

  delegate void StockHandler(Quote[] quotes, Spread spread);
  delegate void TickHandler(int skey, Tick tick);

  delegate void TraderReplyHandler(int tid, long oid, string error);
  delegate void OrderUpdateHandler(long oid, int active, int filled);
  delegate void OwnTradeHandler(OwnTrade trade);

  delegate void LastPriceHandler(int price);

  // ************************************************************************

  interface IConnector
  {
    string Name { get; }
    void Setup();
  }

  // ************************************************************************

  interface IStockProvider : IConnector
  {
    event StatusUpdateHandler Connected;
    event StatusUpdateHandler Disconnected;
    event StatusUpdateHandler Broken;

    event StockHandler StockHandler;

    void Subscribe(string symbol);
    void Unsubscribe();
  }

  // ************************************************************************

  interface ITicksProvider : IConnector
  {
    event StatusUpdateHandler Connected;
    event StatusUpdateHandler Disconnected;
    event StatusUpdateHandler Broken;

    event TickHandler TickHandler;

    void Subscribe(HashSet<string> symbols);
    void Unsubscribe();
  }

  // ************************************************************************

  interface ISecListProvider : IConnector
  {
    void GetSecList(Action<SecList> callback);
  }

  // ************************************************************************

  interface ITrader : IConnector
  {
    event StatusUpdateHandler Connected;
    event StatusUpdateHandler Disconnected;
    event StatusUpdateHandler Broken;

    event TraderReplyHandler TraderReplyHandler;
    event OrderUpdateHandler OrderUpdateHandler;
    event OwnTradeHandler OwnTradeHandler;

    bool Activate(string secCode, string classCode);
    void Deactivate();

    string SendBuyOrder(int price, int quantity, out int tid);
    string SendSellOrder(int price, int quantity, out int tid);
    string KillOrder(long oid);
  }

  // ************************************************************************
}
