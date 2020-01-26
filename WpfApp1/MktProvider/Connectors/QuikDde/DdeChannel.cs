// =========================================================================
//    DdeChannel.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =========================================================================

using WpfApp1.Market;
using XlDde;

namespace QuikDdeConnector.Internals
{
  abstract class DdeChannel : XlDdeChannel
  {
    // **********************************************************************

    public event StatusUpdateHandler Connected;
    public event StatusUpdateHandler Disconnected;
    public event StatusUpdateHandler Broken;

    // **********************************************************************

    public DdeChannel()
    {
      this.ConversationAdded += () =>
      {
        if(Connected != null)
          Connected("Канал \'" + Topic + "\' подключен");
      };

      this.ConversationRemoved += () =>
      {
        if(Disconnected != null)
          Disconnected("Канал \'" + Topic + "\' отключен");
      };
    }

    // **********************************************************************

    protected void SetError(string text)
    {
      if(Broken != null)
        Broken(Topic + ": " + text);
    }

    // **********************************************************************
  }
}