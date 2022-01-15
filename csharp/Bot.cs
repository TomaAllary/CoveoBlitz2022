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
            try {
                Team myTeam = gameMessage.getTeamsMapById[gameMessage.teamId];

                var unitsByLifeStatus = myTeam.units.GroupBy(unit => unit.hasSpawned).ToDictionary(group => group.Key, group => group.ToList());
                List<Unit> deadUnits = unitsByLifeStatus.ContainsKey(false) ? unitsByLifeStatus[false] : new List<Unit>();
                List<Unit> aliveUnits = unitsByLifeStatus.ContainsKey(true) ? unitsByLifeStatus[true] : new List<Unit>();

                List<Action> actions = new List<Action>();
                actions.AddRange(deadUnits.Select(unit => new Action(UnitActionType.SPAWN, unit.id, findRandomSpawn(gameMessage.map))).ToList<Action>());


                foreach (Unit u in aliveUnits)
                {
                    if (gameMessage.tick == gameMessage.totalTick - 2)
                    {
                        if (u.hasDiamond)
                        {
                            Position dropPos = DropD(u, gameMessage);
                            if(dropPos != null)
                                actions.Add(new Action(UnitActionType.DROP, u.id, dropPos));
                        }
                    }
                    else
                    {
                        if (u.hasDiamond)
                        {
                            //seuil todo: a changer
                            Position dropPos = DropD(u, gameMessage);
                            if (gameMessage.map.getDiamondById(u.diamondId).points > 67 && dropPos != null)
                            {
                                actions.Add(new Action(UnitActionType.DROP, u.id, dropPos));
                            }
                            else
                            {
                                int closestDistance = closestEnnemyDistance(gameMessage, u.position);
                                if (gameMessage.map.getDiamondById(u.diamondId).summonLevel < 5 && closestDistance - 1 > gameMessage.map.getDiamondById(u.diamondId).summonLevel)
                                {
                                    actions.Add(new Action(UnitActionType.SUMMON, u.id));
                                }
                                else
                                {
                                    Position ennemy = closestEnnemies(gameMessage, u.position).First().position;
                                    int xDist = Math.Abs(ennemy.x - u.position.x);
                                    int yDist = Math.Abs(ennemy.y - u.position.y);
                                    int forward = 1;
                                    if (xDist < yDist)
                                    {
                                        forward = (ennemy.x - u.position.x) / xDist;
                                        actions.Add(new Action(UnitActionType.MOVE, u.id, new Position(u.position.x + forward, u.position.y)));
                                    }
                                    else
                                    {
                                        forward = (ennemy.y - u.position.y) / yDist;
                                        actions.Add(new Action(UnitActionType.MOVE, u.id, new Position(u.position.x, u.position.y + forward)));
                                    }

                                }
                            }
                        }
                        else
                        {
                            //kill if ennemy aside..
                            if (closestEnnemyDistance(gameMessage, u.position) == 1)
                            {
                                List<Unit> attackableUnits = closestEnnemies(gameMessage, u.position);
                                
                            }

                            //vine an ennemy with diamond if possible
                            Unit toVine = canVineSomeone(u, gameMessage);
                            if (toVine != null)
                                actions.Add(new Action(UnitActionType.VINE, u.id, toVine.position));

                            //Spot and gove nearest diamond
                            Position target = findNearestDiamonds(gameMessage.map, u);
                            if (target.x == 1000)
                                target = getRandomPosition(gameMessage.map.horizontalSize(), gameMessage.map.verticalSize());

                            actions.Add(new Action(UnitActionType.MOVE, u.id, target));
                        }
                    }
                }

                return new GameCommand(actions);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new GameCommand(new List<Action>());
            }
        }


        private Position DropD(Unit u, GameMessage gameMessage)
        {
            if(gameMessage.map.doesTileExists(new Position(u.position.x, u.position.y + 1)))
                if (gameMessage.map.getTileTypeAt(new Position(u.position.x, u.position.y + 1)) == TileType.EMPTY)
                    return new Position(u.position.x, u.position.y + 1);
            else if (gameMessage.map.doesTileExists(new Position(u.position.x, u.position.y - 1)))
                if (gameMessage.map.getTileTypeAt(new Position(u.position.x, u.position.y - 1)) == TileType.EMPTY)
                    return new Position(u.position.x, u.position.y - 1);
            else if (gameMessage.map.doesTileExists(new Position(u.position.x + 1, u.position.y)))
                if (gameMessage.map.getTileTypeAt(new Position(u.position.x + 1, u.position.y)) == TileType.EMPTY)
                    return new Position(u.position.x + 1, u.position.y);
            else if (gameMessage.map.doesTileExists(new Position(u.position.x - 1, u.position.y)))
                return new Position(u.position.x - 1, u.position.y);

            return null;
        }

        private Unit canVineSomeone(Unit u, GameMessage gameMessage)
        {

            foreach (Team team in gameMessage.teams)
            {
                if (team.id != gameMessage.teamId)
                {
                    foreach (Unit ennemy in team.units)
                    {
                        if (ennemy.hasSpawned)
                        {
                            if (ennemy.position.x == u.position.x || ennemy.position.y == u.position.y)
                            {
                                if (ennemy.hasDiamond)
                                {
                                    //check if vines can go
                                    TileType[] obstacles = getTilesBetweenPos(gameMessage.map, u.position, ennemy.position);
                                    Boolean canVine = true;
                                    foreach (TileType tile in obstacles)
                                    {
                                        if (tile == TileType.SPAWN || tile == TileType.WALL) { canVine = false; break; }
                                    }
                                    if (canVine)
                                    {
                                        return ennemy;
                                    }
                                }
                            }
                        }
                        //for each ennemy
                       
                    }
                }
            }

            return null;
        }


        private Position findNearestDiamonds(Map map, Unit unit)
        {
            Position nearest = new Position(1000, 1000);
            foreach (Diamond diamonds in map.diamonds)
            {
                if (diamonds.ownerId == null && Distance(diamonds.position, unit.position) < Distance(nearest, unit.position))
                    nearest = diamonds.position;
            }
            return nearest;
        }

        private TileType[] getTilesBetweenPos(Map map, Position from, Position target)
        {
            TileType[] tiles = null;
            if(from.y == target.y)
            {
                int distance = Math.Abs(from.x - target.x);
                int Backward = (target.x - from.x) / distance; //give 1 or -1
                tiles = new TileType[distance - 1];
                for(int i = 1; i < tiles.Count(); i++)
                {
                    tiles[i] = map.getTileTypeAt(new Position(from.x + (i * Backward), from.y));
                }
            }
            else if(from.x == target.x)
            {
                int distance = Math.Abs(from.y - target.y);
                int Backward = (target.y - from.y) / distance; //give 1 or -1
                tiles = new TileType[distance - 1];
                for (int i = 1; i < tiles.Count(); i++)
                {
                    tiles[i] = map.getTileTypeAt(new Position(from.x, from.y + (i * Backward)));
                }
            }
            return tiles;
        }

        private int Distance(Position diamond, Position unit)
        {
            return Math.Abs((diamond.x - unit.x)) + Math.Abs((diamond.y - unit.y));
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
                    //if (map.getTileTypeAt(position) == TileType.SPAWN && checkAroundForEMPTY(map, position))
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
        private bool checkAroundForEMPTY(Map map,Position position) 
        {
            bool retour = false;
            Position cmp = position;
            //regarde si la case à gauche est vide
            cmp.x = position.x-1;
            if (map.getTileTypeAt(cmp) == TileType.EMPTY) 
            { 
                retour = true;
            }
            //regarde si la case à droite est vide
            cmp.x = position.x + 1;
            if (map.getTileTypeAt(cmp) == TileType.EMPTY)
            {
                retour = true;
            }
            cmp.x = position.x;
            //regarde si la case en bas est vide
            cmp.y = position.y + 1;
            if (map.getTileTypeAt(cmp) == TileType.EMPTY)
            {
                retour = true;
            }
            //reagarde si la case en haut est vide
            cmp.y = position.y - 1;
            if (map.getTileTypeAt(cmp) == TileType.EMPTY)
            {
                retour = true;
            }
            return retour;
        }

        private Position getRandomPosition(int horizontalSize, int verticalSize)
        {
            Random rand = new Random();
            return new Position(rand.Next(horizontalSize), rand.Next(verticalSize));
        }

       /* private bool doWePlayBeforeThem (string otherTeam, GameMessage gm)
        {
            if (gm.teamPlayOrderings.getByValueKey(gm.teamId) < gm.teamPlayOrderings.getByValueKey(otherTeam))
                return true;
            else
                return false;
        }*/

        //Returns an int with the distance between player and closest ennemies
        private int closestEnnemyDistance(GameMessage gm, Position position)
        {
            int closest = 10000;
            foreach (Team t in gm.teams)
            {
                if (t.id != gm.teamId)
                {
                    foreach (Unit u in t.units)
                    {
                        if (u.hasSpawned)
                        {
                            int current = getAbsoluteDistance(u.position, position);
                            if (current < closest)
                                closest = current;
                        }

                    }
                }
            }
            return closest;
        }

        //Returns a table with all the closest ennemy units 
        private List<Unit> closestEnnemies(GameMessage gm, Position position)
        {
            int closest = 10000;
            List<Unit> units = new List<Unit>();
            foreach (Team t in gm.teams)
            {
                if (t.id != gm.teamId)
                {
                    foreach (Unit u in t.units)
                    {
                        if (u.hasSpawned)
                        {
                            int current = getAbsoluteDistance(u.position, position);
                            if (current == closest)
                            {
                                units.Add(u);
                            }
                            else if (current < closest)
                            {
                                closest = current;
                                units.Clear();
                                units.Add(u);
                            }
                        }

                    }
                }
            }
            return units;
        }

        private int getAbsoluteDistance(Position badGuyPos, Position goodGuyPos)
        {
            int x, y;
            x = Math.Abs(badGuyPos.x - goodGuyPos.x);
            y = Math.Abs(badGuyPos.y - goodGuyPos.y);
            return x + y;
        }
    }
}