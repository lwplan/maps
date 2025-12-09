using Xunit;
using maps.Map3D;
using System.Text;

namespace Map3D.Tests
{
    public class TestMapGeneratorTest
    {
        [Fact]
        public void GenerateTestMap_Creates16by16Grid()
        {
            var tiles = TestMapGenerator.GenerateTestMap();
            Assert.Equal(16, tiles.GetLength(0));
            Assert.Equal(16, tiles.GetLength(1));

            var sb = new StringBuilder();
            for (int y = 15; y >= 0; y--)
            {
                for (int x = 0; x < 16; x++)
                    sb.Append(tiles[x, y].IsPaved ? '#' : '.');
                sb.AppendLine();
            }
            // Just log it
            System.Console.WriteLine(sb.ToString());
            // Example assertion
            Assert.True(tiles[8, 8].IsPaved);
        }
    }
}
