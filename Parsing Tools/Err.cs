// 2013-Feb-4
using System . Text;


/// <summary>
/// This is a helper to gather error
/// </summary>
class Err
{
   /// <param name="SB">Where to place any descriptive error messages</param>
   /// <param name="linkTo">Helper where to link definitions to</param>
   /// <summary>
   /// Where to place any descriptive error messages
   /// </summary>
   internal StringBuilder SB = new StringBuilder();
   StringBuilder SBEnd = new StringBuilder();

   /// <summary>
   /// A helper to create a link to the original source or description within
   /// </summary>
   /// <param name="SB">The StringBuilder to append to</param>
   /// <param name="Line">The line number it was defined at</param>
   internal delegate void dLinkTo(Err err, int Line);

   /// <summary>
   /// Helper where to link definitions to
   /// </summary>
   internal readonly dLinkTo _linkTo;

   internal Err(dLinkTo linkTo)
   {
      this . _linkTo = linkTo;
   }

   public override string ToString()
   {
      return SB.ToString()+SBEnd.ToString();
   }

   #region Helpers
   /// <summary>
   /// A helper to (typically) create a link to the original source
   /// </summary>
   /// <param name="SB">The StringBuilder to append to</param>
   /// <param name="Line">The line number it was defined at</param>
   internal static void NoLinkTo(Err err, int Line)
   {
   }


   /// <summary>
   /// A helper to create a link to the original source
   /// </summary>
   /// <param name="SB">The StringBuilder to append to</param>
   /// <param name="Line">The line number it was defined at</param>
   internal static void InformLinkTo(Err err, int Line)
   {
      err. SB . AppendFormat("<a href='inform:{0}'>", Line);
      err. SBEnd . Insert(0, "</a>");
   }


   /// <summary>
   /// A helper to create a link to the text
   /// </summary>
   /// <param name="SB">The StringBuilder to append to</param>
   /// <param name="Line">The line number it was defined at</param>
   internal void LocalLinkTo(Err err, int Line)
   {
      err . SB . AppendFormat("<a href='#{0}'>", Line);
      err . SBEnd . Insert(0, "</a>");
   }
   #endregion

   /// <summary>
   /// A helper to create a link to the original source or description within
   /// </summary>
   /// <param name="link">The file & line number it was defined at</param>
   internal void linkTo(SrcInFile link)
   {
      // Call the link delegate
      _linkTo(this, link . line);
   }

   string ToBe(object NP)
   {
      return "is";  // are
   }
   /// <summary>
   /// This is used when a thing is in a singly valued relationship,
   /// but has a value that is in conflict with the asserted value.
   /// </summary>
   /// <param name="x"></param>
   /// <param name="v"></param>
   /// <param name="srcLine"></param>
   /// <returns>false</returns>
   internal bool IsAlready(object x, object v, SrcInFile srcLine)
   {
      linkTo(srcLine);
      SB . AppendFormat("{0} {1} {2}.  ",
            x, ToBe(x), v);
      return false;
   }
#if false
            err . SB . AppendFormat("The '{0}' is ", _subject);
            err . linkTo(line);
            err . SB . Append("being declared</a> but was ");
            err . linkTo(subb . definedAtLine);
            err . SB . Append("already declared earlier.</a>");
#endif

}
