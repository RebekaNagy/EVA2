using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Escape.Persistence;
using Escape.Model;
using Escape.ViewModel;
using Microsoft.Win32;

namespace Escape.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private IPersistence dataAccess;
        private EscapeModel model;
        private EscapeViewModel viewModel;
        private MainWindow window;
        private LoadWindow loadWindow;
        private SaveWindow saveWindow;
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }
        private void App_Startup(object sender, StartupEventArgs e)
        {
            dataAccess = new DbPersistence("name=EscapeModel");

            model = new EscapeModel(dataAccess);
            model.GameOver += new EventHandler<GameOverEventArgs>(Model_GameOver);

            viewModel = new EscapeViewModel(model);
            viewModel.ExitGame += new EventHandler(ViewModel_Exit);
            viewModel.LoadGameOpen += new EventHandler(ViewModel_LoadGameOpen);
            viewModel.LoadGameClose += new EventHandler<String>(ViewModel_LoadGameClose);
            viewModel.SaveGameOpen += new EventHandler(ViewModel_SaveGameOpen);
            viewModel.SaveGameClose += new EventHandler<String>(ViewModel_SaveGameClose);

            window = new MainWindow();
            window.DataContext = viewModel;
            window.Show();
        }

        private void ViewModel_LoadGameOpen(object sender, System.EventArgs e)
        {
            model.Pause(true);

            viewModel.SelectedGame = null;

            loadWindow = new LoadWindow();
            loadWindow.DataContext = viewModel;
            loadWindow.ShowDialog(); 

        }
        private async void ViewModel_LoadGameClose(object sender, String name)
        {
            if (name != null)
            {
                try
                {
                    await model.Load(name);
                }
                catch
                {
                    MessageBox.Show("Játék betöltése sikertelen!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            loadWindow.Close();
        }
        private void ViewModel_SaveGameOpen(object sender, EventArgs e)
        {
            model.Pause(true);
            
            viewModel.SelectedGame = null;
            viewModel.NewName = String.Empty;

            saveWindow = new SaveWindow(); 
            saveWindow.DataContext = viewModel;
            saveWindow.ShowDialog(); 
        }
        
        private async void ViewModel_SaveGameClose(object sender, String name)
        {
            if (name != null)
            {
                try
                {
                    var games = await model.ListGamesAsync();
                    if (games.All(g => g.Name != name) ||
                        MessageBox.Show("Biztos, hogy felülírja a meglévő mentést?", "Sudoku",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        await model.Save(name);
                    }
                }
                catch
                {
                    MessageBox.Show("Játék mentése sikertelen!", "Hiba!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            saveWindow.Close();
        }
        

        private void ViewModel_Exit(object sender, EventArgs e)
        {
            Shutdown();
        }
        private void Model_GameOver(object sender, GameOverEventArgs e)
        {
            viewModel.GameTime.Stop();
            viewModel.isGameOver = false;
            viewModel.OnPropertyChanged("isGameOver");

            if (e.GameWon)
            {
                MessageBox.Show(viewModel.GameTimer + "-ig tartott megnyerni a jatekot.", "Win", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(viewModel.GameTimer + "-ig birtad.", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }
    }
}
