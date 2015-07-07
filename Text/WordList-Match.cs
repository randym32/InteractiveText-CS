// 2015-6-24
using System;
using System.Collections;
using System.Collections.Generic;

partial class WordList
{
   /// <summary>
   /// Count the number of words in this list that matches in the phrase
   /// </summary>
   /// <param name="lexer">The words to match</param>
   /// <param name="count">The number of words that match [out]</param>
   /// <returns>Score for the match: 0 if no match</returns>
   internal double matchMembers(Lexer lexer, out int count)
   {
      var cnt = 0  ; // The number of words that match
      var score=0.0; // The score to match

      // for each of the remaining words
      while (!lexer.EOF)
      {
         lexer.Preprocess();
         var word =lexer.Symbol();

         // See if it matches a modifier
         double score1 = matchMembers(word);

         // check for a minimum score
         if (score1 <= 0.0) break;

         // Update the tracking info
         cnt++;
         score += score1;
      }

      // return the results
      count = cnt;
      return score;
   }


   /// <summary>
   /// Returns a score of how well the word matches the set of words
   /// </summary>
   /// <param name="word">The word in question (uppercase)</param>
   /// <returns>1.0 for a perfect match, 0 for no match, otherwise how close the match was</returns>
   double matchMembers(string word)
   {
      /// The best score
      double maxMatch = 0.0;

      // Check for the maximal match in the list
      foreach (var item in words)
      {
         // Check for exact, but caseless match
         if (item.Equals(word, StringComparison.CurrentCultureIgnoreCase))
            return 1.0;

         // Check for a partial match
         if ((word. Length < item.Length)
             && item.Substring(0, word.Length).Equals(word, StringComparison.CurrentCultureIgnoreCase))
         {
            // Calculate a score for this match
            double score = ((double)item.Length)/(double)item.Length;
            // Keep the best (highest) score of all the words that might match
            if (score > maxMatch)
            {
               maxMatch = score;
            }
         }
      }
      return maxMatch;
   }

   /// <summary>
   /// Attempt to score how well this WordList matches the actual input
   /// </summary>
   /// <param name="input">The users words</param>
   /// <returns>A score representing how well it matches</returns>
   internal double score(WordList input)
   {
      var score = 1.0;
      // for each word
      int L = words.Length;
      if (L > input.words.Length)
         return 0.0;
      for (var I = 0; I <L; I++)
      {
         var item = words[I];
         var inWord=input.words[I];
         // Check for exact, but caseless match
         if (item.Equals(inWord, StringComparison.CurrentCultureIgnoreCase))
         {
            score += 1.0;
            continue;
         }

         // Check for a partial match
         if ((inWord. Length < item.Length)
             && item.Substring(0, inWord.Length).Equals(inWord, StringComparison.CurrentCultureIgnoreCase))
         {
            // Calculate a score for this match
            score += ((double)item.Length)/(double)item.Length;
         }
      }

      // return the result
      return score;
   }
}
