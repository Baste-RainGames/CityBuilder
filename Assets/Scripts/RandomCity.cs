using System.Collections.Generic;
using UnityEngine;

public class RandomCity : MonoBehaviour {

    private List<Opening> openings;

    private struct Opening {
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

    public City CreateRandomCity() {
        openings = new List<Opening>();
        CityTile[,] grid = new CityTile[100, 100];
        City city = new City(grid);

        List<City> allCities = new List<City>();
        foreach (var source in sources) {
            //.Shuffled()) {
            allCities.Add(City.FromString(source));
        }

        city = InsertInCity(city, allCities[0], 47, 47);
        int safety = 0;
        int insertions = 0;
        while (safety < 100 && insertions < 15 && openings.Count > 0) {
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

            if (city.HasRoomFor(insert, startX, startY)) {
                insertions++;
                city = InsertInCity(city, insert, startX, startY);
            }
            openings.Remove(opening);
        }

        city = ParseOpenings(city);

        return city;
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

    private City ParseOpenings(City city) {
        CityTile[,] grid;
        grid = city.cityGrid;

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
