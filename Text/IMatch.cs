using System;
using System.Collections.Generic;


interface IMatch<Tnode>
{
   /// <summary>
   /// Finds the object that best matches the phrase
   /// </summary>
   /// <param name="lexer">The text being matched</param>
   /// <param name="score">The score for the best match: 0 if no match[out]</param>
   /// <param name="numWordsMatch">The number of words that match [out]</param>
   /// <param name="bestMatch">The object that best matches [out]</param>
   /// <param name="seen">Set of objects already examined</param>
   void match(Lexer lexer, ref double score,
               ref int numWordsMatch,
               ref Tnode bestMatch,
               ref LexState lexerState, Dictionary<Tnode,object> seen);
}
