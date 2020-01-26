// ======================================================================
//    QuikDde.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// ======================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using WpfApp1;
using WpfApp1.Market;
using QuikDdeConnector.Internals;
using XlDde;

namespace QuikDdeConnector
{
  sealed class QuikDde : IStockProvider, ITicksProvider, ISecListProvider
  {
    // **********************************************************************

    StockChannel stockChannel = new StockChannel();
    TicksChannel ticksChannel = new TicksChannel();

    bool stockActive;
    bool ticksActive;

    string service;
    XlDdeServer server;

    StatusUpdateHandler errorHandler;

    // **********************************************************************

    void CreateServer()
    {
      if(server == null)
        try
        {
          server = new XlDdeServer(service);
          server.Register();
        }
        catch(Exception e)
        {
          if(errorHandler != null)
            errorHandler.Invoke("Ошибка создания сервера DDE: " + e.Message);
        }
    }

    // **********************************************************************

    void DisposeServer()
    {
      if(server != null)
      {
        try
        {
          server.Disconnect();
          server.Dispose();
        }
        catch(Exception e)
        {
          if(errorHandler != null)
            errorHandler.Invoke("Ошибка удаления сервера DDE: " + e.Message);
        }

        server = null;

        stockActive = false;
        ticksActive = false;
      }
    }

    // **********************************************************************

    string IConnector.Name { get { return "Quik DDE"; } }

    // **********************************************************************

    void IConnector.Setup()
    {
      if(service != cfg.u.DdeServerName)
      {
        DisposeServer();
        service = cfg.u.DdeServerName;
      }
    }

    // **********************************************************************

    event StatusUpdateHandler IStockProvider.Connected
    {
      add { stockChannel.Connected += value; }
      remove { stockChannel.Connected -= value; }
    }

    event StatusUpdateHandler IStockProvider.Disconnected
    {
      add { stockChannel.Disconnected += value; }
      remove { stockChannel.Disconnected -= value; }
    }

    event StatusUpdateHandler IStockProvider.Broken
    {
      add { stockChannel.Broken += value; errorHandler += value; }
      remove { stockChannel.Broken -= value; errorHandler -= value; }
    }

    event StockHandler IStockProvider.StockHandler
    {
      add { stockChannel.StockHandler += value; }
      remove { stockChannel.StockHandler -= value; }
    }

    // **********************************************************************

    void IStockProvider.Subscribe(string symbol)
    {
      CreateServer();

      if(!stockActive && server != null)
      {
        stockActive = true;
        server.AddChannel(stockChannel);
      }
    }

    // **********************************************************************

    void IStockProvider.Unsubscribe()
    {
      if(stockActive && server != null)
      {
        stockActive = false;
        server.RmvChannel(stockChannel);

        if(!ticksActive)
          DisposeServer();
      }
    }

    // **********************************************************************

    event StatusUpdateHandler ITicksProvider.Connected
    {
      add { ticksChannel.Connected += value; }
      remove { ticksChannel.Connected -= value; }
    }

    event StatusUpdateHandler ITicksProvider.Disconnected
    {
      add { ticksChannel.Disconnected += value; }
      remove { ticksChannel.Disconnected -= value; }
    }

    event StatusUpdateHandler ITicksProvider.Broken
    {
      add { ticksChannel.Broken += value; errorHandler += value; }
      remove { ticksChannel.Broken -= value; errorHandler -= value; }
    }

    event TickHandler ITicksProvider.TickHandler
    {
      add { ticksChannel.TickHandler += value; }
      remove { ticksChannel.TickHandler -= value; }
    }

    // **********************************************************************

    void ITicksProvider.Subscribe(HashSet<string> symbols)
    {
      CreateServer();

      if(!ticksActive && server != null)
      {
        ticksActive = true;
        server.AddChannel(ticksChannel);
      }
    }

    // **********************************************************************

    void ITicksProvider.Unsubscribe()
    {
      if(ticksActive && server != null)
      {
        ticksActive = false;
        server.RmvChannel(ticksChannel);

        if(!stockActive)
          DisposeServer();
      }
    }

    // **********************************************************************

    void ISecListProvider.GetSecList(Action<SecList> callback)
    {
      const string secListFileName = "seclist.csv";

      const int classNameIndex = 0;
      const int classCodeIndex = 1;
      const int secNameIndex = 2;
      const int secCodeIndex = 3;
      const int priceStepIndex = 4;

      SecList secList = new SecList();

      try
      {
        using(StreamReader stream = new StreamReader(cfg.AsmPath + secListFileName))
        {
          char[] delimiter = new char[] { ';' };
          string line;

          while((line = stream.ReadLine()) != null)
          {
            string[] str = line.Split(delimiter);
            double step;

            if(str.Length < 5 || !double.TryParse(str[priceStepIndex],
              NumberStyles.Float, NumberFormatInfo.InvariantInfo, out step))
              throw new FormatException("Неверный формат файла.");

            secList.Add(str[secNameIndex], str[secCodeIndex],
              str[classNameIndex], str[classCodeIndex], step);
          }
        }
      }
      catch(Exception e)
      {
        secList.Error = e.Message;
      }

      callback.Invoke(secList);
    }

    // **********************************************************************
  }
}
