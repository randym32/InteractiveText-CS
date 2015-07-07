// 2013-Feb-10
using System ;
using System.Collections.Generic;

partial class InStory
{
   /// <summary>
   /// This is the kind that is the base kind for "element of space"
   /// </summary>
   static ZRoot roomKind = new ZObject("room");

   /// <summary>
   /// This is the kind that is the base kind for direction
   /// </summary>
   static ZRoot dirKind = new ZObject("direction");

   static WordList east = "east easterly eastward eastwardly";
   static WordList west = "west westward westwardly";
   static WordList north= "north northerly northward northwardly";
   static WordList south= "south southerly southward southwardly";

   /// <summary>
   /// The various directions
   /// </summary>
   static List<WordList> directions = new List<WordList>(){east, west, north,
      south};

   /// <summary>
   /// See if the phrase matches a direction
   /// </summary>
   /// <param name="lexer"></param>
   /// <returns></returns>
   static WordList matchDirection(Lexer lexer)
   {
      // See if the text directly refers to a relationship name (eg a direction)
      //  a. Make a list of relationships
      //  b. See which matches best

      var orig = lexer.Save();
      // Do a match on the directions keeping the best
      WordList best=null;
      var bestScore = 0.0;
      var bestNumWords = 0;
      var bestSave = orig;
      foreach (var dir in directions)
      {
         lexer.Restore(orig);
         var myNumWords=0;
         // Get a score of how well it matches
         var myScore = dir.matchMembers(lexer, out myNumWords);
         if (myNumWords > bestNumWords || myScore > bestScore)
         {
            // Save the results
            bestSave     = lexer.Save();
            bestNumWords = myNumWords;
            bestScore    = myScore;
            best         = dir;
         }
      }
      // Move past the word/phrase
      lexer.Restore(bestSave);
      // 
      return best;
   }
}
