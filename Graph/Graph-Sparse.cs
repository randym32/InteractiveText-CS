// Created 2010. Apr 29
using System;
using System . Collections . Generic;

/// <summary>
/// Simple sparse incidence matrix 
/// This class automatically manages the efficient forms for query -- that is,
/// it automatically computes then and tosses then out if the specification changes
/// Pairs are used to represent a sparse incidence matrix.
/// </summary>
/// <typeparam name="Tnode">The type for the node</typeparam>
/// <typeparam name="Textra">The type for annotation info</typeparam>
partial class SparseGraph<Tnode>
{
   /// <summary>
   /// This is true if the reachability and APSP structures are up to date based on the
   /// spec.  It is false if the spec has been changed and these structures have not yet been
   /// updated.
   /// </summary>
   bool ready = true;

   /// <summary>
   /// This is true if we don't work out any implications; ie don't derive
   /// the full ancestors
   /// </summary>
   internal readonly bool immediate = false;

   /// <summary>
   /// The list of successors for a given node.
   /// </summary>
   internal Dictionary<Tnode, List<Edge<Tnode>>> SuccessorTable = new Dictionary<Tnode, List<Edge<Tnode>>>();

   /// <summary>
   /// The number of predecessors for a given node.
   /// Valid iff ready is true
   /// </summary>
   Dictionary<Tnode, int> NumPredecessors = new Dictionary<Tnode, int>();

     
   /// <summary>
   /// Maps a node to the set of nodes that can be reached from it.  This
   /// differs from ordering in that the there must be a path given in the succession
   /// table.
   /// Valid only iff ready
   /// </summary>
   Dictionary<Tnode, Dictionary<Tnode, Tnode>> nodeToAllSuccessors = new Dictionary<Tnode, Dictionary<Tnode, Tnode>>();


   /// <summary>
   /// Construct the graph data structure
   /// </summary>
   /// <param name="immediate">True if we should not derive ancestors</param>
   internal SparseGraph(bool immediate)
   {
      this.immediate = immediate;
   }

   /// <summary>
   /// Construct the graph data structure
   /// </summary>
   internal SparseGraph():
      this(false)
   {
   }


   /// <summary>
   /// Provides an enumeration of the source nodes in all of the graphs edges
   /// </summary>
   /// <returns>null on error, otherwise the source nodes in the edges</returns>
   IEnumerable<Tnode> EdgeSrcNodes()
   {
      return SuccessorTable . Keys;
   }


   /// <summary>
   /// Retrieves the list of nodes that Node connects to
   /// </summary>
   /// <param name="Node">The starting node</param>
   /// <returns>The list of successors</returns>
   List<Edge<Tnode>> Successors(Tnode Node)
   {
      List<Edge<Tnode>> Ret;
      if (SuccessorTable . TryGetValue(Node, out Ret))
        return Ret;
      return null;
   }


   /// <summary>
   /// Provides a list of the number of possible sources a given sink node
   /// may have.
   /// </summary>
   /// <returns>null on error, otherwise table of counts</returns>
   IDictionary<Tnode, int> PredecessorCounts()
   {
      return NumPredecessors;
   }


   // --- Methods for Inserting nodes into the graph ------------------------------
   /// <summary>
   /// Adds a node to the graph that Node is after Predecessor.
   /// </summary>
   /// <param name="Predecessor">The node that comes first</param>
   /// <param name="Node">The node that comes sometime after</param>
   internal void After(Tnode Predecessor, Edge<Tnode> Node)
   {
      // Do a fast check
      // Look up the set of successors from the source
      Dictionary<Tnode, Tnode> set;
      if (nodeToAllSuccessors.TryGetValue(Predecessor, out set))
      {
         // See if the set contains the sink as reachable
         if (set.ContainsKey(Node.sink))
            return ;
      }

      // Skip nodes already in the list
      List<Edge<Tnode>> Nexts;
      if (  SuccessorTable . TryGetValue(Predecessor, out Nexts))
      {
         // Iterate over each of the nodes
         var L = Nexts.Count;
         for (int I = 0; I < L; I++)
         {
            // See if this is an exact match
            if (Nexts[I]. Equals(Node))
              return ;
         }
      }

      // Add the edge to the table
      if (null == Nexts)
        Nexts = SuccessorTable[Predecessor] = new List<Edge<Tnode>>();
      // Add this node to the list of predecessors
      Nexts . Add(Node);
     
      // Increment the count of predecessors for the destination node
      if (NumPredecessors . ContainsKey(Node.sink))
        NumPredecessors[Node.sink] ++;
       else
        NumPredecessors[Node.sink] = 1;

      // Create entry for the predecessor
      if (!NumPredecessors . ContainsKey(Predecessor))
        NumPredecessors[Predecessor] = 0;

      // Indicate that the graph has changed since the last optimal versions were made
      ready = false;
   }

