using System;
using System.Collections.Generic;
using System.Text;

class propositionContext
{}

public partial class InStory
{
   /// <summary>
   /// The object that is proxy for / represents the player within the
   /// game world
   /// </summary>
   public readonly IObject player;

   /// <summary>
   /// The constructor
   /// </summary>
   public InStory()
   {
      initRelations();
      player =  Person();
      player.shortDescription = "you";
      initPlayerCharacter();
   }


   /// <summary>
   /// Transitively links the two space cells togehter
   /// </summary>
   /// <param name="direction"></param>
   /// <param name="a"></param>
   /// <param name="b"></param>
   internal void inDirection(ZObject direction, ZObject a, ZObject b)
   {
      ((SparseGraph<ZObject>)relations[direction]).After(a, new Edge<ZObject>(b, null));
      List<Edge<ZObject>> edges;
      if (((SparseGraph<ZObject>)relations[opposite]).successors(direction, out edges))
      {
         var opEdge = new Edge<ZObject>(a, null);
         foreach (var op in edges)
            ((SparseGraph<ZObject>)relations[op.sink]).After(b, opEdge);
      }
   }
   /// <summary>
   /// b is west of a
   /// </summary>
   /// <param name="a"></param>
   /// <param name="b"></param>
   public void westOf(IObject a, IObject b)
   {
      inDirection(west, (ZObject)a, (ZObject) b);
   }




   #region Storage
   /// <summary>
   /// Creates JSON from the root object and everything that it refers to
   /// </summary>
   /// <returns>String version (JSON) of the world</returns>
   public StringBuilder toJSON()
   {
      return ZNormalForm.JSONSerialize(this);
   }


   /// <summary>
   /// Takes JSON and creates a world from it
   /// </summary>
   /// <param name="json"></param>
   /// <returns>null on error, otherwise the object that</returns>
   public static InStory fromJSON(string json)
   {
      // Get the normal form from the string
      return new InStory(ZNormalForm.JSONDeserialize(json));
   }
   InStory(ZNormalForm from)
      :this()
   {
      // Convert back into an operational form
      player = from.Objects(this);
      relations = from.Relations();
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
