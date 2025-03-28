﻿namespace PBIRInspectorLibrary
{
    public class MessageIssuedEventArgs : EventArgs
    {
        public MessageIssuedEventArgs(string message, MessageTypeEnum messageType)
        {
            Message = message;
            MessageType = messageType;
        }

        public string Message { get; private set; }
        public MessageTypeEnum MessageType { get; private set; }
        public bool DialogOKResponse { get; set; }
    }

    public enum MessageTypeEnum
    {
        Error,
        Warning,
        Information,
        Dialog,
        Complete
    }
}