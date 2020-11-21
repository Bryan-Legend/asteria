// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2014 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Steamworks {
	public class InteropHelp {
		public static void TestIfPlatformSupported() {
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN && !UNITY_STANDALONE_LINUX && !UNITY_STANDALONE_OSX && !STEAMWORKS_WIN && !STEAMWORKS_LIN_OSX
			throw new System.InvalidOperationException("Steamworks functions can only be called on platforms that Steam is available on.");
#endif
		}

		public static void TestIfAvailableClient() {
			TestIfPlatformSupported();
			if (NativeMethods.SteamClient() == System.IntPtr.Zero) {
				throw new System.InvalidOperationException("Steamworks is not initialized.");
			}
		}

		public static void TestIfAvailableGameServer() {
			TestIfPlatformSupported();
			if (NativeMethods.SteamClientGameServer() == System.IntPtr.Zero) {
				throw new System.InvalidOperationException("Steamworks is not initialized.");
			}
		}
		
		// This continues to exist for 'out string' arguments.
		public static string PtrToStringUTF8(IntPtr nativeUtf8) {
			if (nativeUtf8 == IntPtr.Zero)
				return string.Empty;

			int len = 0;

			while (Marshal.ReadByte(nativeUtf8, len) != 0)
				++len;

			if (len == 0)
				return string.Empty;

			byte[] buffer = new byte[len];
			Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
			return Encoding.UTF8.GetString(buffer);
		}
		
		// At some point this should become an IDisposable
		// We can't use an ICustomMarshaler because Unity dies when MarshalManagedToNative() gets called with a generic type.
		public class SteamParamStringArray {
			// The pointer to each AllocHGlobal() string
			IntPtr[] m_Strings;
			// The pointer to the condensed version of m_Strings
			IntPtr m_ptrStrings;
			// The pointer to the StructureToPtr version of SteamParamStringArray_t that will get marshaled
			IntPtr m_pSteamParamStringArray;

			public SteamParamStringArray(System.Collections.Generic.IList<string> strings) {
				if (strings == null) {
					m_pSteamParamStringArray = IntPtr.Zero;
					return;
				}

				m_Strings = new IntPtr[strings.Count];
				for (int i = 0; i < strings.Count; ++i) {
					byte[] strbuf = new byte[Encoding.UTF8.GetByteCount(strings[i]) + 1];
					Encoding.UTF8.GetBytes(strings[i], 0, strings[i].Length, strbuf, 0);
					m_Strings[i] = Marshal.AllocHGlobal(strbuf.Length);
					Marshal.Copy(strbuf, 0, m_Strings[i], strbuf.Length);
				}

				m_ptrStrings = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * m_Strings.Length);
				SteamParamStringArray_t stringArray = new SteamParamStringArray_t() {
					m_ppStrings = m_ptrStrings,
					m_nNumStrings = m_Strings.Length
				};
				Marshal.Copy(m_Strings, 0, stringArray.m_ppStrings, m_Strings.Length);

				m_pSteamParamStringArray = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SteamParamStringArray_t)));
				Marshal.StructureToPtr(stringArray, m_pSteamParamStringArray, false);
			}

			~SteamParamStringArray() {
				foreach (IntPtr ptr in m_Strings) {
					Marshal.FreeHGlobal(ptr);
				}

				if (m_ptrStrings != IntPtr.Zero) {
					Marshal.FreeHGlobal(m_ptrStrings);
				}

				if (m_pSteamParamStringArray != IntPtr.Zero) {
					Marshal.FreeHGlobal(m_pSteamParamStringArray);
				}
			}

			public static implicit operator IntPtr(SteamParamStringArray that) {
				return that.m_pSteamParamStringArray;
			}
		}
	}

	public class UTF8Marshaler : ICustomMarshaler {
		public const string DoNotFree = "DoNotFree";

		private static UTF8Marshaler static_instance_free = new UTF8Marshaler(true);
		private static UTF8Marshaler static_instance = new UTF8Marshaler(false);

		private bool _freeNativeMemory;

		private UTF8Marshaler(bool freenativememory) {
			_freeNativeMemory = freenativememory;
		}

		public IntPtr MarshalManagedToNative(object managedObj) {
			if (managedObj == null) {
				return IntPtr.Zero;
			}

			string str = managedObj as string;
			if (str == null) {
				throw new Exception("UTF8Marshaler must be used on a string.");
			}

			byte[] strbuf = new byte[Encoding.UTF8.GetByteCount(str) + 1];
			Encoding.UTF8.GetBytes(str, 0, str.Length, strbuf, 0);
			IntPtr buffer = Marshal.AllocHGlobal(strbuf.Length);
			Marshal.Copy(strbuf, 0, buffer, strbuf.Length);
			return buffer;
		}

		public object MarshalNativeToManaged(IntPtr pNativeData) {
			int len = 0;

			while (Marshal.ReadByte(pNativeData, len) != 0) {
				++len;
			}

			if (len == 0) {
				return string.Empty;
			}

			byte[] strbuf = new byte[len];
			Marshal.Copy(pNativeData, strbuf, 0, strbuf.Length);
			return Encoding.UTF8.GetString(strbuf);
		}

		public void CleanUpNativeData(IntPtr pNativeData) {
			if (_freeNativeMemory) {
				Marshal.FreeHGlobal(pNativeData);
			}
		}

		public void CleanUpManagedData(object managedObj) {
		}

		public int GetNativeDataSize() {
			return -1;
		}

		public static ICustomMarshaler GetInstance(string cookie) {
			switch (cookie) {
				case "DoNotFree":
					return static_instance;
				default:
					return static_instance_free;
			}
		}
	}
}
