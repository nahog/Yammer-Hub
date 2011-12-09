using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Phone.Shell;

namespace YammerHub
{
    public partial class MainPage : PhoneApplicationPage
    {
        Yammer.OAuthToken accessToken;
        MainViewModel data = new MainViewModel();

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = data;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            HandleError(() => RefreshFeed());
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            HandleError(() => RefreshFeed(true));
        }

        private void ToogleSignInState(bool state)
        {
            this.data.IsSignedIn = state;
        }

        void RefreshFeed(bool force = false)
        {
            HandleError(() =>
            {
                // Try to get the access token from out isolated storage
                accessToken = Storage.AccessTokenStorage.RetrieveAccessToken();

                if (accessToken != null)
                {
                    // If we already have the access token in the storage, just load the current feed.
                    ToogleSignInState(true);
                    Dispatcher.BeginInvoke(() =>
                    {
                        (this.ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                        (this.ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).IsEnabled = true;
                    });
                    LoadStream(force);
                }
                else
                {
                    Authorize(() =>
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            ToogleSignInState(true);
                            (this.ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                            (this.ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).IsEnabled = true;
                            LoadStream(force);
                        });
                    });
                }
            });
        }

        private void Authorize(Action afterAuthorize)
        {
            HandleError(() =>
            {
                // We don't have the access token in the storage, start the OAuth autorization process.
                var auth = new Yammer.ApiAuthorization(AuthBrowser);
                auth.AfterBrowsingComplete += new Yammer.ApiAuthorization.AfterBrowsingCompleteHandler(auth_AfterBrowsingComplete);
                auth.BeforeBrowsingBegin += new Yammer.ApiAuthorization.BeforeBrowsingBeginHandler(auth_BeforeBrowsingBegin);
                auth.BeginAuthorization(at =>
                {
                    // Now that the authorization process is completed, save the access token for future uses and load the current feed.
                    accessToken = at;
                    Storage.AccessTokenStorage.SaveAccessToken(at);
                    afterAuthorize();
                });
            });
        }

        void auth_BeforeBrowsingBegin()
        {
            // Hide the pivot control and show the browser to let the user interact with the OAuth authorization process
            Dispatcher.BeginInvoke(() =>
            {
                this.AuthBrowserContainer.Visibility = System.Windows.Visibility.Visible;
                this.Pivot.Visibility = System.Windows.Visibility.Collapsed;
                this.PivotIcon.Visibility = System.Windows.Visibility.Collapsed;
            });
        }

        void auth_AfterBrowsingComplete()
        {
            // Once the user has authorized our app, hide the browser
            Dispatcher.BeginInvoke(() =>
            {
                this.PivotIcon.Visibility = System.Windows.Visibility.Visible;
                this.Pivot.Visibility = System.Windows.Visibility.Visible;
                this.AuthBrowserContainer.Visibility = System.Windows.Visibility.Collapsed;
            });
        }

        private void ComposeButton_Click(object sender, EventArgs e)
        {
            Guide.BeginShowKeyboardInput(Microsoft.Xna.Framework.PlayerIndex.One, "Compose", "Write a message on your Yammer network", "", new AsyncCallback(ComposeCompleted), null);
        }

        void ComposeCompleted(IAsyncResult textResult)
        {
            HandleError(() =>
            {
                RefreshFeed();

                string text = Guide.EndShowKeyboardInput(textResult);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    Yammer.Api.BeginComposeMessage(accessToken, text, res =>
                    {
                        HandleError(() =>
                        {
                            var addedMessage = res.messages.FirstOrDefault();
                            if (addedMessage != null)
                            {
                                var users = new Dictionary<string, Yammer.Reference>();
                                foreach (var item in res.references)
                                {
                                    if (item.type == "user")
                                    {
                                        users.Add(item.id, item);
                                    }
                                }

                                var messageView = new MessageViewModel(addedMessage);
                                if (users.ContainsKey(addedMessage.sender_id))
                                {
                                    messageView.LineOne = users[addedMessage.sender_id].full_name;
                                    if (!string.IsNullOrEmpty(users[addedMessage.sender_id].mugshot_url))
                                        messageView.Image = users[addedMessage.sender_id].mugshot_url;
                                }

                                Dispatcher.BeginInvoke(() =>
                                {
                                    HandleError(() =>
                                    {
                                        data.MyFeedItems.Insert(0, messageView);
                                        data.CompanyFeedItems.Insert(0, new MessageViewModel { LineOne = messageView.LineOne, LineTwo = messageView.LineTwo, Date = messageView.Date, Image = messageView.Image });
                                        data.SentFeedItems.Insert(0, new MessageViewModel { LineOne = messageView.LineOne, LineTwo = messageView.LineTwo, Date = messageView.Date, Image = messageView.Image });
                                    });
                                });
                            }
                        });
                    });
                }
            });
        } 

        private void Pivot_LoadingPivotItem(object sender, PivotItemEventArgs e)
        {
            this.data.CurrentFeed = (MainViewModel.CurrentFeedEnum)Enum.Parse(typeof(MainViewModel.CurrentFeedEnum), e.Item.Tag as string, true);
            if (accessToken != null)
                LoadStream();
        }

        void LoadStream(bool force = false)
        {
            HandleError(() =>
            {
                if (this.data.MustUpdate() || force)
                {
                    this.data.Update();
                    switch (this.data.CurrentFeed)
                    {
                        case MainViewModel.CurrentFeedEnum.MyFeed:
                            LoadFeed(Yammer.Api.BeginGetMyFeed, data.MyFeedItems);
                            break;
                        case MainViewModel.CurrentFeedEnum.CompanyFeed:
                            LoadFeed(Yammer.Api.BeginGetCompanyFeed, data.CompanyFeedItems);
                            break;
                        case MainViewModel.CurrentFeedEnum.PrivateFeed:
                            LoadFeed(Yammer.Api.BeginGetPrivateFeed, data.PrivateFeedItems);
                            break;
                        case MainViewModel.CurrentFeedEnum.SentFeed:
                            LoadFeed(Yammer.Api.BeginGetSentFeed, data.SentFeedItems);
                            break;
                        case MainViewModel.CurrentFeedEnum.ReceivedFeed:
                            LoadFeed(Yammer.Api.BeginGetReceivedFeed, data.ReceivedFeedItems);
                            break;
                        default:
                            break;
                    }
                }
            });
        }
        void LoadFeed(Action<Yammer.OAuthToken, Action<Yammer.ApiResponse>> apiCall, ObservableCollection<MessageViewModel> list)
        {
            Loading.Visibility = System.Windows.Visibility.Visible;
            apiCall(accessToken, m =>
            {
                HandleError(() =>
                {
                    if (m == null)
                        return;

                    var users = new Dictionary<string, Yammer.Reference>();
                    foreach (var item in m.references)
                    {
                        if (item.type == "user")
                        {
                            users.Add(item.id, item);
                        }
                    }

                    Dispatcher.BeginInvoke(() =>
                    {
                        list.Clear();
                        foreach (var item in m.messages.OrderByDescending(msg => msg.CreatedAt))
                        {
                            if (string.IsNullOrWhiteSpace(item.replied_to_id))
                            {
                                var messageView = new MessageViewModel(item);
                                if (users.ContainsKey(item.sender_id))
                                {
                                    messageView.LineOne = users[item.sender_id].full_name;
                                    if (!string.IsNullOrEmpty(users[item.sender_id].mugshot_url))
                                        messageView.Image = users[item.sender_id].mugshot_url;
                                }
                                list.Add(messageView);
                            }
                        }
                        Loading.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    );
                });
            });
        }

        private void SignOutButton_Click(object sender, EventArgs e)
        {
            HandleError(() =>
            {
                if (this.data.IsSignedIn)
                {
                    (this.ApplicationBar.MenuItems[0] as ApplicationBarMenuItem).IsEnabled = false;
                    (this.ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = false;
                    Storage.AccessTokenStorage.ClearAccessToken();
                    accessToken = null;
                    data.Reset();
                    ToogleSignInState(false);
                }
            });
        }

        private void HandleError(Action action)
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                Dispatcher.BeginInvoke(() => { this.ErrorGlobalMessage.Visibility = System.Windows.Visibility.Visible; });
                return;
            }

            Dispatcher.BeginInvoke(() => { this.ErrorGlobalMessage.Visibility = System.Windows.Visibility.Collapsed; });
            try
            {
                action();
            }
            catch
            {
                Dispatcher.BeginInvoke(() => { this.ErrorGlobalMessage.Visibility = System.Windows.Visibility.Visible; });
            }
        }

    }
}