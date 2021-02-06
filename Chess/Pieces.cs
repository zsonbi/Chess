using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal abstract class Piece
    {
        /// <summary>
        /// Row where the piece can be found
        /// </summary>
        public sbyte rowPos { get; protected set; }

        /// <summary>
        /// Column where the piece can be found
        /// </summary>
        public sbyte colPos { get; protected set; }

        /// <summary>
        /// The side of the piece white=true black=false
        /// </summary>
        public bool side { get; private set; }

        /// <summary>
        /// Determines whether the piece has died
        /// </summary>
        public bool isDead { get; protected set; }

        protected Piece[,] pieces;

        public Piece(ref Piece[,] pieces, sbyte rowPos, sbyte colPos, bool side)
        {
            this.rowPos = rowPos;
            this.colPos = colPos;
            this.side = side;
            this.isDead = false;
            this.pieces = pieces;
        }

        /// <summary>
        /// Every Piece has it's own movement type
        /// </summary>
        /// <param name="destRow">the row where it should go</param>
        /// <param name="destCol">the column where it should go</param>
        public void Move(sbyte destRow, sbyte destCol)
        {
            if (OutOfBounds((sbyte)(rowPos + destRow), (sbyte)(colPos + destCol)))
                throw new IndexOutOfRangeException("Gone out of the board");

            if (!IsLegalMove((sbyte)(destRow), destCol))
                throw new Exception("Illegal move");

            pieces[rowPos, colPos] = null;
            rowPos += (sbyte)(destRow);
            colPos += destCol;
            pieces[rowPos, colPos] = this;
        }

        /// <summary>
        /// Returns all the possible spots where it can move
        /// </summary>
        /// <returns>Spots where it can move</returns>
        public virtual List<sbyte[]> PossibleMoves()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if these moves takes the piece out of the board
        /// </summary>
        /// <param name="destRow">which row the piece will be</param>
        /// <param name="destCol">which column the piece will be</param>
        /// <returns></returns>
        protected bool OutOfBounds(sbyte destRow, sbyte destCol)
        {
            return (destCol < 0 || destCol >= ChessGame.colSize || destRow < 0 || destRow >= ChessGame.rowSize);
        }

        protected bool IsLegalMove(sbyte destRow, sbyte destCol)
        {
            sbyte row = (sbyte)(destRow + rowPos);
            sbyte col = (sbyte)(destCol + colPos);

            foreach (var item in this.PossibleMoves())
            {
                if (item[0] == row && item[1] == col)
                {
                    return true;
                }
            }
            return false;
        }
    }

    //*************************************************************************
    /// <summary>
    /// The pawn
    /// </summary>
    internal class Pawn : Piece
    {
        public Pawn(ref Piece[,] pieces, sbyte rowPos, sbyte colPos, bool side) : base(ref pieces, rowPos, colPos, side)
        {
        }

        /// <summary>
        /// Returns all the possible spots where the pawn can move
        /// </summary>
        /// <returns>list of all the cords</returns>
        public override List<sbyte[]> PossibleMoves()
        {
            List<sbyte[]> output = new List<sbyte[]>();
            if (rowPos + (side ? -1 : 1) > ChessGame.rowSize || rowPos + (side ? -1 : 1) < 0)
                return output;
            if (pieces[rowPos + (side ? -1 : 1), colPos] == null)
                output.Add(new sbyte[] { (sbyte)(rowPos + (side ? -1 : 1)), colPos });
            if (colPos + 1 < ChessGame.colSize && pieces[rowPos + (side ? -1 : 1), colPos + 1] != null && pieces[rowPos + (side ? -1 : 1), colPos + 1].side != side)
                output.Add(new sbyte[] { (sbyte)(rowPos + (side ? -1 : 1)), (sbyte)(colPos + 1) });
            if (colPos - 1 >= 0 && pieces[rowPos + (side ? -1 : 1), colPos - 1] != null && pieces[rowPos + (side ? -1 : 1), colPos - 1].side != side)
                output.Add(new sbyte[] { (sbyte)(rowPos + (side ? -1 : 1)), (sbyte)(colPos - 1) });
            return output;
        }
    }

    //*************************************************************************
    /// <summary>
    /// The Knight
    /// </summary>
    internal class Knight : Piece
    {
        private static readonly List<sbyte[]> validMoves = new List<sbyte[]>() { new sbyte[] { 1, 2 } };

        public Knight(ref Piece[,] pieces, sbyte rowPos, sbyte colPos, bool side) : base(ref pieces, rowPos, colPos, side)
        {
        }

        /// <summary>
        /// Returns a list of all the spots where the Knight can move (it moves in an L shape)
        /// Uses the f(x)= 3-|x| and f(x)=-3+|x| to get the values
        /// </summary>
        /// <returns>a list of cords where it can move</returns>
        public override List<sbyte[]> PossibleMoves()
        {
            List<sbyte[]> output = new List<sbyte[]>();

            for (sbyte i = -2; i < 3; i++)
            {
                if (i == 0)
                    continue;
                for (sbyte o = -1; o <= 1; o += 2)
                {
                    sbyte tempCol = (sbyte)(colPos + 3 * o - Math.Abs(i) * o);
                    sbyte tempRow = (sbyte)(rowPos + i);
                    if (!OutOfBounds(tempRow, tempCol) && (pieces[tempRow, tempCol] == null || pieces[tempRow, tempCol].side != this.side))
                        output.Add(new sbyte[] { tempRow, tempCol });
                }
            }
            return output;
        }
    }

    //*************************************************************************
    /// <summary>
    /// The Rook
    /// </summary>
    internal class Rook : Piece
    {
        //Constructor
        public Rook(ref Piece[,] pieces, sbyte rowPos, sbyte colPos, bool side) : base(ref pieces, rowPos, colPos, side)
        {
        }

        /// <summary>
        /// Returns a list of all the spots where the Rook can move (moves horizontally and vertically)
        /// </summary>
        /// <returns>a list of cords where it can move</returns>
        public override List<sbyte[]> PossibleMoves()
        {
            List<sbyte[]> output = new List<sbyte[]>();
            sbyte tempRow = base.rowPos;
            sbyte tempCol = base.colPos;
            //Checks the left way
            for (sbyte i = (sbyte)(base.colPos - 1); i >= 0; i--)
            {
                if (pieces[rowPos, i] != null)
                {
                    //if it can attack
                    if (pieces[rowPos, i].side != this.side)
                        output.Add(new sbyte[] { rowPos, (sbyte)(i) });
                    break;
                }
                output.Add(new sbyte[] { rowPos, (sbyte)(i) });
            }
            //Checks upwards
            for (sbyte i = (sbyte)(base.rowPos - 1); i >= 0; i--)
            {
                //if it can attack
                if (pieces[i, colPos] != null)
                {
                    if (pieces[i, colPos].side != this.side)
                        output.Add(new sbyte[] { (sbyte)(i), colPos });
                    break;
                }
                output.Add(new sbyte[] { (sbyte)(i), colPos });
            }
            //Checks the right way
            for (sbyte i = (sbyte)(base.colPos + 1); i < ChessGame.colSize; i++)
            {
                //if it can attack
                if (pieces[rowPos, i] != null)
                {
                    if (pieces[rowPos, i].side != this.side)
                        output.Add(new sbyte[] { rowPos, (sbyte)(i) });

                    break;
                }
                output.Add(new sbyte[] { rowPos, (sbyte)(i) });
            }
            //Checks downwards
            for (sbyte i = (sbyte)(base.rowPos + 1); i < ChessGame.rowSize; i++)
            {
                //if it can attack
                if (pieces[i, colPos] != null)
                {
                    if (pieces[i, colPos].side != this.side)
                        output.Add(new sbyte[] { (sbyte)(i), colPos });
                    break;
                }
                output.Add(new sbyte[] { (sbyte)(i), colPos });
            }
            return output;
        }
    }

    //*************************************************************************
    /// <summary>
    /// The Bishop
    /// </summary>
    internal class Bishop : Piece
    {
        //Constructor
        public Bishop(ref Piece[,] pieces, sbyte rowPos, sbyte colPos, bool side) : base(ref pieces, rowPos, colPos, side)
        {
        }

        /// <summary>
        /// Returns a list of all the spots where the Bishop can move (moves diagonally)
        /// </summary>
        /// <returns>a list of cords where it can move</returns>
        public override List<sbyte[]> PossibleMoves()
        {
            List<sbyte[]> output = new List<sbyte[]>();
            sbyte tempRow;
            sbyte tempCol;

            for (sbyte i = -1; i < 2; i += 2)
            {
                for (sbyte j = -1; j < 2; j += 2)
                {
                    tempCol = colPos;
                    tempRow = rowPos;
                    do
                    {
                        tempCol += i;
                        tempRow += j;
                        if (OutOfBounds(tempRow, tempCol))
                            break;
                        if (pieces[tempRow, tempCol] == null)
                            output.Add(new sbyte[] { tempRow, tempCol });
                        else if (pieces[tempRow, tempCol].side != this.side)
                        {
                            output.Add(new sbyte[] { tempRow, tempCol });
                            break;
                        }
                        else break;
                    }
                    while (tempCol < ChessGame.colSize - 1 && tempCol > 0 && tempRow < ChessGame.rowSize - 1 && tempRow > 0);
                }
            }

            return output;
        }
    }

    //*************************************************************************
    /// <summary>
    /// The Queen
    /// </summary>
    internal class Queen : Piece
    {
        //Constructor
        public Queen(ref Piece[,] pieces, sbyte rowPos, sbyte colPos, bool side) : base(ref pieces, rowPos, colPos, side)
        {
        }

        /// <summary>
        /// Returns a list of all the spots where the Queen can move basicly it's a combination of the rook's and the bishop's moveset
        /// </summary>
        /// <returns>a list of cords where it can move</returns>
        public override List<sbyte[]> PossibleMoves()
        {
            List<sbyte[]> output = new List<sbyte[]>();
            sbyte tempRow = base.rowPos;
            sbyte tempCol = base.colPos;

            for (sbyte i = -1; i < 2; i += 2)
            {
                for (sbyte j = -1; j < 2; j += 2)
                {
                    tempCol = colPos;
                    tempRow = rowPos;
                    do
                    {
                        tempCol += i;
                        tempRow += j;
                        if (OutOfBounds(tempRow, tempCol))
                            break;
                        if (pieces[tempRow, tempCol] == null)
                            output.Add(new sbyte[] { tempRow, tempCol });
                        else if (pieces[tempRow, tempCol].side != this.side)
                        {
                            output.Add(new sbyte[] { tempRow, tempCol });
                            break;
                        }
                        else break;
                    }
                    while (tempCol < ChessGame.colSize - 1 && tempCol > 0 && tempRow < ChessGame.rowSize - 1 && tempRow > 0);
                }
            }

            //Checks the left way
            for (sbyte i = (sbyte)(base.colPos - 1); i >= 0; i--)
            {
                if (pieces[rowPos, i] != null)
                {
                    //if it can attack
                    if (pieces[rowPos, i].side != this.side)
                        output.Add(new sbyte[] { rowPos, (sbyte)(i) });
                    break;
                }
                output.Add(new sbyte[] { rowPos, (sbyte)(i) });
            }
            //Checks upwards
            for (sbyte i = (sbyte)(base.rowPos - 1); i >= 0; i--)
            {
                //if it can attack
                if (pieces[i, colPos] != null)
                {
                    if (pieces[i, colPos].side != this.side)
                        output.Add(new sbyte[] { (sbyte)(i), colPos });
                    break;
                }
                output.Add(new sbyte[] { (sbyte)(i), colPos });
            }
            //Checks the right way
            for (sbyte i = (sbyte)(base.colPos + 1); i < ChessGame.colSize; i++)
            {
                //if it can attack
                if (pieces[rowPos, i] != null)
                {
                    if (pieces[rowPos, i].side != this.side)
                        output.Add(new sbyte[] { rowPos, (sbyte)(i) });

                    break;
                }
                output.Add(new sbyte[] { rowPos, (sbyte)(i) });
            }
            //Checks downwards
            for (sbyte i = (sbyte)(base.rowPos + 1); i < ChessGame.rowSize; i++)
            {
                //if it can attack
                if (pieces[i, colPos] != null)
                {
                    if (pieces[i, colPos].side != this.side)
                        output.Add(new sbyte[] { (sbyte)(i), colPos });
                    break;
                }
                output.Add(new sbyte[] { (sbyte)(i), colPos });
            }
            return output;
        }
    }

    //*************************************************************************
    /// <summary>
    /// The King
    /// </summary>
    internal class King : Piece
    {
        public King(ref Piece[,] pieces, sbyte rowPos, sbyte colPos, bool side) : base(ref pieces, rowPos, colPos, side)
        {
        }

        /// <summary>
        /// Returns a list of all the spots where the Bishop can move
        /// </summary>
        /// <returns>a list of cords where it can move</returns>
        public override List<sbyte[]> PossibleMoves()
        {
            List<sbyte[]> output = new List<sbyte[]>();
            for (sbyte i = -1; i < 2; i++)
            {
                for (sbyte j = -1; j < 2; j++)
                {
                    sbyte tempRow = (sbyte)(rowPos + i);
                    sbyte tempCol = (sbyte)(colPos + j);
                    if ((tempRow == rowPos && tempCol == colPos) || tempCol >= ChessGame.colSize || tempCol < 0 || tempRow >= ChessGame.rowSize || tempRow < 0)
                        continue;
                    if (pieces[tempRow, tempCol] == null)
                        output.Add(new sbyte[] { tempRow, tempCol });
                    else if (pieces[tempRow, tempCol].side != this.side)
                        output.Add(new sbyte[] { tempRow, tempCol });
                }
            }
            return output;
        }
    }
}