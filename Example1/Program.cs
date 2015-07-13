using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Program
{
   static void Main(string[] args)
   {
      var story = new InStory();
      // Create a room
      var room1 = story.Cell();
      var room2 = story.Cell();
      room2.shortDescription="anteroom";
      // room2 is west of room1
      story.westOf(room1, room2);

      // Create a player character
      var pc = story.player;

      // Put the character in the room
      room1.AddChild(pc);

      // Create an object
      var tmp = story.Object();
      tmp.shortDescription = "my thing";

      // Give it to the player
      pc.AddChild(tmp);

      // Print an inventory
      story.interp("list");

      // Go to other room
      story.interp("go to anteroom");
      story.interp("go east");
      story.interp("west");
      story.interp("south");


      // Drop -- but don't say what
      story.interp("drop");

      // Drop my thing
      story.interp("drop my thing");

      // Drop my thing again
      story.interp("drop my thing");

      // Print an inventory
      story.interp("list");

      // Pick up thing
      story.interp("get thing");
      // Try to pick up room
      story.interp("get anteroom");
      story.interp("get west");

      // Print an inventory
      story.interp("list");


      // Dump the JSON
      var str = story.toJSON();
   }
}
