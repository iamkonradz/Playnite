﻿using Playnite;
using PlayniteUI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PlayniteUI.Windows
{
    public class User
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Mail { get; set; }
    }

    public class ListViewMock
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Mail { get; set; }

        public ObservableCollection<User> MyListBoxItems { get; set; }

        public ListViewMock()
        {
            MyListBoxItems = new ObservableCollection<User>();
            MyListBoxItems.Add(new User() { Name = "John Doe", Age = 42, Mail = "john@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Jane Doe", Age = 39, Mail = "jane@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Sammy Doe", Age = 7, Mail = "sammy.doe@gmail.com" });
            MyListBoxItems.Add(new User() { Name = "John Doe", Age = 42, Mail = "john@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Jane Doe", Age = 39, Mail = "jane@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Sammy Doe", Age = 7, Mail = "sammy.doe@gmail.com" });
            MyListBoxItems.Add(new User() { Name = "John Doe", Age = 42, Mail = "john@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Jane Doe", Age = 39, Mail = "jane@doe-family.com" });
            MyListBoxItems.Add(new User() { Name = "Sammy Doe", Age = 7, Mail = "sammy.doe@gmail.com" });
        }
    }

    /// <summary>
    /// Interaction logic for ThemeTesterWindow.xaml
    /// </summary>
    public partial class ThemeTesterWindow : WindowBase, INotifyPropertyChanged
    {
        public enum SourceType
        {
            Normal,
            Fullscreen
        }

        public IEnumerable<string> SkinList
        {
            get
            {
                if (SkinType == SourceType.Normal)
                {
                    return Skins.AvailableSkins.SelectMany(a => a.Profiles.Select(b => $"{a.Name}\\{b}"));
                }
                else
                {
                    return Skins.AvailableFullscreenSkins.SelectMany(a => a.Profiles.Select(b => $"{a.Name}\\{b}"));
                }
            }
        }

        private SourceType skinType = SourceType.Normal;
        public SourceType SkinType
        {
            get => skinType;
            set
            {
                if (watcher != null)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }

                skinType = value;
                OnPropertyChanged("SkinType");
                OnPropertyChanged("SkinList");
            }
        }

        private string validationError = string.Empty;
        public string ValidationError
        {
            get => validationError;
            set
            {
                validationError = value;
                OnPropertyChanged("ValidationError");
            }
        }

        private bool watchSkinFile = false;
        public bool WatchSkinFile
        {
            get => watchSkinFile;
            set
            {
                if (watcher != null)
                {
                    watcher.EnableRaisingEvents = false;
                }

                if (value == true)
                {
                    if (SkinType == SourceType.Normal)
                    {
                        watcher = new FileSystemWatcher(Paths.SkinsPath, "*.xaml");
                    }
                    else
                    {
                        watcher = new FileSystemWatcher(Paths.SkinsFullscreenPath, "*.xaml");
                    }

                    watcher.Changed += Watcher_Changed;
                    watcher.IncludeSubdirectories = true;
                    watcher.EnableRaisingEvents = true;
                }

                watchSkinFile = value;
                OnPropertyChanged("WatchSkinFile");
            }
        }

        public Tuple<string, string> SelectedSkinData
        {
            get
            {
                var name = SelectedSkin.Split('\\')[0];
                var color = SelectedSkin.Split('\\')[1];
                return new Tuple<string, string>(name, color);
            }
        }

        private string selectedSkin;
        public string SelectedSkin
        {
            get => selectedSkin;
            set
            {
                selectedSkin = value;
                OnPropertyChanged("SelectedSkin");
            }
        }

        private FileSystemWatcher watcher;
        public event PropertyChangedEventHandler PropertyChanged;

        public ThemeTesterWindow()
        {
            InitializeComponent();
            listview.ItemsSource = new ListViewMock().MyListBoxItems;
            DataContext = this;
        }        

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Paths.AreEqual(e.FullPath, Skins.GetSkinPath(SelectedSkinData.Item1, false)) ||
                Paths.AreEqual(e.FullPath, Skins.GetColorPath(SelectedSkinData.Item1, SelectedSkinData.Item2, false)))
            {
                Dispatcher.Invoke(() => ChangeSkin(SelectedSkin, false));
            }
        }

        public void ChangeSkin(string skin, bool validateToDialog)
        {
            var name = skin.Split('\\')[0];
            var color = skin.Split('\\')[1];

            var skinValid = Skins.IsSkinValid(name, SkinType == SourceType.Fullscreen);
            if (skinValid.Item1 == false)
            {
                if (validateToDialog)
                {
                    PlayniteMessageBox.Show(
                        $"Error in skin file \"{name}\"\n\n{skinValid.Item2}",
                        "Skin Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    ValidationError = skinValid.Item2;
                }
                return;
            }

            var colorValid = Skins.IsColorProfileValid(name, color, SkinType == SourceType.Fullscreen);
            if (colorValid.Item1 == false)
            {
                if (validateToDialog)
                {
                    PlayniteMessageBox.Show(
                        $"Error in skin color file \"{color}\"\n\n{colorValid.Item2}",
                        "Skin Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    ValidationError = colorValid.Item2;
                }
                return;
            }

            ValidationError = string.Empty;
            if (SkinType == SourceType.Normal)
            {
                Skins.ApplySkin(name, color, true);
            }
            else
            {
                Skins.ApplyFullscreenSkin(name, color, true);
            }
        }

        private void ButtonReloadSkinList_Click(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged("SkinList");
        }

        private void ButtonApplySkin_Click(object sender, RoutedEventArgs e)
        {
            ChangeSkin(SelectedSkin, false);
        }
    }
}
