using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace TicTacToeTah
{
    /// <summary>
    /// Full game board
    /// </summary>
    public partial class GameBoard
    {
        //default dimension of the puzzle
        private int dimension;
        private int boardWidth = 600;

        private readonly List<LevelGrid> gameLevels = new List<LevelGrid>();
        private Move lastPlacedMove;
        public NextMoveNotify nextMoveNotify;

        /// <summary>
        /// Set the dimension of the board
        /// </summary>
        public int Dimension
        {
            set { dimension = value; }
        }

        /// <summary>
        /// Set the width of the board
        /// </summary>
        public int BoardWidth
        {
            set { boardWidth = value; }
        }


        public GameBoard()
        {
            InitializeComponent();
            AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnCellButtonClick));
        }


        private Button FindCellButton(Move cell)
        {
            foreach (Grid grid in Children)
            {
                if (Convert.ToInt32(grid.Tag.ToString()) == cell.Z)
                {
                    foreach (Button b in grid.Children)
                    {
                        int row = (int) b.GetValue(RowProperty);
                        int col = (int) b.GetValue(ColumnProperty);

                        if (row == cell.Y && col == cell.X)
                        {
                            return b;
                        }
                    }
                }
            }

            return null;
        }

        public bool PlaceMoveInGrid(Move move, string symbol)
        {
            Button b = FindCellButton(move);
            if (b != null)
            {
                b.Content = symbol;
                ChangeCellTextColor(b, Brushes.Blue);
                if (lastPlacedMove != null)
                {
                    ChangeCellTextColor(lastPlacedMove, Brushes.Black);
                }
                lastPlacedMove = move;
            }

            return false;
        }


        private void OnCellButtonClick(object sender, RoutedEventArgs e)
        {
            Button b = e.Source as Button;
            Grid a = (Grid) b.Parent;

            if (b != null)
            {
                int level = Convert.ToInt32(a.Tag.ToString());
                int row = (int) b.GetValue(RowProperty);
                int col = (int) b.GetValue(ColumnProperty);
                Move playerMove = new Move(col, row, level);
                nextMoveNotify(playerMove);
            }
        }

        public void ChangeCellTextColor(Button b, Brush colorBrush)
        {
            TextBlock cellBack = (TextBlock) b.Template.FindName("SymbolBlock", b);
            cellBack.Foreground = colorBrush;
        }

        public void ChangeCellTextColor(Move cell, Brush colorBrush)
        {
            Button b = FindCellButton(cell);
            if (b != null)
            {
                ChangeCellTextColor(b, colorBrush);
            }
        }


        public void MarkWinningLine(List<Move> winningMoveList)
        {
            if (winningMoveList == null) return;

            foreach (Move move in winningMoveList)
            {
                ChangeCellTextColor(move, Brushes.Green);
            }
        }


        public void ResetBoard()
        {
            foreach (Grid level in Children)
            {
                foreach (Button cell in level.Children)
                {
                    cell.Content = String.Empty;
                }
            }
        }

        public void SetupGameBoard()
        {
            int columnSize = boardWidth / dimension;

            int cellElementSize = (columnSize);

            Width = boardWidth;


            // Define rows and columns in the Grid
            for (int pos = 0; pos < dimension; pos++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = new GridLength(columnSize);

                RowDefinitions.Add(r);

                ColumnDefinition c = new ColumnDefinition();
                c.Width = new GridLength(columnSize);
                ColumnDefinitions.Add(c);
            }


            int level = 0;
            // Now add sub grids to it
            for (int pos = 0; pos < dimension; pos++)
            {
                LevelGrid lGrid = new LevelGrid();
                lGrid.Dimension = dimension;
                lGrid.LevelWidth = cellElementSize;
                lGrid.SetupThePuzzleGridStructure();

                lGrid.SetValue(RowProperty, pos);
                lGrid.SetValue(ColumnProperty, pos);

                lGrid.Tag = level; //set which level this is


                TextBlock textBlock = new TextBlock();
                textBlock.FontSize = 35;
                textBlock.Opacity = 0.1;
                int curLevel = pos + 1;
                textBlock.Text = curLevel.ToString();
                VisualBrush vb = new VisualBrush(textBlock);
                lGrid.Background = vb;

                gameLevels.Add(lGrid);

                Children.Add(lGrid);
                level++;
            }
        }


        private void GameBoardLoaded(object sender, RoutedEventArgs e)
        {
        }
    }
}