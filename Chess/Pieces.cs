using System;
using System.Collections.Generic;
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

        //The pieces on the board
        protected Piece[,] pieces;

        //Constructor
        public Piece(ref Piece[,] pieces, sbyte rowPos, sbyte colPos, bool side)
        {
            this.rowPos = rowPos;
            this.colPos = colPos;
            this.side = side;
            this.pieces = pieces;
        }

        //****************************************************************
        //Private methods
        private bool IsLegalMove(sbyte destRow, sbyte destCol)
        {
            sbyte row = (sbyte)(destRow + rowPos);
            sbyte col = (sbyte)(destCol + colPos);

            foreach (var item in this.PossibleMoves(false))
            {
                if (item[0] == row && item[1] == col)
                {
                    return true;
                }
            }
            return false;
        }

        //****************************************************************
        //Protected Methods
        //Test the king's
        protected List<sbyte[]> TestForKingSafety(List<sbyte[]> unTestedMoves)
        {
            List<sbyte[]> testedMoves = new List<sbyte[]>();
            //Finds the allied king's position
            King alliedKing = null;
            for (sbyte i = 0; i < ChessGame.rowSize; i++)
            {
                for (sbyte j = 0; j < ChessGame.colSize; j++)
                {
                    if (pieces[i, j] != null && pieces[i, j].side == this.side && pieces[i, j] is King)
                        alliedKing = pieces[i, j] as King;
                }
            }
            if (alliedKing == null)
                throw new Exception("Where did your king go seriously");

            //Go through every move and test if the outcome would lead to the king being in chess
            foreach (var item in unTestedMoves)
            {
                Piece save = pieces[item[0], item[1]];
                pieces[item[0], item[1]] = this;
                pieces[this.rowPos, this.colPos] = null;
                sbyte rowSave = this.rowPos;
                sbyte colSave = this.colPos;

                this.rowPos = item[0];
                this.colPos = item[1];
                if (!alliedKing.IsKingInDanger())
                {
                    testedMoves.Add(item);
                }

                pieces[item[0], item[1]] = save;
                this.rowPos = rowSave;
                this.colPos = colSave;
                pieces[this.rowPos, this.colPos] = this;
            }
            return testedMoves;
        }

        //--------------------------------------------------------
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

        //***********************************************************
        //Public Methods
        /// <summary>
        /// Every Piece has it's own movement type
        /// </summary>
        /// <param name="destRow">the row where it should go</param>
        /// <param name="destCol">the column where it should go</param>
        public virtual async Task Move(sbyte destRow, sbyte destCol)
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
        public virtual List<sbyte[]> PossibleMoves(bool kingMoveTest = false, bool kingIsInDanger = false)
        {
            throw new NotImplementedException();
        }
    }

    //*************************************************************************
    /// <summary>
    /// The pawn
    /// </summary>
    internal class Pawn : Piece
    {
        private bool firstMove = true;

        public Pawn(ref Piece[,] pieces, sbyte rowPos, sbyte colPos, bool side) : base(ref pieces, rowPos, colPos, side)
        {
        }

        /// <summary>
        /// Returns all the possible spots where the pawn can move
        /// </summary>
        /// <returns>list of all the cords</returns>
        public override List<sbyte[]> PossibleMoves(bool kingMoveTest = false, bool kingIsInDanger = false)
        {
            List<sbyte[]> output = new List<sbyte[]>();
            if (rowPos + (side ? -1 : 1) > ChessGame.rowSize || rowPos + (side ? -1 : 1) < 0)
                return output;
            if (!kingMoveTest && pieces[rowPos + (side ? -1 : 1), colPos] == null)
                output.Add(new sbyte[] { (sbyte)(rowPos + (side ? -1 : 1)), colPos });
            if (!kingMoveTest && output.Count != 0 && firstMove && pieces[rowPos + (side ? -2 : 2), colPos] == null)
                output.Add(new sbyte[] { (sbyte)(rowPos + (side ? -2 : 2)), colPos });
            if (colPos + 1 < ChessGame.colSize && (kingMoveTest || pieces[rowPos + (side ? -1 : 1), colPos + 1] != null && pieces[rowPos + (side ? -1 : 1), colPos + 1].side != side))
                output.Add(new sbyte[] { (sbyte)(rowPos + (side ? -1 : 1)), (sbyte)(colPos + 1) });
            if (colPos - 1 >= 0 && (kingMoveTest || pieces[rowPos + (side ? -1 : 1), colPos - 1] != null && pieces[rowPos + (side ? -1 : 1), colPos - 1].side != side))
                output.Add(new sbyte[] { (sbyte)(rowPos + (side ? -1 : 1)), (sbyte)(colPos - 1) });

            if (kingIsInDanger)
                output = TestForKingSafety(output);
            return output;
        }

        /// <summary>
        /// The movement of the pawn
        /// </summary>
        /// <param name="destRow">how much it should go forward</param>
        /// <param name="destCol">how much it should go sideways</param>
        public override async Task Move(sbyte destRow, sbyte destCol)
        {
            await base.Move(destRow, destCol);
            firstMove = false;
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
        public override List<sbyte[]> PossibleMoves(bool kingMoveTest = false, bool kingIsInDanger = false)
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
                    if (!OutOfBounds(tempRow, tempCol) && (kingMoveTest || (pieces[tempRow, tempCol] == null || pieces[tempRow, tempCol].side != this.side)))
                        output.Add(new sbyte[] { tempRow, tempCol });
                }
            }
            if (kingIsInDanger)
                output = TestForKingSafety(output);
            return output;
        }
    }

    //*************************************************************************
    /// <summary>
    /// The Rook
    /// </summary>
    internal class Rook : Piece
    {
        internal bool firstMove = true;

        //Constructor
        public Rook(ref Piece[,] pieces, sbyte rowPos, sbyte colPos, bool side) : base(ref pieces, rowPos, colPos, side)
        {
        }

        /// <summary>
        /// Returns a list of all the spots where the Rook can move (moves horizontally and vertically)
        /// </summary>
        /// <returns>a list of cords where it can move</returns>
        public override List<sbyte[]> PossibleMoves(bool kingMoveTest = false, bool kingIsInDanger = false)
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
                    if (kingMoveTest || pieces[rowPos, i].side != this.side)
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
                    if (kingMoveTest || pieces[i, colPos].side != this.side)
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
                    if (kingMoveTest || pieces[rowPos, i].side != this.side)
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
                    if (kingMoveTest || pieces[i, colPos].side != this.side)
                        output.Add(new sbyte[] { (sbyte)(i), colPos });
                    break;
                }
                output.Add(new sbyte[] { (sbyte)(i), colPos });
            }
            if (kingIsInDanger)
                output = TestForKingSafety(output);
            return output;
        }

        /// <summary>
        /// The movement of the rook
        /// </summary>
        /// <param name="destRow">how much it should go forward</param>
        /// <param name="destCol">how much it should go sideways</param>
        public override async Task Move(sbyte destRow, sbyte destCol)
        {
            await base.Move(destRow, destCol);
            firstMove = false;
        }

        /// <summary>
        /// This will only be called when the rook and king makes a swap
        /// </summary>
        /// <param name="destCol">the column where the rook will go</param>
        internal void SwapMove(sbyte destCol)
        {
            if (!firstMove || pieces[rowPos, destCol] != null)
                throw new Exception("Error at SwapMove");

            pieces[rowPos, colPos] = null;
            this.colPos = destCol;
            pieces[rowPos, colPos] = this;
            this.firstMove = false;
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
        public override List<sbyte[]> PossibleMoves(bool kingMoveTest = false, bool kingIsInDanger = false)
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
                        else if (kingMoveTest || pieces[tempRow, tempCol].side != this.side)
                        {
                            output.Add(new sbyte[] { tempRow, tempCol });
                            break;
                        }
                        else break;
                    }
                    while (tempCol < ChessGame.colSize - 1 && tempCol > 0 && tempRow < ChessGame.rowSize - 1 && tempRow > 0);
                }
            }
            if (kingIsInDanger)
                output = TestForKingSafety(output);
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
        public override List<sbyte[]> PossibleMoves(bool kingMoveTest = false, bool kingIsInDanger = false)
        {
            List<sbyte[]> output = new List<sbyte[]>();
            output.AddRange(new Rook(ref pieces, rowPos, colPos, side).PossibleMoves());
            output.AddRange(new Bishop(ref pieces, rowPos, colPos, side).PossibleMoves());

            /*  if (kingIsInDanger)
                  output = TestForKingSafety(output);*/
            return output;
        }
    }

    //*************************************************************************
    /// <summary>
    /// The King
    /// </summary>
    internal class King : Piece
    {
        /// <summary>
        /// is the king is in danger
        /// </summary>
        internal bool isThreatened = false;

        /// <summary>
        /// determines who won if the game is over
        /// 0 is still in progress 1 is lose 2 is draw
        /// </summary>
        public byte gameOver { get; private set; }

        //A bool which stores if the king has moved yet
        internal bool firstMove = true;

        public King(ref Piece[,] pieces, sbyte rowPos, sbyte colPos, bool side) : base(ref pieces, rowPos, colPos, side)
        {
            this.gameOver = 0;
        }

        //-----------------------------------------------------------------------
        //Gets the enemy pieces possible moves so we can filter the king's movement by them
        private List<sbyte[]> GetOtherPlayersEveryMove()
        {
            List<sbyte[]> output = new List<sbyte[]>();
            for (int i = 0; i < ChessGame.rowSize; i++)
            {
                for (int j = 0; j < ChessGame.colSize; j++)
                {
                    if (pieces[i, j] != null && pieces[i, j].side != this.side)
                    {
                        output.AddRange(pieces[i, j].PossibleMoves(true));
                    }
                }
            }
            return output;
        }

        //--------------------------------------------------------------------------
        //Checks if the king can be attacked or not
        public bool IsKingInDanger()
        {
            List<sbyte[]> enemyMoves = GetOtherPlayersEveryMove();
            foreach (var item in enemyMoves)
            {
                if (item[0] == rowPos && item[1] == colPos)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a list of all the spots where the Bishop can move
        /// </summary>
        /// <returns>a list of cords where it can move</returns>
        public override List<sbyte[]> PossibleMoves(bool kingMoveTest = false, bool kingIsInDanger = false)
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

            //Rook - King swap move
            if (!kingMoveTest && firstMove)
            {
                for (sbyte i = (sbyte)(this.colPos - 1); i >= 0; i--)
                {
                    if (pieces[rowPos, i] is Rook && (pieces[rowPos, i] as Rook).firstMove && i == 0)
                        output.Add(new sbyte[] { rowPos, 1 });
                    else if (pieces[rowPos, i] != null)
                        break;
                }
                for (sbyte i = (sbyte)(this.colPos + 1); i < ChessGame.colSize; i++)
                {
                    if (pieces[rowPos, i] is Rook && (pieces[rowPos, i] as Rook).firstMove && i == ChessGame.colSize - 1)
                        output.Add(new sbyte[] { rowPos, 6 });
                    else if (pieces[rowPos, i] != null)
                        break;
                }
            }

            //this if is needed here if you remove it you will hate yourself
            //btw it is needed here so we don't get into an infinite loop
            if (!kingMoveTest)
            {
                //Gets the other player's every move
                List<sbyte[]> possibleEnemyMoves = GetOtherPlayersEveryMove();
                foreach (var enemyMove in possibleEnemyMoves)
                {
                    for (int i = 0; i < output.Count; i++)
                    {
                        if (output[i][0] == enemyMove[0] && output[i][1] == enemyMove[1])
                        {
                            output.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
            if (kingIsInDanger)
                output = TestForKingSafety(output);
            return output;
        }

        /// <summary>
        /// The movement of the king
        /// </summary>
        /// <param name="destRow">how much it should go forward</param>
        /// <param name="destCol">how much it should go sideways</param>
        public override async Task Move(sbyte destRow, sbyte destCol)
        {
            await base.Move(destRow, destCol);
            if (Math.Abs(destCol) > 1)
            {
                Rook rookToSwapWith = pieces[rowPos, (colPos - 1 + colPos / 3)] as Rook;
                rookToSwapWith.SwapMove((sbyte)(colPos + 1 - colPos / 3));
            }
            this.firstMove = false;
            this.isThreatened = false;
        }
    }
}