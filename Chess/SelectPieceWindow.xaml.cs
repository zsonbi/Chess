using System;
using System.Collections.Generic;
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

namespace Chess
{
    /// <summary>
    /// Interaction logic for SelectPieceWindow.xaml
    /// </summary>
    public partial class SelectPieceWindow : Window
    {
        public SelectPieceWindow(bool side)
        {
            InitializeComponent();

            ((mainGrid.Children[0] as Border).Child as Image).Source = new BitmapImage(new Uri(("Pieces/" + (side ? "White" : "Black") + "Knight" + ".png"), UriKind.Relative));
            ((mainGrid.Children[1] as Border).Child as Image).Source = new BitmapImage(new Uri(("Pieces/" + (side ? "White" : "Black") + ((mainGrid.Children[1] as Border).Child as Image).Name + ".png"), UriKind.Relative));
            ((mainGrid.Children[2] as Border).Child as Image).Source = new BitmapImage(new Uri(("Pieces/" + (side ? "White" : "Black") + ((mainGrid.Children[2] as Border).Child as Image).Name + ".png"), UriKind.Relative));
            ((mainGrid.Children[3] as Border).Child as Image).Source = new BitmapImage(new Uri(("Pieces/" + (side ? "White" : "Black") + ((mainGrid.Children[3] as Border).Child as Image).Name + ".png"), UriKind.Relative));
        }
    }
}