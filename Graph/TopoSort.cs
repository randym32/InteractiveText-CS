// Ported 2009 Oct 9
using System;
using System . Collections . Generic;

// This provides topological sorts for the graph
partial class SparseGraph<Tnode>
{
   /// <summary>
   /// Builds a mutable version of the node-predecessor counts
   /// </summary>
   /// <param name="Orig">The original table of node predecessor counts</param>
   /// <returns>mutable dictionary of predecessor counts</returns>
   static Dictionary<Tnode, int> BuildTable(IDictionary<Tnode, int> Orig)
   {
      if (null == Orig)
        return null;
      var NumPredecessors = new Dictionary<Tnode, int> ();
      foreach (var I in Orig)
       NumPredecessors[I.Key] = I.Value;
      return NumPredecessors;
   }


   /// <summary>
   /// Builds a table of the number of predecessors for each node
   /// </summary>
   /// <param name="Graph">The graph to analyze</param>
   /// <returns>Dictionary that maps each node to the number of predecessors</returns>
   Dictionary<Tnode, int> BuildPredecessors()
   {
      var NumPredecessors = new Dictionary<Tnode, int> ();
      foreach (var K in EdgeSrcNodes())
      {
         // Add the key as needed
         if (!NumPredecessors . ContainsKey(K))
           NumPredecessors[K] = 0;
         var Ends = Successors(K);

         // Iteratve over each the Ends
         var L= Ends.Count;
         for (int Idx = 0; Idx < L; Idx++)
         {
            // Increment the count of predecessors for the destination node
            var node = Ends [Idx] . sink;
            if (NumPredecessors . ContainsKey(node))
               NumPredecessors[node] ++;
            else
               NumPredecessors[node] = 1;
         }
      }

      return NumPredecessors;
   }

   
   /// <summary>
   /// Topologically sort a acyclic, directed graph using a depth-first search.
   /// </summary>
   /// <param name="ResidueNumPredecessors">Table of the number of predecessors
   /// each node has.  A successful run will have an empty table</param>
   /// <returns>The list of nodes, in the sorted order.</returns>
   /// <remarks>
   /// References
   /// Jeffery Copeland, Jeffrey Haemer, Work: Puzzle Posters, Part 2 
   /// SunExpert Magazine, October 1998, p53-57
   ///
   /// Jon Bentley (More Programming Pearls, pp. 20-23)
   ///
   /// Don Knuth (Art of Computer Programming, Volume 1: Fundamental Algortihms,
   /// Section 2.2.3)
   /// </remarks>
   internal List<Tnode> SortDFS(out IDictionary<Tnode, int> ResidueNumPredecessors)
   {
      // The number of predecessors for a given node
      var NumPredecessors = BuildTable(PredecessorCounts());
      if (null == NumPredecessors)
        NumPredecessors = BuildPredecessors();
      ResidueNumPredecessors = NumPredecessors;
      return SortDFS(NumPredecessors);
   }


   /// <summary>
   /// Sorts the graph using a depth-first search.
   /// </summary>
   /// <param name="NumPredecessors">Table of the number of predecessors each
   /// node has.  This will be modified as the procedure runs, and indicates
   /// the state of conflict at the end.</param>
   /// <returns>The list of nodes, in the sorted order.</returns>
   /// <remarks>
   /// Complexity is O(number of nodes + number of defined edges)
   /// 
   /// References
   /// Jeffery Copeland, Jeffrey Haemer, Work: Puzzle Posters, Part 2 
   /// SunExpert Magazine, October 1998, p53-57
   ///
   /// Jon Bentley (More Programming Pearls, pp. 20-23)
   ///
   /// Don Knuth (Art of Computer Programming, Volume 1: Fundamental Algortihms,
   /// Addison-Wesley 1997 Section 2.2.3)
   /// </remarks>
   List<Tnode> SortDFS(Dictionary<Tnode, int> NumPredecessors)
   {
      var Ret = new List<Tnode>();

      // Start with an empty stack
      // Push the root of the tree onto the stack to initialize
      // Create a list of nodes without predecessors
      var IndependentNodes = new List<Tnode>();
      foreach (var Node in NumPredecessors . Keys)
       if (0 == NumPredecessors[Node])
         IndependentNodes . Add(Node);

      // Continue until the stack is empty
      // This could be done recursively, but that would croak with graphs
      // that aren't small
      while (IndependentNodes . Count > 0)
      {
         // "Pop the stack, [push] what you find [onto the result stack] and push 
         //  its children back on the the stack in its place"
         int Idx = IndependentNodes . Count-1;
         var Node = IndependentNodes[Idx];
         IndependentNodes . RemoveAt(Idx);
         NumPredecessors . Remove(Node);
         Ret . Add(Node);

         // Scan each of the successors for which is available
         var Nexts = Successors(Node);
         if (null != Nexts)
         {
            var L = Nexts. Count;
            for (int I = 0; I < L; I++)
            {
               var Child = Nexts[I].sink;
               // Decrement the number of wait-count for the node
               // and append it if there are no more waits
               if (0 == --NumPredecessors[Child])
                  IndependentNodes . Add(Child);
           }
         }
      }

      return Ret;
   }


