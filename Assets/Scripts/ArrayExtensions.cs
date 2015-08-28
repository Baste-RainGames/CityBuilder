using System;
using System.Text;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions {

    public static bool Contains<T>(this T[] array, T element) {
        for (int i = 0; i < array.Length; i++) {
            if (array[i].Equals(element))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Adds a blank space in an array at and index, moving every element after the index to make room for the space.
    /// </summary>
    /// <typeparam name="T">Type of the array</typeparam>
    /// <param name="array">The array to add a blank space to</param>
    /// <param name="position">The position of the space</param>
    /// <returns>A copy of the old array, with an extra, open index in the given space</returns>
    public static T[] AddSpaceInArray<T>(this T[] array, int position) {
        int newLength = array.Length + 1;
        T[] newArray = new T[newLength];
        for (int i = 0; i < array.Length; i++) {
            int otherArrayIndex = i;
            if (i >= position) {
                otherArrayIndex++;
            }

            newArray[otherArrayIndex] = array[i];
        }
        return newArray;
    }

    public static T[] AppendToArray<T>(this T[] array, T element) {
        int newIndex = array.Length;
        array = array.AddSpaceInArray(newIndex);
        array[newIndex] = element;
        return array;
    }

    /// <summary>
    /// Removes an element from an array at the given index. Shrinks the array, moving all of the subsequent elements back
    /// one space.
    /// 
    /// Can give a negative index. In that case, removes from the end, so removeFromArray(-1) means "remove last element"
    /// </summary>
    /// <typeparam name="T">Type of the array</typeparam>
    /// <param name="array">Array to remove the element from</param>
    /// <param name="index">Index to remove the element at</param>
    /// <returns>A copy of the old array, with the index removed</returns>
    public static T[] RemoveFromArray<T>(this T[] array, int index) {
        int newLength = array.Length - 1;
        T[] newArray = new T[newLength];

        if (index < 0)
            index = array.Length + index;

        for (int i = 0; i < newLength; i++) {
            int otherArrayIndex = i;
            if (i >= index) {
                otherArrayIndex++;
            }

            newArray[i] = array[otherArrayIndex];
        }
        return newArray;
    }

    public static void Swap<T>(this T[] array, int idx1, int idx2) {
        T temp = array[idx1];
        array[idx1] = array[idx2];
        array[idx2] = temp;
    }

    /// <summary>
    /// Finds the index of an element in an array. Looks for Equal objects, 
    /// which is reference equality if Equals hasn't been overridden.
    /// </summary>
    /// <typeparam name="T">Type of the array and element</typeparam>
    /// <param name="array">Array to look in.</param>
    /// <param name="element">The element to look for</param>
    /// <returns>First index of element in arr. -1 if not found</returns>
    public static int IndexOf<T>(this T[] array, T element) {
        for (int i = 0; i < array.Length; i++) {
            if (array[i].Equals(element))
                return i;
        }
        return -1;
    }

    public static T[] Concatenate<T>(this T[] array, T[] otherArray) {
        T[] newArr = new T[array.Length + otherArray.Length];
        array.CopyTo(newArr, 0);
        otherArray.CopyTo(newArr, array.Length);

        return newArr;
    }

    /// <summary>
    /// Fills an array with an element
    /// 
    /// If the element is a pointer, the array will be filled with that pointer, so you probably don't want to do
    /// this with string arrays and such.
    /// </summary>
    public static void Populate<T>(this T[] array, T element) {
        for (int i = 0; i < array.Length; i++) {
            array[i] = element;
        }
    }

    /// <summary>
    /// Fills an array with the elements supplied by a function that takes the index of the array and returns the type.
    /// 
    /// To fill an int-array with it's indices (such that array[i] == i), do:
    ///     array.Populate(x => x);
    /// 
    /// To reverse oldArray into newArray, do:
    ///     newArray.Populate(x => oldArray.Length - x - 1);
    ///
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="populateFunc"></param>
    public static void PopulateByIndexFunc<T>(this T[] array, Func<int, T> populateFunc) {
        for (int i = 0; i < array.Length; i++) {
            array[i] = populateFunc(i);
        }
    }

    public static void Each<T>(this IEnumerable<T> array, Action<T> function) {
        foreach (var element in array) {
            function(element);
        }
    }

    /// <summary>
    /// Returns a pretty string representation of an Array. Or anything else that's IEnumerable. Like a list or whatever.
    /// 
    /// Does basic [element,element] formatting, and also does recursive calls to inner lists. You can also give it a functon to
    /// do even prettier printing, usefull to get IE. a GameObject's name instead of "name (UnityEngine.GameObject)". If the function
    /// isn't supplied, toString is used.
    /// 
    /// Also turns null into "null" instead of ""
    /// 
    /// Will cause a stack overflow if you put list A in list B and list B in list A, but you wouldn't do that, would you?
    /// </summary>
    /// <param name="array">Some array</param>
    /// <param name="newLines">Set to true if the elements should be separated with a newline</param>
    /// <param name="printFunc">An optional function that you can use in place of ToString</param>
    /// <typeparam name="T">The type of the array</typeparam>
    /// <returns>a pretty printing of the array</returns>
    public static string PrettyPrint<T>(this IEnumerable<T> array, bool newLines = false, System.Func<T, string> printFunc = null) {
        if (array == null)
            return "null"; //won't this cause a nullpointer instead of calling this method?

        StringBuilder builder = new StringBuilder();
        builder.Append("[");
        bool added = false;
        foreach (T t in array) {
            added = true;
            if (t == null)
                builder.Append("null");
            else if (t is IEnumerable<T>)
                builder.Append(((IEnumerable<T>) t).PrettyPrint());
            else {
                if (printFunc == null)
                    builder.Append(t.ToString());
                else
                    builder.Append(printFunc(t));
            }

            builder.Append(newLines ? "\n " : ", ");
        }

        if (added) //removes the trailing ", "
            builder.Remove(builder.Length - 2, 2);
        builder.Append("]");

        return builder.ToString();
    }

    public static string Join<T>(this IEnumerable<T> array, string join) {
        if (array == null)
            return null;

        StringBuilder builder = new StringBuilder();
        foreach (T t in array) {
            if (t == null)
                builder.Append("null");
            else if (t is IEnumerable<T>)
                builder.Append(((IEnumerable<T>) t).Join(join));
            else
                builder.Append(t.ToString());

            builder.Append(join);
        }
        builder.Remove(builder.Length - join.Length, join.Length);
        return builder.ToString();
    }

    public static T[] CopyArray<T>(this T[] array) {
        T[] newArray = new T[array.Length];
        for (int i = 0; i < array.Length; i++) {
            newArray[i] = array[i];
        }
        return newArray;
    }

    public static T[,] CopyArray<T>(this T[,] array) {
        T[,] newArray = new T[array.GetLength(0), array.GetLength(1)];
        for (int i = 0; i < array.GetLength(0); i++) {
            for (int j = 0; j < array.GetLength(1); j++) {
                newArray[i, j] = array[i, j];
            }
        }
        return newArray;
    }

    public static T[] CopyRow<T>(this T[,] array, int row) {
        T[] rowArray = new T[array.GetLength(0)];
        for (int i = 0; i < array.GetLength(0); i++) {
            rowArray[i] = array[i, row];
        }
        return rowArray;
    }

    public static T[] CopyColumn<T>(this T[,] array, int column) {
        T[] rowArray = new T[array.GetLength(1)];
        for (int i = 0; i < array.GetLength(1); i++) {
            rowArray[i] = array[column, i];
        }
        return rowArray;
    }

    /// <summary>
    /// Given a two-dimensional array, creates a spiraling iterator.
    /// 
    /// The iterator returns the elements starting at the given x- and y-coordinates, and "spirals", starting to the right.
    /// The spiral goes through all of the elements, skipping over points "outside" the spiral
    /// 
    /// If XY is the starting point, the order looks like this:
    /// 
    /// [06][07][08][09]       [xy][01][04][09]       [12][13][14][15]
    /// [05][xy][01][10]       [03][02][05][10]       [11][06][07][08]
    /// [04][03][02][11]       [08][07][06][11]       [10][05][xy][01]
    /// [15][14][13][12]       [15][14][13][12]       [09][04][03][02]
    /// 
    /// Note that the algorithm allows you to start outside the array. It still "checks" the empty outside spiral points, which means that
    /// doing that will take a long time
    public static IEnumerable<T> GetSpiralIterator<T>(this T[,] squareArr, int startX, int startY) {
        return GetSpiralIterator<T, T>(squareArr, startX, startY, (arr, x, y) => arr[x, y]);
    }

    /// <summary>
    /// See the version without the return function.
    /// 
    /// This method does not return the element at the spiral index, but rather the return function of a function that takes the array and the
    /// spiral indices. 
    /// </summary>
    public static IEnumerable<V> GetSpiralIterator<T, V>(this T[,] squareArr, int startX, int startY, Func<T[,], int, int, V> returnFunc) {
        int numToLookAt = squareArr.GetLength(0) * squareArr.GetLength(1);
        int numLookedAt = 0;

        int currentX = startX;
        int currentY = startY;

        int xDir = 1;
        int yDir = 1;

        int xTarget = startX + 1;
        int yTarget = startY + 1;

        bool movingX = true;

        while (numLookedAt < numToLookAt) {
            if (currentX >= 0 && currentX < squareArr.GetLength(0) && currentY >= 0 && currentY < squareArr.GetLength(1)) {
                yield return returnFunc(squareArr, currentX, currentY);
                numLookedAt++;
            }

            if (movingX) {
                currentX += xDir;
                if (currentX == xTarget) {
                    movingX = false;
                }
            }
            else {
                currentY += yDir;
                if (currentY == yTarget) {
                    movingX = true;

                    if (xDir > 0) {
                        xTarget = startX - (xTarget - startX);
                        yTarget = startY - (yTarget - startY);
                    }
                    else {
                        xTarget = startX + (startX - xTarget) + 1;
                        yTarget = startY + (startY - yTarget) + 1;
                    }

                    xDir *= -1;
                    yDir *= -1;

                }
            }
        }
    }

    public static T[] Shuffled<T>(this T[] array) {
        T[] copy = array.CopyArray();
        // Knuth shuffle algorithm
        for (int t = 0; t < copy.Length; t++) {
            T tmp = copy[t];
            int r = UnityEngine.Random.Range(t, copy.Length);
            copy[t] = copy[r];
            copy[r] = tmp;
        }
        return copy;
    }

    public static T GetRandom<T>(this T[] array) {
        if (array.Length == 0)
            return default(T);
        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    public static T GetRandom<T>(this List<T> array) {
        if (array.Count == 0)
            return default(T);
        return array[UnityEngine.Random.Range(0, array.Count)];
    }

}
