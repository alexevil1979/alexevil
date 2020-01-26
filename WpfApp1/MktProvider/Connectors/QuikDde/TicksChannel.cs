// =========================================================================
//   TicksChannel.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =========================================================================

using System;
using System.Globalization;

using WpfApp1;
using WpfApp1.Market;
using XlDde;

namespace QuikDdeConnector.Internals
{
  sealed class TicksChannel : DdeChannel
  {
    // **********************************************************************

    public override string Topic { get { return "trades"; } }

    // **********************************************************************

    public event TickHandler TickHandler;

    // **********************************************************************

    const string cnDate = "TRADEDATE";
    const string cnTime = "TRADETIME";
    const string cnSecCode = "SECCODE";
    const string cnClassCode = "CLASSCODE";
    const string cnPrice = "PRICE";
    const string cnQuantity = "QTY";
    const string cnOperation = "BUYSELL";

    const string strBuyOp = "BUY";
    const string strSellOp = "SELL";

    // **********************************************************************

    DateTimeFormatInfo dtFmtInfo;
    string dtFmt;

    // **********************************************************************

    int cDate;
    int cTime;
    int cPrice;
    int cQuantity;
    int cOp;
    int cSecCode;
    int cClassCode;

    bool columnsUnknown = true;

    // **********************************************************************

    public TicksChannel()
    {
      dtFmtInfo = DateTimeFormatInfo.CurrentInfo;
      dtFmt = dtFmtInfo.ShortDatePattern + dtFmtInfo.LongTimePattern;

      this.ConversationRemoved += () => { columnsUnknown = true; };
    }

    // **********************************************************************

    public override void ProcessTable(XlTable xt)
    {
      int row = 0;

      // ------------------------------------------------------------

      if(columnsUnknown)
      {
        cDate = -1;
        cTime = -1;
        cPrice = -1;
        cQuantity = -1;
        cOp = -1;
        cSecCode = -1;
        cClassCode = -1;

        for(int col = 0; col < xt.Columns; col++)
        {
          xt.ReadValue();

          if(xt.ValueType == XlTable.BlockType.String)
            switch(xt.StringValue)
            {
              case cnDate:
                cDate = col;
                break;
              case cnTime:
                cTime = col;
                break;
              case cnPrice:
                cPrice = col;
                break;
              case cnQuantity:
                cQuantity = col;
                break;
              case cnOperation:
                cOp = col;
                break;
              case cnSecCode:
                cSecCode = col;
                break;
              case cnClassCode:
                cClassCode = col;
                break;
            }
        }

        if(cDate < 0
          || cTime < 0
          || cPrice < 0
          || cQuantity < 0
          || cOp < 0
          || cSecCode < 0
          || cClassCode < 0)
        {
          SetError("нет нужных столбцов");
          return;
        }

        row++;
        columnsUnknown = false;
      }

      // ------------------------------------------------------------

      while(row++ < xt.Rows)
      {
        bool rowCorrect = true;

        string secCode = string.Empty;
        string classCode = string.Empty;

        string date = string.Empty;
        string time = string.Empty;

        Tick t = new Tick();

        // ----------------------------------------------------------

        for(int col = 0; col < xt.Columns; col++)
        {
          xt.ReadValue();

          if(col == cDate)
          {
            if(xt.ValueType == XlTable.BlockType.String)
              date = xt.StringValue;
            else
              rowCorrect = false;
          }
          else if(col == cTime)
          {
            if(xt.ValueType == XlTable.BlockType.String)
              time = xt.StringValue;
            else
              rowCorrect = false;
          }
          else if(col == cPrice)
          {
            if(xt.ValueType == XlTable.BlockType.Float)
              t.RawPrice = xt.FloatValue;
            else
              rowCorrect = false;
          }
          else if(col == cQuantity)
          {
            if(xt.ValueType == XlTable.BlockType.Float)
              t.Volume = (int)xt.FloatValue;
            else
              rowCorrect = false;
          }
          else if(col == cOp)
          {
            if(xt.ValueType == XlTable.BlockType.String)
              switch(xt.StringValue)
              {
                case strBuyOp:
                  t.Op = TradeOp.Buy;
                  break;
                case strSellOp:
                  t.Op = TradeOp.Sell;
                  break;
              }
            else
              rowCorrect = false;
          }
          else if(col == cSecCode)
          {
            if(xt.ValueType == XlTable.BlockType.String)
              secCode = xt.StringValue;
            else
              rowCorrect = false;
          }
          else if(col == cClassCode)
          {
            if(xt.ValueType == XlTable.BlockType.String)
              classCode = xt.StringValue;
            else
              rowCorrect = false;
          }
        }

        // ----------------------------------------------------------

        if(rowCorrect)
        {
          if(DateTime.TryParseExact(date + time, dtFmt, dtFmtInfo,
            DateTimeStyles.None, out t.DateTime))
          {
            if(TickHandler != null)
              TickHandler(Security.GetKey(secCode, classCode), t);
          }
          else
          {
            SetError("не распознан формат даты или времени");
            return;
          }
        }
        else
        {
          SetError("ошибка в данных");
          return;
        }

        // ----------------------------------------------------------
      }
    }

    // **********************************************************************
  }
}
