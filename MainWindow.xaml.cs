using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Kviz
{
    public partial class MainWindow : Window
    {
        private Grid mainGrid;
        public List<Quiz> QuizList = new();
        public HashSet<string> ReadFiles = new();
        public int index = 0;
        public Random random = new();
        public List<int> questionIndexes = new();
            

        public MainWindow()
        {
            InitializeComponent();
            InitializeUI();
            MainMenu();
        }

        public void InitializeUI()
        {
            // Alap beállítások
            this.Title = "Kvízműsor";
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;

            // Fő Grid
            mainGrid = new Grid();
            this.Content = mainGrid;

            // Háttér Border (BlurEffect csak erre hat)
            Border backgroundBorder = new Border();
            backgroundBorder.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Hatter.jpg", UriKind.Absolute)));
            backgroundBorder.Effect = new BlurEffect() { Radius = 15 }; // Csak a háttér homályos

            // Rétegek egymásra helyezése
            mainGrid.Children.Add(backgroundBorder); // Háttér
            mainGrid.Children.Add(MainMenu());      // UI elemek
        }

        // Gomb létrehozása
        private Button CreateButton(string text, int fontSize)
        {
            return new Button()
            {
                Content = text,
                FontSize = fontSize,
                Padding = new Thickness(3),
                Margin = new Thickness(15),
                Background = Brushes.Gray,
                Foreground = Brushes.Black,
                Width = 400,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }


        // Főmenü
        public Grid MainMenu()
        {
            // UI elemek tárolására egy külön Grid
            Grid contentGrid = new Grid();
            contentGrid.HorizontalAlignment = HorizontalAlignment.Center;
            contentGrid.VerticalAlignment = VerticalAlignment.Center;

            

            // Sorok és oszlopok beállítása a gombokhoz
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Cím
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Játék
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Beállítások
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Kilépés
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Készítők


            Label label = new Label()
            {
                Content = "Kvízműsor",
                FontSize = 60,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10),
            };
            Grid.SetRow(label, 0);

            // Gombok létrehozása
            Button btnStart = CreateButton("Kezdés", 24);
            btnStart.Click += BtnStart_Click;
            Grid.SetRow(btnStart, 1);

            Button btnSettings = CreateButton("Beállítások", 24);
            btnSettings.Click += BtnSettings_Click;
            Grid.SetRow(btnSettings, 2);

            Button btnQuit = CreateButton("Kilépés", 24);
            btnQuit.Click += BtnQuit_Click;
            Grid.SetRow(btnQuit, 3);

            Label Authors = new Label()
            {
                Content = "Készítette: Bordás Dániel és Dudás Ádám",
                FontSize = 40,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            Grid.SetRow(Authors, 4);

            // Gombok hozzáadása a Gridhez
            contentGrid.Children.Add(label);
            contentGrid.Children.Add(btnStart);
            contentGrid.Children.Add(btnSettings);
            contentGrid.Children.Add(btnQuit);
            contentGrid.Children.Add(Authors);

            return contentGrid;
        }


        // A beállítás menü
        private Grid SettingsMenu()
        {
            Grid contentGrid = new Grid();
            contentGrid.HorizontalAlignment = HorizontalAlignment.Center;
            contentGrid.VerticalAlignment = VerticalAlignment.Center;

            // Sorok és oszlopok beállítása a gombokhoz
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Beolvas gomb
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Lista törlése gomb
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Jelenlegi kérdések
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Képernyő mód váltás
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Vissza gomb

            // Gombok létrehozása és pozicionálása
            Button buttonRead = CreateButton("Beolvas", 24);
            buttonRead.Click += ButtonRead_Click;
            Grid.SetRow(buttonRead, 0);

            Button deleteListContainer = CreateButton("Lista törlése", 24);
            deleteListContainer.Click += DeleteListContainer_Click;
            Grid.SetRow(deleteListContainer, 1);

            Button currentQuestions = CreateButton("Jelenlegi kérdések", 24);
            currentQuestions.Click += CurrentQuestions_Click;
            Grid.SetRow(currentQuestions, 2);

            Button screenChange = CreateButton("Ablak mód", 24);
            screenChange.Click += ScreenChange_Click;
            Grid.SetRow(screenChange, 3);


            Button btnBack = CreateButton("Vissza", 24);
            btnBack.Click += BtnBack_Click;

            Grid.SetRow(btnBack, 4);

            contentGrid.Children.Add(buttonRead);
            contentGrid.Children.Add(deleteListContainer);
            contentGrid.Children.Add(currentQuestions);
            contentGrid.Children.Add(screenChange);
            contentGrid.Children.Add(btnBack);

            return contentGrid;
        }

        

        // Elhelyezés listába
        private void FileReading(string fileName)
        {
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        QuizList.Add(new Quiz(line));
                    }
                }
                ReadFiles.Add(Path.GetFileName(fileName));
                MessageBox.Show($"Sikeresen beolvastad: {Path.GetFileName(fileName)}");
            }
            catch (Exception)
            {
                MessageBox.Show("Hiba a fájl beolvasásakor!");
            }
        }


        #region Quiz része
        public void Game()
        {
            Grid contentGrid = new Grid();
 
            void NewGame()
            {
                // Korábbi tartalom törlése
                contentGrid.Children.Clear();
                contentGrid.RowDefinitions.Clear();
                contentGrid.ColumnDefinitions.Clear();

                // Rácsszerkezet beállítása
                contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2, GridUnitType.Star) }); // Kérdés
                contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }); // Első 2 válasz
                contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }); // 3. válasz

                contentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Bal válasz
                contentGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // Jobb válasz

                // Kérdés megjelenítése
                TextBlock tbQuestion = new TextBlock()
                {
                    Text = QuizList[index].Question,
                    FontSize = 50,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(20),
                };
                Grid.SetRow(tbQuestion, 0);
                Grid.SetColumnSpan(tbQuestion, 2); // Középre igazítás
                contentGrid.Children.Add(tbQuestion);

                // Válaszok listába gyűjtése és keverése
                List<(string text, bool isCorrect)> answers = new()
                {
                    (QuizList[index].Answer1, QuizList[index].Answer1Ans),
                    (QuizList[index].Answer2, QuizList[index].Answer2Ans),
                    (QuizList[index].Answer3, QuizList[index].Answer3Ans)
                };
                answers = answers.OrderBy(_ => random.Next()).ToList(); // Véletlenszerű sorrend

                // Első két válasz gomb
                Button btnAnswer1 = CreateButton(answers[0].text, 30);
                Button btnAnswer2 = CreateButton(answers[1].text, 30);

                btnAnswer1.Click += (sender, e) => Checker(answers[0].isCorrect);
                btnAnswer2.Click += (sender, e) => Checker(answers[1].isCorrect);

                btnAnswer1.HorizontalAlignment = HorizontalAlignment.Stretch;
                btnAnswer2.HorizontalAlignment = HorizontalAlignment.Stretch;

                Grid.SetRow(btnAnswer1, 1);
                Grid.SetColumn(btnAnswer1, 0);
                Grid.SetRow(btnAnswer2, 1);
                Grid.SetColumn(btnAnswer2, 1);

                contentGrid.Children.Add(btnAnswer1);
                contentGrid.Children.Add(btnAnswer2);

                // Harmadik válasz gomb
                Button btnAnswer3 = CreateButton(answers[2].text, 30);
                btnAnswer3.Click += (sender, e) => Checker(answers[2].isCorrect);

                btnAnswer3.HorizontalAlignment = HorizontalAlignment.Stretch;
                Grid.SetRow(btnAnswer3, 2);
                Grid.SetColumnSpan(btnAnswer3, 2);
                contentGrid.Children.Add(btnAnswer3);

                // Vissza gomb 
                Button btnBack = new Button()
                {
                    Content = "X",
                    FontSize = 30,
                    Width = 50,
                    Height = 50,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Background = Brushes.Red,
                    Foreground = Brushes.White
                };

                btnBack.Click += BtnBack_Click;
                Grid.SetRow(btnBack, 0);
                Grid.SetColumn(btnBack, 1);
                contentGrid.Children.Add(btnBack);

                // UI frissítése
                if (mainGrid.Children.Count > 1)
                    mainGrid.Children.RemoveAt(1);

                mainGrid.Children.Add(contentGrid);
            }


            void Checker(bool correctAnswer)
            {
                if (correctAnswer)
                {
                    MessageBox.Show("Helyes válasz!");
                    if (questionIndexes.Count > 0)
                    {
                        Roll();  // Új kérdés kiválasztása
                        NewGame();
                    }
                    else
                    {
                        MessageBox.Show("Gratulálok, végigjátszottad a kvízt!");

                        // Csak a második elem törlése (a főmenü visszaáll)
                        if (mainGrid.Children.Count > 1)
                            mainGrid.Children.RemoveAt(1);

                        mainGrid.Children.Add(MainMenu());
                    }
                }
                else
                {
                    MessageBox.Show("Helytelen válasz!");
                }
            }


            // Kezdeti kérdéslista létrehozása
            questionIndexes = Enumerable.Range(0, QuizList.Count).ToList();

            Roll(); // Első kérdés kiválasztása
            NewGame();
        }

        // Random kérdés húzása
        private void Roll()
        {
            if (questionIndexes.Count > 0)
            {
                int randomIndex = random.Next(questionIndexes.Count);
                index = questionIndexes[randomIndex];
                questionIndexes.RemoveAt(randomIndex); // Kiválasztott kérdés eltávolítása
            }
        }
        #endregion


        
        #region Eseménykezelők

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (QuizList.Count > 0)
            {
                mainGrid.Children.RemoveAt(1);
                Game();
            }
            else
            {
                MessageBox.Show("Nincs beolvasott fájl!");
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Children.RemoveAt(1);
            mainGrid.Children.Add(SettingsMenu());
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }



        
        #region Beállítások
        private void ButtonRead_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Szöveges dokumentum|*.txt|Táblázat (*.csv)|*.csv";
            openFileDialog.Title = "Fájl megnyitása";
            
            try
            {
                bool? eredmeny = openFileDialog.ShowDialog();
                if (eredmeny == true)
                {
                    if (!ReadFiles.Contains(Path.GetFileName(openFileDialog.FileName)))
                    {
                        FileReading(openFileDialog.FileName);
                    }
                    else
                    {
                        MessageBox.Show("Ezt a fájlt már beolvastad!");
                    }
                }
            } catch (Exception)
            {
                MessageBox.Show("Hiba a fájl megnyitásakor!");
            }
        }

        // Datagridben kérdések megjelenítése
        private void CurrentQuestions_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Children.RemoveAt(1);
            Grid contentGrid = new();
            contentGrid.HorizontalAlignment = HorizontalAlignment.Center;
            contentGrid.VerticalAlignment = VerticalAlignment.Center;

            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            DataGrid questionsDataGrid = new DataGrid();
            questionsDataGrid.ItemsSource = new ObservableCollection<Quiz>(QuizList);
            questionsDataGrid.Items.Refresh();
            Grid.SetRow(questionsDataGrid, 0);


            Button btnBack = new Button()
            {
                Content = "Vissza",
                FontSize = 24,
                Padding = new Thickness(3),
                Margin = new Thickness(15),
                Background = Brushes.Gray,
                Foreground = Brushes.Black,
                Width = 400,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            btnBack.Click += BtnBack_Click;
            Grid.SetRow(btnBack, 1);

            contentGrid.Children.Add(btnBack);
            contentGrid.Children.Add(questionsDataGrid);

            mainGrid.Children.Add(contentGrid);

        }

        // Kérdések listájának törlése.
        private void DeleteListContainer_Click(object sender, RoutedEventArgs e)
        {
            if (QuizList.Count > 0)
            {
                QuizList.Clear();
                ReadFiles.Clear();
                Quiz.lastQuestionsIndex = 0;
                MessageBox.Show("Sikeresen törölted a listát!");
            }
            else
            {
                MessageBox.Show("Nincs beolvasott fájl!");
            }
        }


        private void ScreenChange_Click(object sender, RoutedEventArgs e)
        {
            Button screenChangeButton = (Button)sender;

            if (this.WindowState == WindowState.Maximized && this.WindowStyle == WindowStyle.SingleBorderWindow)
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
                screenChangeButton.Content = "Ablak mód";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                screenChangeButton.Content = "Teljes képernyő";
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Children.RemoveAt(1);
            mainGrid.Children.Add(MainMenu());
        }
        #endregion

        #endregion
    }
}
