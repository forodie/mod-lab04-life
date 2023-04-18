using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using cli_life;

namespace LifeTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void InitializeBoard()
        {
            Board board = new Board(50, 20, 1, 0.5);
            Assert.AreEqual(20, board.Rows);
            Assert.AreEqual(50, board.Columns);
        }

        [TestMethod]
        public void DefaultState()
        {
            Cell cell = new Cell();
            Assert.IsFalse(cell.IsAlive);
        }

        [TestMethod]
        public void Randomize()
        {
            Board board = new Board(50, 20, 1, 0);
            board.Randomize(1);
            int liveCells = 0;
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    if (board.Cells[col, row].IsAlive)
                    {
                        liveCells++;
                    }
                }
            }
            Assert.IsTrue(liveCells > 0);
        }

        [TestMethod]
        public void ConnectNeighbors()
        {
            Board board = new Board(50, 20, 1, 0.5);
            Cell cell = board.Cells[0, 0];
            int neighborCount = cell.neighbors.Count;
            Assert.AreEqual(8, neighborCount);
        }

        [TestMethod]
        public void TestInitialBoard_AllAlive()
        {
            int width = 4;
            int height = 4;
            Board board = new Board(width, height, 1, 1.0);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    Assert.IsTrue(board.Cells[col, row].IsAlive, $"Cell at ({col}, {row}) is not alive.");
                }
            }
        }
    }
}