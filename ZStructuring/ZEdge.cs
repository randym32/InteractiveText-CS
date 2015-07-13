using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Represents a directed link to a node
/// </summary>
partial class InStory
{
   /// <summary>
   /// Checks to see if the gate if open or not
   /// </summary>
   /// <param name="gate"></param>
   /// <returns></returns>
   bool isOpen(ZObject gate)
   {
      return true;
   }

   /// <summary>
   /// Checks to see if we can go thru the edge/into the room
   /// </summary>
   /// <returns>true if we can traverse into it, false otherwise</returns>
   internal bool canTraverseTo(Edge<ZObject> edge)
   {
      if (null == edge.gate || isOpen(edge.gate))
         return true;
      return false;
   }

#if false
   /// <summary>
   /// Finds the object that best matches the phrase
   /// </summary>
   /// <param name="phrase">The words to match</param>
   /// <param name="index">The first word in phrase to match</param>
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
//      edge.match(lexer, ref score, ref numWordsMatch, ref bestMatch, ref lexerState, seen);
#if true
      /// We only check to see if the gate matches the phrase
      if (null != edge.gate)
         edge.gate.match(lexer, ref score, ref numWordsMatch,
                             ref bestMatch, ref lexerState, seen);
      // Should only match if no door, or door is open
      if (null != edge.sink && (null == edge.gate || isOpen(edge.gate)))
      {
         // we only want match -- matchInContext checks the stuff from the parents..
         edge.sink.match(lexer, ref score, ref numWordsMatch,
                     ref bestMatch, ref lexerState, seen);
      }
#endif
   }
#endif

   /// <summary>
   /// Finds the object that best matches the phrase
   /// </summary>
   /// <param name="phrase">The words to match</param>
   /// <param name="index">The first word in phrase to match</param>
   /// <param name="score">The score for the best match: 0 if no match[out]</param>
   /// <param name="numWordsMatch">The number of words that match [out]</param>
   /// <param name="bestMatch">The object that best matches [out]</param>
   /// <param name="seen">Set of objects already examined</param>
   internal static void match(Edge<ZObject> edge, Lexer lexer, ref double score,
               ref int numWordsMatch,
               ref object bestMatch,
               ref LexState lexerState, Dictionary<object,object> seen)
   {
      /// We only check to see if the gate matches the phrase
      if (null != edge.gate)
         edge.gate.match(lexer, ref score, ref numWordsMatch,
                             ref bestMatch, ref lexerState, seen);
      // Should only match if no door, or door is open
      if (null != edge.sink && (null == edge.gate /*|| isOpen(gate)*/))
      {
         // we only want match -- matchInContext checks the stuff from the parents..
         edge.sink.match(lexer, ref score, ref numWordsMatch,
                     ref bestMatch, ref lexerState, seen);
      }
   }

}