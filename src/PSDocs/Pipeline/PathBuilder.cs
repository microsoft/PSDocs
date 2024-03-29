﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using PSDocs.Configuration;
using PSDocs.Data;

namespace PSDocs.Pipeline
{
    public interface IPathBuilder
    {
        void Add(string path);

        void Add(System.IO.FileInfo[] fileInfo);

        void Add(PathInfo[] pathInfo);

        InputFileInfo[] Build();
    }

    internal abstract class PathBuilder
    {
        // Path separators
        private const char Slash = '/';
        private const char BackSlash = '\\';

        private const char Dot = '.';
        private const string CurrentPath = ".";
        private const string RecursiveSearchOperator = "**";

        private static readonly char[] PathLiteralStopCharacters = new char[] { '*', '[', '?' };
        private static readonly char[] PathSeparatorCharacters = new char[] { '\\', '/' };

        private readonly IPipelineWriter _Logger;
        private readonly List<InputFileInfo> _Files;
        private readonly HashSet<string> _Paths;
        private readonly string _BasePath;
        private readonly string _DefaultSearchPattern;
        private readonly PathFilter _GlobalFilter;

        protected PathBuilder(IPipelineWriter logger, string basePath, string searchPattern, PathFilter filter)
        {
            _Logger = logger;
            _Files = new List<InputFileInfo>();
            _Paths = new HashSet<string>();
            _BasePath = NormalizePath(PSDocumentOption.GetRootedBasePath(basePath));
            _DefaultSearchPattern = searchPattern;
            _GlobalFilter = filter;
        }

        public void Add(string[] path)
        {
            if (path == null || path.Length == 0)
                return;

            for (var i = 0; i < path.Length; i++)
                Add(path[i]);
        }

        public void Add(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            FindFiles(path);
        }

        public InputFileInfo[] Build()
        {
            try
            {
                return _Files.ToArray();
            }
            finally
            {
                _Files.Clear();
                _Paths.Clear();
            }
        }

        private void FindFiles(string path)
        {
            if (TryUrl(path) || TryPath(path, out path))
                return;

            var pathLiteral = GetSearchParameters(path, out var searchPattern, out var searchOption, out var filter);
            var files = Directory.EnumerateFiles(pathLiteral, searchPattern, searchOption);
            foreach (var file in files)
                if (ShouldInclude(file, filter))
                    AddFile(file);
        }

        private bool TryUrl(string path)
        {
            if (!path.IsUri())
                return false;

            AddFile(path);
            return true;
        }

        private bool TryPath(string path, out string normalPath)
        {
            normalPath = path;
            if (path.IndexOfAny(PathLiteralStopCharacters) > -1)
                return false;

            var rootedPath = GetRootedPath(path);
            if (Directory.Exists(rootedPath) || path == CurrentPath)
            {
                if (IsBasePath(rootedPath))
                    normalPath = CurrentPath;

                return false;
            }
            if (!File.Exists(rootedPath))
            {
                ErrorNotFound(path);
                return false;
            }
            AddFile(rootedPath);
            return true;
        }

        private bool IsBasePath(string path)
        {
            path = IsSeparator(path[path.Length - 1]) ? path : string.Concat(path, Path.DirectorySeparatorChar);
            return NormalizePath(path) == _BasePath;
        }

        private void ErrorNotFound(string path)
        {
            if (_Logger == null)
                return;

            _Logger.WriteError(new ErrorRecord(new FileNotFoundException(), "PSDocs.PathBuilder.ErrorNotFound", ErrorCategory.ObjectNotFound, path));
        }

        private void AddFile(string path)
        {
            if (_Paths.Contains(path))
                return;

            _Files.Add(new InputFileInfo(_BasePath, path));
            _Paths.Add(path);
        }

