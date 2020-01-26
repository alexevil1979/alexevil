// =========================================================================
//   StockChannel.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =========================================================================

using WpfApp1;
using WpfApp1.Market;
using XlDde;

namespace QuikDdeConnector.Internals
{
  sealed class StockChannel : DdeChannel
  {
    // **********************************************************************

    public override string Topic { get { return "stock"; } }

    public event StockHandler StockHandler;

    // **********************************************************************

    const string cnAskVolume = "SELL_VOLUME";
    const string cnBidVolume = "BUY_VOLUME";
    const string cnPrice = "PRICE";

    // **********************************************************************

    public override void ProcessTable(XlTable xt)
    {
      if(xt.Rows < 3)
      {
        SetError("стакан пуст");
        return;
      }

      // ------------------------------------------------------------

      int cAskVolume = -1, cBidVolume = -1, cPrice = -1;

      for(int col = 0; col < xt.Columns; col++)
      {
        xt.ReadValue();

        if(xt.ValueType == XlTable.BlockType.String)
          switch(xt.StringValue)
          {
            case cnAskVolume:
              cAskVolume = col;
              break;
            case cnBidVolume:
              cBidVolume = col;
              break;
            case cnPrice:
              cPrice = col;
              break;
          }
      }

      if(cAskVolume < 0 || cBidVolume < 0 || cPrice < 0)
      {
        SetError("нет нужных столбцов");
        return;
      }

      // ------------------------------------------------------------

      Quote[] quotes = new Quote[xt.Rows - 1];
      int ask = -1, bid = -1;

      // ------------------------------------------------------------

      for(int row = 0; row < quotes.Length; row++)
      {
        int p = 0, av = 0, bv = 0, sc = 0;

        for(int col = 0; col < xt.Columns; col++)
        {
          xt.ReadValue();

          switch(xt.ValueType)
          {
            case XlTable.BlockType.Float:
              if(col == cAskVolume)
                av = (int)xt.FloatValue;
              else if(col == cBidVolume)
                bv = (int)xt.FloatValue;
              else if(col == cPrice)
                p = Price.GetInt(xt.FloatValue);
              break;

            case XlTable.BlockType.String:
              sc++;
              break;
          }
        }

        if(p <= 0)
        {
          if(sc == xt.Columns)
            break;
          else
          {
            SetError("ошибка в данных");
            return;
          }
        }

        if(av > 0)
        {
          ask = row;

          quotes[row] = new Quote(p, av, QuoteType.Ask);
        }
        else if(bv > 0)
        {
          if(bid == -1)
            bid = row;

          quotes[row] = new Quote(p, bv, QuoteType.Bid);
        }
        else
        {
          SetError("нет объема");
          return;
        }
      }

      // ------------------------------------------------------------

      if(quotes[0].Price <= quotes[1].Price)
      {
        SetError("стакан перевернут");
        return;
      }

      if(ask == -1 || bid == -1)
      {
        SetError("неполный спред");
        return;
      }

      // ------------------------------------------------------------

      quotes[ask].Type = QuoteType.BestAsk;
      quotes[bid].Type = QuoteType.BestBid;

      if(StockHandler != null)
        StockHandler(quotes, new Spread(quotes[ask].Price, quotes[bid].Price));
    }

    // **********************************************************************
  }
}
