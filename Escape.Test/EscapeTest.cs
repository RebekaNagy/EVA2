using System;
using System.Runtime.CompilerServices;
using Escape.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Escape.Persistence;
using Moq;
using System.Threading.Tasks;

namespace Escape.Test
{
    [TestClass]
    public class EscapeTest
    {
        private EscapeModel model;
        private PrivateObject wrappedModel;
        private Mock<IPersistence> mock;

        [TestInitialize]
        public void Initialize()
        {
            mock = new Mock<IPersistence>();
            mock.Setup(mock => mock.Load(It.IsAny<String>())).Returns(() => Task.FromResult<object>(null));
            mock.Setup(mock => mock.Save(It.IsAny<String>(), 0, null, 0, 0)).Returns(() => Task.FromResult<object>(null));

            model = new EscapeModel(mock.Object);
            wrappedModel = new PrivateObject(model);

        }

        [TestMethod]
        public void EscapeLoadGameTest()
        {
            model.Load(String.Empty).Wait();
            mock.Verify(mock => mock.Load(It.IsAny<String>()), Times.Once());
        }

        [TestMethod]
        public void EscapeNewGameTest()
        {
            model.NewGame(15);

            Assert.AreEqual(model.Size, 15);

            Assert.AreEqual(model.Player.Tile, model.Tiles[0, 7]);

            Assert.AreEqual(model.Enemies[0].Tile, model.Tiles[14, 0]);

            Assert.AreEqual(model.Enemies[1].Tile, model.Tiles[14, 14]);

            Assert.AreEqual(model.Paused, false);

        }

        [TestMethod]
        public void EscapeMovePlayerTest()
        {
            model.NewGame(11);

            Assert.AreEqual(model.Player.Tile, model.Tiles[0, 5]);

            model.MovePlayer(Direction.Up);

            Assert.AreEqual(model.Player.Tile, model.Tiles[0, 5]);

            model.MovePlayer(Direction.Down);

            Assert.AreEqual(model.Player.Tile, model.Tiles[1, 5]);

            model.MovePlayer(Direction.Left);

            Assert.AreEqual(model.Player.Tile, model.Tiles[1, 4]);

            model.MovePlayer(Direction.Right);

            Assert.AreEqual(model.Player.Tile, model.Tiles[1, 5]);
        }

        [TestMethod]
        public void EscapeMoveEnemiesTest()
        {
            model.NewGame(11);
            wrappedModel.Invoke("MoveEnemies");

            Assert.AreEqual(model.Enemies[0].Tile, model.Tiles[9, 0]);
            Assert.AreEqual(model.Enemies[1].Tile, model.Tiles[9, 10]);

            wrappedModel.Invoke("MoveEnemies");

            Assert.AreEqual(model.Enemies[0].Tile, model.Tiles[8, 0]);
            Assert.AreEqual(model.Enemies[1].Tile, model.Tiles[8, 10]);
        }

        [TestMethod]
        public void EscapeGameWonTest()
        {
            model.NewGame(11);

            model.Tiles[7, 0].Type = TileType.Mine;
            model.Tiles[7, 10].Type = TileType.Mine;

            bool eventRaised = false;
            model.GameOver += delegate (object sender, GameOverEventArgs e) {
                eventRaised = true;
            };
            wrappedModel.Invoke("MoveEnemies");
            wrappedModel.Invoke("MoveEnemies");
            wrappedModel.Invoke("MoveEnemies");
            wrappedModel.Invoke("MoveEnemies");

            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void EscapeGameOverTest()
        {
            model.NewGame(11);

            model.Tiles[0, 4].Type = TileType.Mine;

            bool eventRaised = false;
            model.GameOver += delegate (object sender, GameOverEventArgs e) {
                eventRaised = true;
            };

            model.MovePlayer(Direction.Left);

            Assert.IsTrue(eventRaised);

        }
    }
}
