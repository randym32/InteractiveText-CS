using System;
using System.Collections.Generic;
using System.Text;


partial class ZEdge:ZRoot
{
   /// <summary>
   /// This is the (shared) object that may regulate passage,
   /// or has properties for the edge
   /// </summary>
   internal ZObject door;

   /// <summary>
   /// This is the other side of the edge
   /// </summary>
   internal ZCell sink;

   internal ZEdge(string name, ZCell sink, ZObject door)
      :base(name)
   {
      this.sink = sink;
      this.door = door;
   }


   /// <summary>
   /// Finds the object that best matches the phrase
   /// </summary>
   /// <param name="phrase">The words to match</param>
   /// <param name="index">The first word in phrase to match</param>
   /// <param name="score">The score for the best match: 0 if no match[out]</param>
   /// <param name="numWordsMatch">The number of words that match [out]</param>
   /// <param name="bestMatch">The object that best matches [out]</param>
   /// <param name="seen">Set of objects already examined</param>
   internal void matchInContext(Lexer lexer, ref double score,
               ref int numWordsMatch,
               ref ZObject bestMatch,
               ref LexState lexerState, Dictionary<ZRoot,object> seen)
   {
      // Skip if we've already seen this object
      if (seen.ContainsKey(this))
         return;
      // Add ourself to prevent cycles
      seen[this] = this;

      /// We only check to see if the door matches the phrase
      if (null != door)
         door.matchInContext(lexer, ref score, ref numWordsMatch,
                             ref bestMatch, ref lexerState, seen);
      else
      {
         // Should only match if no door, or door is open
         if (null != sink )
            // we only want match -- matchInContext checks the stuff from the parents..
            sink.match(lexer, ref score, ref numWordsMatch,
                             ref bestMatch, ref lexerState, seen);
      }

   }
}
