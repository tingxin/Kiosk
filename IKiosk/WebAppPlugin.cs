using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IKiosk
{
    public abstract class WebAppPlugin
    {
        public event EventHandler<NotificationArgs> OnNotificationEvent;

        public void SendNotification(string messageKey, string messageContent)
        {
            if (this.OnNotificationEvent != null)
            {
                this.OnNotificationEvent(this, new NotificationArgs(this.GetPluginName(), messageKey, messageContent));
            }
        }

        public abstract string GetPluginName();

        public abstract string GetHostName();

        public virtual void OnNotification(string from, string messageKey, string messageConetnt)
        {

        }
    }
}
