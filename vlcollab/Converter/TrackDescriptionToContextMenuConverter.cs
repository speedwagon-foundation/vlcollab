using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Vlc.DotNet.Core;
using vlcollab.HelperClasses;

namespace vlcollab.Converter
{
    [ValueConversion(typeof(List<ExtendedTrackInfo>), typeof(ContextMenu))]
    class TrackDescriptionToContextMenuConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //todo split TrackDescription into audio and Subtitle info IAudioManagement ISubTitleManagement
            var tracks = values[0] as List<ExtendedTrackInfo>;
            var subTitles = tracks.Where(x => x.IsSubtitle);
            var audioTracks = tracks.Where(x => !x.IsSubtitle);
            var contextMenu = new ContextMenu();
            var menuItems = new List<MenuItem>();
            var audioItem = new MenuItem { Header="Audio Track"};
            var subItem = new MenuItem { Header="Subtitles"};
            var audioChangeCommand = values[1] as ICommand;
            var subtitleChangeCommand = values[2] as ICommand;
            menuItems.Add(audioItem);
            menuItems.Add(subItem);
            subItem.ItemsSource = subTitles.Select(x => new MenuItem {
                Header = x.Name,
                Command = subtitleChangeCommand,
                CommandParameter=x
            });
            audioItem.ItemsSource = audioTracks.Select(x => new MenuItem
            {
                Header = x.Name,
                Command = audioChangeCommand,
                CommandParameter=x
            });
            contextMenu.ItemsSource = menuItems;
            return contextMenu;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
