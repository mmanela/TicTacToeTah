using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TicTacToeTah
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class LevelGrid
    {
        //default dimension of the puzzle
        private int dimmension;
        private int levelWidth = 200;


        /// <summary>
        /// Set the dimension of the board
        /// </summary>
        public int Dimension
        {
            set { dimmension = value; }
        }

        /// <summary>
        /// Set the width of the board
        /// </summary>
        public int LevelWidth
        {
            set { levelWidth = value; }
        }

        public LevelGrid()
        {
            InitializeComponent();
        }


        public void SetupThePuzzleGridStructure()
        {
            // Define rows and columns in the Grid
            for (int row = 0; row < dimmension; row++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = GridLength.Auto;
                RowDefinitions.Add(r);

                ColumnDefinition c = new ColumnDefinition();
                c.Width = GridLength.Auto;
                ColumnDefinitions.Add(c);
            }

            Style buttonStyle = (Style) Resources["CellButtonStyle"];

            // Now add the buttons in
            int i = 1;
            for (int row = 0; row < dimmension; row++)
            {
                for (int col = 0; col < dimmension; col++)
                {
                    Button b = new Button();
                    b.FontSize = 24;

                    b.Style = buttonStyle;

                    b.SetValue(RowProperty, row);
                    b.SetValue(ColumnProperty, col);


                    i++;

                    Children.Add(b);
                }
            }
        }


        private void LevelGridLoaded(object sender, RoutedEventArgs e)
        {
            Width = levelWidth;
            Height = levelWidth;

            double colSize = levelWidth / dimmension;


            // Set up viewbox appropriately for each tile.
            foreach (Button b in Children)
            {
                Border cellBack = (Border) b.Template.FindName("CellBorderBack", b);
                int row = (int) b.GetValue(RowProperty);
                int col = (int) b.GetValue(ColumnProperty);


                cellBack.Height = colSize;
                cellBack.Width = colSize;
                b.FontSize = colSize * 0.7;

                cellBack.Background = Brushes.Transparent;
            }
        }
    }
}