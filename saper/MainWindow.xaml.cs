using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace saper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int xSize;
        private int ySize;
        List<List<MyFieldCell>> cells = null;
        HashSet<MyFieldCell> was = new HashSet<MyFieldCell>();
        private int countOfOpened;
        private int countOfBombs;

        public MainWindow()
        {
            InitializeComponent();
            InitializeField();
        }

        public void InitializeField()
        {

            this.xSize = string.IsNullOrEmpty(Regex.Replace(lbBombs.Text, @"\s", "")) ? 8 : Convert.ToInt32(Regex.Replace(lbRows.Text, @"\s", ""));
            this.ySize = string.IsNullOrEmpty(Regex.Replace(lbBombs.Text, @"\s", "")) ? 8 : Convert.ToInt32(Regex.Replace(lbColumns.Text, @"\s", ""));
            countOfBombs = string.IsNullOrEmpty(Regex.Replace(lbBombs.Text, @"\s", "")) ? xSize*ySize/10 : Convert.ToInt32(Regex.Replace(lbBombs.Text, @"\s", ""));

            if (xSize * ySize < countOfBombs || countOfBombs == 0) { countOfBombs = xSize * ySize / 10; MessageBox.Show("Wrong number of bombs!"); }
            if (xSize == 0) { xSize = 8; MessageBox.Show("Wrong number of rows!"); }
            if (ySize == 0) { ySize = 8; MessageBox.Show("Wrong number of columns!"); }

            countOfOpened = 0;
            if (cells != null)
                foreach (var it in cells)
                    foreach (var item in it)
                    {
                        item.button.Click -= cellOfField_Click;
                        item.button.MouseRightButtonUp -= Button_MouseRightButtonUp;
                    }
            cells = null;
            MainField.Children.Clear();
            MainField.RowDefinitions.Clear();
            MainField.ColumnDefinitions.Clear();

            for (int i = 0; i < xSize; i++)
            {
                MainField.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < ySize; i++)
            {
                MainField.ColumnDefinitions.Add(new ColumnDefinition());
            }

            cells = new List<List<MyFieldCell>>();
            for (int i = 0; i < xSize; i++)
            {
                var tmp = new List<MyFieldCell>();
                for (int j = 0; j < ySize; j++)
                {
                    Button bt = new Button();
                                        bt.Tag = new KeyValuePair<int, int>(i, j);
                   // bt.Name = "x" + i + "y" + j;
                    //bt.Background = new SolidColorBrush(Colors.DarkGray);
                    bt.Click += cellOfField_Click;
                    bt.MouseRightButtonUp += Button_MouseRightButtonUp;
                    bt.SetValue(Grid.RowProperty, i);
                    bt.SetValue(Grid.ColumnProperty, j);
                    MainField.Children.Add(bt);
                    tmp.Add(new MyFieldCell(bt, i, j));
                }
                cells.Add(tmp);
            }

            Random rnd = new Random();
            for (int i = 0; i < countOfBombs; i++)
            {
                int x = rnd.Next(0, xSize);
                int y = rnd.Next(0, ySize);
                if (cells[x][y].haveBomb)
                {
                    i--;
                }
                else
                {
                    cells[x][y].haveBomb = true;
                }
            }
            // cells[0][0].haveBomb = true;

            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    cells[i][j].counOfBombAround = countBombs(i, j);
                }
            }
        }

        private void cellOfField_Click(object sender, RoutedEventArgs e)
        {
            Button curBtn = (Button)sender;
            //Messages.Text = curBtn.Name;
            if (curBtn.Content != null && curBtn.Content.ToString() == "F") return;

            int xCell = ((KeyValuePair<int, int>)curBtn.Tag).Key;
            int yCell = ((KeyValuePair<int, int>)curBtn.Tag).Value;

            if (cells[xCell][yCell].haveBomb)
            {
                foreach (var it in cells)
                    foreach (var item in it)
                    {
                        if (item.haveBomb)
                        {
                            item.button.Background = new SolidColorBrush(Colors.Red);
                            //item.button.b
                            item.button.Content = "B";
                            item.button.Click -= cellOfField_Click;
                            item.button.MouseRightButtonUp -= Button_MouseRightButtonUp;
                        }
                        else
                        {
                            if (item.button.IsEnabled) checkCell(item.x, item.y);
                            //if (item.button.IsEnabled) item.button.IsEnabled = false;
                            //item.button.IsEnabled = false;
                        }
                    }
                MessageBox.Show("You lose!");
            }
            else
            {
                checkCell(xCell, yCell);

                if (xSize * ySize - countOfOpened <= countOfBombs)
                {
                    foreach (var it in cells)
                        foreach (var item in it)
                        {
                            if (item.haveBomb)
                            {
                                item.button.Background = new SolidColorBrush(Colors.Green);
                                item.button.Content = "B";
                                item.button.Click -= cellOfField_Click;
                                item.button.MouseRightButtonUp -= Button_MouseRightButtonUp;
                            }
                        }
                    MessageBox.Show("You WIN!");
                }
            }
        }

        private void addWithCkeck(Stack<MyFieldCell> stk, HashSet<MyFieldCell> st, MyFieldCell mfc)
        {
            if (!st.Contains(mfc) && !stk.Contains(mfc)) stk.Push(mfc);
        }

        private void checkCell(int xCell, int yCell)
        {
            Stack<MyFieldCell> stk = new Stack<MyFieldCell>();


            stk.Push(cells[xCell][yCell]);

            while (stk.Count > 0)
            {
                var cell = stk.Pop();
                was.Add(cell);

                if (cell.counOfBombAround != 0)
                {
                    cell.button.Content = cell.counOfBombAround;
                }
                else
                {
                    if (cell.x - 1 >= 0)
                    {
                        if (cell.x + 1 < xSize)
                        {
                            if (cell.y - 1 >= 0)
                            {
                                if (cell.y + 1 < ySize)
                                {
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y - 1]); addWithCkeck(stk, was, cells[cell.x][cell.y - 1]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y - 1]);
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y]);
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y + 1]); addWithCkeck(stk, was, cells[cell.x][cell.y + 1]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y + 1]);
                                }
                                else
                                {
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y - 1]); addWithCkeck(stk, was, cells[cell.x][cell.y - 1]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y - 1]);
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y]);
                                }
                            }
                            else
                            {
                                if (cell.y + 1 < ySize)
                                {
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y]);
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y + 1]); addWithCkeck(stk, was, cells[cell.x][cell.y + 1]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y + 1]);
                                }
                                else
                                {
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y]);
                                }
                            }
                        }
                        else
                        {
                            if (cell.y - 1 >= 0)
                            {
                                if (cell.y + 1 < ySize)
                                {
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y - 1]); addWithCkeck(stk, was, cells[cell.x][cell.y - 1]);
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y]);
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y + 1]); addWithCkeck(stk, was, cells[cell.x][cell.y + 1]);
                                }
                                else
                                {
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y - 1]); addWithCkeck(stk, was, cells[cell.x][cell.y - 1]);
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y]);
                                }
                            }
                            else
                            {
                                if (cell.y + 1 < ySize)
                                {
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y]);
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y + 1]); addWithCkeck(stk, was, cells[cell.x][cell.y + 1]);
                                }
                                else
                                {
                                    addWithCkeck(stk, was, cells[cell.x - 1][cell.y]);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (cell.x + 1 < xSize)
                        {
                            if (cell.y - 1 >= 0)
                            {
                                if (cell.y + 1 < ySize)
                                {
                                    addWithCkeck(stk, was, cells[cell.x][cell.y - 1]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y - 1]);
                                    addWithCkeck(stk, was, cells[cell.x + 1][cell.y]);
                                    addWithCkeck(stk, was, cells[cell.x][cell.y + 1]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y + 1]);
                                }
                                else
                                {
                                    addWithCkeck(stk, was, cells[cell.x][cell.y - 1]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y - 1]);
                                    addWithCkeck(stk, was, cells[cell.x + 1][cell.y]);
                                }
                            }
                            else
                            {
                                if (cell.y + 1 < ySize)
                                {
                                    addWithCkeck(stk, was, cells[cell.x + 1][cell.y]);
                                    addWithCkeck(stk, was, cells[cell.x][cell.y + 1]); addWithCkeck(stk, was, cells[cell.x + 1][cell.y + 1]);
                                }
                                else
                                {
                                    addWithCkeck(stk, was, cells[cell.x + 1][cell.y]);
                                }
                            }
                        }
                        else
                        {
                            if (cell.y - 1 >= 0)
                            {
                                if (cell.y + 1 < ySize)
                                {
                                    addWithCkeck(stk, was, cells[cell.x][cell.y - 1]);
                                    addWithCkeck(stk, was, cells[cell.x][cell.y + 1]);
                                }
                                else
                                {
                                    addWithCkeck(stk, was, cells[cell.x][cell.y - 1]);
                                }
                            }
                            else
                            {
                                if (cell.y + 1 < ySize)
                                {
                                    addWithCkeck(stk, was, cells[cell.x][cell.y + 1]);
                                }
                            }
                        }
                    }
                }
                //cell.button.IsEnabled = false;
                if (!cell.button.IsEnabled)
                    MessageBox.Show("something broken!!!!");
                cell.button.IsEnabled = false;
                //cell.button.Content = (cell.button.Content == null ? 1 : (int)cell.button.Content + 1);
                countOfOpened++;
            }
        }

        //private void checkCell(int x, int y)
        //{
        //    if (x < 0 || y < 0 || x >= xSize || y >= ySize || !cells[x][y].button.IsEnabled || cells[x][y].haveBomb)
        //    {
        //        return;
        //    }

        //    if (cells[x][y].counOfBombAround != 0)
        //    {
        //        cells[x][y].button.Content = cells[x][y].counOfBombAround;
        //    }
        //    else
        //    {
        //        checkCell(x - 1, y - 1); checkCell(x, y - 1); checkCell(x + 1, y - 1);
        //        checkCell(x - 1, y); checkCell(x + 1, y);
        //        checkCell(x - 1, y + 1); checkCell(x, y + 1); checkCell(x + 1, y + 1);
        //    }
        //    cells[x][y].button.IsEnabled = false;
        //}

        private int countBombs(int x, int y)
        {
            return cb(x - 1, y - 1) + cb(x, y - 1) + cb(x + 1, y - 1)
                    + cb(x - 1, y) + cb(x + 1, y)
                    + cb(x - 1, y + 1) + cb(x, y + 1) + cb(x + 1, y + 1);
        }

        private int cb(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < xSize && y < ySize && cells[x][y].haveBomb) ? 1 : 0;
        }

        class MyFieldCell
        {
            public Button button;
            public bool haveBomb = false;
            public int counOfBombAround = 0;
            public int x;
            public int y;

            public MyFieldCell(Button button, int x, int y)
            {
                this.button = button;
                this.x = x;
                this.y = y;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InitializeField();
        }

        private void Button_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button bt = (Button)sender;
            if (bt.Content != null && bt.Content.ToString() == "F")
            {
                bt.Content = "";
            }
            else
            {
                bt.Content = "F";
            }
        }

        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
    }
}
