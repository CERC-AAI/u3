using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Text;
using System;
using System.Text.RegularExpressions;

public class EnvironmentSideChannel : SideChannel
{
    EnvironmentBrain mBrain;

    public EnvironmentSideChannel(EnvironmentBrain brain)
    {
        ChannelId = new Guid("621f0a70-4f87-11ea-a6bf-784f4387d1f7");
        mBrain = brain;
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        string receivedString = msg.ReadString();

        //Debug.Log(receivedString);

        if (receivedString.Substring(0, 2) == "id")
        {
            string idString = receivedString.Substring(2);

            mBrain.SetID(idString);
        }
        else if (receivedString.Substring(0, 3) == "run")
        {
            string commandString = receivedString.Substring(3);

            mBrain.RunCommand(commandString);
        }
        else if (receivedString.Substring(0, 4) == "seed")
        {
            string seedString = receivedString.Substring(4);
            int seedValue;

            if (int.TryParse(seedString, out seedValue))
            {
                mBrain.Seed(seedValue);
            }
        }
        else if (receivedString.Substring(0, 5) == "param")
        {
            string staticString = receivedString.Substring(5);

            JSONObject paramData = new JSONObject(staticString);

            mBrain.SetParams(paramData);
        }
        else if (receivedString.Substring(0, 6) == "static")
        {
            string staticString = receivedString.Substring(6);
            int staticValue;

            if (int.TryParse(staticString, out staticValue))
            {
                mBrain.SetStatic(staticValue != 0);
            }
        }
        else if (receivedString.Substring(0, 8) == "training")
        {
            string trainingString = receivedString.Substring(8);
            int trainingValue;

            if (int.TryParse(trainingString, out trainingValue))
            {
                mBrain.SetPythonTraining(trainingValue != 0);
            }
        }
        else if (receivedString.Substring(0, 7) == "element")
        {
            string elementString = receivedString.Substring(7);
            string elementName;
            string elementData;

            Regex regex = new Regex(@"([^|]*)\|(.*)");
            MatchCollection matches = regex.Matches(elementString);

            if (matches.Count == 1)
            {
                elementName = matches[0].Groups[1].Value;
                elementData = matches[0].Groups[2].Value;

                mBrain.SetEnvironmentElement(elementName, elementData);
            }
            else
            {
                Debug.Log("Could not parse element string: " + elementString);
            }
        }
        else if (receivedString.Substring(0, 5) == "reset")
        {
            mBrain.ClearEnvironmentElements();
        }
        else if (receivedString.Substring(0, 8) == "getstate")
        {
            mBrain.SendChannelMessage("state" + mBrain.BuildRunStateToJSON().ToString());
        }
        
        else if (receivedString.Substring(0, 3) == "get")
        {
            string getString = receivedString.Substring(3);

            JSONObject tempJSON = mBrain.BuildRunStateToJSON();

            if (tempJSON.keys.Contains(getString))
            {
                mBrain.SendChannelMessage("get" + tempJSON[getString].ToString());
            }
        }        
        else if (receivedString.Substring(0, 9) == "loadstate")
        {
            string loadStateString = receivedString.Substring(9);

            mBrain.LoadRunStateFromJSON(new JSONObject(loadStateString));
        }
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