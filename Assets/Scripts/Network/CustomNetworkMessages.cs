using Mirror;
using System;

[System.Serializable]
public struct CountdownMessage : NetworkMessage
{
    public enum DisplayMode
    {
        Both,
        Minutes,
        Seconds,
        None,
    }

    public int Duration;
    public string MessageFormat;
    public string MessageElement;
    public DisplayMode MessageDisplay;
}
