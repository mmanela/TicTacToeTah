using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TicTacToeTah
{
    public delegate void NextMoveNotify(Move nextMove);

    public delegate void ThreadDelegate();

    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow
    {
        public int dimension = 4;

        private readonly GameLogic gameLogic = new GameLogic();
        private GameBoard gameGrid;
        private PlayerNumber currentPlayer = PlayerNumber.First;
        private PlayerNumber currentTurn = PlayerNumber.First;
        private readonly Dictionary<PlayerNumber, String> playerSymbol = new Dictionary<PlayerNumber, string>();
        private bool isOnlineMode;
        private readonly Communication onlineCommunication = new Communication();
        private DifficultyLevels computerLevel = DifficultyLevels.Medium;

        private GameType gameType = GameType.HumanVsHuman; //default to human vs human

        public GameWindow()
        {
            InitializeComponent();
            playerSymbol[PlayerNumber.First] = "X";
            playerSymbol[PlayerNumber.Second] = "O";
            onlineCommunication.NotifyCommunication += CommunicationMessage;
        }

        private void CommunicationMessage(CommunicationState state, string message)
        {
            if (state == CommunicationState.Connected)
            {
                isOnlineMode = true;
                Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadDelegate(MakeGameBoard));
                Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadDelegate(ConnectedMessage));
            }
            else if (state == CommunicationState.MessageRecieved)
            {
                if (message[0] == 't') //turn message
                {
                    int x = Convert.ToInt32(message[1].ToString());
                    int y = Convert.ToInt32(message[2].ToString());
                    int z = Convert.ToInt32(message[3].ToString());
                    int p = Convert.ToInt32(message[4].ToString());

                    Move recievedMove = new Move(x, y, z);
                    if (p == ((int) PlayerNumber.First))
                    {
                        recievedMove.Player = PlayerNumber.First;
                    }
                    else if (p == ((int) PlayerNumber.Second))
                    {
                        recievedMove.Player = PlayerNumber.Second;
                    }
                    else
                    {
                        return; //error
                    }
                    Dispatcher.Invoke(DispatcherPriority.Normal, new NextMoveNotify(ThreadSafeUpdate), recievedMove);
                }
                else if (message[0] == 'g') //new game message
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadDelegate(MakeGameBoard));
                }
            }
        }

        private void ConnectedMessage()
        {
            onlineStatus.Text = "Connected .. Go Time!";
        }

        private void ThreadSafeUpdate(Move recievedMove)
        {
            ProcessMove(recievedMove);
        }


        private void OnConnect(object sender, RoutedEventArgs e)
        {
            if (onlineCommunication.IsConnected)
            {
                onlineCommunication.Disconnect();
            }

            onlineCommunication.ConnectToHost(ipAddress.Text);
            currentPlayer = PlayerNumber.Second;
            onlineStatus.Text = "Attemping to connect...";
        }

        private void OnHost(object sender, RoutedEventArgs e)
        {
            if (onlineCommunication.IsConnected)
            {
                onlineCommunication.Disconnect();
            }
            onlineCommunication.ListenForConnection();
            currentPlayer = PlayerNumber.First;
            onlineStatus.Text = "Hosting...Waiting for a connection";
        }


        private void OnOfflineMode(object sender, RoutedEventArgs e)
        {
            if (onlineControl == null) return;
            if (onlineControl.Visibility == Visibility.Visible)
            {
                onlineControl.Visibility = Visibility.Collapsed;
            }
            offlineControl.Visibility = Visibility.Visible;
        }

        private void OnOnlineMode(object sender, RoutedEventArgs e)
        {
            if (onlineControl.Visibility == Visibility.Collapsed)
            {
                onlineControl.Visibility = Visibility.Visible;
            }
            myAddress.Text = onlineCommunication.MyIP;
            offlineControl.Visibility = Visibility.Collapsed;
        }

        private void OnHumanVsHuman(object sender, RoutedEventArgs e)
        {
            gameType = GameType.HumanVsHuman;
        }

        private void OnHumanVsComputer(object sender, RoutedEventArgs e)
        {
            gameType = GameType.HumanVsComputer;
        }

        private void OnComputerVsHuman(object sender, RoutedEventArgs e)
        {
            gameType = GameType.ComputerVsHuman;
        }

        private void OnComputerVsComputer(object sender, RoutedEventArgs e)
        {
            gameType = GameType.ComputerVsComputer;
        }

        private void OnDifficulty(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (sender != null)
            {
                computerLevel = (DifficultyLevels) Enum.Parse(typeof (DifficultyLevels), radio.Name);
            }
        }


        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            currentPlayer = PlayerNumber.First;
            MakeGameBoard();
        }


        /// <summary>
        /// Called when a new offline game is being created
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewGame(object sender, RoutedEventArgs e)
        {
            onlineStatus.Text = "";
            isOnlineMode = false;
            currentPlayer = PlayerNumber.First;
            MakeGameBoard();
        }


        /// <summary>
        /// Called when the current online session wants to play again
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlineAgain(object sender, RoutedEventArgs e)
        {
            onlineCommunication.WriteMessage("g");
            MakeGameBoard();
        }


        private void OnNextMove(Move recievedMove)
        {
            recievedMove.Player = currentPlayer;
            if (currentPlayer != currentTurn) return;

            if (isOnlineMode && !onlineCommunication.IsConnected)
            {
                return;
            }

            if (ProcessMove(recievedMove))
            {
                if (isOnlineMode)
                {
                    string message = "t" + recievedMove;
                    onlineCommunication.WriteMessage(message);
                }
            }
        }

        private bool ProcessMove(Move recievedMove)
        {
            MoveResult result = gameLogic.PlaceMove(recievedMove);
            if (result.status == MoveStatus.GameOver || result.status == MoveStatus.InvalidMove)
            {
                return false;
            }

            //this move is valid so place the peice ont he board
            gameGrid.PlaceMoveInGrid(recievedMove, playerSymbol[recievedMove.Player]);

            if (result.status == MoveStatus.WinningMove)
            {
                gameInfo.Content = playerSymbol[recievedMove.Player] + " is the winner!";
                gameGrid.MarkWinningLine(result.moveList);
            }
            else if (result.status == MoveStatus.TurnOver)
            {
                ToggleCurrentPlayer();
                UpdateTurnInfo();
            }
            else if (result.status == MoveStatus.TieGame)
            {
                gameInfo.Content = "Game Over: Tie Game";
                return true;
            }

            return false;
        }

        private void ToggleCurrentPlayer()
        {
            if (!isOnlineMode)
            {
                if (currentPlayer == PlayerNumber.First)
                {
                    currentPlayer = PlayerNumber.Second;
                }
                else
                {
                    currentPlayer = PlayerNumber.First;
                }
            }

            if (currentTurn == PlayerNumber.First)
            {
                currentTurn = PlayerNumber.Second;
            }
            else
            {
                currentTurn = PlayerNumber.First;
            }
        }

        private void UpdateTurnInfo()
        {
            if (!isOnlineMode)
            {
                gameInfo.Content = "It is " + playerSymbol[currentTurn] + "'s turn.";
                if (gameType == GameType.ComputerVsComputer
                    || (gameType == GameType.HumanVsComputer && currentTurn == PlayerNumber.Second)
                    || (gameType == GameType.ComputerVsHuman && currentTurn == PlayerNumber.First)
                    )
                {
                    //get computer move
                    ProcessMove(gameLogic.AI.NextAIMove(currentTurn));
                }
            }
            else
            {
                if (currentPlayer == currentTurn)
                {
                    gameInfo.Content = "It is your turn " + playerSymbol[currentPlayer];
                }
                else
                {
                    gameInfo.Content = "Waiting for " + playerSymbol[currentTurn] + " to move.";
                }
            }
        }


        private void MakeGameBoard()
        {
            gameLogic.Dimension = dimension;
            gameLogic.ComputerDifficulty = computerLevel;

            if (!gameLogic.SetUpBoardLogic())
            {
                return;
            }

            currentTurn = PlayerNumber.First;
            if (gameGrid != null)
            {
                GridHostingPanel.Children.Remove(gameGrid);
            }

            gameGrid = new GameBoard();
            gameGrid.nextMoveNotify += OnNextMove;
            gameGrid.Loaded += gameGrid_Loaded;
            gameGrid.Dimension = dimension;
            gameGrid.BoardWidth = 600;

            gameGrid.SetupGameBoard();

            GridHostingPanel.Children.Add(gameGrid);
        }

        private void gameGrid_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTurnInfo();
        }
    }
}