   internal delegate bool db(Tnode node);

   /// <summary>
   /// Checks to see if the the sink node is reachable from the source node.
   /// This may recompute the reachability cache
   /// </summary>
   /// <param name="source">Node</param>
   /// <param name="dest">Node</param>
   /// <returns>true if the dest node is reachable from the source node; false otherwise</returns>
   internal bool reachable(Tnode source, Tnode dest, db isInstance)
   {
      // Look up the set of successors from the source
      Dictionary<Tnode, Tnode> set;
      if (nodeToAllSuccessors.TryGetValue(source, out set))
      {
         // See if the set contains the sink as reachable
         if (set.ContainsKey(dest))
            return true;
      }

      // If the structures are up to date, then we are done
      if (ready)
         return false;
      // If immediate scan the explicit table
      if (immediate)
      {
         // Get the successor list
         List<Edge<Tnode>> edges;
         if (!SuccessorTable.TryGetValue(source, out edges))
            return false;
         // See if any equal
         foreach (var edge in edges)
            if (edge.sink.Equals(dest))
               return true;
         // None did, quit
         return false;
      }


      // Ok, we need to compute the reachability set.  This is done last, as it is slow
      // and not always needed during each world building change
      formReachability(isInstance);

      // Look up the set of successors from the source, and 
      // See if the set contains the sink as reachable
      return nodeToAllSuccessors.TryGetValue(source, out set) &&
             set.ContainsKey(dest);
   }

   /// <summary>
   /// Fetchs the table of immediate successors
   /// </summary>
   /// <param name="source">The node from</param>
   /// <param name="edges">The set of edges out from that node</param>
   /// <returns>true if there was a record, false otherwise</returns>
   internal bool successors(Tnode source, out List<Edge<Tnode>> edges)
   {
      return SuccessorTable.TryGetValue(source, out edges);
   }


   /// <summary>
   /// This computes the reachability set
   /// </summary>
   void formReachability(db isInstance)
   {
      // First, topological sort the relation in an array
      IDictionary<Tnode, int> ResidueNumPredecessors;
      var ary = SortBFS(out ResidueNumPredecessors);
      ary.Reverse();

      // Create the structure to hold the reachability set;
      var newReachability = new Dictionary<Tnode, Dictionary<Tnode, Tnode>>();

      // Next, use the array to construct the set of all successors, efficiently
      foreach (var node in ary)
      {
         // Create a set of items
         var set = new Dictionary<Tnode, Tnode>();

         // The node is part of the successors (as a convenience) if it is not an instance
         if (!isInstance(node))
         {
            // Add it to the set
            set[node] = node;
         }

         // Go thru the specification of successors and construct the set
         // The topological ordering guarantees that we have already computed
         // the sets for all of the successors
         List<Edge<Tnode>> successors;
         if (SuccessorTable.TryGetValue(node, out successors))
         foreach (var successor in successors)
         {
            // Look up it set
             Dictionary<Tnode, Tnode> successorSet;
            if (!newReachability.TryGetValue(successor.sink, out successorSet))
               continue;
            // Add each item to the current set
            foreach (var ancestor in successorSet)
            {
               set[ancestor.Key] = ancestor.Key;
            }
         }

         // As a memory optimization, we will go thru an reuse an earlier set,
         // as it will be identical.  That is, we toss out this duplicate set.
         if (!isInstance(node))
         {
            var tmp = set;
            foreach (var ancestor in tmp)
            {
               // Keep the set that is the same size (there isn't one larger)
               Dictionary<Tnode, Tnode> ancestorSet;
               if (newReachability.TryGetValue(ancestor.Key, out ancestorSet)
                   && ancestorSet . Count == set.Count)
                  set = ancestorSet;
            }
         }

         // Store it in the set of results
         newReachability[node] = set;
      }

      nodeToAllSuccessors = newReachability;
      // Update the flag indicating that the results are properly computed
      ready = true;
   }
}


