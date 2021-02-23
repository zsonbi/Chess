using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;

namespace Chess
{
    internal class ChessGame
    {
        private struct PieceSave
        {
            public char type { get; private set; }
            public sbyte rowPos { get; private set; }
            public sbyte colPos { get; private set; }
            public bool side { get; private set; }

            public PieceSave(char type, sbyte rowPos, sbyte colPos, bool side)
            {
                this.type = type;
                this.rowPos = rowPos;
                this.colPos = colPos;
                this.side = side;
            }
        }

        internal static readonly byte rowSize = 8;
        internal static readonly byte colSize = 8;
        private Piece[,] pieces;

        //pointer to the white king
        internal King whiteKing { get; private set; }

        //pointer to the black king
        internal King blackKing { get; private set; }

        //Access even to the dead members
        private List<Piece> pieceList = new List<Piece>();

        /// <summary>
        /// Gets the current side (white = true; black = false)
        /// </summary>
        public bool currSide { get; private set; }

        /// <summary>
        /// Gets if the king is threatened
        /// </summary>
        public bool kingIsThreatened { get; private set; }

        /// <summary>
        /// Gets the white king's position
        /// </summary>
        public sbyte[] whiteKingPos { get => new sbyte[] { whiteKing.rowPos, whiteKing.colPos }; }

        /// <summary>
        /// Gets the black king's position
        /// </summary>
        public sbyte[] blackKingPos { get => new sbyte[] { blackKing.rowPos, blackKing.colPos }; }

        /// <summary>
        /// A bool which stores if someone has lost the game
        /// </summary>
        public bool gameOver { get; private set; }

        //Constructor
        public ChessGame()
        {
            kingIsThreatened = false;
            //The side which comes next
            pieces = new Piece[rowSize, colSize];
            //If there is an already saved user config for that user load it
            if (File.Exists("baseBoard.json"))
            {
                string json;
                List<PieceSave> pieceSaves;
                using (StreamReader r = new StreamReader("../../baseBoard.json"))
                {
                    json = r.ReadToEnd();
                    pieceSaves = JsonConvert.DeserializeObject<List<PieceSave>>(json);
                }

                foreach (var item in pieceSaves)
                {
                    pieces[item.rowPos, item.colPos] = CharToPiece(item.type, item.rowPos, item.colPos, item.side);
                    pieceList.Add(pieces[item.rowPos, item.colPos]);
                }
            }
            gameOver = false;
            currSide = true;
        }

        //************************************************************************
        //Private Methods
        //returns the appropiate Piece for the char
        //{P-Pawn, N-Knight, R-Rook, B-Bishop, Q-Queen, K-King}
        private Piece CharToPiece(char input, sbyte rowPos, sbyte colPos, bool side)
        {
            switch (input)
            {
                case 'P':
                    return new Pawn(ref pieces, rowPos, colPos, side);

                case 'N':
                    return new Knight(ref pieces, rowPos, colPos, side);

                case 'R':
                    return new Rook(ref pieces, rowPos, colPos, side);

                case 'B':
                    return new Bishop(ref pieces, rowPos, colPos, side);

                case 'Q':
                    return new Queen(ref pieces, rowPos, colPos, side);

                case 'K':
                    King king = new King(ref pieces, rowPos, colPos, side);
                    if (king.side)
                        whiteKing = king;
                    else
                        blackKing = king;
                    return king;

                default:
                    throw new Exception("Unknown character");
                    break;
            }
        }

        //--------------------------------------------------------------------------
        //Checks if the king is threatened by that move
        private void CheckForKingThreat(King whichKing)
        {
            if (whichKing.IsKingInDanger())
            {
                whichKing.isThreatened = true;
                kingIsThreatened = true;
                //If the king has no other moves test for other pieces
                if (whichKing.PossibleMoves().Count == 0)
                {
                    List<sbyte[]> allyMoves = new List<sbyte[]>();
                    //get every move which the other pieces can make if there is atleast one
                    //then the king can still survive
                    for (int i = 0; i < rowSize; i++)
                    {
                        for (int j = 0; j < colSize; j++)
                        {
                            if (pieces[i, j].side == whichKing.side)
                            {
                                allyMoves.AddRange(pieces[i, j].PossibleMoves(false, true));
                            }
                        }
                    }

                    if (allyMoves.Count == 0)
                        gameOver = true;
                }
                return;
            }
        }

        //-------------------------------------------------------------------------
        //Checks if that piece can be upgraded
        private bool CanUpgrade(Piece piece)
        {
            if (!(piece is Pawn))
                return false;
            return piece.rowPos == (piece.side ? 0 : 7);
        }

        //**************************************************************************
        //Public Methods
        /// <summary>
        /// Checks whether that spot has a piece or not
        /// </summary>
        /// <param name="rowPos">piece's position</param>
        /// <param name="colPos">piece's position</param>
        /// <returns>true if there is a piece false if not</returns>
        public bool ContainsPiece(sbyte rowPos, sbyte colPos)
        {
            return pieces[rowPos, colPos] != null;
        }

