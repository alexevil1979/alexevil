// =========================================================================
//    QuikTrader.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =========================================================================

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using WpfApp1;
using WpfApp1.Market;
using Trans2QuikAPI;

namespace QuikTraderConnector
{
  sealed class QuikTrader : ITrader
  {
    // **********************************************************************

    public static int ClientCodeMaxLength { get { return 18 - cfg.FullProgName.Length; } }

    // **********************************************************************

    static string Trim(string str) { return str.Trim('.', ' '); }

    // **********************************************************************

    const string NoConnectionText = "Соединение разорвано";

    int transId;

    int error;
    StringBuilder msg;

    string quikFolder, secCode, classCode;

    bool working;
    bool connected;

    Timer connecting;

    double lastBuyId;
    double lastSellId;

    // **********************************************************************

    public string Name { get { return "QuikTrader"; } }

    // **********************************************************************

    public event StatusUpdateHandler Connected;
    public event StatusUpdateHandler Disconnected;
    public event StatusUpdateHandler Broken;

    public event TraderReplyHandler TraderReplyHandler;
    public event OrderUpdateHandler OrderUpdateHandler;
    public event OwnTradeHandler OwnTradeHandler;

    void SetStatus(StatusUpdateHandler status, string text)
    {
      if(status != null)
        status.Invoke(Name + ": " + text);
    }

    // **********************************************************************

    public QuikTrader()
    {
      msg = new StringBuilder(256);
      connecting = new Timer(TryConnect);
    }

    // **********************************************************************
    // *                             Соединение                             *
    // **********************************************************************

    void TryConnect(Object state)
    {
            if (working) { 
     

        SetStatus(Connected, "Соединение с сервером установлено");
      }
            connected = true;
            SetStatus(Connected, "Соединение с сервером установлено");

            connecting.Change(Timeout.Infinite, Timeout.Infinite);
    }

    // **********************************************************************

    void IConnector.Setup()
    {
      if(quikFolder != cfg.u.QuikFolder)
      {
        Deactivate();
        quikFolder = cfg.u.QuikFolder;
      }
    }

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

      if(!working)
      {
        working = true;
        lastBuyId = 0;
        lastSellId = 0;
        connecting.Change(0, cfg.TryConnectInterval);
      }

      return result;
    }

    // **********************************************************************

    public void Deactivate()
    {
      working = false;

      if(connected)
        Trans2Quik.DISCONNECT(out error, msg, msg.Capacity);
    }

    // **********************************************************************

    void StatusCallback(Trans2Quik.Result evnt, int err, string m)
    {
      switch(evnt)
      {
        case Trans2Quik.Result.QUIK_DISCONNECTED:
          SetStatus(Broken, m);
          connecting.Change(0, cfg.TryConnectInterval);
          break;

        case Trans2Quik.Result.DLL_DISCONNECTED:
          SetStatus(Broken, "Связь с терминалом разорвана");
          connected = false;
          connecting.Change(0, cfg.TryConnectInterval);
          break;
      }
    }

    // **********************************************************************
    // *                               Сделки                               *
    // **********************************************************************

    void TradeStatusCallback(
      int nMode,
      double trade_id,
      double order_id,
      string classCode,
      string secCode,
      double price,
      int quantity,
      double msum,
      int isSell,
      int tradeDescriptor)
    {
      string comment = Marshal.PtrToStringAnsi(Trans2Quik.TRADE_BROKERREF(tradeDescriptor));

      if((comment != null && comment.EndsWith(cfg.FullProgName)) || cfg.u.AcceptAllTrades)
      {
        if(isSell == 0)
        {
          if(lastBuyId >= trade_id)
            return;
          else
            lastBuyId = trade_id;
        }
        else
        {
          if(lastSellId >= trade_id)
            return;
          else
            lastSellId = trade_id;

          quantity = -quantity;
        }

        if(nMode == 0 && OwnTradeHandler != null)
        {
          int date = Trans2Quik.TRADE_DATE(tradeDescriptor);
          int time = Trans2Quik.TRADE_TIME(tradeDescriptor);

          int year, month, day;
          int hour, min, sec;

          year = date / 10000;
          month = (day = date - year * 10000) / 100;
          day -= month * 100;

          hour = time / 10000;
          min = (sec = time - hour * 10000) / 100;
          sec -= min * 100;

          OwnTradeHandler(new OwnTrade((long)order_id,
            new DateTime(year, month, day, hour, min, sec),
            Price.GetInt(price), quantity));
        }
      }
    }

