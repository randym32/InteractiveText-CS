// 2015-6-27
using System ;
using System . Collections . Generic;

public partial class InStory
{
   /// <summary>
   /// Gets the objects referred to by each of the noun phrases
   /// </summary>
   /// <param name="lexer">The lexer that provides the input</param>
   /// <param name="addressedTo"></param>
   /// <param name="err"></param>
   /// <returns></returns>
   List<ZObject> getNounPhrases(Lexer lexer, ZObject addressedTo, Err err)
   {
      // The list of referred to objects
      var ret = new List<ZObject>();
      lexer.Preprocess();

      // Map the rest of the words to nouns
      while (!lexer.EOF)
      {
         int matchLength  = 0;
         LexState lexerState = null;
         // Match the next noun phrase
         var t = addressedTo.matchInContext(lexer, out matchLength, ref lexerState);
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

   public bool interp(Lexer lexer)
   {
      var err = new Err(Err.NoLinkTo);
      var ret = interp (null, lexer, err);
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
      int line = lexer.Line;
      // 1. See if there is a first noun, to whom the message is address
      var addressedTo = player;
      int matchLength  = 0;
      LexState lexerState = null;
      var tmp = ((ZObject) player).matchInContext(lexer, out matchLength, ref lexerState);
      if (null != tmp)
      {
         // Use the new object as the item to be commanded
         addressedTo = tmp;
      }
      // Move past the words in that subphrase
      if (null != lexerState)
         lexer.Restore(lexerState);

      // The handler for the different number of arguments
      dStatement[] handlers = null;

      // Find a matching verb phrase
      var phrase = findVerbPhrase(lexer);

      // If can't find the verb at all, complain
      if (null == phrase || !verbs.TryGetValue(phrase, out handlers))
      {
         // Format an error message
         err . linkTo(line);
         err . SB . AppendFormat("The verb isn't understood.");
         return false;
      }

      // Get the noun phrases
      var args = getNounPhrases(lexer, (ZObject) player, err);

      // If there isn't a form for that number of verbs, complain
      var n = null == args? 0: args.Count;
      var h = n >= handlers . Length ? null : handlers[n];
      if (null == h)
      {
         err . linkTo(line);
         err . SB . AppendFormat("Verb '{0}' isn't known for that arity.", phrase);
         return false;
      }

      // Call the delegate
      return h(ctxt, args, line, err);
   }
}
