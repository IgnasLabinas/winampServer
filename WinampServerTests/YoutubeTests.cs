using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinampServer.Handlers;
using WinampServer.Models;

namespace WinampServerTests
{
    [TestClass]
    public class YoutubeTests
    {
        [TestMethod]
        public void FetchSongTest()
        {
            Song song = Youtube.FetchSong("XvdATQf6wtw");

            Assert.IsNotNull(song);
            Assert.AreEqual(257, song.Length);
        }

        [TestMethod]
        public void FindNextSongKeyTest()
        {
            string songKey1 = "XvdATQf6wtw";
            string songKey2 = Youtube.FindNextSongKey(songKey1);
            string songKey3 = Youtube.FindNextSongKey(songKey1);

            Assert.AreNotEqual(songKey2, songKey3);
        }
    }
}
