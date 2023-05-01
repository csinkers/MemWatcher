﻿using System.Buffers;

namespace CorrelateSymbols;

public static class LevenshteinDistance // Adapted from https://gist.github.com/Davidblkx/e12ab0bb2aff7fd8072632b396538560
{
    /// <summary>
    ///     Calculate the difference between 2 strings using the Levenshtein distance algorithm
    /// </summary>
    /// <param name="source1">First string</param>
    /// <param name="source2">Second string</param>
    /// <returns></returns>
    public static int Calculate(string source1, string source2) //O(n*m)
    {
        var source1Length = source1.Length;
        var source2Length = source2.Length;
        var rows = source1Length + 1;
        var columns = source2Length + 1;
        var matrix = ArrayPool<int>.Shared.Rent(rows * columns);

        // First calculation, if one entry is empty return full length
        if (source1Length == 0)
            return source2Length;

        if (source2Length == 0)
            return source1Length;

        // Initialization of matrix with row size source1Length and columns size source2Length
        for (var i = 0; i <= source1Length; matrix[i] = i++) { }
        for (var j = 0; j <= source2Length; matrix[j * rows] = j++) { }

        // Calculate row and column distances
        for (var i = 1; i <= source1Length; i++)
        {
            for (var j = 1; j <= source2Length; j++)
            {
                var cost = source2[j - 1] == source1[i - 1] ? 0 : 1;

                matrix[j * rows + i] = Math.Min(
                    Math.Min(
                        matrix[j * rows + i - 1] + 1,
                        matrix[(j - 1) * rows + i] + 1),
                    matrix[(j - 1) * rows + (i - 1)] + cost);
            }
        }

        var result = matrix[source2Length * rows + source1Length];
        ArrayPool<int>.Shared.Return(matrix);
        return result;
    }
}