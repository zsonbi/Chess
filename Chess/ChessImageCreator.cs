using System.Drawing;
using System.Threading.Tasks;

namespace Chess
{
    internal abstract class ChessImageCreator
    {
        public static short width = 2000; //The width of the Image the class will return
        public static short height = 2000; //The height of the Image the class will return
        private static sbyte rowCount = 10; //The number of squares each col will have
        private static sbyte colCount = 10; //The number of squares each row will have
        private const bool saveImage = true; //mainly for debug

        //**********************************************************************************
        //This will handle a single square
        //Needs a reference to the Bitmap the method will return
        //Needs the index of the square row,col
        //Needs the string which tells it if there is a piece
        private static async Task FillSquare(Bitmap bmp, sbyte rowIndex, sbyte colIndex, string imageToLoad)
        {
            short maxHeight = (short)(height / rowCount * (rowIndex + 1));//The height of this square
            short maxWidth = (short)(width / colCount * (colIndex + 1));//The width of this square
            short pieceRowIndex = 0;//Separate rowIndex for the piece's picture
            short pieceColIndex = 0;//Separate colIndex for the piece's picture
            Bitmap piecePic = null;//Make an empty Bitmap
            piecePic = new Bitmap(new Bitmap("..\\..\\Pieces\\" + imageToLoad.Replace("_", "") + ".png"), width / colCount, height / rowCount);

            for (short i = (short)(rowIndex * height / rowCount); i < maxHeight; i++)
            {
                for (short j = (short)(colIndex * width / colCount); j < maxWidth; j++)
                {
                    //if it isn't transparent
                    if (piecePic.GetPixel(pieceColIndex, pieceRowIndex).A != 0)
                        //Draws the piece's pixel
                        bmp.SetPixel(j, i, piecePic.GetPixel(pieceColIndex, pieceRowIndex));
                    pieceColIndex++;
                }

                pieceColIndex = 0;
                pieceRowIndex++;
            }
        }

        /// <summary>
        /// Creates a Bitmap out of the board's state which the ChessGame provided
        /// </summary>
        /// <param name="piecePos">provided by ChessGame</param>
        /// <returns></returns>
        public static async Task<Bitmap> CreateImageFromBoard(string[,] piecePos)
        {
            //The code will return this Bitmap
            Bitmap output = new Bitmap(new Bitmap("..\\..\\Labels\\board.png"), width, height);
            for (sbyte i = 1; i < rowCount - 1; i++)
            {
                for (sbyte j = 1; j < colCount - 1; j++)
                {
                    if (piecePos[i - 1, j - 1] != "empty")
                        await FillSquare(output, i, j, piecePos[i - 1, j - 1]);
                }
            }
            if (saveImage)
                output.Save("kep.png");

            return output;
        }
    }
}