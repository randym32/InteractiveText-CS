// November 14 2009
using System;
partial class ZObject
{
   #region Parent relationship
   /// <summary>
   /// The parent of this node
   /// </summary>
   internal ZObject Parent;
   #endregion

   #region Child relationships
   /// <summary>
   /// The children of this node
   /// </summary>
   internal ZObject[] Children;

   /// <summary>
   /// The number of children of this node
   /// </summary>
   internal int NumChildren = 0;

   /// <summary>
   /// Checks to see if this node is descended from the given node
   /// </summary>
   /// <param name="Ancestor">A possible parent, grandparent, etc. node</param>
   /// <returns>true if this node is a descendent of the ancestor</returns>
   internal bool isDescendentOf(ZObject Ancestor)
   {
      if (Parent == Ancestor)
        return true;
      return null != Parent && Parent . isDescendentOf(Ancestor);
   }

   /// <summary>
   /// Adds a child to the children list
   /// </summary>
   /// <param name="child"></param>
   public void AddChild(IObject child)
   {
      var Child = (ZObject) child;
      // assert(Child != this);
      Child . Parent = this;
      if (null == Children)
        Children = new ZObject[1]{Child};
      else if (Children . Length > NumChildren)
        Children [ NumChildren ] = Child;
      else
        {
           Array . Resize(ref Children, NumChildren+1);
           Children [ NumChildren ] = Child;
        }
      NumChildren ++;
   }

   /// <summary>
   /// Remove a child node from the list of tracked children
   /// </summary>
   /// <param name="Child">Child to remove</param>
   internal void RemoveChild(ZObject Child)
   {
      for (int I = 0; I < NumChildren; I++)
       if ((object) Child == (object) Children[I])
         {
            NumChildren--;
            Children[I] = Children[NumChildren];
            Children[NumChildren] = null;
            break;
         }
   }
   #endregion
}
