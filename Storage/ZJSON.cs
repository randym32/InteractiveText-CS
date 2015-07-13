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
   public Dictionary<object, object> objects;

   /// <summary>
   /// The children of each object
   /// </summary>
   public List<object[]>         children;

   /// <summary>
   /// This maps a relation name to the relations on objects
   /// </summary>
   public Dictionary<string,object> relations;


   /// <summary>
   /// This converts the world into a JSON format
   /// </summary>
   /// <param name="root"></param>
   /// <returns></returns>
   internal static StringBuilder JSONSerialize(InStory story)
   {
      // First create the normal form
      var tmp = buildNormalForm(story);
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
   static ZNormalForm buildNormalForm(InStory story)
   {
      // first, create the instance to hold it all
      var thing = new ZNormalForm();
      // go thru and add stuff
      thing.addRelation("instance", (Dictionary<ZObject, ZObject>) story.relations[InStory._instance]);
      foreach (var item in story.relations)
      {
         if (item.Value is SparseGraph<ZObject>)
            thing.addRelation((string) item.Key.name, (SparseGraph<ZObject>) item.Value);
      }
      thing.add((ZObject)story.player);
      thing.rootObject = ((ZObject)story.player).name;
      return thing;
   }


   /// <summary>
   /// Constructor
   /// </summary>
   public ZNormalForm()
   {
      /// Create the arrays to hold the stuff
      objects = new Dictionary<object, object>();
      children= new List<object[]>();
      relations= new Dictionary<string,object>();
   }


   /// <summary>
   /// Adds a relation of arity 1 to the JSON table
   /// </summary>
   /// <param name="name">The stored name of the table</param>
   /// <param name="rel">The relation</param>
   void addRelation(string name, Dictionary<ZObject, ZObject> rel)
   {
      // Store the relation for later
      relations[name] = add(rel.Keys);
   }


   /// <summary>
   /// Creates a list of node references, adding them to the set of objects
   /// if they are not already
   /// </summary>
   /// <param name="list">The list of nodes to add</param>
   /// <returns>The list of node names</returns>
   List<object> add(IEnumerable<ZObject> list)
   {
      // Create the list of things in the relation
      var tmp = new List<object>();
      foreach (var item in list)
      {
         // Just keep the name
         tmp.Add(item.name);
         // Add the object to the table of referenced stuff
         add((ZObject)item);
      }
      return tmp;
   }


   /// <summary>
   /// This adds the relation to the output
   /// </summary>
   /// <param name="name">The name of the relation</param>
   /// <param name="rel"></param>
   void addRelation(string name, SparseGraph<ZObject> rel)
   {
      // Make a normal version of the relation spec
      var spec = new Dictionary<object,object>();
      foreach (var item in rel.SuccessorTable)
      {
         add((ZObject)item.Key);
         var L = new List<object[]>();
         foreach (var edge in item.Value)
         {
            add(edge.sink);
            add(edge.gate);
            L.Add(new object[]{edge.sink.name,null==edge.gate?null:edge.gate.name});
         }
         spec[item.Key.name] = L;
      }
      // Add that to the relations table
      var cb = new Dictionary<string,object>();
      cb["derive"]=rel.immediate;
      if (spec.Count > 0)
         cb["spec"]=spec;
      relations[name] =  cb;
   }


   /// <summary>
   /// Add the given object to the tables.
   /// This is done before deserialization
   /// </summary>
   /// <param name="obj"></param>
   void add(ZObject obj)
   {
      if (null == obj)
         return;
      // Skip the object if we already have it
      if (objects . ContainsKey(obj.name))
         return;
      // Add it to the table of objects
      objects [obj.name] = obj;

      // Add it's parent
      if (null != obj.Parent)
         add(obj.Parent);

      // Add each of it's children
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


   /// <summary>
   /// Deserializes a JSON encoding of the world
   /// </summary>
   /// <param name="str">The JSON string</param>
   /// <returns></returns>
   internal static ZNormalForm JSONDeserialize(string str)
   {
      // First deserialize the JSON
      var serializer = new JavaScriptSerializer();
      return serializer.Deserialize<ZNormalForm>(str);
   }


   /// <summary>
   /// Get the objects, esp root object
   /// </summary>
   /// <returns>The root object</returns>
   internal ZObject Objects(InStory story)
   {
      // Go thru an make the ZObjects;
      var newObjects=new Dictionary<object,object>();
      foreach (var item in objects)
      {
         var d = (Dictionary<string,object>) item.Value;
         var n = new ZObject();
         object obj;
         if (d.TryGetValue("shortDescription", out obj))
            n.shortDescription = (string) obj;
         if (d.TryGetValue("nouns", out obj))
            n.nouns = (object[]) obj;
         newObjects[item.Key] = n;
      }

      // Go thru and use the internal "special" forms for some objects
      newObjects[InStory. _instance.name] = InStory. _instance;
      newObjects[InStory.    __kind.name] = InStory. __kind;
      newObjects[InStory.  opposite.name] = InStory. opposite;
      newObjects[InStory.  roomKind.name] = InStory. roomKind;
      newObjects[InStory.   dirKind.name] = InStory.  dirKind;
      newObjects[InStory.  cellKind.name] = InStory. cellKind;
      newObjects[InStory.personKind.name] = InStory.personKind;
      objects = newObjects;

      // Go thru and replace references out special one
      replace(story.east);
      replace(story.west);
      replace(story.north);
      replace(story.south);

      // Link the table back together
      // Link each parent to child
      foreach (var I in children)
      {
         var obj = newObjects[I[0]];
         var L = I. Length;
         for (int J = 1; J < L; J++)
            ((ZObject)obj).AddChild((ZObject) objects[I[J]]);
      }

      // And return the root object
      return (ZObject) objects[rootObject];
   }

   /// <summary>
   /// Replace items similarly named to x with x
   /// </summary>
   /// <param name="x"></param>
   void replace(ZObject x)
   {
      object it = null;
      // Scan (this can be slow)
      foreach (var item in objects)
      {
         var y = (ZObject) item.Value;
         if (x.nouns.Equals(y.nouns) &&
            ((x.modifiers == null && y.modifiers == null) ||
            (x.modifiers != null && x.modifiers.Equals(y.modifiers))))
         {
            it = item.Key;
            break;
         }
      }
      // Replace the item, if it was found
      if (null != it)
         objects[it] = x;
   }

   /// <summary>
   /// This reconstructs the relations table
   /// </summary>
   /// <returns>The relations table</returns>
   internal Dictionary<ZObject,object> Relations()
   {
      // Link the relations structures
      var r2 =  new Dictionary<ZObject,object>();
      foreach (var I in relations)
      {
         var v = I.Value;
         // If it's an array, it's a relation 1
         if (v is IList)
         {
            // Go thru, get the objects and populate a dictionary
            var d = new Dictionary<ZObject, ZObject>();
            foreach (var item in (IList) v)
            {
               var obj = (ZObject) objects[item];
               d[obj] = obj;
            }
            // there, done
            r2[(ZObject) objects[I.Key]] = d;
         }
         // If it's a dictionary it's a relation 2
         else if (v is Dictionary<string,object>)
         {
            var d = (Dictionary<string,object>)v;
            // each key is an array of arrays...
            // First get the immediate vs derived
            bool immediate = false;
            object b;
            if (d.TryGetValue("derive", out b) && b is bool)
               immediate = !(bool)b;
            // Next get the values
            var n = new SparseGraph<ZObject>(immediate);
            object val;
            if (d.TryGetValue("spec", out val))
            {
               var d2 = (Dictionary<string,object>)val;
               foreach (var edges in d2)
               {
                  var src = (ZObject)objects[edges.Key];
                  foreach (var edge in (IList)edges.Value)
                  {
                     // Get the nodes for the sink and gate
                     var tuple = (IList) edge;
                     var sink = (ZObject) objects[tuple[0]];
                     ZObject gate = null;
                     if (tuple.Count > 1 && null != tuple[1])
                        gate = (ZObject) objects[tuple[1]];
                     // add them
                     n.After(src, new Edge<ZObject>(sink, gate));
                  }
               }
            }

            // Add the relation to the table
            r2[(ZObject) objects[I.Key]] = n;
         }
      }
      // Return the relations
      return r2;
   }
}

