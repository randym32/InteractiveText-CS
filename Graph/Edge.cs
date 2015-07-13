using System;
using System . Collections . Generic;

/// <summary>
/// Where a thing came from within a file
/// </summary>
struct SrcInFile
{
   /// <summary>
   /// The file it was defined in
   /// </summary>
   internal readonly string file;

   /// <summary>
   /// The line at which it was defined
   /// </summary>
   internal readonly int line;

   /// <summary>
   /// Defines a point in the file
   /// </summary>
   /// <param name="file"></param>
   /// <param name="line"></param>
   internal SrcInFile(string file, int line)
   {
      this.file = file;
      this.line = line;
   }
}

/// <summary>
/// This is a class of information about the edges
/// </summary>
/// <typeparam name="Tnode">The type for the node</typeparam>
partial struct Edge<Tnode>
{
   /// <summary>
   /// This is the node on other side of the edge
   /// </summary>
   internal Tnode sink;

   #region Annotations
   /// <summary>
   /// This is the (shared) object that may regulate passage,
   /// or has properties for the edge
   /// </summary>
   internal Tnode gate;
   #endregion
   
   /// <summary>
   /// Determines whether or not "a" is equal to this object
   /// </summary>
   /// <param name="a">An object to compare against</param>
   /// <returns>True if they are the same</returns>
   bool Equals(Edge<Tnode> a)
   {
      if (!sink . Equals(a.sink))
         return false;
      return true;
   }

   /// <summary>
   /// Adds the edge to the list
   /// </summary>
   /// <param name="Node">The sink node</param>
   /// <param name="gate">The node that gates traversal</param>
   internal Edge(Tnode Node,Tnode gate)
   {
      sink = Node;
      this.gate = gate;
   }
}
 
