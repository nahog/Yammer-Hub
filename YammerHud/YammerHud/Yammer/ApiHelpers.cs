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
    public static class ApiHelpers
    {
        public static string GetQueryParameter(this string uri, string parameter)
        {
            var parameters = uri.Split('&');

            foreach (var p in parameters)
            {
                if (p.Contains(parameter))
                {
                    var parts = p.Split('=');
                    if (parts.Length == 2)
                        return parts[1];
                    else
                        return string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