    // **********************************************************************
    // *                               Заявки                               *
    // **********************************************************************

    void TransactionReplyCallback(
      Trans2Quik.Result r,
      int err,
      int rc,
      int tid,
      double order_id,
      string msg)
    {
      if(TraderReplyHandler != null)
        if(r == Trans2Quik.Result.SUCCESS && rc == 3)
          TraderReplyHandler(tid, (long)order_id, null);
        else
          TraderReplyHandler(tid, (long)order_id, msg.Length > 0 ? Trim(msg) : r + ", " + err);
    }

    // **********************************************************************

    void OrderStatusCallback(
      int nMode,
      int tid,
      double order_id,
      string classCode,
      string secCode,
      double price,
      int balance,
      double msum,
      int isSell,
      int status,
      int orderDescriptor)
    {
      if(nMode == 0 && OrderUpdateHandler != null)
      {
        int filled;

        if(isSell == 0)
          filled = Trans2Quik.ORDER_QTY(orderDescriptor) - balance;
        else
        {
          filled = balance - Trans2Quik.ORDER_QTY(orderDescriptor);
          balance = -balance;
        }

        if(status == 1)
          OrderUpdateHandler((long)order_id, balance, filled);
        else
          OrderUpdateHandler((long)order_id, 0, filled);
      }
    }

    // **********************************************************************
    // *                        Управление заявками                         *
    // **********************************************************************

    string SendOrder(char op, int price, int quantity, out int tid)
    {
      if(connected)
      {
        tid = ++transId;

        Trans2Quik.Result r = Trans2Quik.SEND_ASYNC_TRANSACTION(
          "TRANS_ID=" + tid +
          "; ACCOUNT=" + cfg.u.QuikAccount +
          "; CLIENT_CODE=" + cfg.u.QuikClientCode + "//" + cfg.FullProgName +
          "; SECCODE=" + secCode +
          "; CLASSCODE=" + classCode +
          "; ACTION=NEW_ORDER; OPERATION=" + op +
          "; PRICE=" + Price.GetRaw(price) +
          "; QUANTITY=" + quantity +
          ";",
          out error, msg, msg.Capacity);

        if(r == Trans2Quik.Result.SUCCESS)
          return null;
        else
        {
          tid = 0;
          return msg.Length > 0 ? Trim(msg.ToString()) : r + ", " + error;
        }
      }
      else
      {
        tid = 0;
        return NoConnectionText;
      }
    }

    // **********************************************************************

    public string SendBuyOrder(int price, int quantity, out int tid)
    {
      return SendOrder('B', price, quantity, out tid);
    }

    // **********************************************************************

    public string SendSellOrder(int price, int quantity, out int tid)
    {
      return SendOrder('S', price, quantity, out tid);
    }

    // **********************************************************************

    public string KillOrder(long oid)
    {
      if(connected)
      {
        transId++;

        Trans2Quik.Result r = Trans2Quik.SEND_ASYNC_TRANSACTION(
          "TRANS_ID=" + transId +
          "; SECCODE=" + secCode +
          "; CLASSCODE=" + classCode +
          "; ACTION=KILL_ORDER; ORDER_KEY=" + oid +
          ";",
          out error, msg, msg.Capacity);

        if(r == Trans2Quik.Result.SUCCESS)
          return null;
        else
          return msg.Length > 0 ? Trim(msg.ToString()) : r + ", " + error;
      }
      else
        return NoConnectionText;
    }

    // **********************************************************************
  }
}
