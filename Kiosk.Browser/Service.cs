using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awesomium.Windows.Controls;
using IKiosk;

namespace Kiosk.Browser
{
    public class Service : ApplicatoinService
    {
        const string AppUriFormat = "{0}?{1}";
        const string MultipleUriFormat = "{0}&{1}";
        string name = string.Empty;
        string uri = string.Empty;
        Wall wall;

        List<WebAppPlugin> plugins;
        public Service(string sName, string sUri)
            : base()
        {
            this.name = sName;
            this.uri = sUri;
            this.plugins = new List<WebAppPlugin>();
        }


        public override object GetViewer()
        {
            if (wall == null)
            {
                wall = new Wall();
                wall.SetAddress(this.uri);
                wall.btnBack.Click += btnBack_Click;
            }
            return wall;
        }

        public override string GetApplicationName()
        {
            return this.name;
        }

        public override void OnBackground()
        {
            base.OnBackground();
            this.wall.ShowUI();
        }

        public override void OnPresentation(bool needStarted)
        {
            base.OnPresentation(needStarted);
            this.wall.ShowUI();
            this.wall.btnBack.Visibility = System.Windows.Visibility.Visible;
        }

        public override void BeginBackground()
        {
            base.BeginBackground();
            this.wall.ShowSnapShot();
            this.wall.btnBack.Visibility = System.Windows.Visibility.Collapsed;
        }

        public override void BeginPresentation(bool needStarted)
        {
            base.BeginPresentation(needStarted);
            this.wall.ShowSnapShot();
        }

        public void AddPlugin(WebAppPlugin plugin)
        {

            bool isIn = false;
            foreach (WebAppPlugin item in this.plugins)
            {
                if (item.GetPluginName() == plugin.GetPluginName())
                {
                    isIn = true;
                    break;
                }
            }
            if (isIn == false)
            {
                plugin.OnNotificationEvent += plugin_OnNotificationEvent;
                this.plugins.Add(plugin);
            }
        }

        public override void OnNotification(string from, string messageKey, string messageContent)
        {
            base.OnNotification(from, messageKey, messageContent);

            foreach (WebAppPlugin item in this.plugins)
            {
                item.OnNotification(from, messageKey, messageContent);
            }
        }

        void plugin_OnNotificationEvent(object sender, NotificationArgs e)
        {
            string newAddress = this.uri;
            if (this.uri.Contains('?'))
            {
                newAddress = string.Format(MultipleUriFormat, this.uri, e.Content.ToLower());
            }
            else
            {
                newAddress = string.Format(AppUriFormat, this.uri, e.Key.ToLower(), e.Content.ToLower());
            }
            wall.SetAddress(newAddress);

            this.RequestPresentation();
        }


        void btnBack_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseCloseAppEvent(this, e);
        }



    }
}
