using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAdventure
{
    public class MazeGenerator
    {
        private int _width, _height;
        private bool[,] _maze;
        private List<(int X, int Y)> _walls = new();

        public MazeGenerator(int width, int height)
        {
            _width = width;
            _height = height;
            _maze = new bool[_width, _height];
        }

        public bool[,] GenerateMaze()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _maze[x, y] = false;
                }
            }

            Random rand = new Random();
            int startX = rand.Next(_width);
            int startY = rand.Next(_height);
            _maze[startX, startY] = true;

            AddAdjacentWalls(startX, startY);

            while (_walls.Count > 0)
            {
                int randomWallIndex = rand.Next(_walls.Count);
                var (x, y) = _walls[randomWallIndex];
                _walls.RemoveAt(randomWallIndex);

                if (CanCarvePassage(x, y))
                {
                    _maze[x, y] = true;
                    AddAdjacentWalls(x, y);
                }
            }

            return _maze;
        }

        private void AddAdjacentWalls(int x, int y)
        {
            if (x > 1) _walls.Add((x - 1, y));
            if (y > 1) _walls.Add((x, y - 1));
            if (x < _width - 2) _walls.Add((x + 1, y));
            if (y < _height - 2) _walls.Add((x, y + 1));
        }

        private bool CanCarvePassage(int x, int y)
        {
            int passageCount = 0;
            if (x > 0 && _maze[x - 1, y]) passageCount++;
            if (y > 0 && _maze[x, y - 1]) passageCount++;
            if (x < _width - 1 && _maze[x + 1, y]) passageCount++;
            if (y < _height - 1 && _maze[x, y + 1]) passageCount++;

            return passageCount == 1;
        }
    }

}
