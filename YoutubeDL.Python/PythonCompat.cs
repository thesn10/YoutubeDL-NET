using System;
using System.Collections.Generic;
using System.Text;
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

        [DllImport("python37", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyObject_GetBuffer(IntPtr exporter, IntPtr view, int flags);

        [DllImport("python37", CallingConvention = CallingConvention.Cdecl)]
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

        public static Dictionary<string, object> PythonDictToManaged(dynamic pythonDict)
        {
            Dictionary<string, object> infoDict = new Dictionary<string, object>();
            foreach (string key in pythonDict)
            {
                switch ((string)pythonDict[key].__class__.__name__)
                {
                    case "str":
                        infoDict.Add(key, (string)pythonDict[key]);
                        break;
                    case "int":
                        infoDict.Add(key, (int)pythonDict[key]);
                        break;
                    case "float":
                        infoDict.Add(key, (float)pythonDict[key]);
                        break;
                    case "dict":
                        infoDict.Add(key, PythonCompat.PythonDictToManaged(pythonDict[key]));
                        break;
                    case "list":
                        infoDict.Add(key, PythonCompat.PythonListToManaged(pythonDict[key]));
                        break;
                    case "generator":
                        PyObject obj = (PyObject)pythonDict[key];
                        dynamic list = new PyList(Runtime.PySequence_List(obj.Handle));
                        infoDict.Add(key, PythonCompat.PythonListToManaged(list));
                        break;
                    case "NoneType":
                        infoDict.Add(key, null);
                        break;
                    default:
                        Debug.WriteLine("unsupported type:" + (string)pythonDict[key].__class__.__name__);
                        break;
                }
            }
            return infoDict;
        }

        public static List<object> PythonListToManaged(dynamic pythonList)
        {
            List<object> infoDict = new List<object>();
            foreach (dynamic val in pythonList)
            {
                switch ((string)val.__class__.__name__)
                {
                    case "str":
                        infoDict.Add((string)val);
                        break;
                    case "int":
                        infoDict.Add((int)val);
                        break;
                    case "float":
                        infoDict.Add((float)val);
                        break;
                    case "dict":
                        infoDict.Add(PythonCompat.PythonDictToManaged(val));
                        break;
                    case "list":
                        infoDict.Add(PythonCompat.PythonListToManaged(val));
                        break;
                    case "NoneType":
                        infoDict.Add(null);
                        break;
                    default:
                        Debug.WriteLine("unsupported type:" + (string)val.__class__.__name__);
                        break;
                }
            }
            return infoDict;
        }
    }
}
