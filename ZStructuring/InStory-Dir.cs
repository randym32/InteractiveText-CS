// 2013-Feb-10
using System ;
using System.Collections.Generic;

partial class InStory
{
   /// <summary>
   /// This is the kind that is the base kind for "element of space"
   /// </summary>
   internal static ZObject roomKind = new ZObject("room");

   /// <summary>
   /// This is the kind that is the base kind for direction
   /// </summary>
   internal static ZObject dirKind = new ZObject("direction");

   /// <summary>
   /// A direction
   /// </summary>
   /// <returns>The new object</returns>
   ZObject Direction(WordList nouns)
   {
      var ret= Kind((ZObject) Object(), dirKind);
      ret.shortDescription=nouns.words[0];
      ret.nouns = nouns;
      return ret;
   }

   /// <summary>
   /// The different names for east
   /// </summary>
   static WordList eastWords="east easterly eastward eastwardly";
   /// <summary>
   /// The different names for west
   /// </summary>
   static WordList westWords = "west westward westwardly";
   /// <summary>
   /// The different names for north
   /// </summary>
   static WordList northWords= "north northerly northward northwardly";
   /// <summary>
   /// The different names for south
   /// </summary>
   static WordList southWords= "south southerly southward southwardly";

   /// <summary>
   /// The representation of the east direction
   /// </summary>
   internal ZObject east;
   /// <summary>
   /// The representation of the west direction
   /// </summary>
   internal ZObject west;
   /// <summary>
   /// The representation of the north direction
   /// </summary>
   internal ZObject north;
   /// <summary>
   /// The representation of the south direction
   /// </summary>
   internal ZObject south;

   /// <summary>
   /// A delegate to that adds the relation between the two objects
   /// </summary>
   /// <param name="source">The source party in the relation</param>
   /// <param name="sink">The sink party in the relation</param>
   /// <description>
   /// See directed, and transitive
   /// </description>
   delegate void dRelation2Add(ZObject source, ZObject sink);

   /// <summary>
   /// Maps the name of the relation to the handler to add it
   /// </summary>
   Dictionary<WordList,dRelation2Add> relation2Add =
      new Dictionary<WordList,dRelation2Add>();

#if false
   void define(WordList relationName)
   {
      // Get the relation for the name; if none exists, add the structure
      Dictionary<ZObject,ZEdge> relation;
      if (!relation2.TryGetValue(relationName, out relation))
      {
         // Define a structure to hold the relation
         relation2[relationName] = relation = new Dictionary<ZObject,ZEdge>();
      }

      // Create a delegate to add the items in directed
      relation2Add[relationName] =  delegate(ZObject source, ZObject sink)
      {
         ZEdge edge;
         // Add the entry to the table.. if none exists.
         if (!relation.TryGetValue(source, out edge))
         {
            // Doesn't exist, create the relation
            relation[source] = new ZEdge(newName(), sink, null);
            return;
         }
         // Check that the relation is sensible
         if (edge.sink == sink)
            return;
         // Oh-oh, there is a problem.
      };
   }
#endif
   /// <summary>
   /// Adds the pair to the relation
   /// </summary>
   /// <param name="relationName"></param>
   /// <param name="a"></param>
   /// <param name="b"></param>
   void add(WordList relationName, ZObject a, ZObject b)
   {
      relation2Add[relationName](a,b);
   }

   /// <summary>
   /// An object to track it's opposite
   /// </summary>
   internal static ZObject opposite = new ZObject("opposite");
   internal static ZObject _instance = new ZObject("instance");

   /// <summary>
   /// Create the relations
   /// </summary>
   void initRelations()
   {
      // This tracks which entities are "concrete" instances rather than abstract kinds
      relations[_instance] = new Dictionary<ZObject, ZObject>();
      addRelation2(__kind);
      addRelation2(opposite);


      // Define the directions
      east = Direction(eastWords);
      west = Direction(westWords);
      north = Direction(northWords);
      south = Direction(southWords);
      ((SparseGraph<ZObject>)relations[opposite]).After(east, new Edge<ZObject>(west, null));
      ((SparseGraph<ZObject>)relations[opposite]).After(west, new Edge<ZObject>(east, null));
      ((SparseGraph<ZObject>)relations[opposite]).After(north, new Edge<ZObject>(south, null));
      ((SparseGraph<ZObject>)relations[opposite]).After(south, new Edge<ZObject>(north, null));

      // Define the relations for each direction
      addRelation2(east);
      addRelation2(west);
      addRelation2(north);
      addRelation2(south);
   }
   /// <summary>
   /// Creates the underlying structures for the relation
   /// </summary>
   /// <param name="name">The relation to create</param>
   void addRelation2(ZObject name)
   {
      if (!relations.ContainsKey(name))
         relations[ name] =  new SparseGraph<ZObject>(true);
   }

}
