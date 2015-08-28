using System;
using System.Linq;
using UnityEngine;
using System.Text;

/// <summary>
/// A representation of a city!
/// 
/// Each city is a rectangular grid. To represent non-rectangular cities, simply leave empty space
/// </summary>
public class City {

    public int width {
        get { return _cityGrid.GetLength(0); }
    }

    public int height {
        get { return _cityGrid.GetLength(1); }
    }

    public City(CityTile[,] cityGrid) {
        _cityGrid = cityGrid.CopyArray();

        var upRow = cityGrid.CopyRow(0);
        up = upRow.Contains(CityTile.Street) ? new CityRow(upRow) : null;

        var downRow = cityGrid.CopyRow(cityGrid.GetLength(0) - 1);
        down = downRow.Contains(CityTile.Street) ? new CityRow(downRow) : null;

        var rightRow = cityGrid.CopyColumn(cityGrid.GetLength(1) - 1);
        right = rightRow.Contains(CityTile.Street) ? new CityRow(rightRow) : null;

        var leftRow = cityGrid.CopyColumn(0);
        left = leftRow.Contains(CityTile.Street) ? new CityRow(leftRow) : null;
    }

    //These are immutable, so being readonly does not break immutability.
    public readonly CityRow up, right, down, left;

    public CityRow GetBorder(Dir dir) {
        switch (dir) {
            case Dir.Down:
                return down;
            case Dir.Left:
                return left;
            case Dir.Right:
                return right;
            default:
                return up;

        }
    }

    /// <summary>
    /// This is readonly, and the getter returns a copy -> immutable as long as internal methods
    /// don't muck around with the contents.
    /// </summary>
    private readonly CityTile[,] _cityGrid;

    /// <summary>
    /// The grid's (0,0) is the upper left. I mean, you could parse it however you want
    /// 
    /// This returns a copy.
    /// </summary>
    public CityTile[,] cityGrid {
        get { return _cityGrid.CopyArray(); }
    }

    public bool IsVacant(int x, int y) {
        if (x < 0 || x >= width)
            return false;
        if (y < 0 || y >= height)
            return false;

        return _cityGrid[x, y] == CityTile.Empty;
    }

    public override string ToString() {
        StringBuilder builder = new StringBuilder();

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                builder.Append(_cityGrid[x, y].ToChar());
            }
            builder.Append('\n');
        }

        builder.Remove(builder.Length - 1, 1);

        return builder.ToString();
    }

    /// <summary>
    /// Generates a city from a string.
    /// 
    /// Assumes that the input string is rectangular
    /// </summary>
    /// <param name="str">representation of a city</param>
    /// <returns>The generated city</returns>
    public static City FromString(string str) {
        string[] splitLines = str.Split('\n');

        string[] cityRepr = new string[splitLines.Length];
        for (int i = 0; i < cityRepr.Length; i++) {
            cityRepr[i] = splitLines[i].TrimEnd('\n');
        }

        var grid = new CityTile[cityRepr[0].Length, cityRepr.Length];

        for (int y = 0; y < cityRepr.Length; y++) {
            for (int x = 0; x < cityRepr[y].Length; x++) {
                CityTile tile = cityRepr[y][x].ToCityTile();
                grid[x, y] = tile;
            }
        }

        return new City(grid);
    }

    public static City FromString(TextAsset textAsset) {
        return FromString(textAsset.text);
    }

    /// <summary>
    /// Inserts otherCity into this city, with otherCity's (0,0) in this city's (x,y)
    /// </summary>
    /// <param name="otherCity">Some other city</param>
    /// <param name="xStart">What x-coordinate otherCity should be inserted at</param>
    /// <param name="yStart">What y-coordinate otherCity should be insterted at</param>
    public City Insert(City otherCity, int xStart, int yStart) {
        if (xStart + otherCity.width >= width || yStart + otherCity.height >= height)
            throw new Exception("Trying to insert a city that there's no room for!");

        var copyGrid = cityGrid; //getter clones

        for (int x = 0; x < otherCity.width; x++) {
            for (int y = 0; y < otherCity.height; y++) {
                var tile = otherCity._cityGrid[x, y];
                copyGrid[x + xStart, y + yStart] = tile;
            }
        }

        return new City(copyGrid);
    }

    /// <summary>
    /// The sum c1 + c2 is c1 with c2 placed to the right of it.
    /// blanks are empty
    /// 
    /// This is not an commutative operation. SUCK IT!
    /// </summary>
    public static City operator +(City c1, City c2) {
        var c1Width = c1.width;
        var c2Width = c2.width;
        var c1Height = c1.height;
        var c2Height = c2.height;

        var sumGrid = new CityTile[c1Width + c2Width, Mathf.Max(c1Height, c2Height)];

        for (int i = 0; i < sumGrid.GetLength(0); i++) {
            for (int j = 0; j < sumGrid.GetLength(1); j++) {
                bool inC1 = i < c1Width;

                if (inC1 && j >= c1Height)
                    continue;

                if (!inC1 && j >= c2Height)
                    continue; //under c2

                int x = inC1 ? i : i - c1Width;
                sumGrid[i, j] = inC1 ? c1._cityGrid[x, j] : c2._cityGrid[x, j];
            }
        }

        return new City(sumGrid);
    }

    /// <summary>
    /// Checks if there's room for a city, if it's inserted at startX/startY in this city.
    /// </summary>
    public bool HasRoomFor(City city, int startX, int startY) {
        if (startX + city.width >= width || startY + city.height >= height)
            return false;

        for (int x = 0; x < city.width; x++) {
            for (int y = 0; y < city.height; y++) {
                if (city.IsVacant(x, y))
                    continue;
                if (!IsVacant(x + startX, y + startY))
                    return false;
            }
        }

        return true;
    }
}

/// <summary>
/// These are used to represent the edges of cities.
/// 
/// A city has the up, right, down and left rows. The up and down rows are left-to-right,
/// while the left and right are top-to-bottom
/// </summary>
public class CityRow {

    public CityRow(CityTile[] row) {
        _row = row;
    }

    private readonly CityTile[] _row;

    public int Length {
        get { return _row.Length; }
    }

    public CityTile[] tiles {
        get { return _row.CopyArray(); }
    }

    public bool Matches(CityRow row) {
        for (int i = 0; i < _row.Length; i++) {
            if (row._row[i] != _row[i])
                return false;
        }
        return true;
    }
}

public enum CityTile {
    Empty = 0,
    Street = 1,
    House = 2,
    DEBUG = 100
}

public static class CityTileExtensions {

    /// <summary>
    /// Translates a CityTile to a char.
    /// 
    /// Note that every char-representation is lower-case
    /// </summary>
    public static char ToChar(this CityTile tile) {
        switch (tile) {
            case CityTile.House:
                return 'x';
            case CityTile.Street:
                return '#';
            case CityTile.DEBUG:
                return '�';
            default:
                return ' '; //empty!
        }
    }

    public static CityTile ToCityTile(this char c) {
        switch (c) {
            case 'x':
                return CityTile.House;
            case '#':
                return CityTile.Street;
            case '�':
                return CityTile.DEBUG;
            default:
                return CityTile.Empty;
        }
    }

}
