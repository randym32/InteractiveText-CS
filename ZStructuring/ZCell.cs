using System;
using System.Collections.Generic;

public interface ICell:IObject
{
}

/// <summary>
/// This is a class for little bits of space that connect to others
/// </summary>
partial class ZCell:ZObject, ICell
{
   /// <summary>
   /// Maps a direction the ZCell in that direction
   /// </summary>
   internal Dictionary<WordList,ZEdge> edgeInDirection = new Dictionary<WordList,ZEdge>();

   /// <summary>
   /// An internal constructor
   /// </summary>
   /// <param name="name">The name of the thing</param>
   internal ZCell(object name)
      :base(name)
   {
   }

   /// <summary>
   /// Finds the object that best matches the phrase
   /// </summary>
   /// <param name="phrase">The words to match</param>
   /// <param name="score">The score for the best match: 0 if no match[out]</param>
   /// <param name="numWordsMatch">The number of words that match [out]</param>
   /// <param name="bestMatch">The object that best matches [out]</param>
   /// <param name="seen">Set of objects already examined</param>
   internal override void matchInContext(Lexer lexer,
               ref double score, ref int numWordsMatch,
               ref ZObject bestMatch,
               ref LexState lexerState, Dictionary<ZRoot,object> seen)
   {
      // Make a note of where we are in the text
      var state = lexer.Save();
      base.matchInContext(lexer, ref score, ref numWordsMatch, ref bestMatch,
                           ref lexerState, seen);
      // Go back to the saved point in the text
      lexer.Restore(state);

      // Check to see if it a reference to the other party of a relationship
      //  a. Make a list of objects; eg connected to this via relationship/2
      //   i. If the link has a gatekeeper, see if the gate is open or closed
      // b. See which matches best the noun phrases


      //TODO: Bump the score to keep things closer to ourselves ?
      // Scan in each of the directions
      // We can't traverse the edge though
      foreach (var pair in edgeInDirection)
      {
         // Match the direction
         var myNumWords=0;
         var myScore = pair.Key.matchMembers(lexer, out myNumWords);
         // Keep if matched atleast that many words,
         // or had a better score
         if (myNumWords >= numWordsMatch || myScore > score)
         {
            // We are better than the previous
            bestMatch     = pair.Value.sink; // TODO: check door!
            score         = myScore;
            numWordsMatch = myNumWords;
            lexerState    = lexer.Save();
         }

         // Go back to the saved point in the text
         lexer.Restore(state);

         // Match the room or gating door
         pair.Value.matchInContext(lexer, ref score, ref numWordsMatch,
                                    ref bestMatch, ref lexerState, seen);
         // Go back to the saved point in the text
         lexer.Restore(state);
      }
   }

}
