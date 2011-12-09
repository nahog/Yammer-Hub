using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace YammerHub.Yammer
{
    public class Message
    {
        public Body body { get; set; }
        public string sender_id { get; set; }
        public string replied_to_id { get; set; }
        public string created_at { get; set; }

        public DateTimeOffset CreatedAt
        {
            get
            {
                DateTimeOffset output;
                if (DateTimeOffset.TryParse(created_at, out output))
                    return output;
                else
                    return DateTimeOffset.MinValue;
            }
        }
    }
}
