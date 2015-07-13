// 2013-Feb-8
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// An interface so that we don't have to expose the whole ZObject
/// </summary>
public interface IObject
{
   /// <summary>
   /// The description to use when briefly listing the item
   /// </summary>
   string shortDescription {get; set;}

   /// <summary>
   /// Adds a child to the children list
   /// </summary>
   /// <param name="Child"></param>
   void AddChild(IObject Child);
}


/// <summary>
///  This is a base class of the dimension and space identifiers
/// </summary>
partial class ZObject : IObject
{
   #region instance variables
   /// <summary>
   /// The name of this thing / object
   /// </summary>
   readonly internal object name;
   #endregion
   #region Internal names
   /// <summary>
   /// Unless a name was given, we "uniquely" name each object with a number
   /// </summary>
   static int nameInt=0;

   /// <summary>
   /// Creates a "unique" name for each object
   /// </summary>
   /// <returns>The new name</returns>
   static string newName()
   {
      return Interlocked.Add(ref nameInt, 1).ToString();
   }
   #endregion

   /// <summary>
   /// An internal constructor
   /// </summary>
   /// <param name="name">The name of the thing</param>
   internal ZObject(object name)
   {
      this . name = name;
   }
   public ZObject():this(newName())
   {
   }


   #region .NET interface
   /// <summary>
   /// Gets the hash code for this object
   /// </summary>
   /// <returns>The hash code</returns>
   public override int GetHashCode()
   {
      return name . GetHashCode();
   }

   /// <summary>
   /// Produces the string version of this object
   /// </summary>
   /// <returns>A string representative of this object</returns>
   public override string ToString()
   {
      return name.ToString();
   }


   /// <summary>
   /// Determines whether or not B is equal to this object
   /// </summary>
   /// <param name="B">An object to compare against</param>
   /// <returns>True if they are the same</returns>
   public override bool Equals(object a)
   {
      if (this == a)
         return true;
      if (!base.Equals(a))
         return false;
      if ((a is ZObject))
      {
         var A = (ZObject) a;
         return name . Equals(A.name) && nouns.Equals(A.nouns) && modifiers.Equals(A.modifiers);
      }
      return name . Equals(a);
   }
   #endregion
}

