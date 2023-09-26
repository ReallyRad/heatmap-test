using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JSONEventReader : IEventReader
{
    private readonly string path;

    private readonly bool hasFileToRead;

    public JSONEventReader(string path)
    {
        this.path = path;
        hasFileToRead = Startup();
    }

    bool IEventReader.ReaderIsAvailable()
    {
        return hasFileToRead;
    }

    List<EventData> IEventReader.ReadEvents()
    {
        Dictionary<string, EventData> events = new Dictionary<string, EventData>();

        using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (BufferedStream bs = new BufferedStream(fs))
        using (StreamReader sr = new StreamReader(bs))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                BaseEvent baseEvent = JsonUtility.FromJson<BaseEvent>(line);

                if (baseEvent.EventName != null)
                {
                    AddBaseEventToEventData(baseEvent, events);
                }
                else
                {
                    Debug.Log("line is invalid : " + line);
                }
            }
        }

        return new List<EventData>(events.Values);
    }

    private void AddBaseEventToEventData(BaseEvent baseEvent, Dictionary<string, EventData> events)
    {
        EventData currentLineEvent;

        // if event name is not in the EventData list, new EventData should be created
        if (!events.TryGetValue(baseEvent.EventName, out currentLineEvent))
        {
            currentLineEvent = new EventData();
            currentLineEvent.EventName = baseEvent.EventName;
            events.Add(baseEvent.EventName, currentLineEvent);
        }

        MergedEventPosition eventPosition = new MergedEventPosition();
        eventPosition.Position = baseEvent.Position;

        currentLineEvent.Positions.Add(eventPosition);
    }

    private bool Startup()
    {
        if (File.Exists(path))
        {
            return true;
        }
        else
        {
            Debug.LogError("Invalid path, no file found: " + path);
            return false;
        }
    }
}
