// ==========================================================================
//    XlDdeServer.cs (c) 2012 Nikolay Moroshkin, http://www.moroshkin.com/
// ==========================================================================

using System;
using System.Collections.Generic;

using NDde.Server;

namespace XlDde
{
  // ************************************************************************
  // *                       XlDdeChannel interface                         *
  // ************************************************************************

  abstract class XlDdeChannel
  {
    // --------------------------------------------------------------

    public abstract string Topic { get; }
    public abstract void ProcessTable(XlTable xt);

    // --------------------------------------------------------------

    public event Action ConversationAdded;
    public event Action ConversationRemoved;

    // --------------------------------------------------------------

    List<DdeConversation> conversations = new List<DdeConversation>();

    // --------------------------------------------------------------

    public int Conversations { get { return conversations.Count; } }

    // --------------------------------------------------------------

    public void AddConversation(DdeConversation dc)
    {
      lock(conversations)
        conversations.Add(dc);

      if(ConversationAdded != null)
        ConversationAdded();
    }

    // --------------------------------------------------------------

    public void RemoveConversation(DdeConversation dc)
    {
      bool removed;

      lock(conversations)
        removed = conversations.Remove(dc);

      if(removed && ConversationRemoved != null)
        ConversationRemoved();
    }

    // --------------------------------------------------------------

    public DdeConversation[] DropConversations()
    {
      DdeConversation[] dcArray;

      lock(conversations)
      {
        dcArray = conversations.ToArray();
        conversations.Clear();
      }

      if(ConversationRemoved != null)
        for(int i = 0; i < dcArray.Length; i++)
          ConversationRemoved();

      return dcArray;
    }

    // --------------------------------------------------------------
  }

  // ************************************************************************
  // *                          XlDdeServer class                           *
  // ************************************************************************

  sealed class XlDdeServer : DdeServer
  {
    // --------------------------------------------------------------

    Dictionary<string, XlDdeChannel> channels;

    // --------------------------------------------------------------

    public XlDdeServer(string service)
      : base(service)
    {
      channels = new Dictionary<string, XlDdeChannel>();
    }

    // --------------------------------------------------------------

    public void AddChannel(XlDdeChannel channel)
    {
      channels.Add(channel.Topic, channel);
    }

    // --------------------------------------------------------------

    public void RmvChannel(XlDdeChannel channel)
    {
      channels.Remove(channel.Topic);

      foreach(DdeConversation c in channel.DropConversations())
        try { base.Disconnect(c); }
        catch { }
    }

    // --------------------------------------------------------------

    public override void Disconnect(DdeConversation dc)
    {
      XlDdeChannel channel;
      if(channels.TryGetValue(dc.Topic, out channel))
        channel.RemoveConversation(dc);

      base.Disconnect(dc);
    }

    // --------------------------------------------------------------

    public override void Disconnect()
    {
      foreach(XlDdeChannel channel in channels.Values)
        channel.DropConversations();

      base.Disconnect();
    }

    // --------------------------------------------------------------

    protected override bool OnBeforeConnect(string topic)
    {
      return channels.ContainsKey(topic);
    }

    // --------------------------------------------------------------

    protected override void OnAfterConnect(DdeConversation dc)
    {
      XlDdeChannel channel = channels[dc.Topic];
      dc.Tag = channel;
      channel.AddConversation(dc);
    }

    // --------------------------------------------------------------

    protected override void OnDisconnect(DdeConversation dc)
    {
      ((XlDdeChannel)dc.Tag).RemoveConversation(dc);
    }

    // --------------------------------------------------------------

    protected override PokeResult OnPoke(DdeConversation dc,
      string item, byte[] data, int format)
    {
      //if(format != xlTableFormat)
      //  return PokeResult.NotProcessed;

      using(XlTable xt = new XlTable(data))
        ((XlDdeChannel)dc.Tag).ProcessTable(xt);

      return PokeResult.Processed;
    }

    // --------------------------------------------------------------
  }

  // ************************************************************************
}
