using System;
using System.Windows.Media;
using Escape.Model;
using Escape.Persistence;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace Escape.ViewModel
{
    public class EscapeViewModel : ViewModelBase
    {
        private EscapeModel model;
        private SaveEntry selectedGame;
        private string newName = string.Empty;
        public bool isPaused { get; set; }
        public bool isGameOver { get; set; }
        
        public DispatcherTimer GameTime;
        public ObservableCollection<ViewField> Fields { get; private set; }

        public int MapSize
        {
            get { return model.Size; }
        }

        public string GameTimer { get; set; }

        public ObservableCollection<SaveEntry> Games { get; set; }

        public SaveEntry SelectedGame
        {
            get { return selectedGame; }
            set
            {
                selectedGame = value;
                if (selectedGame != null)
                    NewName = string.Copy(selectedGame.Name);

                OnPropertyChanged();
                LoadCloseCommand.RaiseCanExecuteChanged();
                SaveCloseCommand.RaiseCanExecuteChanged();
            }
        }

        public string NewName
        {
            get { return newName; }
            set
            {
                newName = value;

                OnPropertyChanged();
                SaveCloseCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand UpCommand { get; private set; }
        public DelegateCommand DownCommand { get; private set; }
        public DelegateCommand LeftCommand { get; private set; }
        public DelegateCommand RightCommand { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }
        public DelegateCommand SaveCloseCommand { get; private set; }
        public DelegateCommand SaveOpenCommand { get; private set; }
        public DelegateCommand LoadOpenCommand { get; private set; }
        public DelegateCommand LoadCloseCommand { get; private set; }
        public DelegateCommand PauseCommand { get; private set; }
        public DelegateCommand New11Command { get; private set; }
        public DelegateCommand New15Command { get; private set; }
        public DelegateCommand New21Command { get; private set; }


        public event EventHandler ExitGame;
        public event EventHandler<string> LoadGameClose;
        public event EventHandler LoadGameOpen;
        public event EventHandler<string> SaveGameClose;
        public event EventHandler SaveGameOpen;

        public EscapeViewModel(EscapeModel model)
        {
            this.model = model;

            model.MapChanged += new EventHandler<MapChangedEventArgs>(Model_MapChanged);
            model.TileChanged += new EventHandler<TileChangedEventArgs>(Model_TileChanged);

            UpCommand = new DelegateCommand(param => model.MovePlayer(Direction.Up));
            DownCommand = new DelegateCommand(param => model.MovePlayer(Direction.Down));
            LeftCommand = new DelegateCommand(param => model.MovePlayer(Direction.Left));
            RightCommand = new DelegateCommand(param => model.MovePlayer(Direction.Right));
            ExitCommand = new DelegateCommand(param => OnExit());
            SaveOpenCommand = new DelegateCommand(
                async param =>
                {
                    Games = new ObservableCollection<SaveEntry>(await model.ListGamesAsync());
                    OnSaveOpen();
                });
            SaveCloseCommand = new DelegateCommand(
                param => NewName.Length > 0, 
                param =>
                {
                    OnSaveClose(NewName);
                });
            LoadOpenCommand = new DelegateCommand(
                async param =>
                {
                    Games = new ObservableCollection<SaveEntry>(await model.ListGamesAsync());
                    OnLoadOpen();
                });
            LoadCloseCommand = new DelegateCommand(
                param => SelectedGame != null, 
                param =>
                {
                    OnLoadClose(SelectedGame.Name); 

                });
            PauseCommand = new DelegateCommand(param => OnPause());
            New11Command = new DelegateCommand(param => model.NewGame(11));
            New15Command = new DelegateCommand(param => model.NewGame(15));
            New21Command = new DelegateCommand(param => model.NewGame(21));

            Fields = new ObservableCollection<ViewField>();

            GameTime = new DispatcherTimer();
            GameTime.Tick += GameAdvanced;

        }

        private void Model_MapChanged(object sender, MapChangedEventArgs e)
        {
            Fields.Clear();
            for (int i = 0; i < e.Size; i++)
            {
                for (int j = 0; j < e.Size; j++)
                {
                    Fields.Add(new ViewField());
                }
            }

            GameTimer = string.Format("{0:00}:{1:00}", model.mins, model.secs); ;
            OnPropertyChanged("GameTimer");

            isPaused = false;
            OnPropertyChanged("isPaused");

            isGameOver = true;
            OnPropertyChanged("isGameOver");
            
            GameTime.Interval = new TimeSpan(0, 0, 1);
            GameTime.Start();
        }

        private void Model_TileChanged(object sender, TileChangedEventArgs e)
        {
            switch (e.Type)
            {
                case TileType.Empty:
                    Fields[e.X * model.Size + e.Y].Color = Color.FromRgb(0, 0, 0);
                    break;
                case TileType.Player:
                    Fields[e.X * model.Size + e.Y].Color = Color.FromRgb(0, 255, 0);
                    break;
                case TileType.Enemy:
                    Fields[e.X * model.Size + e.Y].Color = Color.FromRgb(255, 0, 0);
                    break;
                case TileType.Mine:
                    Fields[e.X * model.Size + e.Y].Color = Color.FromRgb(255, 255, 0);
                    break;
            }
        }

        private void GameAdvanced(object sender, EventArgs e)
        {
            GameTimer = string.Format("{0:00}:{1:00}", model.mins, model.secs); ;
            OnPropertyChanged("GameTimer");
        }

        private void OnPause()
        {
            if (!model.Paused)
            {
                model.Pause(true);
                isPaused = true;
                OnPropertyChanged("isPaused");
                GameTime.Stop();
            }
            else
            {
                model.Pause(false);
                isPaused = false;
                OnPropertyChanged("isPaused");
                GameTime.Start();
            }
        }
        private void OnLoadOpen()
        {
            LoadGameOpen?.Invoke(this, EventArgs.Empty);
        }
        private void OnLoadClose(String name)
        {
            LoadGameClose?.Invoke(this, name);
        }
        private void OnSaveOpen()
        {
            SaveGameOpen?.Invoke(this, EventArgs.Empty);
        }
        private void OnSaveClose(String name)
        {
            SaveGameClose?.Invoke(this, name);
        }
        private void OnExit()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }
    }
}
