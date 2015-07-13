// 2013-Feb-12
using System;
using System.Collections.Generic;


/// <summary>
/// This is a common chunk of code for handling lists of words that
/// are expected to work as a single unit
/// </summary>
partial class WordList
{
   /// <summary>
   /// The list of words
   /// </summary>
   readonly internal string[] words;

   /// <summary>
   /// This creates a list of words that are expected to act as a group
   /// </summary>
   /// <param name="words">The list of words</param>
   public WordList(string[] words)
   {
      this . words = words;
   }
   public WordList(List<string> words)
   {
      this.words = words.ToArray();
   }


   /// <summary>
   /// This creates a list of words that are expected to act as a group
   /// </summary>
   /// <param name="words">The list of words</param>
   public static implicit operator WordList(List<string> words)
   {
      // Create the object
      return new WordList(words.ToArray());
   }

   /// <summary>
   /// This creates a list of words that are expected to act as a group
   /// </summary>
   /// <param name="words">The list of words</param>
   public static implicit operator WordList(object[] objects)
   {
      var L = objects.Length;
      var words = new string[L];
      for (var Idx = 0; Idx < L; Idx++)
         words[Idx] = objects[Idx].ToString();

      // Create the object
      return new WordList(words);
   }


   /// <summary>
   /// This makes it easy to pass a string as an argument to things,
   /// and automatically convert to their preferred form
   /// </summary>
   /// <param name="str">The string to parse into words</param>
   /// <returns>A wordlist</returns>
   public static implicit operator WordList(string str)
   {
      // Parse the string into words
      var lex = new Lexer(str);
      lex.Preprocess();
      var words = new List<string>();
      while (!lex.EOF)
      {
         // Get the next word
         var word = lex.Symbol();
         if (null == word)
            break;
         // Add the word
         words.Add(word);
         // Skip any white space
         lex.Preprocess();
      }
      // Check for words
      if (words.Count < 1)
         return null;

      // Create the object
      return (WordList) words;
   }

   /// <summary>
   /// This is used to tell if two words are the same.
   /// TODO: use metaphone
   /// </summary>
   /// <param name="a"></param>
   /// <param name="b"></param>
   /// <returns></returns>
   internal static bool wordEq(string a, string b)
   {
      // perform a simple match
      if (a == b)
         return true;
      // catch one being null
      if (null == a || null == b)
         return false;
      return a.Equals(b, StringComparison.CurrentCultureIgnoreCase);
   }


   #region .NET interface
   /// <summary>
   /// Gets the hash code for this object
   /// </summary>
   /// <returns>The hash code</returns>
   public override int GetHashCode()
   {
      unchecked // Overflow is fine, just wrap
      {
         int hash = 17;
         for (var I = 0; I < words.Length; I++)
            hash = hash * 23 + words[I].ToUpper().GetHashCode();
         return hash;
      }
   }

   /// <summary>
   /// Produces the string version of this object
   /// </summary>
   /// <returns>A string representative of this object</returns>
   public override string ToString()
   {
      var s = words[0];
      for (var I = 1; I < words.Length; I++)
         s+= " "+words[I];
      return s;
   }


   /// <summary>
   /// Determines whether or not "a" is equal to this object
   /// </summary>
   /// <param name="a">An object to compare against</param>
   /// <returns>True if they are the same</returns>
   public override bool Equals(object a)
   {
      if (this == a)
         return true;
      if (!(a is WordList))
         return false;
      var b= (WordList) a;
      var L =words.Length;
      if (b.words.Length != L)
         return false;
      for (var I = 0; I <L; I++)
         if (!wordEq(words[I], b.words[I]))
            return false;
      return true;
   }
   #endregion
}
