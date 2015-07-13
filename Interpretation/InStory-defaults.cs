using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public partial class InStory
{
   /// <summary>
   /// Describes the items being carried.
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="src">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool inventory(propositionContext context, List<object> items, SrcInFile src, Err err)
   {
      // List the contents of the player character
      inventory(player, (ZObject) player);
      return true;
   }
   static void inventory(IObject player, object item)
   {
      if (null == item)
         return;
      var z = (ZObject) item;
      // Sanity check the argument
      if (null == z.Children)
         return;
      // Go thru and group the items by kind
      // [todo]
      // Next print them out
      for (var I = 0; I < z.NumChildren; I++)
      {
         // Skip the users avatar
         if (z.Children[I] == player)
            continue;
         // Describe the item
         Console.WriteLine(z.Children[I].shortDescription);
      }
   }

   /// <summary>
   /// Describes the items in the current room
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="src">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool look(propositionContext context, List<object> items, SrcInFile src, Err err)
   {
      // Have the parent list it's children
      inventory(player, ((ZObject) player).Parent);
      return true;
   }

   /// <summary>
   /// Describes the items listed
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="src">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool look1(propositionContext context, List<object> items, SrcInFile src, Err err)
   {
      // Have the parent list it's children
      inventory(player, items[0]);
      return true;
   }


   /// <summary>
   /// Would drop an item if one had been specified
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="src">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool dropWhat(propositionContext context, List<object> items, SrcInFile src, Err err)
   {
      // Format an error message
      err . linkTo(src);
      err . SB . AppendFormat("Drop what?");
      return true;
   }

   /// <summary>
   /// Drops the item.
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="src">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be done), or false if not syntactically sensible</returns>
   bool drop(propositionContext context, List<object> items, SrcInFile src, Err err)
   {
      var from = (ZObject) player;
      // Check that we are holding the items
      List<ZObject> notHolding = new List<ZObject>();
      foreach (var item in items)
      {
         if (item is ZObject)
         {
            if (!((ZObject)item).isDescendentOf(from))
               notHolding.Add((ZObject)item);
         }
         else
         {
            // Format an error message
            err . linkTo(src);
            err.SB.AppendFormat("That doesn't makes sense... it isn't even a thing we can hold..");
            return false;
         }
      }
      if (notHolding.Count > 0)
      {
         // Not all of the items is there
         // Format an error message
         err . linkTo(src);
         err . SB . AppendFormat("{0} is not holding ", from.shortDescription);
         bool needComma = false;
         foreach (var item in notHolding)
         {
            // todo: add and
            err.SB.AppendFormat(needComma?", {0}":"{0}", item.shortDescription);
            needComma = true;
         }
         return false;
      }

      // Drop each of the items;
      // we do this by moving each of the items to the parent
      return move(((ZObject) player).Parent, items, src, err);
   }

   
   /// <summary>
   /// Would get an item if one had been specified
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="src">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool getWhat(propositionContext context, List<object> items, SrcInFile src, Err err)
   {
      // Format an error message
      err . linkTo(src);
      err . SB . AppendFormat("Get what?");
      return true;
   }

   /// <summary>
   /// Drops the item.
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="src">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool get(propositionContext context, List<object> items, SrcInFile src, Err err)
   {
      // Drop each of the items;
      // we do this by moving each of the items to the parent
      return move((ZObject) player, items, src, err);
   }
   bool move(ZObject to, List<object> items, SrcInFile src, Err err)
   {
      // Check that we have items that we can move
      if (null == items || items.Count < 1)
      {
         // Didn't specify the items to move
         // Format an error message
         err . linkTo(src);
         err . SB . AppendFormat("What should be moved?");
         return false;
      }

      // An array to hold the items that we can move
      var movables = new List<ZObject>();

      // Check that the movement is sane
      foreach (var item in items)
      {
         // Check that it is an item that we can move
         if (!(item is ZObject) || isKind((ZObject)item, cellKind) || isKind((ZObject)item, dirKind))
         {
            // Format an error message
            err . linkTo(src);
            err . SB . AppendFormat("That makes no sense.. we can't move that");
            return true;
         }
         var movable = (ZObject) item;
         if (movable . Parent == to)
         {
            // Format an error message
            err . linkTo(src);
            err . SB . AppendFormat("That makes no sense.. that doesn't really move an item anywhere!");
            return true;
         }
         movables.Add(movable);
      }


      // Reparent each of the items now
      foreach (var item in movables)
      {
         item.Parent.RemoveChild(item);
         to.AddChild(item);
      }

      return true;
   }

   
   /// <summary>
   /// Would "go" if a direction had been specified
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="src">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool goWhere(propositionContext context, List<object> items, SrcInFile src, Err err)
   {
      // Format an error message
      err . linkTo(src);
      err . SB . AppendFormat("Go where?");
      return true;
   }
   /// <summary>
   /// Would "go" if a direction had been specified
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="src">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool go(propositionContext context, List<object> items, SrcInFile src, Err err)
   {
      var dest = items[0];
      // Check that the destination is not the same as the current
      if (((ZObject)player).Parent == dest)
      {
         // Format an error message
         err . linkTo(src);
         err . SB . AppendFormat("We are already there.");
         return true;
      }

      // If it is an edge, get the door part
      if (dest is Edge<ZObject>)
      {
         var edge = (Edge<ZObject>) dest;
         // See if we can go into the room
         if (!canTraverseTo(edge))
         {
            // Format an error message
            err . linkTo(src);
            err . SB . AppendFormat("We can't go there.");
            return true;
         }
         dest = edge.sink;
      }

      // Check that the destination is a room
      if (isKind((ZObject)dest, cellKind))
      {
         // Make the move
         ((ZObject)player).Parent.RemoveChild((ZObject)player);
         ((ZObject)dest).AddChild(player);
         return true;
      }
      // See if it refers to a direction
      if (isKind((ZObject)dest, dirKind))
      {
         // Format an error message
         err . linkTo(src);
         err . SB . AppendFormat("There is nothing in the {0} direction!", ((ZObject)dest).shortDescription);
         return true;
      }
      // Format an error message
      err . linkTo(src);
      err . SB . AppendFormat("{0} is not a place!", dest);
      return true;
   }


   /// <summary>
   /// Configures the verb table
   /// </summary>
   internal void initPlayerCharacter()
   {
      // Define some handlers
      add("describe", 0, look);
      add("describe", 1, look1);
      add("look",     0, look);
      add("look",     1, look1);
      add("look at",  1, look1);
      add("inventory",0, inventory);
      add("list",     0, inventory);
//      sameAs("list", "inventory");
//      sameAs("look", "describe");
      // drop
      add("drop",     0, dropWhat);
      add("drop",     1, drop);
      // pickup
      add("get",     0, getWhat);
      add("get",     1, get);
      add("pick",    0, getWhat);
      add("pick up", 0, getWhat);
      add("pick", 1, get);
      add("pick up", 1, get);
      // directions
      add("go",  0, goWhere);
      add("go",  1, go);
      add("go to",  0, goWhere);
      add("go to",  1, go);
   }

}
