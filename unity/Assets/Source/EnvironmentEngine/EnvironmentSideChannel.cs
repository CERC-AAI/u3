using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Text;
using System;
using System.Text.RegularExpressions;

public class EnvironmentSideChannel : SideChannel
{
    public EnvironmentSideChannel()
    {
        ChannelId = new Guid("621f0a70-4f87-11ea-a6bf-784f4387d1f7");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        Debug.Log(msg.ToString());
        EnvironmentManager.Instance.OnMessageReceived(msg);
    }

    public void SendEventToPython(string eventText)
    {
        using (var msgOut = new OutgoingMessage())
        {
            msgOut.WriteString(eventText);
            QueueMessageToSend(msgOut);
        }
    }
}