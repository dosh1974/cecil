//
// TypeDefinitionCollection.cs
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

using Mono.Collections.Generic;

namespace Mono.Cecil {

	sealed class TypeDefinitionCollection : Collection<TypeDefinition> {

		readonly ModuleDefinition container;
		readonly Dictionary<string, TypeDefinition> name_cache = new Dictionary<string, TypeDefinition> ();

		internal TypeDefinitionCollection (ModuleDefinition container)
		{
			this.container = container;
		}

		internal TypeDefinitionCollection (ModuleDefinition container, int capacity)
			: base (capacity)
		{
			this.container = container;
		}

		protected override void OnAdd (TypeDefinition item, int index)
		{
			Attach (item);
		}

		protected override void OnSet (TypeDefinition item, int index)
		{
			Attach (item);
		}

		protected override void OnInsert (TypeDefinition item, int index)
		{
			Attach (item);
		}

		protected override void OnRemove (TypeDefinition item, int index)
		{
			Detach (item);
		}

		protected override void OnClear ()
		{
			foreach (var type in this)
				Detach (type);
		}

		void Attach (TypeDefinition type)
		{
			if (type.Module != null && type.Module != container)
				throw new ArgumentException ("Type already attached");

			type.module = container;
			name_cache [type.FullName] = type;
		}

		void Detach (TypeDefinition type)
		{
			type.module = null;
			name_cache.Remove (type.FullName);
		}

		public TypeDefinition GetType (string fullname)
		{
			TypeDefinition type;
			if (name_cache.TryGetValue (fullname, out type))
				return type;

			return null;
		}
	}
}
