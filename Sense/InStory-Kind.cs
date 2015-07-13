using System;
using System.Collections.Generic;

public partial class InStory
{
   /// <summary>
   /// The kind(s) of thing that an object is.
   /// Kind is used to group items when print descriptions;
   /// used to resolve references to generic items
   /// </summary>
   internal static ZObject __kind = new ZObject("kind");

   /// <summary>
   /// Maps a direction to a set of object in that direction
   /// f:direction -> object -> {object}*
   /// f:instance  -> object*
   /// </summary>
   internal Dictionary<ZObject,object> relations =
      new Dictionary<ZObject,object>();


   /// <summary>
   /// Asserts that the object is of kind "kind"
   /// </summary>
   /// <param name="obj">The object</param>
   /// <param name="kind">It's kind</param>
   ZObject Kind(ZObject obj, ZObject kind)
   {
      // It is a kind
      ((SparseGraph<ZObject>)relations[__kind]).After(obj, new Edge<ZObject>(kind, null));
      return obj;
   }

   /// <summary>
   /// Checks to see if object (obj) is of the specified kind. 
   /// </summary>
   /// <param name="obj"></param>
   /// <param name="kind"></param>
   /// <returns></returns>
   bool isKind(ZObject obj, ZObject kind)
   {
      return ((SparseGraph<ZObject>)relations[__kind]).reachable(obj, kind, delegate(ZObject n)
      {
         return ((Dictionary<ZObject, ZObject>)relations[_instance]).ContainsKey(n);
      });
   }

   #region Basic Entity Types
   /// <summary>
   /// The prototype for cells of space
   /// </summary>
   internal static ZObject cellKind = new ZObject("cell");

   /// <summary>
   /// The prototype for people
   /// </summary>
   internal static ZObject personKind = new ZObject("person");

   /// <summary>
   /// Creates a new kind
   /// </summary>
   /// <param name="name">The name of the kind</param>
   /// <param name="parentKind">The parent kind (may be null)</param>
   /// <returns>The new kind</returns>
   ZObject Kind(string name, ZObject parentKind)
   {
      // Create the new object
      var ret = new ZObject(name);
      Kind(ret, parentKind);
      return ret;
   }


   /// <summary>
   /// This create a "concrete" entity instance rather than abstract kinds
   /// </summary>
   /// <param name="name">The internal name</param>
   /// <returns>The new object</returns>
   ZObject Instance()
   {
      // Create the new object
      var ret = new ZObject();
      // It is an instance
      ((Dictionary<ZObject, ZObject>)relations[_instance])[ret] = ret;
      return ret;
   }

   /// <summary>
   /// Creates an object that can hold stuff
   /// </summary>
   /// <returns>The new object</returns>
   public IObject Object()
   {
      return Instance();
   }

   /// <summary>
   /// A person, including the player
   /// </summary>
   /// <returns>The new object</returns>
   ZObject Person()
   {
      return Kind((ZObject) Object(), personKind);
   }


   /// <summary>
   /// Creates an object that can hold stuff and links to other such
   /// objects in a direction
   /// </summary>
   /// <returns>The new object</returns>
   public IObject Cell()
   {
      // Create the object with an internal name
      // Claim that it is a cell
      return (IObject) Kind(Instance(), cellKind);
   }
   #endregion
}

