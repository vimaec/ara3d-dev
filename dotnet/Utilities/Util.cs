using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.IO.MemoryMappedFiles;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ara3D
{
    // TODO: these are some question that I would like to address
    // https://stackoverflow.com/questions/1103495/is-there-a-proper-way-to-read-csv-files
    // https://docs.microsoft.com/en-us/dotnet/visual-basic/developing-apps/programming/drives-directories-files/how-to-read-from-comma-delimited-text-files
    // https://stackoverflow.com/questions/1050112/how-to-read-a-csv-file-into-a-net-datatable?noredirect=1&lq=1
    // https://www.codeproject.com/Articles/11698/A-Portable-and-Efficient-Generic-Parser-for-Flat-F
    // https://stackoverflow.com/questions/1898/csv-file-imports-in-net
    // https://stackoverflow.com/questions/217902/reading-writing-an-ini-file
    // https://www.codeproject.com/Tips/771772/A-Simple-and-Efficient-INI-File-Reader-in-Csharp

    public static unsafe class Util
    {
        public static IEnumerable<T> ForEach<T, U>(this IEnumerable<T> xs, Func<T, U> f)
        {
            foreach (var x in xs) f(x);
            return xs;
        }

        public static string ToIdentifier(this string self)
        {
            return String.IsNullOrEmpty(self) ? "_" : self.ReplaceNonAlphaNumeric("_");
        }

        public static string ReplaceNonAlphaNumeric(this string self, string replace)
        {
            return Regex.Replace(self, "[^a-zA-Z0-9]", replace);
        }

        // https://stackoverflow.com/questions/4959722/c-sharp-datatable-to-csv

        #region Writing CSV Data C# 

        //public static string EscapeQuotes(this string self)
        //{
        //    return self?.Replace("\"", "\"\"") ?? "";
        //}

        //public static string Surround(this string self, string before, string after)
        //{
        //    return $"{before}{self}{after}";
        //}

        //public static string Quoted(this string self, string quotes = "\"")
        //{
        //    return self.Surround(quotes, quotes);
        //}

        //public static string QuotedCSVFieldIfNecessary(this string self)
        //{
        //    return (self == null) ? "" : self.Contains('"') || self.Contains(',') ? self.Quoted() : self;
        //}

        //public static string ToCsvField(this string self)
        //{
        //    return self.EscapeQuotes().QuotedCSVFieldIfNecessary();
        //}

        //public static string ToCsvRow(this IEnumerable<string> self)
        //{
        //    return String.Join(",", self.Select(ToCsvField));
        //}

        //public static IEnumerable<string> ToCsvRows(this DataTable self)
        //{
        //    yield return self.Columns.OfType<object>().Select(c => c.ToString()).ToCsvRow();
        //    foreach (var dr in self.Rows.OfType<DataRow>())
        //        yield return dr.ItemArray.Select(i => ToCsvField(i.ToString())).ToCsvRow();
        //}

        //public static void ToCsvFile(this DataTable self, string path)
        //{
        //    File.WriteAllLines(path, self.ToCsvRows());
        //}

        //public static void ToCsvFile<T>(this IEnumerable<T> self, string path)
        //{
        //    self.PropertiesToDataTable().ToCsvFile(path);
        //}

        //public static void ToCsvFile<K, V>(this IDictionary<K, V> self, string path)
        //{
        //    self.PropertiesToDataTable().ToCsvFile(path);
        //}

        #endregion

        // https://stackoverflow.com/questions/4823467/using-linq-to-find-the-cumulative-sum-of-an-array-of-numbers-in-c-sharp/

        #region LINQ to find the cumulative sum of an array 

        public static IEnumerable<U> Accumulate<T, U>(this IEnumerable<T> self, U init, Func<U, T, U> f)
        {
            foreach (var x in self)
                yield return init = f(init, x);
        }

        public static IEnumerable<T> Accumulate<T>(this IEnumerable<T> self, Func<T, T, T> f)
        {
            return self.Accumulate(default(T), f);
        }

        public static IEnumerable<double> PartialSums(this IEnumerable<double> self)
        {
            return self.Accumulate((x, y) => x + y);
        }

        public static IEnumerable<int> PartialSums(this IEnumerable<int> self)
        {
            return self.Accumulate((x, y) => x + y);
        }

        #endregion

        #region Reflection to create a DataTable from a Class?

        /// <summary>
        /// Creates a data table from an array of classes, using the properties of the clases as column values
        /// https://stackoverflow.com/questions/18746064/using-reflection-to-create-a-datatable-from-a-class
        /// </summary>
        //public static DataTable PropertiesToDataTable<T>(this IEnumerable<T> self)
        //{
        //    var properties = typeof(T).GetProperties();

        //    // create a new data table if needed. Otherwise we are adding to the passed dataTable
        //    var dataTable = new DataTable();

        //    foreach (var info in properties)
        //        dataTable.Columns.Add(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType);

        //    foreach (var entity in self)
        //        dataTable.Rows.Add(properties.Select(p => p.GetValue(entity)).ToArray());

        //    return dataTable;
        //}

        /// <summary>
        /// Adds a new column to the datatable with the specific name, position, and values.
        /// </summary>
        //public static DataTable AddColumn<T>(this DataTable self, string name, IEnumerable<T> values, int position = -1)
        //{
        //    var dc = self.Columns.Add(name, typeof(T));
        //    if (position >= 0)
        //        dc.SetOrdinal(position);
        //    var i = 0;
        //    foreach (var v in values)
        //        self.Rows[i++].SetField(dc, v);
        //    return self;
        //}

        /// <summary>
        /// Given a dictionary creates a datatable from the values of the properties or fields 
        /// of the classes in the values, and a first column made from the keys of the dictionary
        /// </summary>
        //public static DataTable PropertiesOrFieldsToDataTable<K, V>(this IDictionary<K, V> self,
        //    string keyColumnName = "keys", bool propertiesOrFields = true)
        //{
        //    var dt = propertiesOrFields
        //        ? self.Values.PropertiesToDataTable()
        //        : self.Values.FieldsToDataTable();
        //    return dt.AddColumn(keyColumnName, self.Keys, 0);
        //}

        /// <summary>
        /// Creates a data table from an array of classes, using the fields of the clases as column values
        /// https://stackoverflow.com/questions/18746064/using-reflection-to-create-a-datatable-from-a-class
        /// </summary>
        //public static DataTable FieldsToDataTable<T>(this IEnumerable<T> self)
        //{
        //    var fields = typeof(T).GetAllFields();

        //    // create a new data table if needed. Otherwise we are adding to the passed dataTable
        //    var dataTable = new DataTable();

        //    foreach (var info in fields)
        //        dataTable.Columns.Add(info.Name, Nullable.GetUnderlyingType(info.FieldType) ?? info.FieldType);

        //    foreach (var entity in self)
        //        dataTable.Rows.Add(fields.Select(p => p.GetValue(entity)).ToArray());

        //    return dataTable;
        //}

        /// <summary>
        /// Given a data table, computes stats for every column in the data table that can be converted into numbers.
        /// </summary>
        //public static Dictionary<string, Statistics> ColumnStatistics(this DataTable self)
        //{
        //    return self.Columns.OfType<DataColumn>().Where(IsNumericColumn).ToDictionary(
        //        dc => dc.ColumnName,
        //        dc => dc.ColumnValues<double>().Statistics());
        //}

        /// <summary>
        /// Given a data table, computes stats for every column in the data table that can be converted into numbers.
        /// The result is returned as a datatable
        /// </summary>
        //public static DataTable ColumnStatisticsAsTable(this DataTable self)
        //{
        //    return ColumnStatistics(self).PropertiesOrFieldsToDataTable("statistic", false);
        //}

        /// <summary>
        /// Returns true if a data column can be cast to doubles.
        /// </summary>
        public static bool IsNumericColumn(this DataColumn dc)
        {
            return dc.DataType.CanCastToDouble();
        }

        #endregion

        /// <summary>
        /// Returns all the values in a data column converted into the specified type if possible.
        /// https://stackoverflow.com/questions/2916583/how-to-get-a-specific-column-value-from-a-datatable
        /// </summary>
        public static IEnumerable<T> ColumnValues<T>(this DataColumn self)
        {
            return self.Table.Select().Select(dr => (T) Convert.ChangeType(dr[self], typeof(T)));
        }

        public static string IfEmpty(this string self, string other)
        {
            return String.IsNullOrWhiteSpace(self) ? other : self;
        }

        public static string ElidedSubstring(this string self, int start, int length, int max)
        {
            return (length > max) ? self.Substring(start, max) + "..." : self.Substring(start, length);
        }

        public static IEnumerable<T> DifferentFromPrevious<T>(this IEnumerable<T> self)
        {
            var first = true;
            T prev = default(T);
            foreach (var x in self)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (!x.Equals(prev))
                        yield return x;
                }

                prev = x;
            }
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> self)
        {
#pragma warning disable RECS0017 // Possible compare of value type with 'null'
            return self.Where(x => x != null);
#pragma warning restore RECS0017 // Possible compare of value type with 'null'
        }

        /// <summary>
        /// Returns the current date-time in a format appropriate for appending to files.
        /// </summary>
        public static string GetTimeStamp()
            => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");       

        /// <summary>
        /// Appends a timestamp to a file name (just before extension). 
        /// NOTE: will strip path information. 
        /// </summary>
        public static string AddTimeStamp(this string filePath)
        {
            var ext = Path.GetExtension(filePath);
            var baseName = Path.GetFileNameWithoutExtension(filePath);
            return $"{baseName}-{GetTimeStamp()}{ext}";
        }

        public static string CopyToFolder(this string path, string dir, bool dontCreate = false)
        {
            if (!dontCreate)
                Directory.CreateDirectory(dir);
            var newPath = Path.Combine(dir, Path.GetFileName(path));
            File.Copy(path, newPath);
            return newPath;
        }

        public static string MoveToFolder(this string path, string dir, bool dontCreate = false)
        {
            if (!dontCreate)
                Directory.CreateDirectory(dir);
            var newPath = Path.Combine(dir, Path.GetFileName(path));
            File.Move(path, newPath);
            return newPath;
        }

        /// <summary>
        /// Given a file name, returns a new file name that has the parent directory name prepended to it.
        /// </summary>
        public static string GetFileNameWithParentDirectory(this string file, string sep = "-")
        {
            var baseName = Path.GetFileName(file);
            var dir = Path.GetDirectoryName(file);
            var dirName = new DirectoryInfo(dir).Name;
            return $"{dirName}{sep}{baseName}";
        }

        public static Action ToAction<R>(Func<R> f)
        {
            return () =>
            {
                f();
                return;
            };
        }

        public static Action<A0> ToAction<A0, R>(Func<A0, R> f)
        {
            return (x) =>
            {
                f(x);
                return;
            };
        }

        public static Action<A0, A1> ToAction<A0, A1, R>(Func<A0, A1, R> f)
        {
            return (x0, x1) =>
            {
                f(x0, x1);
                return;
            };
        }

        public static Func<bool> ToFunction(this Action action)
        {
            return () =>
            {
                action();
                return true;
            };
        }

        public static void TimeIt(this Action action, string label = "")
        {
            TimeIt(action.ToFunction(), label);
        }

        public static string PrettyPrintTimeElapsed(this Stopwatch sw)
            => $"{sw.Elapsed.Minutes}:{sw.Elapsed.Seconds}.{sw.Elapsed.Milliseconds}";

        public static void OutputTimeElapsed(this Stopwatch sw, string label)
            => Console.WriteLine($"{label}: time elapsed {sw.PrettyPrintTimeElapsed()}");

        public static T TimeIt<T>(this Func<T> function, string label = "")
        {
            var sw = new Stopwatch();
            sw.Start();
            var r = function();
            sw.Stop();
            sw.OutputTimeElapsed(label);
            return r;
        }

        /// <summary>
        /// Creates a disposable pinned array that contains a pointer for unsafe access to the elements in memory.
        /// </summary>
        public static PinnedArray<T> Pin<T>(this T[] xs)
        {
            return new PinnedArray<T>(xs);
        }

        /// <summary>
        /// Creates a disposable pinned array that contains a pointer for unsafe access to the elements in memory. Use in a "using" block. 
        /// </summary>
        public static PinnedArray Pin(this Array xs)
        {
            return new PinnedArray(xs);
        }

        /// <summary>
        /// Creates a disposable pinned struct that contains a pointer for unsafe access to the elements in memory. Use in a "using" block. 
        /// </summary>
        public static PinnedStruct<T> Pin<T>(this T x) where T : struct
        {
            return new PinnedStruct<T>(x);
        }

        /// <summary>
        /// Returns all instance fields, public and private.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetAllFields(this Type self)
        {
            return self.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Returns true if the type is a "plain old data" type (is a struct type that contains no references).
        /// This means that we should be able to create pointers to the type, and copying 
        /// arrays of them into buffers makes sense.
        /// </summary>
        public static bool ContainsNoReferences(this Type t)
        {
            return t.IsPrimitive || t.GetAllFields().Select(f => f.FieldType).All(ContainsNoReferences);
        }

        /// <summary>
        /// Performs an action using the the underlying bytes of an array.
        /// </summary>
        public static void UsingBytes<T>(this T[] self, Action<IBytes> action)
        {
            using (var data = self.Pin())
                action(data);
        }

        /// <summary>
        /// Converts a struct to bytes. 
        /// </summary>
        public static byte[] ToBytes<T>(this T self) where T : struct
        {
            return StructToBytes(self);
        }

        /// <summary>
        /// Converts a struct instance to a series of bytes 
        /// </summary>
        public static byte[] StructToBytes(object self)
        {
            if (self == null) return null;
            var t = self.GetType();
            var r = new byte[Marshal.SizeOf(t)];
            using (var pin = r.Pin())
                Marshal.StructureToPtr(self, pin.Ptr, false);
            return r;
        }     

        // Make-ref routines 
        // http://benbowen.blog/post/fun_with_makeref/
        // https://docs.microsoft.com/en-us/dotnet/api/system.typedreference?redirectedfrom=MSDN&view=netframework-4.7.2
        // https://stackoverflow.com/questions/9033/hidden-features-of-c/9125#9125
        // https://stackoverflow.com/questions/50618424/c-sharp-memory-address-extension-with-code
        // https://stackoverflow.com/questions/50831091/how-to-determine-whether-two-ref-variables-refer-to-the-same-variable-even-if/50831776#50831776
        // https://stackoverflow.com/questions/4764573/why-is-typedreference-behind-the-scenes-its-so-fast-and-safe-almost-magical

        public static IntPtr ToIntPtr<T>(T value) where T : struct
        {
            return __makeref(value).ToIntPtr();
        }

        public static long Distance(this IntPtr a, IntPtr b)
        {
            return Math.Abs(((byte*) b) - ((byte*) a));
        }

        public static IntPtr ToIntPtr(this TypedReference self)
        {
            return *(IntPtr*) &self;
        }

        public static byte* ToBytePtr(this IntPtr self)
        {
            return (byte*) self;
        }

        public static byte* ToBytePtr(this TypedReference self)
        {
            return self.ToIntPtr().ToBytePtr();
        }
        

        /// <summary>
        /// Given a dictionary looks up the key, or uses the function to add to the dictionary, and returns that result.  
        /// </summary>
        public static V GetOrCompute<K, V>(this IDictionary<K, V> self, K key, Func<K, V> func)
        {
            if (self.ContainsKey(key))
                return self[key];
            var value = func(key);
            self.Add(key, value);
            return value;
        }

        /// <summary>
        /// Returns a value if found in the dictionary, or default if not present.
        /// </summary>
        public static V GetOrDefault<K, V>(this IDictionary<K, V> self, K key)
            => self.ContainsKey(key) ? self[key] : default;

        /// <summary>
        /// Computes the size of the given managed type. Slow, but reliable. Does not give the same result as Marshal.SizeOf
        /// https://stackoverflow.com/questions/8173239/c-getting-size-of-a-value-type-variable-at-runtime
        /// https://stackoverflow.com/questions/3804638/whats-the-size-of-this-c-sharp-struct?noredirect=1&lq=1
        /// NOTE: this is not the same as what is the distance between these types in an array. That varies depending on alignment.
        /// </summary>
        public static int ComputeSizeOf(Type t)
        {
            // all this just to invoke one opcode with no arguments!
            var method = new DynamicMethod("ComputeSizeOfImpl", typeof(int), Type.EmptyTypes, typeof(Util), false);
            var gen = method.GetILGenerator();
            gen.Emit(OpCodes.Sizeof, t);
            gen.Emit(OpCodes.Ret);
            var func = (Func<int>) method.CreateDelegate(typeof(Func<int>));
            return func();
        }

        // Used to cache type sizes for types 
        public static Dictionary<Type, int> TypeSizes = new Dictionary<Type, int>();

        /// <summary>
        /// Returns the size of the managed type. 
        /// </summary>
        public static int SizeOf(this Type t)
        {
            return Marshal.SizeOf(t);
            //return TypeSizes.GetOrCompute(t, ComputeSizeOf);
        }

        /// <summary>
        /// Returns the size of the managed type. 
        /// </summary>
        public static int SizeOf<T>()
        {
            return SizeOf(typeof(T));
        }

        /// <summary>
        /// Reinterprets the bytes of a value type as another value type. Warning this should only ever be called 
        /// on objects that are target struct is smaller than 
        /// https://stackoverflow.com/questions/3139651/generic-method-to-cast-one-arbitrary-type-to-another-in-c-sharp
        /// </summary>
        public static TDest ReinterpretCast<TSrc, TDest>(TSrc source)
            where TDest : struct
        {
#if DEBUG
            if (SizeOf<TSrc>() < SizeOf<TDest>())
                throw new Exception("The destination type is smaller than the source type");
            if (!ContainsNoReferences(typeof(TSrc)))
                throw new Exception("The source type contains references");
            if (!ContainsNoReferences(typeof(TDest)))
                throw new Exception("The target type contains references");
#endif
            return __makeref(source).ToIntPtr().Cast<TDest>();
        }

        /// <summary>
        /// Casts the IntPtr into a new type
        /// </summary>
        public static TDest Cast<TDest>(this IntPtr self)
            where TDest : struct
        {
            var dest = default(TDest);
            var destRef = __makeref(dest);
            *(IntPtr*) &destRef = self;
            return __refvalue(destRef, TDest);
        }

        /// <summary>
        /// Copies the specified number of bytes between memory locations.
        /// Alternatives include: Marshal.Copy, and PInvoke to CopyMem
        /// https://stackoverflow.com/questions/15975972/copy-data-from-from-intptr-to-intptr        
        /// </summary>
        public static void MemoryCopy(IntPtr source, IntPtr dest, long size)
        {
            MemoryCopy(source.ToPointer(), dest.ToPointer(), size);
        }

        /// <summary>
        /// Copies the specified number of bytes between memory locations.
        /// Alternatives include: Marshal.Copy, and PInvoke to CopyMem
        /// https://stackoverflow.com/questions/15975972/copy-data-from-from-intptr-to-intptr        
        /// </summary>
        public static void MemoryCopy(void* source, void* dest, long size)
        {
            Buffer.MemoryCopy(source, dest, size, size);
        }

        /// <summary>
        /// Returns true if we can pin the type and copy its data using memory 
        /// https://stackoverflow.com/questions/10574645/the-fastest-way-to-check-if-a-type-is-blittable
        /// </summary>
        public static bool IsBlittable<T>()
        {
            return IsBlittableCache<T>.Value;
        }

        /// <summary>
        /// Returns true if we can pin the type and copy its data using memory 
        /// https://stackoverflow.com/questions/10574645/the-fastest-way-to-check-if-a-type-is-blittable
        /// </summary>
        public static bool IsBlittable(this Type type)
        {
            if (type.IsArray)
            {
                var elem = type.GetElementType();
                return elem.IsValueType && IsBlittable(elem);
            }

            try
            {
                var instance = FormatterServices.GetUninitializedObject(type);
                GCHandle.Alloc(instance, GCHandleType.Pinned).Free();
                return true;
            }
            catch
            {
                return false;
            }
        }

        static class IsBlittableCache<T>
        {
            public static readonly bool Value = IsBlittable(typeof(T));
        }

        /// <summary>
        /// Converts an array of blittable structs to an array of bytes. 
        /// </summary>
        public static byte[] ToBytes<T>(this T[] self) where T : struct
        {
            return (self as Array).ToBytes();
        }

        /// <summary>
        /// Returns the number of bytes in an array that has been pinned by GCHandle
        /// </summary>
        public static long SizeOfPinnedArray(this Array self)
        {
            return self.PinnedArrayBegin().Distance(self.PinnedArrayEnd());
        }

        /// <summary>
        /// Returns the memory address of the beginning of an array pinned by GCHandle
        /// </summary>
        public static IntPtr PinnedArrayBegin(this Array self)
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(self, 0);
        }

        /// <summary>
        /// Returns the memory address of the end of an array pinned by GCHandle
        /// </summary>
        public static IntPtr PinnedArrayEnd(this Array self)
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(self, self.Length);
        }

        /// <summary>
        /// Uses marshalling to converts an array of structs to an array of bytes. 
        /// </summary>
        public static byte[] ToBytes(this Array self)
        {
            if (self == null) return new byte[0];
            using (var pin = self.Pin())
                return pin.ToBytes();
        }

        /// <summary>
        /// Provides access to raw memory as an unmanaged memory stream.
        /// https://docs.microsoft.com/en-us/dotnet/api/system.io.unmanagedmemorystream?view=netframework-4.7.2
        /// </summary>
        public static UnmanagedMemoryStream ToMemoryStream(this IBytes self)
        {
            return new UnmanagedMemoryStream(self.Ptr.ToBytePtr(), self.ByteCount);
        }

        /// <summary>
        /// Provides access to a byte array as a stream.
        /// https://docs.microsoft.com/en-us/dotnet/api/system.io.memorystream?view=netframework-4.7.2
        /// </summary>
        public static MemoryStream ToMemoryStream(this byte[] self)
        {
            return new MemoryStream(self);
        }

        /// <summary>
        /// Reads all bytes from a stream
        /// https://stackoverflow.com/questions/1080442/how-to-convert-an-stream-into-a-byte-in-c
        /// </summary>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            using (var memoryStream = new MemoryStream()) {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
    /// Writes raw bytes to the stream by creating a memory stream around it. 
    /// </summary>
    public static void Write(this BinaryWriter self, IBytes bytes)
        {
            self.Write(bytes.ToBytes());
        }

        /// <summary>
        /// Writes a struct to a stream without any size
        /// </summary>
        public static void Write<T>(this BinaryWriter self, T value) where T: struct
        {
            using (var pin = value.Pin())
                self.Write(pin);
        }

        /// <summary>
        /// Writes an array of structs to a stream without any count
        /// </summary>
        public static void Write<T>(this BinaryWriter self, T[] value) where T : struct
        {
            using (var pin = value.Pin())
                self.Write(pin);
        }

        /// <summary>
        /// Given an array of blittable types writes the contents out as a raw array. 
        /// </summary>
        public static void Write(this BinaryWriter self, Array xs)
        {
            using (var pin = xs.Pin())
                self.Write(pin);
        }

        /// <summary>
        /// Returns true if the type self is an instance of the given generic type
        /// </summary>
        public static bool InstanceOfGenericType(this Type self, Type genericType)
        {
            return self.IsGenericType && self.GetGenericTypeDefinition() == genericType;
        }

        /// <summary>
        /// Returns true if the type self is an instance of the given generic interface, or implements the interface
        /// </summary>
        public static bool InstanceOfGenericInterface(this Type self, Type ifaceType)
        {
            return self.InstanceOfGenericType(ifaceType)
                   || self.GetInterfaces().Any(i => i.InstanceOfGenericType(ifaceType));
        }

        /// <summary>
        /// Returns true if the type implements IList with a generic parmaeter.
        /// </summary>
        public static bool ImplementsIList(this Type t)
        {
            return t.InstanceOfGenericInterface(typeof(IList<>));
        }

        /// <summary>
        /// Returns true if the value is in between two values.
        /// </summary>
        public static bool Between(this long value, long min, long max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Returns true if the value is in between two values.
        /// </summary>
        public static bool Between(this int value, long min, long max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Returns true if the value is in between two values.
        /// </summary>
        public static bool Between(this float value, double min, double max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Returns true if the value is in between two values.
        /// </summary>
        public static bool Between(this double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Returns true if the source type can be cast to doubles.
        /// </summary>
        public static bool CanCastToDouble(this Type typeSrc)
        {
            return typeSrc.IsPrimitive
                   && typeSrc != typeof(char)
                   && typeSrc != typeof(decimal)
                   && typeSrc != typeof(bool);
        }

        public static FileStream OpenFileStreamWriting(string filePath, int bufferSize)
        {
            return new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.None, bufferSize);
        }

        public static FileStream OpenFileStreamReading(string filePath, int bufferSize)
        {
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
        }

        /// <summary>
        /// The official Stream.Read iis a PITA, because it could return anywhere from 0 to the number of bytes
        /// requested, even in mid-stream. This call will read everything it can until it reaches 
        /// the end of the stream of "count" bytes.
        /// </summary>
        public static int SafeRead(this Stream stream, byte[] buffer, int offset, int count)
        {
            var r = stream.Read(buffer, offset, count);
            if (r != 0 && r < count)
            {
                // We didn't read everything, so let's keep trying until we get a zero 
                while (true)
                {
                    var tmp = stream.Read(buffer, r, count - r);
                    if (tmp == 0)
                        break;
                    r += tmp;
                }
            }

            return r;
        }

        public static bool NaiveSequenceEqual<T>(T[] buffer1, T[] buffer2) where T : IEquatable<T>
        {
            if (buffer1 == buffer2) return true;
            if (buffer1 == null || buffer2 == null) return false;
            if (buffer1.Length != buffer2.Length) return false;
            for (var i = 0; i < buffer1.Length; ++i)
                if (buffer1[i].Equals(buffer2[i]))
                    return false;
            return true;
        }

        public static bool SequenceEqual<T>(T[] buffer1, T[] buffer2) where T : IEquatable<T>
        {
            if (buffer1 == buffer2) return true;
            if (buffer1 == null || buffer2 == null) return false;
            return buffer1.AsSpan().SequenceEqual(buffer2.AsSpan());
        }

        // Improved answer over: 
        // https://stackoverflow.com/questions/211008/c-sharp-file-management
        // https://stackoverflow.com/questions/1358510/how-to-compare-2-files-fast-using-net?noredirect=1&lq=1
        // Should be faster than the SHA version, with no chance of a mismatch. 
        public static bool CompareFiles(string filePath1, string filePath2)
        {
            if (!File.Exists(filePath2) || !File.Exists(filePath2))
                return false;

            if (new FileInfo(filePath1).Length != new FileInfo(filePath2).Length)
                return false;

            // Default buffer size of File stream * 16. 
            // Profiling revealed this to be faster than the default 
            const int bufferSize = 4096 * 16;
            var buf1 = new byte[bufferSize];
            var buf2 = new byte[bufferSize];

            // open both for reading
            using (var stream1 = OpenFileStreamReading(filePath1, bufferSize))
            using (var stream2 = OpenFileStreamReading(filePath1, bufferSize))
            {
                // Read buffers (need to be careful because of contract of stream.Read)
                var tmp1 = stream1.SafeRead(buf1, 0, bufferSize);
                var tmp2 = stream2.SafeRead(buf2, 0, bufferSize);

                // Check that we read the same size
                if (tmp1 != tmp2) return false;

                // Compare the bytes 
                for (var i = 0; i < tmp1; ++i)
                    if (buf1[i] != buf2[i])
                        return false;
            }

            return true;
        }

        public static bool NaiveCompareFiles(string filePath1, string filePath2)
        {
            return SequenceEqual(
                File.ReadAllBytes(filePath1), File.ReadAllBytes(filePath2));
        }

        public static byte[] FileSHA256(string filePath)
        {
            return SHA256.Create().ComputeHash(File.OpenRead(filePath));
        }

        public static bool HashCompareFiles(string filePath1, string filePath2)
        {
            return SequenceEqual(FileSHA256(filePath1), FileSHA256(filePath2));
        }

        /// <summary>
        /// Executes an action capturing the console output.
        /// Improved answer over: 
        /// https://stackoverflow.com/questions/11911660/redirect-console-writeline-from-windows-application-to-a-string
        /// </summary>
        public static string RunCodeReturnConsoleOut(Action action)
        {
            var originalConsoleOut = Console.Out;
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);
                try
                {
                    action();
                    writer.Flush();
                    return writer.GetStringBuilder().ToString();
                }
                finally
                {
                    Console.SetOut(originalConsoleOut);
                }
            }
        }

        /// <summary>
        /// Perform some action when the current process shuts down. 
        /// </summary>
        public static void OnShutdown(Action action)
        {
            AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) => { action(); };
        }

        /// <summary>
        /// Closes a process if it isin't null and hasn't already exited. 
        /// </summary>
        /// <param name="process"></param>
        public static void SafeClose(this Process process)
        {
            if (process != null && !process.HasExited) process.CloseMainWindow();
        }

        /// <summary>
        /// Given a memory mapped file, creates a buffere, and reads data into it.
        /// </summary>
        public static byte[] ReadBytes(this MemoryMappedFile mmf, long offset, int count)
        {
            return mmf.ReadBytes(offset, new byte[count]);
        }

        /// <summary>
        /// Given a memory mapped file and a buffer fills it with data from the given offset. 
        /// </summary>
        public static byte[] ReadBytes(this MemoryMappedFile mmf, long offset, byte[] buffer)
        {
            using (var view = mmf.CreateViewStream(offset, buffer.Length, MemoryMappedFileAccess.Read))
            {
                view.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        /// <summary>
        /// Outputs information about the current memory state to the passed textWriter, or standard out if null. 
        /// </summary>
        public static void SnapshotMemory(TextWriter tw = null)
        {
            var process = Process.GetCurrentProcess();
            tw = tw ?? Console.Out;
            if (process != null)
            {
                long memsize = process.PrivateMemorySize64 >> 10;
                tw.WriteLine("Private memory: " + memsize.ToString("n"));
                memsize = process.WorkingSet64 >> 10;
                tw.WriteLine("Working set: " + memsize.ToString("n"));
                memsize = process.PeakWorkingSet64 >> 10;
                tw.WriteLine("Peak working set: " + memsize.ToString("n"));
            }
        }

        public static string EnumName<T>(this T x) where T: Enum
        {
            return Enum.GetName(typeof(T), x);
        }

        public static T ToStruct<T>(this byte[] bytes) where T : struct
        {
            return bytes.ToStructs<T>()[0];
        }

        public static T ToStruct<T>(this Span<byte> span) where T : struct
        {
            return span.ToStructs<T>()[0];
        }

        public static Span<T> Cast<T>(this Span<byte> span) where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(span);
        }

        public static Span<byte> AsBytes<T>(this Span<T> span) where T : struct
        {
            return MemoryMarshal.AsBytes(span);
        }

        public static Memory<T> ToMemory<T>(this T[] data) where T : struct
        {
            return new Memory<T>(data);
        }

        public static T[] ToStructs<T>(this Span<byte> span) where T : struct
        {
            return span.Cast<T>().ToArray();
        }

        public static T[] ToStructs<T>(this byte[] bytes) where T : struct
        {
            return bytes.AsSpan().ToStructs<T>();
        }

        public static string ToUtf8(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string ToAscii(this byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        public static byte[] ToBytesUtf8(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        public static byte[] ToBytesAscii(this string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }

        public static JObject ToJObject(this object o)
            => JObject.FromObject(o);

        public static JArray ToJArray<T>(this IEnumerable<T> xs)
            => xs.ToList().ToJArray();

        public static JArray ToJArray<T>(this IList<T> xs)
            => JArray.FromObject(xs);

        public static string ToJson(this object o)
            => o?.ToJObject()?.ToString() ?? "null";

        public static void ToJsonFile(this object o, string filePath)
        {
            using (var tw = File.CreateText(filePath))
                new JsonSerializer
                {
                    Formatting = Formatting.Indented,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                }.Serialize(tw, o);
        }

        public static string PrettifyJson(this string s)
        {
            return JToken.Parse(s).ToString();
        }

        /// <summary>
        /// Useful quick test to assure that we can create a file in the folder and write to it.
        /// </summary>
        public static void TestWrite(this DirectoryInfo di)
            => TestWrite(di.FullName);

        /// <summary>
        /// Useful quick test to assure that we can create a file in the folder and write to it.
        /// </summary>
        public static void TestWrite(string folder)
        {
            var fileName = Path.Combine(folder, "_deleteme_.tmp");
            File.WriteAllText(fileName, "test");
            File.Delete(fileName);
        }

        public static JObject LoadJson(string filePath) 
            => JObject.Parse(File.ReadAllText(filePath));

        public static JArray LoadJsonArray(string filePath) 
            => JArray.Parse(File.ReadAllText(filePath));

        public static T LoadJsonFromFile<T>(string filePath)
        {
            using (var file = File.OpenText(filePath))
                return LoadJsonFromStream<T>(file);
        }

        public static T LoadJsonFromStream<T>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
                return LoadJsonFromStream<T>(reader);
        }

        public static T LoadJsonFromStream<T>(StreamReader streamReader)
            => (T)(new JsonSerializer()).Deserialize(streamReader, typeof(T));       

        // File size reporting

        static readonly string[] ByteSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB

        /// Improved version of https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
        public static string BytesToString(long byteCount, int numPlacesToRound = 1)
        {
            if (byteCount == 0) return "0B";
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), numPlacesToRound);
            return $"{Math.Sign(byteCount) * num}{ByteSuffixes[place]}";
        }

        /// <summary>
        /// Returns the file size in bytes, or 0 if there is no file.
        /// </summary>
        public static long FileSize(string fileName)
            => File.Exists(fileName) ? new FileInfo(fileName).Length : 0;

        /// <summary>
        /// Returns the file size in bytes, or 0 if there is no file.
        /// </summary>
        public static string FileSizeAsString(string fileName, int numPlacesToShow = 1)
            => BytesToString(FileSize(fileName), numPlacesToShow);


        /// <summary>
        /// Returns the total file size of all files given
        /// </summary>
        public static long TotalFileSize(IEnumerable<string> files)
            => files.Sum(FileSize);

        /// <summary>
        /// Returns the total file size of all files given as a human readable string
        /// </summary>
        public static string TotalFileSizeAsString(IEnumerable<string> files, int numPlacesToShow = 1)
            => BytesToString(TotalFileSize(files), numPlacesToShow);

        /// <summary>
        /// Returns the most recently written to sub-folder
        /// </summary>
        public static string GetMostRecentSubFolder(string folderPath)
            => Directory.GetDirectories(folderPath).OrderByDescending(f => new DirectoryInfo(f).LastWriteTime)
                .FirstOrDefault();

        /// <summary>
        /// Adds an item to the referenced list, creating it if needed
        /// </summary>
        public static void AddToList<T>(ref IList<T> xs, T x)
            => (xs ?? (xs = new List<T>())).Add(x);
    }
}


