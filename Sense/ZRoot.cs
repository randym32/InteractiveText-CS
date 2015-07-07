// 2013-Feb-8

/// <summary>
///  This is a base class of the dimension and space identifiers
/// </summary>
abstract partial class ZRoot
{
   #region instance variables
   /// <summary>
   /// The name of this thing / object
   /// </summary>
   readonly internal object name;
   #endregion

   /// <summary>
   /// An internal constructor
   /// </summary>
   /// <param name="name">The name of the thing</param>
   internal ZRoot(object name)
   {
      this . name = name;
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
      if ((a is ZRoot))
         return name . Equals(((ZRoot)a).name);
      return name . Equals(a);
   }
   #endregion
}

