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
using System.IO.IsolatedStorage;
using System.IO;

namespace YammerHub.Storage
{
    public static class AccessTokenStorage
    {

        public static Yammer.OAuthToken RetrieveAccessToken()
        {
            string key;

            IsolatedStorageFile myStore = IsolatedStorageFile.GetUserStoreForApplication();

            using (var isoFileStream = new IsolatedStorageFileStream("key.txt", FileMode.OpenOrCreate, myStore))
            {
                using (var isoFileReader = new StreamReader(isoFileStream))
                {
                    key = isoFileReader.ReadToEnd();
                }
            }

            if (string.IsNullOrWhiteSpace(key))
                return null;

            var keyParts = key.Split('&');
            if (keyParts.Length != 2)
                return null;

            if (string.IsNullOrWhiteSpace(keyParts[0]))
                return null;

            if (string.IsNullOrWhiteSpace(keyParts[1]))
                return null;

            return new Yammer.OAuthToken
            {
                Key = keyParts[0],
                Secret = keyParts[1]
            };
        }

        public static void SaveAccessToken(Yammer.OAuthToken token)
        {
            IsolatedStorageFile myStore = IsolatedStorageFile.GetUserStoreForApplication();

            using (var isoFileStream = new IsolatedStorageFileStream("key.txt", FileMode.OpenOrCreate, myStore))
            {
                // Read the data.
                using (var isoFileWriter = new StreamWriter(isoFileStream))
                {
                    isoFileWriter.Write(token.Key + '&' + token.Secret);
                }
            }
        }

        public static void ClearAccessToken()
        {
            IsolatedStorageFile myStore = IsolatedStorageFile.GetUserStoreForApplication();

            using (var isoFileStream = new IsolatedStorageFileStream("key.txt", FileMode.Create, myStore))
            {
                //Empty the file.
            }
        }

    }
}