        private string GetRootedPath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(_BasePath, path));
        }

        /// <summary>
        /// Split a search path into components based on wildcards.
        /// </summary>
        private string GetSearchParameters(string path, out string searchPattern, out SearchOption searchOption, out PathFilter filter)
        {
            searchOption = SearchOption.AllDirectories;
            var pathLiteral = TrimPath(path, out var relativeAnchor);

            if (TryFilter(pathLiteral, out searchPattern, out filter))
                return _BasePath;

            pathLiteral = SplitSearchPath(pathLiteral, out searchPattern);
            if ((relativeAnchor || !string.IsNullOrEmpty(pathLiteral)) && !string.IsNullOrEmpty(searchPattern))
                searchOption = SearchOption.TopDirectoryOnly;

            if (string.IsNullOrEmpty(searchPattern))
                searchPattern = _DefaultSearchPattern;

            return GetRootedPath(pathLiteral);
        }

        private static string SplitSearchPath(string path, out string searchPattern)
        {
            // Find the index of the first expression character i.e. out/modules/**/file
            var stopIndex = path.IndexOfAny(PathLiteralStopCharacters);

            // Track back to the separator before any expression characters
            var literalSeparator = stopIndex > -1 ? path.LastIndexOfAny(PathSeparatorCharacters, stopIndex) + 1 : path.LastIndexOfAny(PathSeparatorCharacters) + 1;
            searchPattern = path.Substring(literalSeparator, path.Length - literalSeparator);
            return path.Substring(0, literalSeparator);
        }

        private bool TryFilter(string path, out string searchPattern, out PathFilter filter)
        {
            searchPattern = null;
            filter = null;
            if (UseSimpleSearch(path))
                return false;

            filter = PathFilter.Create(_BasePath, path);
            var patternSeparator = path.LastIndexOfAny(PathSeparatorCharacters) + 1;
            searchPattern = path.Substring(patternSeparator, path.Length - patternSeparator);
            return true;
        }

        /// <summary>
        /// Remove leading ./ or .\ characters indicating a relative path anchor.
        /// </summary>
        /// <param name="path">The path to trim.</param>
        /// <param name="relativeAnchor">Returns true when a relative path anchor was present.</param>
        /// <returns>Return a clean path without the relative path anchor.</returns>
        private static string TrimPath(string path, out bool relativeAnchor)
        {
            relativeAnchor = false;
            if (path.Length >= 2 && path[0] == Dot && IsSeparator(path[1]))
            {
                relativeAnchor = true;
                return path.Remove(0, 2);
            }
            return path;
        }

        private bool ShouldInclude(string file, PathFilter filter)
        {
            return (filter == null || filter.Match(file)) &&
                (_GlobalFilter == null || _GlobalFilter.Match(file));
        }

        [DebuggerStepThrough]
        private static bool IsSeparator(char c)
        {
            return c == Slash || c == BackSlash;
        }

        /// <summary>
        /// Determines if a simple search can be used.
        /// </summary>
        [DebuggerStepThrough]
        private static bool UseSimpleSearch(string s)
        {
            return s.IndexOf(RecursiveSearchOperator, System.StringComparison.OrdinalIgnoreCase) == -1;
        }

        [DebuggerStepThrough]
        private static string NormalizePath(string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
    }

    internal sealed class PathFilterBuilder
    {
        private const string GitIgnoreFileName = ".gitignore";

        private readonly string _BasePath;
        private readonly List<string> _Expressions;
        private readonly bool _MatchResult;

        private PathFilterBuilder(string basePath, string[] expressions, bool matchResult)
        {
            _BasePath = basePath;
            _Expressions = expressions == null || expressions.Length == 0 ? new List<string>() : new List<string>(expressions);
            _MatchResult = matchResult;
        }

        internal static PathFilterBuilder Create(string basePath, string[] expressions, bool matchResult = false)
        {
            return new PathFilterBuilder(basePath, expressions, matchResult);
        }

        internal void UseGitIgnore(string basePath = null)
        {
            _Expressions.Add(".git/");
            _Expressions.Add("!.git/HEAD");
            ReadFile(Path.Combine(basePath ?? _BasePath, GitIgnoreFileName));
        }

        internal PathFilter Build()
        {
            return PathFilter.Create(_BasePath, _Expressions.ToArray(), _MatchResult);
        }

        private void ReadFile(string filePath)
        {
            if (File.Exists(filePath))
                _Expressions.AddRange(File.ReadAllLines(filePath));
        }
    }

    /// <summary>
    /// Filters paths based on predefined rules.
    /// </summary>
    internal sealed class PathFilter
    {
        // Path separators
        private const char Slash = '/';
        private const char BackSlash = '\\';

        // Operators
        private const char Asterix = '*'; // Match multiple characters except '/'
        private const char Question = '?'; // Match any character except '/'
        private const char Hash = '#'; // Comment
        private const char Exclamation = '!'; // Include a previously excluded path

        private readonly string _BasePath;
        private readonly PathFilterExpression[] _Expression;
        private readonly bool _MatchResult;

        private PathFilter(string basePath, PathFilterExpression[] expression, bool matchResult)
        {
            _BasePath = NormalDirectoryPath(basePath);
            _Expression = expression ?? Array.Empty<PathFilterExpression>();
            _MatchResult = matchResult;
        }

        #region PathStream

        [DebuggerDisplay("Path = '{_Path}', Position = {_Position}, Current = '{_Current}'")]
        private sealed class PathStream
        {
            private readonly string _Path;
            private int _Position;
            private char _Current;

            public PathStream(string path)
            {
                _Path = path;
                Reset();
            }

            /// <summary>
            /// Resets the cursor to the start of the path stream.
            /// </summary>
            public void Reset()
            {
                _Position = -1;
                _Current = char.MinValue;
                if (_Path[0] == Exclamation)
                    Next();
            }

            /// <summary>
            /// Move to the next character.
            /// </summary>
            public bool Next()
            {
                _Position++;
                if (_Position >= _Path.Length)
                {
                    _Current = char.MinValue;
                    return false;
                }
                _Current = _Path[_Position];
                return true;
            }

            public bool TryMatch(PathStream other, int offset)
            {
                return other.Peak(offset, out var c) && IsMatch(c);
            }

            public bool IsUnmatchedSingle(PathStream other, int offset)
            {
                return other.Peak(offset, out var c) && IsWilcardQ(c) && other.Peak(offset + 1, out var cnext) && IsMatch(cnext);
            }

            private bool IsMatch(char c)
            {
                return _Current == c ||
                    (IsSeparator(_Current) && IsSeparator(c)) ||
                    (!IsSeparator(_Current) && IsWilcardQ(c));
            }

            /// <summary>
            /// Determine if the current character sequence is ** or **/.
            /// </summary>
            public bool IsAnyMatchEnding(int offset = 0)
            {
                if (!IsWildcardAA(offset))
                    return false;

                var pos = _Position + offset;

                // Ends in **
                if (pos + 1 == _Path.Length - 1)
                    return true;

                // Ends in **/
                if (pos + 2 == _Path.Length - 1 && IsSeparator(_Path[pos + 2]))
                    return true;

                return false;
            }

            public bool SkipMatchAA()
            {
                if (!IsWildcardAA())
                    return false;


                Skip(2); // Skip **
                SkipSeparator(); // Skip **/
                SkipMatchAA(); // Skip **/**/
                return true;
            }

            public bool SkipMatchA()
            {
                if (!IsWildardA())
                    return false;

                Skip(1);
                return true;
            }

            private bool Skip(int count)
            {
                if (count > 1)
                    _Position += count - 1;

                return Next();
            }

            [DebuggerStepThrough]
            private void SkipSeparator()
            {
                if (IsSeparator(_Current))
                    Next();
            }

            /// <summary>
            /// Determine if the current character sequence is **.
            /// </summary>
            [DebuggerStepThrough]
            private bool IsWildcardAA(int offset = 0)
            {
                var pos = _Position + offset;
                return pos + 1 < _Path.Length && _Path[pos] == Asterix && _Path[pos + 1] == Asterix;
            }

            [DebuggerStepThrough]
            private bool IsWildardA(int offset = 0)
            {
                var pos = _Position + offset;
                return pos < _Path.Length && _Path[pos] == Asterix;
            }

            [DebuggerStepThrough]
            private static bool IsWilcardQ(char c)
            {
                return c == Question;
            }

            [DebuggerStepThrough]
            private static bool IsSeparator(char c)
            {
                return c == Slash || c == BackSlash;
            }

            /// <summary>
            /// Match **
            /// </summary>
            public bool TryMatchAA(PathStream other, int start)
            {
                var offset = start;
                do
                {
                    if (IsUnmatchedSingle(other, offset))
                        offset++;

                    // Determine if fully matched to end or the next any match
                    if (other.IsWildcardAA(offset))
                    {
                        other.Skip(offset);
                        return true;
                    }
                    else if (other.IsWildardA(offset) && TryMatchA(other, offset + 1))
                    {
                        return true;
                    }

                    //(IsSingleWildcard(c) && other.Peak(offset + 1, out char cnext) && IsMatch(cnext))
                    //if (TryMatchCharacter(other, offset))
                    //{

                    //}
                    // Try to match the remaining
                    if (TryMatch(other, offset))
                    {
                        offset++;
                    }
                    else
                    {
                        offset = start;
                    }

                    if (offset + other._Position >= other._Path.Length)
                    {
                        other.Skip(offset);
                        return true;
                    }
                } while (Next());
                return false;
            }

            /// <summary>
            /// Match *
            /// </summary>
            public bool TryMatchA(PathStream other, int start)
            {
                var offset = start;
                do
                {
                    // Determine if fully matched to end or the next any match
                    if (other.IsWildcardAA(offset))
                    {
                        other.Skip(offset);
                        return true;
                    }
                    //if (other.IsCharacterMatch(offset))

                    // Try to match the remaining
                    if (TryMatch(other, offset))
                    {
                        offset++;
                    }
                    else
                    {
                        offset = start;
                    }

                    if (offset + other._Position >= other._Path.Length)
                    {
                        Next();
                        other.Skip(offset);
                        return true;
                    }
                } while (Next());
                return false;
            }

            private bool Peak(int offset, out char c)
            {
                if (offset + _Position >= _Path.Length)
                {
                    c = char.MinValue;
                    return false;
                }
                c = _Path[offset + _Position];
                return true;
            }
        }

        #endregion PathStream

        #region PathFilterExpression

        [DebuggerDisplay("{_Expression}")]
        private sealed class PathFilterExpression
        {
            private readonly PathStream _Expression;
            private readonly bool _Include;

            private PathFilterExpression(string expression, bool include)
            {
                _Expression = new PathStream(expression);
                _Include = include;
            }

            public static PathFilterExpression Create(string expression)
            {
                if (string.IsNullOrEmpty(expression))
                    throw new ArgumentNullException(nameof(expression));

                var actualInclude = true;
                if (expression[0] == Exclamation)
                    actualInclude = false;

                return new PathFilterExpression(expression, actualInclude);
            }

            /// <summary>
            /// Determine if the path matches the expression.
            /// </summary>
            public void Match(string path, ref bool include)
            {
                // Only process if the result would change
                if (_Include == include || !Match(path))
                    return;

                include = !include;
            }

            /// <summary>
            /// Determines if the path matches the expression.
            /// </summary>
            private bool Match(string path)
            {
                _Expression.Reset();
                var stream = new PathStream(path);
                while (stream.Next() && _Expression.Next())
                {
                    // Match characters
                    if (stream.TryMatch(_Expression, 0))
                        continue;

                    // Skip ? when zero characters are being matched
                    else if (stream.IsUnmatchedSingle(_Expression, 0))
                        _Expression.Next();

                    // Match ending wildcards e.g. src/** or src/**/
                    else if (_Expression.IsAnyMatchEnding())
                        break;

                    // Match ending with depth e.g. src/**/bin/
                    else if (_Expression.SkipMatchAA())
                    {
                        if (!stream.TryMatchAA(_Expression, 0))
                            return false;
                    }

                    // Match wildcard *
                    else if (_Expression.SkipMatchA())
                    {
                        if (!stream.TryMatchA(_Expression, 0))
                            return false;
                    }

                    else return false;
                }
                return true;
            }
        }

        #endregion PathFilterExpression

        #region Public methods

        public static PathFilter Create(string basePath, string expression, bool matchResult = true)
        {
            if (!ShouldSkipExpression(expression))
                return new PathFilter(basePath, new PathFilterExpression[] { PathFilterExpression.Create(expression) }, matchResult);

            return new PathFilter(basePath, null, matchResult);
        }

        public static PathFilter Create(string basePath, string[] expression, bool matchResult = true)
        {
            var result = new List<PathFilterExpression>(expression.Length);
            for (var i = 0; i < expression.Length; i++)
                if (!ShouldSkipExpression(expression[i]))
                    result.Add(PathFilterExpression.Create(expression[i]));

            return result.Count == 0 ? new PathFilter(basePath, null, matchResult) : new PathFilter(basePath, result.ToArray(), matchResult);
        }

        /// <summary>
        /// Determine if the specific path is matched.
        /// </summary>
        /// <param name="path">The path to evaluate.</param>
        public bool Match(string path)
        {
            var start = 0;

            // Check if path is within base path
            if (Path.IsPathRooted(path))
            {
                if (!path.StartsWith(_BasePath))
                    return !_MatchResult;

                start = _BasePath.Length;
            }
            var cleanPath = start > 0 ? path.Remove(0, start) : path;

            // Include unless excluded
            var result = false;

            // Compare expressions
            for (var i = 0; i < _Expression.Length; i++)
                _Expression[i].Match(cleanPath, ref result);

            // Flip the result if a match should return false
            return _MatchResult ? result : !result;
        }

        #endregion Public methods

        private static bool ShouldSkipExpression(string expression)
        {
            return string.IsNullOrEmpty(expression) || expression[0] == Hash;
        }

        private static string NormalDirectoryPath(string path)
        {
            var c = path[path.Length - 1];
            if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
                return path;

            return string.Concat(path, Path.DirectorySeparatorChar);
        }
    }
}
