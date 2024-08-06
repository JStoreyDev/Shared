using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JS
{
    public class Autocomplete
    {
        Dictionary<string, int> searchFrequency;
        Trie trie;
        Dictionary<string, HashSet<string>> metaphoneIndex;

        public bool usePhonetic = true;
        
        public Autocomplete(IEnumerable<string> words)
        {
            var wordList = words as List<string> ?? words.ToList();
            trie = Trie.Create(wordList);
            searchFrequency = new Dictionary<string, int>();
            metaphoneIndex = new Dictionary<string, HashSet<string>>();
            BuildMetaphoneIndex(wordList);
        }

        void BuildMetaphoneIndex(List<string> words)
        {
            foreach (var word in words)
            {
                var metaphone = Trie.Metaphone.Encode(word);

                if (!metaphoneIndex.ContainsKey(metaphone))
                {
                    metaphoneIndex[metaphone] = new HashSet<string>();
                }

                metaphoneIndex[metaphone].Add(word);
            }
        }

        public List<ScoredResult> SearchWithScore(string query, int maxResults = 10)
        {
            var results = new Dictionary<string, AutocompleteResult>();
            var tokens = TokenizeQuery(query);

            // Perform exact search
            foreach (var result in trie.ExactSearch(query))
                results[result.Word] = result;

            // Perform token-based search
            foreach (var token in tokens)
            {
                foreach (var result in trie.PrefixSearch(token))
                {
                    if (!results.ContainsKey(result.Word))
                    {
                        results[result.Word] = result;
                        if (results.Count >= maxResults * 2)
                            break;
                    }
                }

                if (results.Count >= maxResults * 2) break;

                foreach (var result in trie.FuzzySearch(token))
                {
                    if (!results.ContainsKey(result.Word))
                    {
                        results[result.Word] = result;
                        if (results.Count >= maxResults * 2) break;
                    }
                }

                if (usePhonetic)
                {
                    foreach (var result in PhoneticSearch(token))
                    {
                        if (!results.ContainsKey(result.Word))
                        {
                            results[result.Word] = result;
                            if (results.Count >= maxResults * 2) break;
                        }
                    }
                }

                if (results.Count >= maxResults * 2) break;
            }

            return RankResultsWithScore(query, results.Values, maxResults);
        }

        public List<AutocompleteResult> Search(string query, int maxResults = 10)
        {
            var results = new Dictionary<string, AutocompleteResult>();
            var tokens = TokenizeQuery(query);

            // Perform exact search
            foreach (var result in trie.ExactSearch(query))
                results[result.Word] = result;

            // Perform token-based search
            foreach (var token in tokens)
            {
                foreach (var result in trie.PrefixSearch(token))
                {
                    if (results.ContainsKey(result.Word)) continue;
                    results[result.Word] = result;
                    if (results.Count >= maxResults * 2) break;
                }

                if (results.Count >= maxResults * 2) break;

                foreach (var result in trie.FuzzySearch(token))
                {
                    if (results.ContainsKey(result.Word)) continue;
                    results[result.Word] = result;
                    if (results.Count >= maxResults * 2) break;
                }

                if (usePhonetic)
                {
                    foreach (var result in PhoneticSearch(token))
                    {
                        if (!results.ContainsKey(result.Word))
                        {
                            results[result.Word] = result;
                            if (results.Count >= maxResults * 2) break;
                        }
                    }
                }

                if (results.Count >= maxResults * 2) break;
            }

            return RankResults(query, results.Values, maxResults);
        }

        IEnumerable<AutocompleteResult> PhoneticSearch(string token)
        {
            var metaphone = Trie.Metaphone.Encode(token);
            if (!metaphoneIndex.ContainsKey(metaphone)) yield break;
            foreach (var word in metaphoneIndex[metaphone])
                yield return new AutocompleteResult(word, Array.Empty<int>(), 0);
        }

        List<string> TokenizeQuery(string query) =>
            Regex.Split(query, @"[\s\.\-_]+|(?<=[a-z])(?=[A-Z])")
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.ToLower())
                .ToList();

        List<ScoredResult> RankResultsWithScore(string query, IEnumerable<AutocompleteResult> results,
            int maxResults) =>
            results
                .Select(r => new ScoredResult(r, CalculateScore(query, TokenizeQuery(query), r)))
                .OrderByDescending(sr => sr.Score)
                .Take(maxResults)
                .ToList();

        List<AutocompleteResult> RankResults(string query, IEnumerable<AutocompleteResult> results, int maxResults) =>
            results
                .Select(r => new ScoredResult(r, CalculateScore(query, TokenizeQuery(query), r)))
                .OrderByDescending(sr => sr.Score)
                .Take(maxResults)
                .Select(sr => sr.Result)
                .ToList();

        float CalculateScore(string query, List<string> queryTokens, AutocompleteResult result)
        {
            float score = 100 - result.EditDistance * 5; // Base score

            if (result.Word.Equals(query, StringComparison.OrdinalIgnoreCase))
                score += 50; // Exact match bonus

            if (result.Word.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                score += 30; // Prefix match bonus

            score += result.MatchedIndexes.Length * 2; // Matched characters bonus

            var consecutiveMatches = CountConsecutiveMatches(result.MatchedIndexes);
            score += consecutiveMatches * 3; // Consecutive matches bonus

            score += 1.0f / result.Word.Length * 10; // Shorter words bonus

            if (searchFrequency.ContainsKey(result.Word))
                score += Math.Min(searchFrequency[result.Word], 10) * 2; // Frequency bonus

            // Token matching bonus
            var resultTokens = TokenizeQuery(result.Word);
            score += queryTokens.Count(qt => resultTokens.Any(rt => rt.StartsWith(qt))) * 10;

            // Phonetic matching bonus
            if (Trie.Metaphone.Encode(query) == Trie.Metaphone.Encode(result.Word))
                score += 20;

            return score;
        }

        int CountConsecutiveMatches(int[] matchedIndexes)
        {
            var maxConsecutive = 0;
            var currentConsecutive = 1;

            for (var i = 1; i < matchedIndexes.Length; i++)
                if (matchedIndexes[i] == matchedIndexes[i - 1] + 1)
                    currentConsecutive++;
                else
                {
                    maxConsecutive = Math.Max(maxConsecutive, currentConsecutive);
                    currentConsecutive = 1;
                }

            return Math.Max(maxConsecutive, currentConsecutive);
        }

        public void ClearSelections() => searchFrequency.Clear();

        public void RecordSelection(string selectedWord)
        {
            if (!searchFrequency.ContainsKey(selectedWord))
                searchFrequency[selectedWord] = 1;
            else
                searchFrequency[selectedWord]++;
        }

        public static Autocomplete Create(IEnumerable<string> words) => new Autocomplete(words);
        public static implicit operator Autocomplete(string[] words) => new Autocomplete(words);
        public static implicit operator Autocomplete(List<string> words) => new Autocomplete(words);

        [Serializable]
        public class ScoredResult
        {
            public AutocompleteResult Result;

            public ScoredResult(AutocompleteResult result, float score)
            {
                Result = result;
                Score = score;
            }

            public float Score;

            public override string ToString() => $"{Result.Word} ({Score})";
        }
    }

    [Serializable]
    public class AutocompleteResult
    {
        public string Word;
        public int[] MatchedIndexes;
        public int EditDistance;

        public AutocompleteResult(string word, int[] matchedIndexes, int editDistance)
        {
            Word = word;
            MatchedIndexes = matchedIndexes;
            EditDistance = editDistance;
        }
    }

    public class Trie
    {
        readonly int maxLevenshteinDistance;
        readonly TrieNode root = new TrieNode();
        Trie(int maxLevenshteinDistance = 2) => this.maxLevenshteinDistance = maxLevenshteinDistance;

        public static Trie Create(IEnumerable<string> words, int maxLevenshteinDistance = 2)
        {
            var trie = new Trie(maxLevenshteinDistance);
            foreach (var word in words) trie.Insert(word);
            return trie;
        }

        void Insert(string word)
        {
            var tokens = TokenizeWord(word);
            foreach (var token in tokens)
            {
                var node = root;
                foreach (var ch in token)
                {
                    var lowerCh = char.ToLower(ch);
                    if (!node.Children.ContainsKey(lowerCh))
                    {
                        node.Children[lowerCh] = new TrieNode();
                    }

                    node = node.Children[lowerCh];
                }

                node.IsEndOfWord = true;
                node.Word = word;
            }
        }

        List<string> TokenizeWord(string word) =>
            Regex.Split(word, @"[\s\.\-_]+|(?<=[a-z])(?=[A-Z])")
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.ToLower())
                .ToList();

        public IEnumerable<AutocompleteResult> ExactSearch(string query)
        {
            var tokens = TokenizeWord(query);
            foreach (var token in tokens)
            {
                var node = FindNode(token);
                if (node?.IsEndOfWord == true)
                    yield return new AutocompleteResult(node.Word, Enumerable.Range(0, token.Length).ToArray(), 0);
            }
        }

        public IEnumerable<AutocompleteResult> PrefixSearch(string prefix)
        {
            var node = FindNode(prefix);
            if (node == null) yield break;
            foreach (var word in CollectWords(node))
                yield return new AutocompleteResult(word, Enumerable.Range(0, prefix.Length).ToArray(), 0);
        }

        public IEnumerable<AutocompleteResult> FuzzySearch(string query) => FuzzySearchIterative(query);

        TrieNode FindNode(string prefix)
        {
            var node = root;
            foreach (var ch in prefix)
            {
                var lowerCh = char.ToLower(ch);
                if (!node.Children.TryGetValue(lowerCh, out var child)) return null;
                node = child;
            }

            return node;
        }

        IEnumerable<string> CollectWords(TrieNode node)
        {
            if (node.IsEndOfWord) yield return node.Word;

            foreach (var child in node.Children.Values)
            foreach (var word in CollectWords(child))
                yield return word;
        }

        IEnumerable<AutocompleteResult> FuzzySearchIterative(string query)
        {
            var queue = new Queue<(TrieNode node, int depth, int edits, List<int> matchedIndexes)>();
            queue.Enqueue((root, 0, 0, new List<int>()));

            while (queue.Count > 0)
            {
                var (node, depth, edits, matchedIndexes) = queue.Dequeue();
                if (edits > maxLevenshteinDistance)
                    continue;

                if (node.IsEndOfWord && depth >= query.Length)
                    yield return new AutocompleteResult(node.Word, matchedIndexes.ToArray(), edits);

                if (depth < query.Length)
                {
                    var queryChar = char.ToLower(query[depth]);

                    foreach (var result in node.Children)
                    {
                        var newMatchedIndexes = new List<int>(matchedIndexes);
                        if (result.Key == queryChar)
                        {
                            newMatchedIndexes.Add(depth);
                            queue.Enqueue((result.Value, depth + 1, edits, newMatchedIndexes));
                        }
                        else
                            queue.Enqueue((result.Value, depth + 1, edits + 1, newMatchedIndexes));

                        queue.Enqueue((result.Value, depth, edits + 1, newMatchedIndexes));
                    }

                    queue.Enqueue((node, depth + 1, edits + 1, matchedIndexes));
                }
                else
                    foreach (var kvp in node.Children)
                        queue.Enqueue((kvp.Value, depth + 1, edits, matchedIndexes));
            }
        }

        [Serializable]
        class TrieNode
        {
            public string Word;
            public bool IsEndOfWord;
            public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();
        }


        public static class Metaphone
        {
            public static string Encode(string word)
            {
                if (string.IsNullOrEmpty(word)) return "";

                word = word.ToUpper();

                if (word.Length == 1) return word;

                var result = new StringBuilder(word.Length);

                for (var i = 0; i < word.Length; i++)
                {
                    var current = word[i];

                    if (i == 0 || !IsVowel(current.ToString()))
                        switch (current)
                        {
                            case 'A':
                            case 'E':
                            case 'I':
                            case 'O':
                            case 'U':
                                if (i == 0) result.Append(current);
                                break;
                            case 'B':
                                result.Append("B");
                                break;
                            case 'C':
                                if (i < word.Length - 1)
                                {
                                    var next = word[i + 1];
                                    if (next != 'H')
                                    {
                                        result.Append("K");
                                    }
                                    else
                                    {
                                        result.Append("X");
                                        i++;
                                    }
                                }

                                break;
                            case 'D':
                                result.Append("T");
                                break;
                            case 'F':
                                result.Append("F");
                                break;
                            case 'G':
                                result.Append("K");
                                break;
                            case 'H':
                                if (i > 0 && !IsVowel(word[i - 1].ToString())) result.Append("H");
                                break;
                            case 'J':
                                result.Append("J");
                                break;
                            case 'K':
                                result.Append("K");
                                break;
                            case 'L':
                                result.Append("L");
                                break;
                            case 'M':
                                result.Append("M");
                                break;
                            case 'N':
                                result.Append("N");
                                break;
                            case 'P':
                                result.Append("P");
                                break;
                            case 'Q':
                                result.Append("K");
                                break;
                            case 'R':
                                result.Append("R");
                                break;
                            case 'S':
                                if (i < word.Length - 1)
                                {
                                    var next = word[i + 1];
                                    if (next != 'H')
                                    {
                                        result.Append("S");
                                    }
                                    else
                                    {
                                        result.Append("X");
                                        i++;
                                    }
                                }

                                break;
                            case 'T':
                                result.Append("T");
                                break;
                            case 'V':
                                result.Append("F");
                                break;
                            case 'W':
                                result.Append("W");
                                break;
                            case 'X':
                                result.Append("KS");
                                break;
                            case 'Y':
                                result.Append("Y");
                                break;
                            case 'Z':
                                result.Append("S");
                                break;
                        }
                }

                return result.ToString();
            }

            static bool IsVowel(string letter) => Array.IndexOf(vowels, letter) >= 0;
            readonly static string[] vowels = { "A", "E", "I", "O", "U" };
        }

    }
}

   