   /// <summary>
   /// Topologically sort a acyclic, directed graph using a breadth-first search.
   /// </summary>
   /// <param name="ResidueNumPredecessors">Table of the number of predecessors
   /// each node has.  A successful run will have an empty table</param>
   /// <returns>The list of nodes, in the sorted order.</returns>
   /// <remarks>
   /// Complexity is O(number of nodes + number of defined edges)
   /// 
   /// References
   /// Jeffery Copeland, Jeffrey Haemer, Work: Puzzle Posters, Part 2 
   /// SunExpert Magazine, October 1998, p53-57
   ///
   /// Jon Bentley (More Programming Pearls, pp. 20-23)
   ///
   /// Don Knuth (Art of Computer Programming, Volume 1: Fundamental Algortihms,
   /// Addison-Wesley 1997 Section 2.2.3)
   /// </remarks>
   internal List<Tnode> SortBFS(out IDictionary<Tnode, int> ResidueNumPredecessors)
   {
      // The number of predecessors for a given node
      var NumPredecessors = BuildTable(PredecessorCounts());
      if (null == NumPredecessors)
        NumPredecessors = BuildPredecessors();
      ResidueNumPredecessors = NumPredecessors;
      return SortBFS(NumPredecessors);
   }


   /// <summary>
   /// Sorts the given graph using breadth-first search.
   /// </summary>
   /// <param name="NumPredecessors">Table of the number of predecessors each
   /// node has.  This will be modified as the procedure runs, and indicates
   /// the state of conflict at the end.</param>
   /// <returns>The list of nodes, in the sorted order.</returns>
   List<Tnode> SortBFS(Dictionary<Tnode, int> NumPredecessors)
   {
      var Ret = new List<Tnode>();

      // "Start with an empty stack"
      // "Push the root of the tree onto the stack to initialize"
      // Create a list of nodes without predecessors
      foreach (var Node in NumPredecessors . Keys)
       if (0 == NumPredecessors[Node])
         Ret . Add(Node);

      // Continue until the stack is empty
      for (int I = 0; I < Ret . Count; I++)
      {
         // "Pop the stack, [push] what you find [onto the result stack] and push
         // its children back on the the stack in its place"
         var Node = Ret[I];
         NumPredecessors . Remove(Node);

         // Scan each of the successors for which is available
         var Nexts = Successors(Node);
         if (null != Nexts)
         {
            var L = Nexts.Count;
            for (int Idx = 0; Idx < L; Idx++)
            {
               var Child = Nexts[Idx].sink;
               // Decrement the number of wait-count for the node
               // and append it if there are no more waits
               if (0 == --NumPredecessors[Child])
                  Ret . Add(Child);
           }
         }
      }
      return Ret;
   }


   /// <summary>
   /// Determins if the graph has atleast one cycles.  Call this after sorting.
   /// </summary>
   /// <returns>True if there is a cycle, false if there is no cycle</returns>
   internal static bool  Cycle_Exists(IDictionary<Tnode, int> NumPredecessors)
   {
      foreach (var N in NumPredecessors . Values)
       if (0 != N)
         return true;
      return false;
   }
}

