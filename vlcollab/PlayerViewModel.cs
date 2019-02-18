using MVVMToolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;
using vlcollab.HelperClasses;
using vlcollab.PlayerEventArgs;

namespace vlcollab
{
    public class PlayerViewModel : ObservableObject
    {

        #region properties

        private VlcControl player;
        //public EventHandler<bool> PlaybackChanged { get; }
        public delegate void PlaybackChangedEventHandler(object sender, PlaybackChangedEventArgs e);
        public event PlaybackChangedEventHandler PlaybackChanged;
        public delegate void CurrentTimeChangedEventHandler(object sender, CurrentTimeChangedEventArgs e);
        public event CurrentTimeChangedEventHandler CurrentTimeChanged;
        private bool isPaused;
        public bool IsPaused
        {
            get { return isPaused; }
            set
            {
                isPaused = value;
                player.SourceProvider.MediaPlayer.SetPause(isPaused);
                PlaybackChanged?.Invoke(this, new PlaybackChangedEventArgs { IsPaused = isPaused });
            }
        }

        private long currentTime;
        public long CurrentTime
        {
            get { return currentTime; }
            set
            {
                currentTime = value;
                RaisePropertyChangedEvent(nameof(currentTime));
            }
        }
        private long timeRemaining;
        public long TimeRemaining
        {
            get { return timeRemaining; }
            set
            {
                timeRemaining = value;
                RaisePropertyChangedEvent(nameof(timeRemaining));
            }
        }
        private long videoLength;
        public long VideoLength
        {
            get { return videoLength; }
            set
            {
                RaisePropertyChangedEvent(nameof(videoLength));
                videoLength = value;
            }
        }
        private int volume = 50;

        public int Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                if (volume < 0) volume = 0;
                if (volume > 125) volume = 125;
                player.SourceProvider.MediaPlayer.Audio.Volume = volume;
                RaisePropertyChangedEvent(nameof(volume));
            }
        }
        private List<ExtendedTrackInfo> tracks = new List<ExtendedTrackInfo>();

        public List<ExtendedTrackInfo> Tracks
        {
            get { return tracks; }
            set
            {
                tracks = value;
                RaisePropertyChangedEvent(nameof(Tracks));
            }
        }

        #endregion
        public PlayerViewModel(VlcControl player)
        {
            this.player = player;
            player.SourceProvider.MediaPlayer.TimeChanged += TimeChanged;
            player.SourceProvider.MediaPlayer.LengthChanged += LengthChanged;
            player.SourceProvider.MediaPlayer.PausableChanged += PausableChanged;
        }

        public PlayerViewModel() { }

        #region methods
        public ICommand ChangePlayback => new RelayCommand<String>(DoChangePlayback);
        private void DoChangePlayback(String obj)
        {
            IsPaused = !IsPaused;
        }
        public ICommand ChangeAudioTrack => new RelayCommand<ExtendedTrackInfo>(DoChangeAudioTrack);
        private void DoChangeAudioTrack(ExtendedTrackInfo obj)
        {
            player.SourceProvider.MediaPlayer.Audio.Tracks.Current = player.SourceProvider.MediaPlayer.Audio.Tracks.All.First(x => x.ID == obj.Id);
        }
        public ICommand ChangeSubtitle => new RelayCommand<ExtendedTrackInfo>(DoChangeSubtitle);
        private void DoChangeSubtitle(ExtendedTrackInfo obj)
        {
            player.SourceProvider.MediaPlayer.SubTitles.Current = player.SourceProvider.MediaPlayer.SubTitles.All.First(x => x.ID == obj.Id);
        }
        public ICommand OpenFile => new RelayCommand<string>(DoOpenFile);

        private void DoOpenFile(string obj)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open Video File";
            dialog.Multiselect = false;
            if(dialog.ShowDialog()== DialogResult.OK)
            {
                PlayVideo(new FileInfo(dialog.FileName));
            }
            
        }

        public void ChangeCurrentTime(long timestamp)
        {
            player.SourceProvider.MediaPlayer.Time = timestamp;
            CurrentTimeChanged?.Invoke(this, new CurrentTimeChangedEventArgs { CurrentTime = timestamp });
        }
        public void PlayVideo(string murl)
        {
            this.player.SourceProvider.MediaPlayer.Play(murl);
        }
        public void PlayVideo(FileInfo file)
        {
            player.SourceProvider.MediaPlayer.Play(file);
        }
        #endregion
        #region vlcEvents
        private void TimeChanged(object sender, VlcMediaPlayerTimeChangedEventArgs e)
        {
            CurrentTime = e.NewTime;
        }
        private void LengthChanged(object sender, VlcMediaPlayerLengthChangedEventArgs e)
        {
            VideoLength = e.NewLength;
        }
        private void PausableChanged(object sender, VlcMediaPlayerPausableChangedEventArgs e)
        {
            var temp = player.SourceProvider.MediaPlayer.SubTitles.All.Select(x => new ExtendedTrackInfo
            {
                Id = x.ID,
                Name = x.Name,
                IsSubtitle = true
            })
            .ToList();
            temp.AddRange(player.SourceProvider.MediaPlayer.Audio.Tracks.All.Select(x => new ExtendedTrackInfo
            {
                Id = x.ID,
                Name = x.Name,
                IsSubtitle = false
            }));
            Tracks = temp;
        }
        #endregion
    }
}
