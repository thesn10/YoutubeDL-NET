using System;
using System.Collections;
using System.Collections.Generic;
using Python.Runtime;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace YoutubeDL.Python
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct Py_ssize_t
    {
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr size;
        public long GetSize()
        {
            return (long)size;
        }
    }

    /* buffer interface */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct Py_buffer
    {
        public IntPtr buf;
        public IntPtr obj;        /* owned reference */
        public Py_ssize_t len;
        public Py_ssize_t itemsize;  /* This is Py_ssize_t so it can be
                             pointed to by strides in simple case.*/
        public int _readonly;
        public int ndim;
        [MarshalAs(UnmanagedType.LPStr)]
        public string format;
        public IntPtr shape;
        public IntPtr strides;
        public IntPtr suboffsets;
        public IntPtr _internal;
    }

    class PythonCompat
    {

#if MONO_LINUX || MONO_OSX // Linux/macOS use dotted version string
        internal const string dllBase = "python3.7m";
#else // Windows
        internal const string dllBase = "python37";
#endif

        [DllImport(dllBase, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyObject_GetBuffer(IntPtr exporter, IntPtr view, int flags);

        [DllImport(dllBase, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void PyBuffer_Release(IntPtr view);

        public static void WriteToBuffer(PyObject obj, byte[] buffer)
        {
            int size = Marshal.SizeOf(typeof(Py_buffer));
            byte[] rawData = new byte[size];
            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            IntPtr view = handle.AddrOfPinnedObject();

            int err = PyObject_GetBuffer(obj.Handle, view, 0);
            if (err >= 0)
            {
                Py_buffer b = Marshal.PtrToStructure<Py_buffer>(view);

                Marshal.Copy(buffer, 0, b.buf, buffer.Length);
                PyBuffer_Release(view);
            }
        }
        public static PyObject ManagedObjectToPython(object obj) => ManagedObjectToPython(obj, obj.GetType());

        public static PyObject ManagedObjectToPython(object obj, Type pyType)
        {
            if (obj is string || obj is int || obj is float || obj is null)
            {
                return obj.ToPython();
            }
            else if (typeof(IList).IsAssignableFrom(pyType))
            {
                PyList list = new PyList();
                for (int i = 0; i < (obj as IList).Count; i++)
                {
                    if (!(obj as IList)[i].GetType().IsPrimitive) continue;
                    list.SetItem(i, (obj as IList)[i].ToPython());
                }
                return list;
            }
            else if (typeof(IDictionary).IsAssignableFrom(pyType))
            {
                PyDict dict = new PyDict();
                foreach (DictionaryEntry kv in (obj as IDictionary))
                {
                    if (!kv.Value.GetType().IsPrimitive) continue;
                    dict.SetItem(kv.Key.ToPython(), kv.Value.ToPython());
                }
                return dict;
            }
            else throw new NotSupportedException("Unsupported type: " + obj.GetType().Name);
        }

        public static object PythonObjectToManaged(dynamic pythonObj)
        {
            switch ((string)pythonObj.__class__.__name__)
            {
                case "str":
                    return (string)pythonObj;
                case "int":
                    long num = (long)pythonObj;
                    if (num >= int.MaxValue || num <= int.MinValue) return num;
                    else return (int)num;
                case "float":
                    return (float)pythonObj;
                case "bool":
                    return (bool)pythonObj;
                case "dict":
                    Dictionary<string, object> infoDict = new Dictionary<string, object>();
                    foreach (string key in pythonObj)
                    {
                        infoDict.Add(key, PythonCompat.PythonObjectToManaged(pythonObj[key]));
                    }
                    return infoDict;
                case "list":
                    List<object> infoDict2 = new List<object>();
                    foreach (dynamic val in pythonObj)
                    {
                        infoDict2.Add(PythonCompat.PythonObjectToManaged(val));
                    }
                    return infoDict2;
                case "generator":
                    PyObject obj = (PyObject)pythonObj;
                    dynamic list = new PyList(Runtime.PySequence_List(obj.Handle));
                    return PythonCompat.PythonObjectToManaged(list);
                case "NoneType":
                    return null;
                default:
                    Debug.WriteLine("Unsupported type: " + (string)pythonObj.__class__.__name__);
                    throw new NotSupportedException("Unsupported type: " + (string)pythonObj.__class__.__name__);
            }
        }
    }
}
