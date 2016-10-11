using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKiosk
{
    public abstract class ApplicatoinService
    {
        public string ID { get; private set; }
        public ApplicatoinService()
        {
            ID = Guid.NewGuid().ToString("N");
        }

        public event EventHandler CloseApp;
        public event EventHandler<NotificationArgs> OnNotificationEvent;
        public event EventHandler RequestPresentationEvent;

        public abstract object GetViewer();

        public abstract string GetApplicationName();

        protected void RaiseCloseAppEvent(object sender, EventArgs arg)
        {
            if (this.CloseApp != null)
            {
                this.CloseApp(sender, arg);
            }
        }

        public void SendNotification(string messageKey, string messageContent)
        {
            if (this.OnNotificationEvent != null)
            {
                this.OnNotificationEvent(this, new NotificationArgs(this.GetApplicationName(), messageKey, messageContent));
            }
        }

        public virtual void BeginPresentation(bool needStarted)
        {

        }


        public virtual void OnPresentation(bool needStarted)
        {

        }

        public virtual void BeginBackground()
        {

        }

        public virtual void OnBackground()
        {

        }

        public virtual void OnNotification(string from, string messageKey, string messageContent)
        {

        }



        protected void RequestPresentation()
        {
            if (this.RequestPresentationEvent != null)
            {
                this.RequestPresentationEvent(this, null);
            }
        }
    }

    public class NotificationArgs : EventArgs
    {
        public string From { private set; get; }
        public string Key { private set; get; }
        public string Content { private set; get; }
        public NotificationArgs(string from, string key, string content)
        {
            this.Key = key;
            this.Content = content;
            this.From = from;
        }
    }
}
