using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

class propositionContext
{}

public partial class InStory
{
   /// <summary>
   /// The object that is proxy for / represents the player within the
   /// game world
   /// </summary>
   public readonly IObject player =  new ZObject(newName());

   /// <summary>
   /// The constructor
   /// </summary>
   public InStory()
   {
      player.shortDescription = "you";
      initPlayerCharacter();
   }

   /// <summary>
   /// b is west of a
   /// </summary>
   /// <param name="a"></param>
   /// <param name="b"></param>
   public void westOf(ICell a, ICell b)
   {
      ((ZCell)a).edgeInDirection["west"] = new ZEdge(newName(), (ZCell) b, null);
      ((ZCell)b).edgeInDirection[east] = new ZEdge(newName(), (ZCell) a, null);
   }


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

   #region Basic Entity Types
   /// <summary>
   /// Creates an object that can hold stuff and links to other such
   /// objects in a direction
   /// </summary>
   /// <returns>The new object</returns>
   public ICell Cell()
   {
      // Create the object with an internal name
      return Cell(newName());
   }

   /// <summary>
   /// Creates an object that can hold stuff and links to other such
   /// objects in a direction
   /// </summary>
   /// <param name="name">The internal name</param>
   /// <returns>The new object</returns>
   public ICell Cell(object name)
   {
      // Create the object with the givne name
      var ret = new ZCell(name);
      // Claim that it is a room
      //_isa.assert(roomKind, ret, 0, null);
      return ret;
   }

   /// <summary>
   /// Creates an object that can hold stuff
   /// </summary>
   /// <returns>The new object</returns>
   public static IObject Object()
   {
      return Object(newName());
   }

   /// <summary>
   /// Creates an object that can hold stuff
   /// </summary>
   /// <param name="name">The internal name</param>
   /// <returns>The new object</returns>
   public static IObject Object(object name)
   {
      return new ZObject(name);
   }
   #endregion


   #region Storage
   /// <summary>
   /// Creates JSON from the root object and everything that it refers to
   /// </summary>
   /// <returns>String version (JSON) of the world</returns>
   public StringBuilder toJSON()
   {
      return ZNormalForm.JSONSerialize((ZObject) player);
   }


   /// <summary>
   /// Takes JSON and creates a world from it
   /// </summary>
   /// <param name="json"></param>
   /// <returns>null on error, otherwise the object that</returns>
   public static IObject fromJSON(string json)
   {
      return ZNormalForm.JSONDeserialize(json);
   }
   #endregion

   #region Descriptions
   /* The descriptions are broken down into several elements
	* An image used to represent the item (more on this in a moment)
	* A minimap to show the where in the world 'we are'
	* A name used in the window title
	* A longer name or brief description  used when listing the item with
	   others (as in when listing the contents of our pockets)
	* A description displayed when in the room or examining the item
	* An introduction displayed the first time we enter a room or
		examine the item.
   */
   #endregion
}
