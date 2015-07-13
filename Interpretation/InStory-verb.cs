// 2013-Feb-10
using System ;
using System . Collections . Generic;

public partial class InStory
{
   /// <summary>
   /// This a function that handles functions of a verb
   /// </summary>
   /// <param name="nouns"></param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="src">The file and line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   delegate bool dStatement(propositionContext context, List<object> Items, SrcInFile src, Err err);


   /// <summary>
   /// This is the table maping verbs to the delegates that try to
   /// interpret them
   /// </summary>
   Dictionary<WordList,dStatement[]> verbs = new Dictionary<WordList,dStatement[]>();

   /// <summary>
   /// Finds a verb phrase that matches
   /// </summary>
   /// <param name="lex"></param>
   /// <param name="stateEndVerbPhrase"></param>
   /// <returns></returns>
   WordList findVerbPhrase(Lexer lex)
   {
      // First, make a list of all verb phrases
      var candidates = new List<WordList>();
      foreach (var item in verbs)
      {
         candidates.Add(item.Key);
      }

      // The words of the verb phrase
      var verbWords = new List<string>();
      var prevState = lex.Save();
      LexState stateEndVerbPhrase=prevState;

      // Next, get words from the lexer as long they match a phrase
      lex.Preprocess();
      while (!lex.EOF)
      {
         var word = lex.Symbol();
         lex.Preprocess();
         
         // See if the word matches any candidate
         var newCandidates = new List<WordList>();
         var numVerbWords = verbWords . Count;
         foreach (var candidate in candidates)
         {
            // Skip "shorter" phrases
            if (numVerbWords >= candidate.words.Length)
                continue;
            // Get the word for this far in
            var item = candidate.words[numVerbWords];

            // Is there a match?
            // Check for exact, but caseless match; or a partial
            if (item.Equals(word, StringComparison.CurrentCultureIgnoreCase)
               || ((word. Length < item.Length)
                     && item.Substring(0, word.Length).Equals(word, StringComparison.CurrentCultureIgnoreCase)))
            {
               // keep it
               newCandidates.Add(candidate);
            }
         }
         // Did anyone match?
         if (newCandidates . Count < 1)
            break;
         // Save the word and the matches
         candidates = newCandidates;
         verbWords.Add(word);
         stateEndVerbPhrase = lex.Save();
      }


      // Check to see if any matched
      if (candidates.Count < 1 || verbWords.Count < 1)
      {
         lex.Restore(prevState);
         return null;
      }

      // Jump back tot he end of the verb phrase
      lex.Restore(stateEndVerbPhrase);

      /// The words for the verb phrase
      WordList verbWordList = new WordList(verbWords.ToArray());

      // Rank the matched phrases, finding the best one
      var bestScore = 0.0;
      WordList bestCandidate = null;
      foreach (var candidate in candidates)
      {
         // Assign a score to the candidate.
         var score = candidate.score(verbWordList);
         // Is it a better match?
         if (score > bestScore)
         {
            // Keep it
            bestScore = score;
            bestCandidate=candidate;
         }
      }

      // Return the best matched phrase
      return bestCandidate;
   }



   /// <summary>
   /// This adds a handler for a verb for a given number of verbs
   /// </summary>
   /// <param name="verb">The verb in question</param>
   /// <param name="n">The number of nouns as argument</param>
   /// <param name="handler">The handler</param>
   void add(WordList verb, int n, dStatement handler)
   {
      // The handler for the different number of arguments
      dStatement[] handlers;

      // If can't find the verb at all, create an entry
      if (!verbs.TryGetValue(verb, out handlers))
      {
         verbs[verb] = handlers = new dStatement[1];
      }

      // expand the list
      if (n >= handlers . Length)
      {
         Array . Resize(ref handlers, n+1);
         verbs[verb]=handlers;
      }

      // if there was a handler already, complain
      if (null != handlers[n])
         throw new System.Exception("already a handler");

      // Put the handler on
      handlers[n] = handler;
   }
}
