using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChessGame game = new ChessGame();
        private Rectangle[,] squares;
        private List<Ellipse> possibleMoveMarkers = new List<Ellipse>();
        private sbyte[] currSelected = new sbyte[2] { -1, -1 };
        private Image currPieceSelected;
        private SelectPieceWindow upgradeWindow = null;

        //Constructor
        public MainWindow()
        {
            InitializeComponent();
            CreateDefinitions(ChessGame.rowSize, ChessGame.colSize);
            CreateSquares();
            LoadPieces();
        }

        //***********************************************************************
        //Private Methods
        //This will create the squares on the board
        private void CreateSquares()
        {
            squares = new Rectangle[ChessGame.rowSize, ChessGame.colSize];
            for (int i = 0; i < ChessGame.rowSize; i++)
            {
                for (int j = 0; j < ChessGame.colSize; j++)
                {
                    Rectangle rect = new Rectangle();
                    Grid.SetRow(rect, i);
                    Grid.SetColumn(rect, j);
                    rect.MouseDown += SelectSquare;
                    rect.Fill = j % 2 == i % 2 ? Brushes.Wheat : Brushes.SandyBrown;
                    boardGrid.Children.Add(rect);
                    squares[i, j] = rect;
                }
            }
        }

        //----------------------------------------------------------------------
        //Checks if the game is over and if it is then overwrites the label to who won
        private void CheckForGameOver()
        {
            if (game.gameOver)
            {
                GameOverLabel.Visibility = Visibility.Visible;
                string whoWon;
                switch (game.WhoWon())
                {
                    case 0:
                        whoWon = "Black won";
                        break;

                    case 1:
                        whoWon = "White won";
                        break;

                    case 2:
                        whoWon = "Draw";
                        break;

                    default:
                        whoWon = "error";
                        break;
                }

                GameOverLabel.Content = whoWon;
            }
        }

        //-----------------------------------------------------------------------
        //Loads the pieces
        private void LoadPieces()
        {
            string[,] pieces = game.ExportBoard();
            Console.WriteLine("../../Pieces/" + pieces[0, 0].Replace("_", "") + ".png");
            for (int i = 0; i < pieces.GetLength(0); i++)
            {
                for (int j = 0; j < pieces.GetLength(1); j++)
                {
                    if (pieces[i, j] != "empty")
                    {
                        Image piece = new Image();
                        piece.Source = new BitmapImage(new Uri(("Pieces/" + pieces[i, j].Replace("_", "") + ".png"), UriKind.Relative));
                        piece.MouseDown += SelectPiece;
                        Grid.SetColumn(piece, j);
                        Grid.SetRow(piece, i);
                        boardGrid.Children.Add(piece);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------
        //Creates the griddefinitions
        private void CreateDefinitions(byte rows, byte cols)
        {
            boardGrid.ColumnDefinitions.Clear();
            boardGrid.RowDefinitions.Clear();
            for (int i = 0; i < rows; i++)
            {
                boardGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < cols; i++)
            {
                boardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        //------------------------------------------------------------------------
        //Clears out all of the possible moves
        private void ClearPossibleMoves()
        {
            foreach (var item in possibleMoveMarkers)
            {
                boardGrid.Children.Remove(item);
            }
            possibleMoveMarkers.Clear();
        }

        //------------------------------------------------------------------------
        //Shows the possible moves
        private void ShowPossibleMoves()
        {
            ClearPossibleMoves();
            foreach (var item in game.GetPossibleMoves(currSelected[0], currSelected[1]))
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 20;
                ellipse.Height = 20;
                ellipse.Fill = Brushes.Brown;
                Grid.SetColumn(ellipse, item[1]);
                Grid.SetRow(ellipse, item[0]);
                ellipse.IsHitTestVisible = false;
                possibleMoveMarkers.Add(ellipse);
                boardGrid.Children.Add(ellipse);
            }
        }

        //-----------------------------------------------------------------------
        //Checks if that move is legal
        private bool IsLegal(sbyte row, sbyte col)
        {
            foreach (var item in possibleMoveMarkers)
            {
                if (Grid.GetColumn(item) == col && Grid.GetRow(item) == row)
                    return true;
            }
            return false;
        }

        //------------------------------------------------------------------------
        //Selects the piece
        private byte Select(sbyte row, sbyte col)
        {
            if (IsLegal(row, col))
            {
                //We paint the square where the king is currently in every time so it won't be left red accidentally
                sbyte[] kingPos = (game.currSide ? game.whiteKingPos : game.blackKingPos);
                squares[kingPos[0], kingPos[1]].Fill = kingPos[1] % 2 == kingPos[0] % 2 ? Brushes.Wheat : Brushes.SandyBrown;
                byte moveResult = game.Move(currSelected[0], currSelected[1], row, col).Result;
                if (moveResult >= 1)
                {
                    ClearPossibleMoves();
                    squares[currSelected[0], currSelected[1]].Margin = new Thickness(0);
                    Grid.SetColumn(currPieceSelected, col);
                    Grid.SetRow(currPieceSelected, row);

                    if (game.CanUpgradeAt(row, col))
                    {
                        CreateUpgradeWindow(!game.currSide);

                        currSelected[0] = row;
                        currSelected[1] = col;
                    }
                    else
                    {
                        currSelected[0] = -1;
                        currSelected[1] = -1;
                    }

                    if (moveResult == 2)
                    {
                        sbyte kingColSpot = (game.currSide ? game.blackKing : game.whiteKing).colPos;
                        sbyte rookColSpot = (sbyte)(kingColSpot - 1 + kingColSpot / 3);
                        foreach (var item in boardGrid.Children)
                        {
                            if (item is Image)
                            {
                                if (Grid.GetRow(item as UIElement) == row && Grid.GetColumn(item as UIElement) == rookColSpot)
                                    Grid.SetColumn(item as Image, (sbyte)((sbyte)(kingColSpot + 1 - kingColSpot / 3)));
                            }
                        }
                    }

                    //Marks the king's position if it's in check
                    kingPos = (game.currSide ? game.whiteKingPos : game.blackKingPos);

                    if (game.kingIsThreatened)
                    {
                        squares[kingPos[0], kingPos[1]].Fill = Brushes.Red;
                    }
                    //Checks if the game is over
                    CheckForGameOver();
                    currPieceSelected = null;
                    return 2;
                }
            }
            else if (currSelected[0] == -1 && currSelected[1] == -1)
            {
                if (!game.ContainsPiece(row, col) || !game.Selectable(row, col))
                    return 0;

                squares[row, col].Margin = new Thickness(3);
                currSelected[0] = row;
                currSelected[1] = col;
                ShowPossibleMoves();
                return 1;
            }
            else
            {
                squares[currSelected[0], currSelected[1]].Margin = new Thickness(0);
                ClearPossibleMoves();

                currSelected[0] = -1;
                currSelected[1] = -1;
            }
            return 0;
        }

        //------------------------------------------------------------------------
        //Adds the events to the Window where the user can select an upgrade for the piece
        private void CreateUpgradeWindow(bool side)
        {
            if (upgradeWindow != null)
                upgradeWindow.Close();
            upgradeWindow = new SelectPieceWindow(side);
            upgradeWindow.Show();
            foreach (var item in upgradeWindow.mainGrid.Children)
            {
                ((item as Border).Child as Image).MouseDown += SelectUpgradePiece;
            }
        }

        //************************************************************************
        //Handlers
        //Selects the piece
        private void SelectPiece(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image piece = sender as Image;
            byte result = Select((sbyte)Grid.GetRow(piece), (sbyte)Grid.GetColumn(piece));
            if (result == 1)
                currPieceSelected = piece;
            else if (result == 2)
                boardGrid.Children.Remove(piece);
        }

        //----------------------------------------------------------------------------------
        //Selects the rectangle
        private void SelectSquare(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            Select((sbyte)Grid.GetRow(rect), (sbyte)Grid.GetColumn(rect));
        }

        //----------------------------------------------------------------------------------
        //Selects the piece which we want to upgrade to
        private void SelectUpgradePiece(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image piece = sender as Image;
            game.Upgrade(currSelected[0], currSelected[1], piece.Name[0]);
            currPieceSelected.Source = piece.Source;
            currSelected[0] = -1;
            currSelected[1] = -1;
            upgradeWindow.Close();
            CheckForGameOver();
            //Marks the king's position if it's in check
            sbyte[] kingPos = (game.currSide ? game.whiteKingPos : game.blackKingPos);
            if (game.kingIsThreatened)
                squares[kingPos[0], kingPos[1]].Fill = Brushes.Red;
        }
    }
}