using System;

namespace Blitz2022
{
    public class Map
    {
        public string[][] tiles;
        public Diamond[] diamonds;

        public void UpdateValues(Map map)
        {
            this.tiles = map.tiles;
            this.diamonds = map.diamonds;
        }

        public class Diamond
        {
            public Position position;
            public string id;
            public int summonLevel;
            public int points;
            public string ownerId;
            public bool isAcessible;

            public Diamond()
            {
                isAcessible = true;
            }


            public string allyTargetingId;
        }

        public class Position
        {
            public Position(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public int x;
            public int y;

            public override string ToString()
            {
                return string.Format("P({0},{1})", this.x, this.y);
            }
        }

        public enum TileType
        {
            EMPTY, WALL, SPAWN
        }

        public class PointOutOfMapException : Exception
        {
            public PointOutOfMapException(Position position, int horizontalSize, int verticalSize)
            : base(String.Format("Point {0} is out of map, x and y must be greater than 0 and x less than {1} and y less than {2}.", position, horizontalSize, verticalSize))
            { }
        }

        public int horizontalSize()
        {
            return this.tiles.Length;
        }

        public int verticalSize()
        {
            return this.tiles[0].Length;
        }

        public Diamond getDiamondById(string id)
        {
            foreach(Diamond diamond in this.diamonds)
            {
                if(diamond.id == id)
                    return diamond;
            }

            return null;
        }

        public Diamond getDiamondByPos(Position pos)
        {
            foreach(Diamond d in this.diamonds)
            {
                if (d.position.x == pos.x && d.position.y == pos.y)
                    return d;
            }

            return null;
        }

        public TileType getTileTypeAt(Position position)
        {
            string rawTile = this.getRawTileValueAt(position);

            switch (rawTile)
            {
                case "EMPTY":
                    return TileType.EMPTY;
                case "WALL":
                    return TileType.WALL;
                case "SPAWN":
                    return TileType.SPAWN;
                default:
                    throw new ArgumentException(String.Format("'{0}' is not a valid tile", rawTile));
            }
        }

        public String getRawTileValueAt(Position position)
        {
            this.validateTileExists(position);
            return this.tiles[position.x][position.y];
        }

        public void validateTileExists(Position position)
        {
            if (position.x < 0 || position.y < 0 || position.x >= this.horizontalSize() || position.y >= this.verticalSize())
            {
                throw new PointOutOfMapException(position, this.horizontalSize(), this.verticalSize());
            }
        }

        public bool doesTileExists(Position position)
        {
            if (position.x < 0 || position.y < 0 || position.x >= this.horizontalSize() || position.y >= this.verticalSize())
            {
                return false;
            }
            return true;
        }
    }
}