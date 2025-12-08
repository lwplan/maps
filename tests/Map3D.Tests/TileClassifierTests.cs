using maps.Map3D;
using Xunit;

namespace Map3D.Tests
{
    public class TileClassifierTests
    {
        [Theory]
        [InlineData(Neighbor8.North, Rotation.R0)]
        [InlineData(Neighbor8.East, Rotation.R90)]
        [InlineData(Neighbor8.South, Rotation.R180)]
        [InlineData(Neighbor8.West, Rotation.R270)]
        public void ClassifyPavingPattern_End_UsesSingleNeighbor(Neighbor8 mask, Rotation expectedRotation)
        {
            var (pattern, rotation) = TileClassifier.ClassifyPavingPattern(mask);

            Assert.Equal(PavingPattern.End, pattern);
            Assert.Equal(expectedRotation, rotation);
        }

        [Theory]
        [InlineData(Neighbor8.East | Neighbor8.West | Neighbor8.South, Rotation.R0)]
        [InlineData(Neighbor8.North | Neighbor8.South | Neighbor8.West, Rotation.R90)]
        [InlineData(Neighbor8.East | Neighbor8.West | Neighbor8.North, Rotation.R180)]
        [InlineData(Neighbor8.North | Neighbor8.South | Neighbor8.East, Rotation.R270)]
        public void ClassifyPavingPattern_TJunction_UsesThreeNeighbors(Neighbor8 mask, Rotation expectedRotation)
        {
            var (pattern, rotation) = TileClassifier.ClassifyPavingPattern(mask);

            Assert.Equal(PavingPattern.TJunction, pattern);
            Assert.Equal(expectedRotation, rotation);
        }
    }
}
