using System;

namespace T3.Core.Utils;

public static class StringUtils
{
    public static bool Equals(ReadOnlySpan<char> a, ReadOnlySpan<char> b, bool ignoreCase)
    {
        var aLength = a.Length;
        if (aLength != b.Length)
            return false;

        if (ignoreCase)
        {
            for (var i = 0; i < aLength; i++)
            {
                if (char.ToLowerInvariant(a[i]) != char.ToLowerInvariant(b[i]))
                    return false;
            }
        }
        else
        {
            for (int i = 0; i < aLength; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Splits the provided path by the directory separator character.
    /// Warning: not for use with windows-style absolute paths (e.g. C:\foo\bar or C:/foo/bar),
    /// though unix-style absolute paths will work (e.g. /foo/bar)
    /// </summary>
    /// <param name="path">The path to split</param>
    /// <param name="ranges">Ranges that can be used to create spans or substrings from the original path</param>
    /// <returns></returns>
    public static int SplitByDirectory(ReadOnlySpan<char> path, Span<Range> ranges)
    {
        int count = 0;
        int start = 0;
        for (int i = 0; i < path.Length; i++)
        {
            if (path[i] == '\\' || path[i] == '/')
            {
                if (i > start)
                {
                    ranges[count++] = new Range(start, i);
                }

                start = i + 1;
            }
        }

        if (path.Length > start)
        {
            ranges[count++] = new Range(start, path.Length);
        }

        return count;
    }

    public enum SearchResultIndex {BeforeTerm, AfterTerm, FirstIndex, LastIndex}
    public static bool TryFindIgnoringAllWhitespace(string text, string searchTerm, SearchResultIndex searchResultIndex, out int indexFollowingSearchTerm, int startIndex = 0)
    {
        // search the given string for the search term, ignoring all whitespace in both strings. " \ta b" == "ab"
        var searchTextLength = text.Length;
        
        // remove all whitespace from searchTerm
        searchTerm = RemoveWhitespaceFrom(searchTerm);
        
        var searchTermLength = searchTerm.Length;

        int currentSearchIndex = 0;
        char currentSearchChar = searchTerm[currentSearchIndex];
        int firstIndex = -1;

        for (int j = startIndex; j < searchTextLength; j++)
        {
            var textChar = text[j];
            
            if (char.IsWhiteSpace(textChar))
                continue;

            if (text[j] != currentSearchChar)
            {
                currentSearchIndex = 0;
                currentSearchChar = searchTerm[0];
                firstIndex = -1;
                continue;
            }
            
            if (firstIndex == -1)
                firstIndex = j;
            
            ++currentSearchIndex;
            if (currentSearchIndex == searchTermLength)
            {
                indexFollowingSearchTerm = searchResultIndex switch
                                               {
                                                   SearchResultIndex.BeforeTerm => firstIndex - 1,
                                                   SearchResultIndex.AfterTerm => j + 1,
                                                   SearchResultIndex.FirstIndex => firstIndex,
                                                   SearchResultIndex.LastIndex => j,
                                                   _ => throw new ArgumentOutOfRangeException(nameof(searchResultIndex))
                                               };
                indexFollowingSearchTerm = j + 1;
                return true;
            }

            currentSearchChar = searchTerm[currentSearchIndex];
        }

        indexFollowingSearchTerm = -1;
        return false;
    }

    public static string RemoveWhitespaceFrom(string str)
    {
        var strLength = str.Length;
        for (var i = 0; i < strLength; i++)
        {
            var c = str[i];
            if (char.IsWhiteSpace(c))
            {
                str = str.Remove(c);
                strLength = str.Length;
                i = 0;
            }
        }

        return str;
    }
}