// 2015-Jun-22
using System;

partial class ZObject
{
   #region Descriptions
   
//   public abstract string firstDescription {get;}
//   public abstract string longDescription {get;}
   /// <summary>
   /// The description to use when briefly listing the item
   /// </summary>
   string _shortDescription;
   /// <summary>
   /// The description to use when briefly listing the item
   /// </summary>
   public string shortDescription
   {
      get
      {
//         if (null != _shortDescription)
            return _shortDescription;
      }
      set
      {
         _shortDescription = value;
         // Make the noun list
         nouns = (WordList) value;
      }
   }
   internal string imagePath;
   #endregion
}
