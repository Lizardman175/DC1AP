using System;

namespace DC1AP.Models
{
    public class ConnectClickedEventArgs : EventArgs
    {
        public string Host { get; set; }
        public string Slot { get; set; }
        public string Password { get; set; }
    }
}
