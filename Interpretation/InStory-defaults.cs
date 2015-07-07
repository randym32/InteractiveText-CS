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
   /// <param name="line">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool inventory(propositionContext context, List<ZObject> items,int line, Err err)
   {
      // List the contents of the player character
      inventory(player, (ZObject) player);
      return true;
   }
   static void inventory(IObject player, ZObject item)
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
   /// <param name="line">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool look(propositionContext context, List<ZObject> items, int line, Err err)
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
   /// <param name="line">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool look1(propositionContext context, List<ZObject> items, int line, Err err)
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
   /// <param name="line">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool dropWhat(propositionContext context, List<ZObject> items, int line, Err err)
   {
      // Format an error message
      err . linkTo(line);
      err . SB . AppendFormat("Drop what?");
      return true;
   }

   /// <summary>
   /// Drops the item.
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="line">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool drop(propositionContext context, List<ZObject> items, int line, Err err)
   {
      var from = (ZObject) player;
      // Check that we are holding the items
      List<ZObject> notHolding = new List<ZObject>();
      foreach (var item in items)
      {
         if (!item.isDescendentOf(from))
            notHolding.Add(item);
      }
      if (notHolding.Count > 0)
      {
         // Not all of the items is there
         // Format an error message
         err . linkTo(line);
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
      return move(((ZObject) player).Parent, items, line, err);
   }

   
   /// <summary>
   /// Would get an item if one had been specified
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="line">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool getWhat(propositionContext context, List<ZObject> items, int line, Err err)
   {
      // Format an error message
      err . linkTo(line);
      err . SB . AppendFormat("Get what?");
      return true;
   }
   /// <summary>
   /// Drops the item.
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="line">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool get(propositionContext context, List<ZObject> items, int line, Err err)
   {
      // Drop each of the items;
      // we do this by moving each of the items to the parent
      return move((ZObject) player, items, line, err);
   }
   static bool move(ZObject to, List<ZObject> items, int line, Err err)
   {
      // Check that we have items that we can move
      if (null == items || items.Count < 1)
      {
         // Didn't specify the items to move
         // Format an error message
         err . linkTo(line);
         err . SB . AppendFormat("What should be moved?");
         return false;
      }

      // Check that the movement is sane
      foreach (var item in items)
      {
         if (item.Parent== to)
      {
         // Format an error message
         err . linkTo(line);
         err . SB . AppendFormat("That makes no sense.. that doesn't really move an item anywhere!");
         return true;
      }
      }


      // Reparent each of the items now
      foreach (var item in items)
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
   /// <param name="line">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool goWhere(propositionContext context, List<ZObject> items, int line, Err err)
   {
      // Format an error message
      err . linkTo(line);
      err . SB . AppendFormat("Go where?");
      return true;
   }
   /// <summary>
   /// Would "go" if a direction had been specified
   /// </summary>
   /// <param name="context">The propositional context</param>
   /// <param name="items">The direct objects,  indirect objects, and other noun phrase</param>
   /// <param name="line">The line number it was invoked</param>
   /// <param name="err">Where to place any descriptive error messages</param>
   /// <returns>True if was able interpret (it was sensible, even though can't be don), or false if not syntactically sensible</returns>
   bool go(propositionContext context, List<ZObject> items, int line, Err err)
   {
      ZObject dest = items[0];
      // Check that the destination is not the same as the current
      if (((ZObject)player).Parent == dest)
      {
         // Format an error message
         err . linkTo(line);
         err . SB . AppendFormat("We are already there.");
         return true;
      }
      // Check that the destination is a room
      if (!(dest is ZCell))
      {
         // Format an error message
         err . linkTo(line);
         err . SB . AppendFormat("{0} is not a place!", dest);
         return true;
      }

      // Make the move
      ((ZObject)player).Parent.RemoveChild((ZObject)player);
      dest.AddChild(player);
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
