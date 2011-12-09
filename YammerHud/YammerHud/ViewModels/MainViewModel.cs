using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;


namespace YammerHub
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const int RefreshWindow = 60;



        public enum CurrentFeedEnum
        {
            MyFeed,
            CompanyFeed,
            PrivateFeed,
            SentFeed,
            ReceivedFeed
        }

        public MainViewModel()
        {
            this.LastUpdate = new Dictionary<CurrentFeedEnum, DateTimeOffset>();
            this.LastUpdate.Add(CurrentFeedEnum.CompanyFeed, DateTimeOffset.MinValue);
            this.LastUpdate.Add(CurrentFeedEnum.MyFeed, DateTimeOffset.MinValue);
            this.LastUpdate.Add(CurrentFeedEnum.PrivateFeed, DateTimeOffset.MinValue);
            this.LastUpdate.Add(CurrentFeedEnum.ReceivedFeed, DateTimeOffset.MinValue);
            this.LastUpdate.Add(CurrentFeedEnum.SentFeed, DateTimeOffset.MinValue);

            this.MyFeedItems = new ObservableCollection<MessageViewModel>();
            this.CompanyFeedItems = new ObservableCollection<MessageViewModel>();
            this.PrivateFeedItems = new ObservableCollection<MessageViewModel>();
            this.SentFeedItems = new ObservableCollection<MessageViewModel>();
            this.ReceivedFeedItems = new ObservableCollection<MessageViewModel>();
        }

        public void Reset()
        {
            this.MyFeedItems.Clear();
            this.CompanyFeedItems.Clear();
            this.PrivateFeedItems.Clear();
            this.SentFeedItems.Clear();
            this.ReceivedFeedItems.Clear();
        }

        public ObservableCollection<MessageViewModel> MyFeedItems { get; private set; }
        public ObservableCollection<MessageViewModel> CompanyFeedItems { get; private set; }
        public ObservableCollection<MessageViewModel> PrivateFeedItems { get; private set; }
        public ObservableCollection<MessageViewModel> SentFeedItems { get; private set; }
        public ObservableCollection<MessageViewModel> ReceivedFeedItems { get; private set; }

        public Dictionary<CurrentFeedEnum, DateTimeOffset> LastUpdate { get; private set; }
      
        private CurrentFeedEnum _currentFeed = CurrentFeedEnum.MyFeed;

        public CurrentFeedEnum CurrentFeed
        {
            get
            {
                return _currentFeed;
            }
            set
            {
                if (value != _currentFeed)
                {
                    _currentFeed = value;
                    NotifyPropertyChanged("CurrentFeed");
                }
            }
        }

        private bool _isSignedIn;
        public bool IsSignedIn
        {
            get
            {
                return _isSignedIn;
            }
            set
            {
                if (value != _isSignedIn)
                {
                    _isSignedIn = value;
                    NotifyPropertyChanged("IsSignedIn");
                }
            }
        }

        public void LoadData()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal bool MustUpdate()
        {
            return DateTimeOffset.Now.AddSeconds(-1 * RefreshWindow).CompareTo(this.LastUpdate[this.CurrentFeed]) > 0;
        }

        internal void Update()
        {
            this.LastUpdate[this.CurrentFeed] = DateTimeOffset.Now;
        }
    }
}