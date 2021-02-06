using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

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

        public bool currSide { get; private set; }
        public static readonly byte rowSize = 8;
        public static readonly byte colSize = 8;
        public Piece[,] pieces;
        internal King whiteKing { get; private set; }
        internal King blackKing { get; private set; }

        //Access even to the dead members
        private List<Piece> pieceList = new List<Piece>();

        //Constructor
        public ChessGame()
        {
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
                return pieces[row, col].PossibleMoves();
            else
                throw new Exception("There is no piece there");
        }

        /// <summary>
        /// Moves the piece specified by the first two parameters to the cords specified by the second two parameters
        /// </summary>
        /// <param name="rowPos">row of the piece</param>
        /// <param name="colPos">column of the piece</param>
        /// <param name="toRowPos">row where the piece should go</param>
        /// <param name="toColPos">column where the piece should go</param>
        /// <returns>true if it was successful false if there was an error</returns>
        public bool Move(sbyte rowPos, sbyte colPos, sbyte toRowPos, sbyte toColPos)
        {
            try
            {
                pieces[rowPos, colPos].Move((sbyte)(toRowPos - rowPos), (sbyte)(toColPos - colPos));
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine("Error at movement Error:" + e);
#endif
                return false;
            }
            currSide = !currSide;
            return true;
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
    }
}