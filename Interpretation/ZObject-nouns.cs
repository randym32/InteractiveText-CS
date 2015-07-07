// 2015-Jun-22
using System;
using System.Collections;
using System.Collections.Generic;
partial class ZObject : ZRoot
{
   /// <summary>
   /// The nouns associated with this object
   /// </summary>
   public WordList nouns;

   /// <summary>
   /// The modifiers associated with this object
   /// </summary>
   public WordList modifiers;

   /// <summary>
   /// Finds the object that best directly matches the phrase.
   /// 
   /// This naively finds the single best match.  Future is to return a list
   /// </summary>
   /// <param name="lexer">The text being matched</param>
   /// <param name="numWordsMatch">The number of words that match [out]</param>
   /// <returns>The object that matches; null on error</returns>
   internal ZObject matchInContext(Lexer lexer, out int numWordsMatch,
                          ref LexState lexerState)
   {
      var seen = new Dictionary<ZRoot,object>();

      // The score board for the search
      var score = 0.0;
      numWordsMatch = 1; // Minimum number of words to match
      ZObject bestMatch = null;

      // Search for the best item within self, children and parents
      matchInContext(lexer, ref score, ref numWordsMatch, ref bestMatch,
                     ref lexerState, seen);

      // Return the best match
      return bestMatch;
   }


   /// <summary>
   /// Finds the object that best matches the phrase
   /// </summary>
   /// <param name="lexer">The text being matched</param>
   /// <param name="score">The score for the best match: 0 if no match[out]</param>
   /// <param name="numWordsMatch">The number of words that match [out]</param>
   /// <param name="bestMatch">The object that best matches [out]</param>
   /// <param name="seen">Set of objects already examined</param>
   internal virtual void matchInContext(Lexer lexer, 
               ref double score, ref int numWordsMatch,
               ref ZObject bestMatch,
               ref LexState lexerState, Dictionary<ZRoot,object> seen)
   {
      var state = lexer.Save();
      // Search for the best item within self and children
      match(lexer, ref score, ref numWordsMatch, ref bestMatch, ref lexerState, seen);

      // bump the search to parents and their children
      if (null != Parent)
      {
         lexer.Restore(state);
         //TODO: Bump the score to keep things closer to ourselves ?
         Parent.matchInContext(lexer, ref score, ref numWordsMatch,
                                 ref bestMatch, ref lexerState, seen);
      }
   }


   
   /// <summary>
   /// Finds the object that best matches the phrase
   /// </summary>
   /// <param name="lexer">The text being matched</param>
   /// <param name="score">The score for the best match: 0 if no match[out]</param>
   /// <param name="numWordsMatch">The number of words that match [out]</param>
   /// <param name="bestMatch">The object that best matches [out]</param>
   /// <param name="seen">Set of objects already examined</param>
   internal void match(Lexer lexer, ref double score,
               ref int numWordsMatch,
               ref ZObject bestMatch,
               ref LexState lexerState, Dictionary<ZRoot,object> seen)
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