using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomCity : MonoBehaviour {

    public List<Opening> openings;
    public IEnumerator<City> currentGenerator;

    [Serializable]
    public class Opening {
        public CityRow row;
        public Dir dir;
        public int x, y;
    }

    public TextAsset testOutput;
    public TextAsset[] sources;
    public CityToBlocks visualizer;

    public City CreateEmptyCity(int width = 10, int height = 10) {
        CityTile[,] grid = new CityTile[width, height];

        return new City(grid);
    }

    public City StartCreatingRandomCity() {
        openings = new List<Opening>();
        CityTile[,] grid = new CityTile[100, 100];
        City city = new City(grid);

        List<City> allCities = new List<City>();
        foreach (var source in sources) {
            allCities.Add(City.FromString(source));
        }

        city = InsertInCity(city, allCities[0], 47, 47);
        currentGenerator = CityGenerator(city, allCities);


        city = showDebugOpenings(city);

        return city;
    }

    private IEnumerator<City> CityGenerator(City city, List<City> allCities) {
        int safety = 0;
        int insertions = 0;
        while (safety < 100 && insertions < 15 && openings.Count > 0) {
            string debug = "stepping generation next. Number of openings: " + openings.Count + ", insertions: " + insertions + ", safety: " + safety;

            safety++;
            Opening opening = openings.GetRandom();
            City insert = FindMatching(opening, allCities);
            if (insert == null) {
                Debug.LogWarning("Found no matching city for an opening!");
                continue;
            }

            var startX = opening.x;
            var startY = opening.y;
            if (opening.dir == Dir.Left)
                startX -= insert.width;
            else if (opening.dir == Dir.Up)
                startY -= insert.height;
            else if (opening.dir == Dir.Down)
                startY++;
            else if (opening.dir == Dir.Right)
                startX++;

            debug += "\nStarting at (" + startX + "/" + startY + ") \nInserting:\n";
            debug += insert.ToString();

            if (city.HasRoomFor(insert, startX, startY)) {
                insertions++;
                city = InsertInCity(city, insert, startX, startY);
                RemoveInvalidOpenings(city.cityGrid);
                debug += "\nInsert successfull!";
            }
            else {
                debug += "\nInsert failed, no room!";
            }
            Debug.Log(debug);

            city = showDebugOpenings(city);

            yield return city;
        }

        Debug.Log("Finished generation: Number of openings: " + openings.Count + ", insertions: " + insertions + ", safety: " + safety);
    }

    private void RemoveInvalidOpenings(CityTile[,] cityGrid) {
        //foreach (var opening in openings) {
        openings.RemoveAll(opening => {
            var xDir = opening.dir.X();
            var yDir = opening.dir.Y();
            var startX = opening.x + xDir;
            var startY = opening.y + yDir;
            //int x = opening.x + opening.dir.X();
            //int y = opening.y + opening.dir.Y();
            CityTile[] tiles = opening.row.tiles;
            string debug = "checking validity of opening " + tiles.PrettyPrint() + " as " + opening.x + "/" + opening.y + " in the direction " + opening.dir;
            for (int i = 0; i < tiles.Length; i++) {
                CityTile cityTile = cityGrid[startX + i * xDir, +startY + i * yDir];
                debug += string.Format("\nChecking tile at {0}/{1}, it has the value {2}", startX + i * xDir, startY + i * yDir, cityTile);
                if (cityTile != CityTile.Empty) {
                    i = tiles.Length;
                    debug += "\nremoved it!";
                    Debug.Log(debug);
                    return true;
                }
            }
            Debug.Log(debug);
            return false;
        });

    }

    private City FindMatching(Opening opening, List<City> allCities) {
        int startCoordinate = Random.Range(0, allCities.Count);
        for (int i = 0; i < allCities.Count; i++) {
            City city = allCities[(i + startCoordinate) % allCities.Count];
            if (Matches(opening, city)) {
                return city;
            }
        }
        return null;
    }

    private bool Matches(Opening opening, City city) {
        var openingRow = opening.row;
        var cityRow = city.GetBorder(opening.dir.Opposite());
        return cityRow != null && cityRow.Matches(openingRow);
    }

    private City showDebugOpenings(City city) {
        CityTile[,] grid;
        grid = city.cityGrid;

        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int y = 0; y < grid.GetLength(1); y++) {
                if(grid[x,y] == CityTile.DEBUG)
                    grid[x,y] = CityTile.Street; //reset old debugs.
            }
        }

        foreach (var opening in openings) {
            var rowLength = opening.row.tiles.Length;
            for (int i = 0; i < rowLength; i++) {
                var tile = opening.row.tiles[i];
                if (tile == CityTile.Street) {
                    switch (opening.dir) {
                        case Dir.Up:
                            grid[opening.x + i, opening.y] = CityTile.DEBUG;
                            break;
                        case Dir.Down:
                            grid[opening.x + i, opening.y] = CityTile.DEBUG;
                            break;
                        case Dir.Right:
                            grid[opening.x, opening.y + i] = CityTile.DEBUG;
                            break;
                        case Dir.Left:
                            grid[opening.x, opening.y + i] = CityTile.DEBUG;
                            break;
                    }
                }
            }

        }

        city = new City(grid);
        return city;
    }

    private City InsertInCity(City city, City insert, int startX, int startY) {
        CheckOpening(city, startX, startY, insert.up, Dir.Up);
        CheckOpening(city, startX, startY + insert.height - 1, insert.down, Dir.Down);
        CheckOpening(city, startX, startY, insert.left, Dir.Left);
        CheckOpening(city, startX + insert.width - 1, startY, insert.right, Dir.Right);

        return city.Insert(insert, startX, startY);
    }

    private void CheckOpening(City c, int startX, int startY, CityRow row, Dir dir) {
        if (row != null) {
            for (int i = 0; i < row.Length; i++) {
                if (!c.IsVacant(startX + dir.X(), startY + dir.Y()))
                    return;
            }

            openings.Add(new Opening {
                dir = dir,
                row = row,
                x = startX,
                y = startY
            });
        }
    }

}

public enum Dir {
    Up,
    Right,
    Down,
    Left
}

public static class DirExtension {
    public static Dir Opposite(this Dir dir) {
        switch (dir) {
            case Dir.Right:
                return Dir.Left;
            case Dir.Down:
                return Dir.Up;
            case Dir.Left:
                return Dir.Right;
            default:
                return Dir.Down;
        }
    }

    public static int X(this Dir dir) {
        switch (dir) {
            case Dir.Right:
                return 1;
            case Dir.Down:
                return 0;
            case Dir.Left:
                return -1;
            default:
                return 0;
        }
    }

    public static int Y(this Dir dir) {
        switch (dir) {
            case Dir.Right:
                return 0;
            case Dir.Down:
                return 1;
            case Dir.Left:
                return 0;
            default:
                return -1;
        }
    }
}
