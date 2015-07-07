// 2015-Jun-22
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// An interface so that we don't have to expose the whole ZObject
/// </summary>
public interface IObject
{
   /// <summary>
   /// The description to use when briefly listing the item
   /// </summary>
   string shortDescription {get; set;}

   /// <summary>
   /// Adds a child to the children list
   /// </summary>
   /// <param name="Child"></param>
   void AddChild(IObject Child);
}


partial class ZObject : ZRoot, IObject
{
   /// <summary>
   /// An internal constructor
   /// </summary>
   /// <param name="name">The name of the thing</param>
   internal ZObject(object name)
      :base(name)
   {
   }


}