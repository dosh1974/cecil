//
// PdbHelper.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2010 Jb Evain
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using Mono.Cecil.Cil;

namespace Mono.Cecil.Pdb {

	class PdbHelper {

		[DllImport("ole32.dll")]
		static extern int CoCreateInstance (
			[In] ref Guid rclsid,
			[In, MarshalAs (UnmanagedType.IUnknown)] object pUnkOuter,
			[In] uint dwClsContext,
			[In] ref Guid riid,
			[Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

		static Guid s_dispenserClassID = new Guid (0xe5cb7a31, 0x7512, 0x11d2, 0x89, 0xce, 0x00, 0x80, 0xc7, 0x92, 0xe5, 0xd8);
		static Guid s_dispenserIID = new Guid (0x809c652e, 0x7396, 0x11d2, 0x97, 0x71, 0x00, 0xa0, 0xc9, 0xb4, 0xd5, 0x0c);
		static Guid CLSID_CorMetaDataRuntime = new Guid ("005023ca-72b1-11d3-9fc4-00c04f79a0a3");
		static Guid IID_IMetaDataEmit = new Guid ("ba3fee4c-ecb9-4e41-83b7-183fa41cd859");

		public static SymWriter CreateWriter (string pdb)
		{
			SymWriter writer = new SymWriter ();

			object objDispenser;
			CoCreateInstance (ref s_dispenserClassID, null, 1, ref s_dispenserIID, out objDispenser);

			object emitter;
			IMetaDataDispenser dispenser = (IMetaDataDispenser) objDispenser;
			dispenser.DefineScope (ref CLSID_CorMetaDataRuntime, 0, ref IID_IMetaDataEmit, out emitter);

			try {
				if (File.Exists (pdb))
					File.Delete (pdb);

				writer.Initialize (emitter, pdb, true);
			} finally {
				Marshal.ReleaseComObject (dispenser);
			}

			return writer;
		}

		public static string GetPdbFileName (string assemblyFileName)
		{
			return Path.ChangeExtension (assemblyFileName, ".pdb");
		}
	}

	public class PdbReaderProvider : Cil.ISymbolReaderProvider {

		public Cil.ISymbolReader GetSymbolReader (ModuleDefinition module, string fileName)
		{
			return new PdbReader (File.OpenRead (PdbHelper.GetPdbFileName (fileName)));
		}

		public Cil.ISymbolReader GetSymbolReader (ModuleDefinition module, Stream symbolStream)
		{
			throw new NotImplementedException ();
		}
	}

	public class PdbWriterProvider : Cil.ISymbolWriterProvider {

		public Cil.ISymbolWriter GetSymbolWriter (ModuleDefinition module, string fileName)
		{
			return new PdbWriter (PdbHelper.CreateWriter (PdbHelper.GetPdbFileName (fileName)));
		}

		public Cil.ISymbolWriter GetSymbolWriter (ModuleDefinition module, Stream symbolStream)
		{
			throw new NotImplementedException ();
		}
	}

	static class GuidMapping {

		static readonly Dictionary<Guid, DocumentLanguage> guid_language = new Dictionary<Guid, DocumentLanguage> ();
		static readonly Dictionary<DocumentLanguage, Guid> language_guid = new Dictionary<DocumentLanguage, Guid> ();

		static GuidMapping ()
		{
			AddMapping (DocumentLanguage.C, new Guid (0x63a08714, 0xfc37, 0x11d2, 0x90, 0x4c, 0x0, 0xc0, 0x4f, 0xa3, 0x02, 0xa1));
			AddMapping (DocumentLanguage.Cpp, new Guid (0x3a12d0b7, 0xc26c, 0x11d0, 0xb4, 0x42, 0x0, 0xa0, 0x24, 0x4a, 0x1d, 0xd2));
			AddMapping (DocumentLanguage.CSharp, new Guid (0x3f5162f8, 0x07c6, 0x11d3, 0x90, 0x53, 0x0, 0xc0, 0x4f, 0xa3, 0x02, 0xa1));
			AddMapping (DocumentLanguage.Basic, new Guid (0x3a12d0b8, 0xc26c, 0x11d0, 0xb4, 0x42, 0x0, 0xa0, 0x24, 0x4a, 0x1d, 0xd2));
			AddMapping (DocumentLanguage.Java, new Guid (0x3a12d0b4, 0xc26c, 0x11d0, 0xb4, 0x42, 0x0, 0xa0, 0x24, 0x4a, 0x1d, 0xd2));
			AddMapping (DocumentLanguage.Cobol, new Guid (0xaf046cd1, 0xd0e1, 0x11d2, 0x97, 0x7c, 0x0, 0xa0, 0xc9, 0xb4, 0xd5, 0xc));
			AddMapping (DocumentLanguage.Pascal, new Guid (0xaf046cd2, 0xd0e1, 0x11d2, 0x97, 0x7c, 0x0, 0xa0, 0xc9, 0xb4, 0xd5, 0xc));
			AddMapping (DocumentLanguage.Cil, new Guid (0xaf046cd3, 0xd0e1, 0x11d2, 0x97, 0x7c, 0x0, 0xa0, 0xc9, 0xb4, 0xd5, 0xc));
			AddMapping (DocumentLanguage.JScript, new Guid (0x3a12d0b6, 0xc26c, 0x11d0, 0xb4, 0x42, 0x0, 0xa0, 0x24, 0x4a, 0x1d, 0xd2));
			AddMapping (DocumentLanguage.Smc, new Guid (0xd9b9f7b, 0x6611, 0x11d3, 0xbd, 0x2a, 0x0, 0x0, 0xf8, 0x8, 0x49, 0xbd));
			AddMapping (DocumentLanguage.MCpp, new Guid (0x4b35fde8, 0x07c6, 0x11d3, 0x90, 0x53, 0x0, 0xc0, 0x4f, 0xa3, 0x02, 0xa1));
		}

		static void AddMapping (DocumentLanguage language, Guid guid)
		{
			guid_language.Add (guid, language);
			language_guid.Add (language, guid);
		}

		static readonly Guid type_text = new Guid (0x5a869d0b, 0x6611, 0x11d3, 0xbd, 0x2a, 0x00, 0x00, 0xf8, 0x08, 0x49, 0xbd);

		public static DocumentType ToType (this Guid guid)
		{
			if (guid == type_text)
				return DocumentType.Text;

			return DocumentType.Other;
		}

		public static Guid ToGuid (this DocumentType type)
		{
			if (type == DocumentType.Text)
				return type_text;

			return new Guid ();
		}

		static readonly Guid hash_md5 = new Guid (0x406ea660, 0x64cf, 0x4c82, 0xb6, 0xf0, 0x42, 0xd4, 0x81, 0x72, 0xa7, 0x99);
		static readonly Guid hash_sha1 = new Guid (0xff1816ec, 0xaa5e, 0x4d10, 0x87, 0xf7, 0x6f, 0x49, 0x63, 0x83, 0x34, 0x60);

		public static DocumentHashAlgorithm ToHashAlgorithm (this Guid guid)
		{
			if (guid == hash_md5)
				return DocumentHashAlgorithm.MD5;

			if (guid == hash_sha1)
				return DocumentHashAlgorithm.SHA1;

			return DocumentHashAlgorithm.None;
		}

		public static Guid ToGuid (this DocumentHashAlgorithm hash_algo)
		{
			if (hash_algo == DocumentHashAlgorithm.MD5)
				return hash_md5;

			if (hash_algo == DocumentHashAlgorithm.SHA1)
				return hash_sha1;

			return new Guid ();
		}

		public static DocumentLanguage ToLanguage (this Guid guid)
		{
			DocumentLanguage language;
			if (!guid_language.TryGetValue (guid, out language))
				return DocumentLanguage.Other;

			return language;
		}

		public static Guid ToGuid (this DocumentLanguage language)
		{
			Guid guid;
			if (!language_guid.TryGetValue (language, out guid))
				return new Guid ();

			return guid;
		}

		static readonly Guid vendor_ms = new Guid (0x994b45c4, 0xe6e9, 0x11d2, 0x90, 0x3f, 0x00, 0xc0, 0x4f, 0xa3, 0x02, 0xa1);

		public static DocumentLanguageVendor ToVendor (this Guid guid)
		{
			if (guid == vendor_ms)
				return DocumentLanguageVendor.Microsoft;

			return DocumentLanguageVendor.Other;
		}

		public static Guid ToGuid (this DocumentLanguageVendor vendor)
		{
			if (vendor == DocumentLanguageVendor.Microsoft)
				return vendor_ms;

			return new Guid ();
		}
	}
}

#if !NET_4_0

namespace System.Runtime.CompilerServices {

	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
	sealed class ExtensionAttribute : Attribute {
	}
}

#endif
