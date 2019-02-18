using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using vlcollab.PlayerEventArgs;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace vlcollab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables
        private PlayerViewModel model;
        private bool isFullscreen = false;
        private Timer mouseIdleTimer = new Timer(2000);
        private bool cursorOverControls = false;


        #endregion

        public MainWindow()
        {
            InitializeComponent();

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mouseIdleTimer.AutoReset = false;
            mouseIdleTimer.Elapsed += HideControlsAndCursor;
            checkVlcPath(false, "Please specify path To Vlc Installation");
            var options = new string[]
            {
                // VLC options can be given here. Please refer to the VLC command line documentation.
            };

            while (true)
            {
                try
                {
                    videoPlayer.SourceProvider.CreatePlayer(new DirectoryInfo(Properties.Settings.Default.VlcPath), options);
                    break;
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    checkVlcPath(true, "Found no VLC installation please try again");
                }
            }
            model = new PlayerViewModel(videoPlayer);
            DataContext = model;
            model.PlaybackChanged += PlaybackChanged;
        }

        private void checkVlcPath(bool ignorecheck, string msg)
        {
            var vlcPath = Properties.Settings.Default.VlcPath;
            if (string.IsNullOrEmpty(vlcPath) || ignorecheck)
            {
                var result = MessageBox.Show(msg,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    var dialog = new CommonOpenFileDialog
                    {
                        Title = "Specify VLC Installation Path",
                        IsFolderPicker = true,
                    };
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        vlcPath = dialog.FileName;
                        Properties.Settings.Default.VlcPath = vlcPath;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    Close();
                }

            }
        }

        private void PlaybackChanged(object sender, PlaybackChangedEventArgs e)
        {
            var url = $@"\Assets\{(e.IsPaused ? "pause" : "play")}.png";
            var uri = new Uri(url, UriKind.Relative);
            playButton.Source = new BitmapImage(uri);
        }

        private void VideoProgessSlider_HandleSliderDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            model.ChangeCurrentTime((long)VideoProgessSlider.Value);
            model.IsPaused = false;
        }
        private void VideoProgessSlider_HandleSliderDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            model.IsPaused = true;
        }

        private void VideoProgessSlider_LeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            model.ChangeCurrentTime((long)VideoProgessSlider.Value);
        }

        private void Window_MouseScrollPreview(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) model.Volume += 5;
            else model.Volume -= 5;
        }

        private void VideoControls_MouseEnter(object sender, MouseEventArgs e)
        {
            cursorOverControls = true;
        }

        private void VideoControls_MouseLeave(object sender, MouseEventArgs e)
        {
            cursorOverControls = false;
        }


        private void FullScreenChangeHandler(object sender, EventArgs e)
        {
            ChangeFullscreen();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            mouseIdleTimer.Stop();
            Mouse.OverrideCursor = null;
            if (videoControls.Opacity == 0) ChangeVisibilityOfControl(false, 500, videoControls);
            mouseIdleTimer.Start();
        }
        private void Window_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            mouseIdleTimer.Stop();
        }

        private void Window_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            mouseIdleTimer.Start();

        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    model.IsPaused = !model.IsPaused;
                    break;
                case Key.Escape:
                    if (isFullscreen) ChangeFullscreen();
                    break;
            }
        }
        #region helperMethods
        private void ChangeFullscreen()
        {
            isFullscreen = !isFullscreen;
            if (isFullscreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                Menu.Visibility = Visibility.Hidden;
            }
            else
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                Menu.Visibility = Visibility.Visible;

            }
        }
        private void ChangeVisibilityOfControl(bool hide, int durationInMillis, UIElement target)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = hide ? 0 : 1,
                Duration = TimeSpan.FromMilliseconds(durationInMillis),
                EasingFunction = new QuadraticEase()
            };

            Storyboard sb = new Storyboard();
            sb.Children.Add(animation);

            target.Opacity = hide ? 1 : 0;

            Storyboard.SetTarget(sb, target);
            Storyboard.SetTargetProperty(sb, new PropertyPath(OpacityProperty));

            sb.Begin();
        }
        private void HideControlsAndCursor(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (cursorOverControls) return;
                Mouse.OverrideCursor = Cursors.None;
                ChangeVisibilityOfControl(true, 500, videoControls);
            });
        }
        #endregion


    }
}
