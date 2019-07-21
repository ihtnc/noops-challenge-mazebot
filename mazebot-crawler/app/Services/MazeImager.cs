using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Shapes;
using SixLabors.Primitives;
using MazebotCrawler.Services.Models;

namespace MazebotCrawler.Services
{
    public interface IMazeImager
    {
        Image<Rgb24> GetImage(Map map, int startX, int startY, string solution, bool includeSolution);
    }

    public class MazeImager : IMazeImager
    {
        const int FRAMES_PER_SEGMENT = 4;

        public Image<Rgb24> GetImage(Map map, int startX, int startY, string solution, bool includeSolution)
        {
            if (map.FloorPlan?.Any() != true) { return null; }

            var blockHeight = GetBlockHeight(map);
            var blockWidth = GetBlockWidth(map);
            var lineWidth = GetLineWidth(map);
            var image = GetMapImage(map);
            if (!includeSolution) { return image; }

            var currentX = startX;
            var currentY = startY;

            foreach(var direction in solution)
            {
                var midX = (float)(currentX + 0.5) * blockWidth;
                var midY = (float)(currentY + 0.5) * blockHeight;
                var xOffset = 0;
                var yOffset = 0;

                switch (direction)
                {
                    case (char)Direction.North:
                        yOffset = -1 * (blockHeight / FRAMES_PER_SEGMENT);
                        currentY--;
                        break;
                    case (char)Direction.South:
                        yOffset = 1 * (blockHeight / FRAMES_PER_SEGMENT);
                        currentY++;
                        break;
                    case (char)Direction.East:
                        xOffset = 1 * (blockWidth / FRAMES_PER_SEGMENT);
                        currentX++;
                        break;
                    case (char)Direction.West:
                        xOffset = -1 * (blockWidth / FRAMES_PER_SEGMENT);
                        currentX--;
                        break;
                }

                AddPath(image, new PointF(midX, midY), xOffset, yOffset, lineWidth);
            }

            return image;
        }

        private int GetBlockWidth(Map map)
        {
            var width = map.FloorPlan.First().Length;

            if (width < 20) { return 50; }
            else if (width < 40) { return 35; }
            else if (width < 60) { return 18; }
            else if (width < 80) { return 12; }
            else { return 10; }
        }

        private int GetBlockHeight(Map map)
        {
            var height = map.FloorPlan.Length;
            if (height < 20) { return 50; }
            else if (height < 40) { return 35; }
            else if (height < 60) { return 18; }
            else if (height < 80) { return 12; }
            else { return 10; }
        }

        private int GetLineWidth(Map map)
        {
            var height = map.FloorPlan.Length;
            if (height < 20) { return 30; }
            else if (height < 40) { return 20; }
            else if (height < 60) { return 7; }
            else if (height < 80) { return 5; }
            else { return 4; }
        }

        private Image<Rgb24> GetMapImage(Map map)
        {
            var blockHeight = GetBlockHeight(map);
            var blockWidth = GetBlockWidth(map);
            var maxY = map.FloorPlan.Length;
            var maxX = map.FloorPlan.First().Length;
            var width = maxX * blockWidth;
            var height = maxY * blockHeight;

            var image = new Image<Rgb24>(width, height);

            for (var y = 0; y < map.FloorPlan.Length; y++)
            {
                for (var x = 0; x < map.FloorPlan[y].Length; x++)
                {
                    Rgb24 color;
                    var current = map.FloorPlan[y][x];
                    if (current == Map.OCCPD) { color = NamedColors<Rgb24>.Black; }
                    else if (current == Map.START) { color = NamedColors<Rgb24>.Red; }
                    else if (current == Map.DESTN) { color = NamedColors<Rgb24>.GreenYellow; }
                    else { color = NamedColors<Rgb24>.White; }

                    var tileX = x * blockWidth;
                    var tileY = y * blockHeight;

                    var tile = new RectangularPolygon(tileX, tileY, blockWidth, blockHeight);
                    image.Mutate(c => c.Fill(color, tile));
                }
            }

            return image;
        }

        private Image<Rgb24> AddPath(Image<Rgb24> image, PointF start, float xOffset, float yOffset, int lineWidth)
        {
            var currentX = start.X;
            var currentY = start.Y;
            for(var i = 0; i < FRAMES_PER_SEGMENT; i++)
            {
                var frame = image.Frames.CloneFrame(image.Frames.Count - 1);
                var newX = currentX + xOffset;
                var newY = currentY + yOffset;
                frame.Mutate(c => c.DrawLines(NamedColors<Rgb24>.Red, lineWidth, new PointF(currentX, currentY), new PointF(newX, newY)));
                currentX = newX;
                currentY = newY;
                image.Frames.AddFrame(frame.Frames.First());
            }

            return image;
        }
    }
}