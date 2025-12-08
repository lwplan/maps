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

        [Theory]
        [InlineData(Neighbor8.North | Neighbor8.East, Neighbor8.NorthEast, Rotation.R0)]
        [InlineData(Neighbor8.East | Neighbor8.South, Neighbor8.SouthEast, Rotation.R90)]
        [InlineData(Neighbor8.South | Neighbor8.West, Neighbor8.SouthWest, Rotation.R180)]
        [InlineData(Neighbor8.West | Neighbor8.North, Neighbor8.NorthWest, Rotation.R270)]
        public void ClassifyPavingPattern_InnerCorner_UsesMissingDiagonal(Neighbor8 cardinals, Neighbor8 missingDiagonal, Rotation expectedRotation)
        {
            var mask = cardinals & ~missingDiagonal;

            var (pattern, rotation) = TileClassifier.ClassifyPavingPattern(mask);

            Assert.Equal(PavingPattern.InnerCorner, pattern);
            Assert.Equal(expectedRotation, rotation);
        }

        [Theory]
        [InlineData(Neighbor8.NorthEast, Rotation.R0)]
        [InlineData(Neighbor8.SouthEast, Rotation.R90)]
        [InlineData(Neighbor8.SouthWest, Rotation.R180)]
        [InlineData(Neighbor8.NorthWest, Rotation.R270)]
        public void ClassifyPavingPattern_OuterCorner_UsesSingleDiagonal(Neighbor8 mask, Rotation expectedRotation)
        {
            var (pattern, rotation) = TileClassifier.ClassifyPavingPattern(mask);

            Assert.Equal(PavingPattern.OuterCorner, pattern);
            Assert.Equal(expectedRotation, rotation);
        }

        [Theory]
        [InlineData(Neighbor8.NorthWest | Neighbor8.NorthEast, Rotation.R0)]
        [InlineData(Neighbor8.NorthEast | Neighbor8.SouthEast, Rotation.R90)]
        [InlineData(Neighbor8.SouthEast | Neighbor8.SouthWest, Rotation.R180)]
        [InlineData(Neighbor8.SouthWest | Neighbor8.NorthWest, Rotation.R270)]
        public void ClassifyPavingPattern_EdgeStrip_UsesDiagonalPair(Neighbor8 mask, Rotation expectedRotation)
        {
            var (pattern, rotation) = TileClassifier.ClassifyPavingPattern(mask);

            Assert.Equal(PavingPattern.EdgeStrip, pattern);
            Assert.Equal(expectedRotation, rotation);
        }

        [Theory]
        [InlineData(Neighbor8.North | Neighbor8.NorthEast, Rotation.R0)]
        [InlineData(Neighbor8.East | Neighbor8.SouthEast, Rotation.R90)]
        [InlineData(Neighbor8.South | Neighbor8.SouthWest, Rotation.R180)]
        [InlineData(Neighbor8.West | Neighbor8.NorthWest, Rotation.R270)]
        public void ClassifyPavingPattern_ChamferedEdge_UsesCardinalAndDiagonal(Neighbor8 mask, Rotation expectedRotation)
        {
            var (pattern, rotation) = TileClassifier.ClassifyPavingPattern(mask);

            Assert.Equal(PavingPattern.ChamferedEdge, pattern);
            Assert.Equal(expectedRotation, rotation);
        }
    }
}
