using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;


/// <summary>
/// This is a private wrapper around the JSON converter to skip null properties
/// </summary>
class JSONConverter : JavaScriptConverter
{
   public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
   {
      throw new NotImplementedException();
   }

   /// <summary>
   /// If the value is not "empty" or null, add it to the dictionary
   /// </summary>
   /// <param name="d">Output dictionary</param>
   /// <param name="name">The name of the property</param>
   /// <param name="value">The value</param>
   static void addNonEmpty(Dictionary<string, object> d, string name, object value)
   {
      if (null == value)
         return;
      if ((value is ICollection) && ((ICollection)value).Count < 1)
         return ;
      if ((value is WordList))
      {
         d.Add(name, ((WordList)value).words);
         return;
      }
      if ((value is string) && ((string) value) . Length < 1)
         return;
      d.Add(name, value);
   }


   /// <summary>
   /// Identifies the field/value pairs to keep
   /// </summary>
   /// <param name="obj"></param>
   /// <param name="serializer"></param>
   /// <returns></returns>
   public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
   {
      var jsonExample = new Dictionary<string, object>();
      foreach (var prop in obj.GetType().GetProperties())
      {
         var value = prop.GetValue(obj, System.Reflection.BindingFlags.Public, null, null, null);
         addNonEmpty(jsonExample, prop.Name, value);
      }
      foreach (var prop in obj.GetType().GetFields(System.Reflection.BindingFlags.Public| System.Reflection.BindingFlags.Instance))
      {
         var value = prop.GetValue(obj);
         addNonEmpty(jsonExample, prop.Name, value);
      }

      return jsonExample;
   }

   /// <summary>
   /// The list of types that this converter supports -- all of them!
   /// </summary>
   public override IEnumerable<Type> SupportedTypes
   {
      get { return GetType().Assembly.GetTypes(); }
   }
}


// This is a helper to form a well-defined json file
//  with possibly circular references
partial class ZNormalForm
{
   /// <summary>
   /// The "main" object that be returned at the end of deserialization
   /// </summary>
   public object rootObject;

   /// <summary>
   /// The table of objects
   /// </summary>
   public Dictionary<object, ZObject> objects;

   /// <summary>
   /// The children of each object
   /// </summary>
   public List<object[]>         children;

   /// <summary>
   /// The edges, in a direction, from one object to another
   /// </summary>
   public List<object[]>             edges;

   /// <summary>
   /// This converts the world into a JSON format
   /// </summary>
   /// <param name="root"></param>
   /// <returns></returns>
   internal static StringBuilder JSONSerialize(ZObject root)
   {
      // First create the normal form
      var tmp = buildNormalForm(root);
      // Next create the temporary buffers to hold the JSON
      var serializer = new JavaScriptSerializer();
      // Tell it to skip nulls and empties
      serializer.RegisterConverters(new JavaScriptConverter[] { new JSONConverter() });

      var sb = new StringBuilder();

      // Finally, Serialize the JSON
      serializer.Serialize(tmp, sb);
      return sb;
   }


   /// <summary>
   /// This creates a normal form of the world given by the root
   /// </summary>
   /// <param name="root">The object that refers to everything else</param>
   /// <returns>null on error, otherwise the normalize representation</returns>
   static ZNormalForm buildNormalForm(ZObject root)
   {
      // first, create the instance to hold it all
      var thing = new ZNormalForm();
      // go thru and add stuff
      thing.add(root);
      thing.rootObject = root.name;
      return thing;
   }


   /// <summary>
   /// Constructor
   /// </summary>
   ZNormalForm()
   {
      /// Create the arrays to hold the stuff
      objects = new Dictionary<object, ZObject>();
      children= new List<object[]>();
      edges   = new List<object[]>();
   }


   /// <summary>
   /// Add the given object to the tables.
   /// This is done before deserialization
   /// </summary>
   /// <param name="obj"></param>
   void add(ZObject obj)
   {
       // Skip the object if we already have it
       if (objects . ContainsKey(obj.name))
          return;
       // Add it to the table of objects
      objects [obj.name] = obj;

      // Add it's parent
      if (null != obj.Parent)
         add(obj.Parent);

      // Add each of it's children
 //     if (obj is ZObject)
      {
         // It has children
         var zobj = (ZObject) obj;
         // Get the number of children
         var L = zobj.NumChildren;
         if (L > 0)
         {
            // Create an array for the object
            var C = new object[L+1];
            // The head of the list is parent object
            C[0] = zobj.name;
            // Add each of the children
            for (int I = 0; I < L; I++)
            {
               var child = zobj.Children[I];
               // Add it to the table of objects first
               add(child);
               // Add it to the parent/child relationship
               C[I+1]= child.name;
           }
            // Add this to the table of parent/children relationships
           children.Add(C);
         }
      }

      // and ditto for the direction, but add the door
      if (obj is ZCell)
      {
         var zobj = (ZCell) obj;
         // for each of the directions
         foreach (var I in zobj.edgeInDirection)
         {
            var edge = I.Value;
            // Create a tuple to hold from->to, with the direction and door
            var E = new object[4]{zobj.name, edge.sink,I.Key, edge.door};
            // Add it to the tables;
            edges.Add(E);
         }
      }
   }


   /// <summary>
   /// Deserializes a JSON encoding of the world
   /// </summary>
   /// <param name="str"></param>
   /// <returns></returns>
   internal static ZObject JSONDeserialize(string str)
   {
      // First deserialize the JSON
      var serializer = new JavaScriptSerializer();
      var tmp = serializer.Deserialize<ZNormalForm>(str);
      if (null == tmp)
         return null;
      // Convert back into an operational form
      return tmp.formRunTime();
   }

   /// <summary>
   /// 
   /// </summary>
   /// <returns></returns>
   ZObject formRunTime()
   {
      // Link the table back together
      // Link each parent to child
      foreach (var I in children)
      {
         var obj = objects[I[0]];
         var L = I. Length;
         for (int J = 1; J < L; J++)
            obj.AddChild(objects[J]);
      }

      // Link the topological structures
      foreach (var I in edges)
      {
         var obj = (ZCell) objects[I[0]];
         var dest= (ZCell) objects[I[1]];
         ZObject door = null;
         if (null != I[3])
           objects.TryGetValue(I[3], out door);
         obj.edgeInDirection[(string)I[2]] = new ZEdge((string)I[2], dest, door);
      }

      // And return the root object
      return objects[rootObject];
   }
}

