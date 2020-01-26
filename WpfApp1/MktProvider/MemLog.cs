// =====================================================================
//    MemLog.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp1.Market.Internals
{
  sealed class MemLog
  {
    // **********************************************************************

    sealed class Record
    {
      public readonly DateTime DateTime;
      public readonly string Text;

      public int Repeats { get; private set; }

      public Record(string text)
      {
        this.DateTime = DateTime.UtcNow;
        this.Text = text;
      }

      public void Repeat() { Repeats++; }
    }

    // **********************************************************************

    int maxDataRecords;
    Queue<Record> data;

    Record last;

    string name;
    DateTime started;

    int eventsCount;

    // **********************************************************************

    public int EventsCount { get { return eventsCount; } }

    // **********************************************************************

    public MemLog(string name, int depth)
    {
      if(depth > 1)
        this.maxDataRecords = depth;
      else
        this.maxDataRecords = 1;

      data = new Queue<Record>(maxDataRecords);
      last = new Record(string.Empty);

      this.name = name;
      this.started = last.DateTime;
    }

    // **********************************************************************

    public void Put(string text)
    {
      eventsCount++;

      if(text == last.Text)
        last.Repeat();
      else
        lock(data)
        {
          while(data.Count >= maxDataRecords)
            data.Dequeue();

          last = new Record(text);
          data.Enqueue(last);
        }
    }

    // **********************************************************************

    public string GetData(string dateTimeFormat = "HH:mm:ss", bool fill = false)
    {
      string str = started.ToLocalTime().ToString(dateTimeFormat);

      StringBuilder sb = new StringBuilder((data.Count + 1) * (str.Length + 60));

      sb.Append(str);
      sb.Append(" ");
      sb.Append(name);
      sb.Append(". Всего событий: ");

      if(eventsCount > 0)
        lock(data)
        {
          sb.AppendLine(eventsCount.ToString());

          foreach(Record r in data)
          {
            str = r.DateTime.ToLocalTime().ToString(dateTimeFormat);

            sb.AppendLine();
            sb.Append(str);
            sb.Append(" ");
            sb.Append(r.Text);

            if(r.Repeats > 0)
            {
              sb.AppendLine();

              if(fill)
                sb.Append(new string(' ', str.Length + 1));

              sb.Append("\x2192 кол-во повторов предыдущего события: ");
              sb.Append(r.Repeats);
            }
          }
        }
      else
        sb.Append("0");

      return sb.ToString();
    }

    // **********************************************************************
  }
}
