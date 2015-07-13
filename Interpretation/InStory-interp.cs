// 2015-6-27
using System ;
using System . Collections . Generic;

public partial class InStory
{
   /// <summary>
   /// Finds the object that best directly matches the phrase.
   /// 
   /// This naively finds the single best match.  Future is to return a list
   /// </summary>
   /// <param name="lexer">The text being matched</param>
   /// <param name="numWordsMatch">The number of words that match [out]</param>
   /// <returns>The object that matches; null on error</returns>
   object matchInContext(ZObject addressedTo, Lexer lexer, out int numWordsMatch,
                          ref LexState lexerState)
   {
      // A table of already examined objects
      var seen = new Dictionary<object,object>();

      // The score board for the search
      var score     = 0.0; // The score for the best match: 0 if no match
      numWordsMatch =   1; // The number of words that match: Minimum of 1
      object bestMatch = null;// The object that best matches
      matchInContext(addressedTo, lexer, ref score, ref numWordsMatch,
                     ref bestMatch, ref lexerState, seen);

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
   void matchInContext(ZObject addressedTo, Lexer lexer, 
               ref double score, ref int numWordsMatch,
               ref object bestMatch,
               ref LexState lexerState, Dictionary<object,object> seen)
   {
      // Make a note of where we are in the text
      var state = lexer.Save();
      // Search for the best item within the addressedTo object and it's children
      addressedTo.match(lexer, ref score, ref numWordsMatch, ref bestMatch,
         ref lexerState, seen);

      // Check to see if it a reference to the other party of a relationship
      //  a. Make a list of objects; eg connected to this via relationship/2
      //   i. If the link has a gatekeeper, see if the gate is open or closed
      // b. See which matches best the noun phrases

      //TODO: Bump the score to keep things closer to ourselves ?
      // Scan in each of the directions
      // We can't traverse the edge though
      foreach (var pair in relations)
      {
         // Go back to the saved point in the text
         lexer.Restore(state);
         // Match the relation/direction
         pair.Key.match(lexer, ref score, ref numWordsMatch, ref bestMatch,
            ref lexerState, seen);

         if (!(pair.Value is SparseGraph<ZObject>))
            continue;

         // Get the edge.. if none, then nothing can match
         List<Edge<ZObject>> edges;
         if (!((SparseGraph<ZObject>)pair.Value).successors(addressedTo, out edges) || edges.Count < 1)
         {
            // There is no specific destination here for this location;
            // the player could have given an abstract direction without a
            // concrete direciton.  We'll keep the direction as a match,
            // if it did match.
            continue;
         }

         // If the direction matched, we will keep the edge as it's referrant
         if (bestMatch == pair.Key) 
         {
            // We match the direction, so we are referring to the edge
            bestMatch     = edges[0];
         }

         // Go back to the saved point in the text
         lexer.Restore(state);

         // Match the room or gating door
         // Search for the best item within the addressedTo object and it's
         // children
         match(edges[0], lexer, ref score, ref numWordsMatch, ref bestMatch,
            ref lexerState, seen);
      }
      // Go back to the saved point in the text
      lexer.Restore(state);

      // bump the search to parents and their children
      if (null != addressedTo.Parent)
      {
         //TODO: Bump the score to keep things closer to ourselves ?
         matchInContext(addressedTo.Parent, lexer, ref score, ref numWordsMatch,
                                 ref bestMatch, ref lexerState, seen);
      }
   }

   


   /// <summary>
   /// Gets the objects referred to by each of the noun phrases
   /// </summary>
   /// <param name="lexer">The lexer that provides the input</param>
   /// <param name="addressedTo"></param>
   /// <param name="err"></param>
   /// <returns></returns>
   List<object> getNounPhrases(Lexer lexer, ZObject addressedTo, Err err)
   {
      // The list of referred to objects
      var ret = new List<object>();
      lexer.Preprocess();

      // Map the rest of the words to nouns
      while (!lexer.EOF)
      {
         int matchLength  = 0;
         LexState lexerState = null;
         // Match the next noun phrase
         var t = matchInContext(addressedTo, lexer, out matchLength, ref lexerState);
         // 5. if no noun was mapped
         if (null == t)
         {
            // Try the main relations (isa)

            // error could not understand at index
            err . SB . AppendFormat("The phrase \"{0}\" isn't understood.", 
                                    lexer.ToEndOfLine().Trim());
            return null;
         }

         // Save the noun phrase
         ret.Add(t);

         // Move past the words in that subphrase
         if (null != lexerState)
            lexer.Restore(lexerState);
         lexer.Preprocess();
      }

      return ret;
      // couldNotUnderstand(string, startingAt);
      // isAmbiguous(words);
   }


   /// <summary>
   /// Interprete and carry out the statement
   /// </summary>
   /// <param name="lexer">The lexer for the input</param>
   /// <returns>True if interpreted, false if not</returns>
   public bool interp(Lexer lexer)
   {
      Console.WriteLine("> {0}", lexer.Str);
      // Create something to hold the error
      var err = new Err(Err.NoLinkTo);
      // Interpret this
      var ret = interp (null, lexer, err);
      // Emit the error message
      Console.WriteLine(err.ToString());
      return ret;
   }

   /// <summary>
   /// This is used to invoked the handler for the verb
   /// </summary>
   /// <param name="stmt">The statment to interpret</param>
   /// <param name="line">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True on success, false on error</returns>
   bool interp(propositionContext ctxt,Lexer lexer, Err err)
   {
      // A reference to where the statement was made
      var src = new SrcInFile(null, lexer.Line);
      // See if there is a first noun, to whom the message is addressed
      var addressedTo = player;
      int matchLength  = 0;
      LexState lexerState = null;
      var tmp = matchInContext(((ZObject) player), lexer, out matchLength,
         ref lexerState);
      if (null != tmp && tmp is ZObject)
      {
         // Use the new object as the item to be commanded
         addressedTo = (ZObject) tmp;
         // Move past the words in that subphrase
         if (null != lexerState)
            lexer.Restore(lexerState);
      }

      // The handler for the different number of arguments
      dStatement[] handlers = null;

      // Find a matching verb phrase
      var phrase = findVerbPhrase(lexer);

      // Get the noun phrases
      var args = getNounPhrases(lexer, (ZObject) player, err);
      var n = null == args? 0: args.Count;

      // See if we need to guess the handler for the noun
      if (null == phrase)
      {
         // If there was nothing resolved about the nouns
         if (null == args)
            args = new List<object>();
         if (addressedTo != player)
         {
            // Move the "addressed to" back to the front
            addressedTo = player;
            args.Insert(0, tmp);
            n = args.Count;
         }

         // See if it said nothing
         if (0 == args.Count)
            return false;

         // See if the first item is a 
         tmp = args[0];

         // If it is a direction roll it around
         if (tmp is ZObject && !isKind((ZObject) tmp, dirKind))
         {
            // Format an error message
            err . linkTo(src);
            err . SB . AppendFormat("What do you mean?");
            return false;
         }

         // It is a direction 
         // Ok, lets just make it go?
         phrase = findVerbPhrase("go");
      }

      // If can't find the verb at all, complain
      if (null == phrase || !verbs.TryGetValue(phrase, out handlers))
      {
         // Format an error message
         err . linkTo(src);
         err . SB . AppendFormat("The verb isn't understood.");
         return false;
      }

      // If there isn't a form for that number of verbs, complain
      var h = n >= handlers . Length ? null : handlers[n];
      if (null == h)
      {
         err . linkTo(src);
         err . SB . AppendFormat("Verb '{0}' isn't known for that arity.", phrase);
         return false;
      }

      // Call the delegate
      return h(ctxt, args, src, err);
   }
}
