// 2015-Jun-22
using System;
using System.Collections;
using System.Collections.Generic;
partial class ZObject : IMatch<object>
{
   /// <summary>
   /// The nouns associated with this object
   /// </summary>
   public WordList nouns;

   /// <summary>
   /// The modifiers associated with this object
   /// </summary>
   public readonly WordList modifiers;

   /// <summary>
   /// Finds the object that best matches the phrase
   /// </summary>
   /// <param name="lexer">The text being matched</param>
   /// <param name="score">The score for the best match: 0 if no match[out]</param>
   /// <param name="numWordsMatch">The number of words that match [out]</param>
   /// <param name="bestMatch">The object that best matches [out]</param>
   /// <param name="seen">Set of objects already examined</param>
   public void match(Lexer lexer, ref double score,
               ref int numWordsMatch,
               ref object bestMatch,
               ref LexState lexerState, Dictionary<object,object> seen)
   {
      // Skip if we've already seen this object
      if (seen.ContainsKey(this))
         return;
      // Add ourself to prevent cycles
      seen[this] = this;
      var state = lexer.Save();

      // Do a match on ourselves
      var myNumWords=0;
      var myScore = matchMembers(lexer, out myNumWords);

      // Keep if matched atleast that many words,
      // or had a better score
      if (myNumWords >= numWordsMatch || myScore > score)
      {
         // We are better than the previous
         bestMatch     = this;
         score         = myScore;
         numWordsMatch = myNumWords;
         lexerState    = lexer.Save();
      }

      // Perfom a match on the children
      for (var I = 0; I < NumChildren; I++)
      {
         lexer.Restore(state);
         // Do a match
         Children[I].match(lexer, ref score, ref numWordsMatch, ref bestMatch, ref lexerState, seen);
      }
   }

   /// <summary>
   /// Count the number of modifiers and nouns that this object matches in the phrase
   /// </summary>
   /// <param name="lexer">The text being matched</param>
   /// <param name="count">The number of words that match [out]</param>
   /// <returns>Score for the match: 0 if no match</returns>
   double matchMembers(Lexer lexer, out int count)
   {
      // See if it matches a modifier
      int numModifiers = 0;
      var modScore = null == modifiers?0.0:modifiers.matchMembers(lexer, out numModifiers);
      // See if it matches a noun
      int numNouns = 0;
      var nounScore= null == nouns?0.0:nouns.matchMembers(lexer, out numNouns);

      // We must match a noun
      if (numNouns < 1)
      {
         // We didn't match a noun, so we didn't match at all
         count = 0;
         return 0.0;
      }

      // Our result is the modifiers and nouns
      count = numModifiers + numNouns;
      return modScore + nounScore;
   }
}