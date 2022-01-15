using System;
using System.Collections.Generic;
using System.Linq;
using static Blitz2022.Action;
using static Blitz2022.Map;

namespace Blitz2022
{
    public class Bot
    {
        public static string NAME = "MyBot C♭";

        public Bot()
        {
            // initialize some variables you will need throughout the game here
            Console.WriteLine("Initializing your super mega bot!");
        }

        /*
        * Here is where the magic happens, for now the moves are random. I bet you can do better ;)
        */
        public GameCommand nextMove(GameMessage gameMessage)
        {
            Team myTeam = gameMessage.getTeamsMapById[gameMessage.teamId];

            var unitsByLifeStatus = myTeam.units.GroupBy(unit => unit.hasSpawned).ToDictionary(group => group.Key, group => group.ToList());
            List<Unit> deadUnits = unitsByLifeStatus.ContainsKey(false) ? unitsByLifeStatus[false] : new List<Unit>();
            List<Unit> aliveUnits = unitsByLifeStatus.ContainsKey(true) ? unitsByLifeStatus[true] : new List<Unit>();

            List<Action> actions = new List<Action>();
            actions.AddRange(deadUnits.Select(unit => new Action(UnitActionType.SPAWN, unit.id, findRandomSpawn(gameMessage.map))).ToList<Action>());
            if (gameMessage.tick == gameMessage.totalTick - 2)
            {
                actions.AddRange(aliveUnits.Select(unit => new Action(UnitActionType.DROP, unit.id, new Position(unit.position.x, unit.position.y +1))).ToList<Action>());
            }
                
            else
                actions.AddRange(aliveUnits.Select(unit => new Action(UnitActionType.MOVE, unit.id, getRandomPosition(gameMessage.map.horizontalSize(), gameMessage.map.verticalSize()))).ToList<Action>());

            return new GameCommand(actions);
        }

        private Position findRandomSpawn(Map map)
        {
            List<Position> spawns = new List<Position>();
            int x = 0;
            foreach (string[] tileX in map.tiles)
            {
                int y = 0;
                foreach (string tileY in tileX)
                {
                    var position = new Position(x, y);
                    if (map.getTileTypeAt(position) == TileType.SPAWN)
                    {
                        spawns.Add(position);
                    }
                    y++;
                }
                x++;
            }
            return spawns[new Random().Next(spawns.Count)];
        }

        private Position getRandomPosition(int horizontalSize, int verticalSize)
        {
            Random rand = new Random();
            return new Position(rand.Next(horizontalSize), rand.Next(verticalSize));
        }
    }
}