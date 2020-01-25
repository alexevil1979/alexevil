// =======================================================================
//    DataFile.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// =======================================================================

using System;
using System.IO;

namespace WpfApp1.History.Internals
{
  // ************************************************************************

  enum StreamType
  {
    Stock = 0x10,
    Ticks = 0x20,
    Orders = 0x30,
    Trades = 0x40,
    Messages = 0x50,
    None = 0
  }

  // ************************************************************************

  sealed class DataFileHeader
  {
    public readonly string RecorderName;
    public readonly string Description;
    public readonly DateTime BaseDateTime;
    public readonly int StreamsCount;

    public DataFileHeader(
      string recorderName, string description,
      DateTime baseDateTime, int streamsCount)
    {
      this.RecorderName = recorderName;
      this.Description = description;
      this.BaseDateTime = baseDateTime;
      this.StreamsCount = streamsCount;
    }
  }

  // ************************************************************************

  struct StreamHeader
  {
    public StreamType Type;

    public Security Security;
    public int PriceRatio;
    public int PriceStep;

    public StreamHeader(StreamType type,
      Security security = new Security(),
      int priceRatio = 0, int priceStep = 0)
    {
      this.Type = type;
      this.Security = security;
      this.PriceRatio = priceRatio;
      this.PriceStep = priceStep;
    }
  }

  // ************************************************************************

  static class DataFile
  {
    // ----------------------------------------------------------------------

    static readonly char[] FilePrefix = "QScalp History Data".ToCharArray();
    const byte FileVersion = 2;

    // ----------------------------------------------------------------------

    public static void WriteHeader(BinaryWriter bw, DataFileHeader h)
    {
      if(h.StreamsCount > byte.MaxValue)
        throw new OverflowException("Слишком много потоков для записи");

      bw.Write(FilePrefix);
      bw.Write(FileVersion);
      bw.Write(h.RecorderName);
      bw.Write(h.Description);
      bw.Write(h.BaseDateTime.Ticks);
      bw.Write((byte)h.StreamsCount);
    }

    // ----------------------------------------------------------------------

    public static DataFileHeader ReadHeader(BinaryReader br)
    {
      char[] prefix = br.ReadChars(FilePrefix.Length);

      for(int i = 0; i < prefix.Length; i++)
        if(prefix[i] != FilePrefix[i])
          throw new FormatException("Неверный формат файла");

      if(br.ReadByte() != FileVersion)
        throw new FormatException("Неподдерживаемая версия файла");

      return new DataFileHeader(br.ReadString(), br.ReadString(),
        new DateTime(br.ReadInt64(), DateTimeKind.Utc), br.ReadByte());
    }

    // ----------------------------------------------------------------------

    public static void WriteStreamHeader(BinaryWriter bw, StreamHeader h)
    {
      bw.Write((byte)h.Type);

      if(h.Type != StreamType.Messages)
      {
        bw.Write(h.Security.SecCode);
        bw.Write(h.Security.ClassCode);

        if(h.Type != StreamType.Ticks)
          bw.Write((float)(h.PriceRatio > 0 ? (double)h.PriceStep / h.PriceRatio : 0));
      }
    }

    // ----------------------------------------------------------------------

    public static StreamHeader ReadStreamHeader(BinaryReader br)
    {
      StreamHeader h = new StreamHeader((StreamType)br.ReadByte());

      if(h.Type != StreamType.Messages)
      {
        h.Security.SecCode = br.ReadString();
        h.Security.ClassCode = br.ReadString();

        double step;

        if(h.Type != StreamType.Ticks && (step = br.ReadSingle()) > 0)
        {
          int precision = (int)Math.Ceiling(-Math.Log10(step));

          if(precision < 0)
            precision = 0;

          h.PriceRatio = (int)Math.Round(Math.Pow(10, precision));
          h.PriceStep = (int)Math.Round(step * h.PriceRatio);
        }
      }

      return h;
    }

    // ----------------------------------------------------------------------
  }

  // ************************************************************************
}