        /// <summary>
        /// Exports the board's content as a string[,]
        /// </summary>
        /// <returns>the content of the chessboard in a string 2d array</returns>
        public string[,] ExportBoard()
        {
            string[,] output = new string[ChessGame.rowSize, ChessGame.colSize];
            for (int i = 0; i < ChessGame.rowSize; i++)
            {
                for (int j = 0; j < ChessGame.colSize; j++)
                {
                    if (pieces[i, j] == null)
                    {
                        output[i, j] = "empty";
                    }
                    else
                    {
                        output[i, j] = (pieces[i, j].side ? "White" : "Black") + "_" + pieces[i, j].GetType().ToString().Replace("Chess.", "");
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Returns the possible moves for the piece specified by the parameters
        /// </summary>
        /// <param name="row">the row if the piece</param>
        /// <param name="col">the</param>
        /// <returns></returns>
        public List<sbyte[]> GetPossibleMoves(sbyte row, sbyte col)
        {
            if (pieces[row, col] != null)
                return pieces[row, col].PossibleMoves(false, true);
            else
                throw new Exception("There is no piece there");
        }

        /// <summary>
        /// Moves the piece which position was specified by the first string
        /// e. g. if the pawn is on the 1,1 spot and you want to move it to 2,1 ypu should write 2b 3b
        /// </summary>
        /// <param name="pieceSpot">The position of the piece which will move</param>
        /// <param name="pieceTarget">The location where we want to move the piece</param>
        /// <returns>1 if it was successful if there was an error 0 and 2 if it was successful, but the move was a rook-king swap</returns>
        public async Task<byte> Move(string pieceSpot, string pieceTarget)
        {
            sbyte pieceSourceRow;
            sbyte pieceSourceCol;
            sbyte targetRow;
            sbyte targetCol;

            try
            {
                pieceSourceCol = Convert.ToSByte((char)pieceSpot[0] - 97);
                pieceSourceRow = (sbyte)Math.Abs(7 - Convert.ToSByte((char)pieceSpot[1]));
                targetCol = Convert.ToSByte((char)pieceTarget[0] - 97);
                targetRow = (sbyte)Math.Abs(7 - Convert.ToSByte((char)pieceTarget[1]));
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine("Error at movement converting:" + e);
#endif
                return 0;
            }

            return await Move(pieceSourceRow, pieceSourceCol, targetRow, targetCol);
        }

        /// <summary>
        /// Moves the piece specified by the first two parameters to the cords specified by the second two parameters
        /// </summary>
        /// <param name="rowPos">row of the piece</param>
        /// <param name="colPos">column of the piece</param>
        /// <param name="toRowPos">row where the piece should go</param>
        /// <param name="toColPos">column where the piece should go</param>
        /// <returns>1 if it was successful if there was an error 0 and 2 if it was successful, but the move was a rook-king swap</returns>
        public async Task<byte> Move(sbyte rowPos, sbyte colPos, sbyte toRowPos, sbyte toColPos)
        {
            if (gameOver)
                return 0;

            try
            {
                pieces[rowPos, colPos].Move((sbyte)(toRowPos - rowPos), (sbyte)(toColPos - colPos));
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine("Error at movement Error:" + e);
#endif
                return 0;
            }
            kingIsThreatened = false;
            CheckForKingThreat(currSide ? blackKing : whiteKing);
            currSide = !currSide;

            if (pieces[toRowPos, toColPos] is King && Math.Abs(toColPos - colPos) > 1)
                return 2;
            return 1;
        }

        /// <summary>
        /// Checks if that piece is selectable
        /// </summary>
        /// <param name="row">row of that piece</param>
        /// <param name="col">column of that piece</param>
        /// <returns>true if it is possible false if not</returns>
        public bool Selectable(sbyte row, sbyte col)
        {
            if (pieces[row, col] == null)
                return false;
            return currSide == pieces[row, col].side;
        }

        /// <summary>
        /// Gets if it is possible to upgrade that piece
        /// </summary>
        /// <param name="row">row of that piece</param>
        /// <param name="col">column of that piece</param>
        /// <returns>true-if possible false if not</returns>
        public bool CanUpgradeAt(sbyte row, sbyte col)
        {
            return CanUpgrade(pieces[row, col]);
        }

        /// <summary>
        /// Upgrades the pawn to an another type
        /// </summary>
        /// <param name="row">row of that piece</param>
        /// <param name="col">column of that piece</param>
        /// <param name="type">character which symbols the piece</param>
        public void Upgrade(sbyte row, sbyte col, char type)
        {
            if (CanUpgradeAt(row, col))
            {
                pieces[row, col] = CharToPiece(type, row, col, pieces[row, col].side);
                kingIsThreatened = false;
                CheckForKingThreat(!currSide ? blackKing : whiteKing);
            }
            else
            {
#if DEBUG
                throw new Exception("Bad upgrade");
#endif
            }
        }

        /// <summary>
        /// Returns who won throws an exception if the game was still in progress
        /// </summary>
        /// <returns>true = white won false = black won</returns>
        public bool WhoWon()
        {
            if (!gameOver)
            {
                throw new Exception("NoOne won");
            }
            else
                return blackKing.isThreatened;
        }

        /// <summary>
        /// Returns a bitmap of the board
        /// </summary>
        /// <returns>A bitmap of the board</returns>
        public async Task<Bitmap> GetImageOfBoard()
        {
            return await ChessImageCreator.CreateImageFromBoard(ExportBoard());
        }
    }
}