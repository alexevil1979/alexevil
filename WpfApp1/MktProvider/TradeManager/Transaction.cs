// ==========================================================================
//    Transaction.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// ==========================================================================

using System;
using System.Threading;

namespace WpfApp1.Market.Manager.Internals
{
  public sealed class Transaction : IDisposable
  {
    // **********************************************************************

    public enum States { Execute, Cancel, Passive, Disposed }

    // **********************************************************************

    public readonly string Descr;
    public OwnAction Source;

    public States State { get; private set; }

    public TradeOp Operation;

    public int TId;
    public long OId;

    public int Filled;

    public int Price;
    public int Volume;

    public bool Trailing;

    public Timer Timer;

    // **********************************************************************

    public Transaction(string descr, OwnAction source)
    {
      this.Descr = descr;
      this.Source = source;

      this.State = States.Execute;
    }

    // **********************************************************************

    public void Execute() { TId = 0; State = States.Execute; }
    public void Cancel() { State = States.Cancel; }
    public void Processed() { State = States.Passive; }

    public void Dispose()
    {
      State = States.Disposed;
      if(Timer != null)
      {
        Timer.Dispose();
        Timer = null;
      }

      //MktProvider.Log.Put("Операция завершена: " + Descr + " " + Source.Operation);
    }

    // **********************************************************************
  }
}
