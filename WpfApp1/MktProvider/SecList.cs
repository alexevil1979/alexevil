// ======================================================================
//    SecList.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// ======================================================================

using System;
using System.Collections.Generic;

namespace WpfApp1.Market
{
  sealed class SecList : SortedDictionary<string, SecList.Class>
  {
    // **********************************************************************

    public sealed class Class : List<Record>
    {
      public readonly string Code;
      public Class(string code) { this.Code = code; }
    }

    // **********************************************************************

    public struct Record : IComparable
    {
      public readonly string Name;
      public readonly string Code;
      public readonly double Step;

      public Record(string name, string code, double step)
      {
        Name = name;
        Code = code;
        Step = step;
      }

      public int CompareTo(object obj)
      {
        if(obj is Record)
          return this.Name.CompareTo(((Record)obj).Name);
        else
          return -1;
      }
    }

    // **********************************************************************

    public string Error { get; set; }

    // **********************************************************************

    public void Add(string secName, string secCode,
      string className, string classCode, double step)
    {
      Class c;

      if(!this.TryGetValue(className, out c))
        this.Add(className, c = new Class(classCode));

      c.Add(new Record(secName, secCode, step));
    }

    // **********************************************************************
  }
}
