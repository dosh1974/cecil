//
// CustomAttribute.cs
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

using Mono.Collections.Generic;

namespace Mono.Cecil {

	public struct CustomAttributeArgument {

		readonly TypeReference type;
		readonly object value;

		public TypeReference Type {
			get { return type; }
		}

		public object Value {
			get { return value; }
		}

		public CustomAttributeArgument (TypeReference type, object value)
		{
			Mixin.CheckType (type);
			this.type = type;
			this.value = value;
		}
	}

	public struct CustomAttributeNamedArgument {

		readonly string name;
		readonly CustomAttributeArgument argument;

		public string Name {
			get { return name; }
		}

		public CustomAttributeArgument Argument {
			get { return argument; }
		}

		public CustomAttributeNamedArgument (string name, CustomAttributeArgument argument)
		{
			Mixin.CheckName (name);
			this.name = name;
			this.argument = argument;
		}
	}

	public sealed class CustomAttribute {

		readonly internal uint signature;
		internal bool resolved;
		MethodReference constructor;
		internal Collection<CustomAttributeArgument> arguments;
		internal Collection<CustomAttributeNamedArgument> fields;
		internal Collection<CustomAttributeNamedArgument> properties;

		public MethodReference Constructor {
			get { return constructor; }
			set { constructor = value; }
		}

		public bool HasConstructorArguments {
			get {
				Resolve ();

				return !arguments.IsNullOrEmpty ();
			}
		}

		public Collection<CustomAttributeArgument> ConstructorArguments {
			get {
				Resolve ();

				if (arguments == null)
					arguments = new Collection<CustomAttributeArgument> ();

				return arguments;
			}
		}

		public bool HasFields {
			get {
				Resolve ();

				return !fields.IsNullOrEmpty ();
			}
		}

		public Collection<CustomAttributeNamedArgument> Fields {
			get {
				Resolve ();

				if (fields == null)
					fields = new Collection<CustomAttributeNamedArgument> ();

				return fields;
			}
		}

		public bool HasProperties {
			get {
				Resolve ();

				return !properties.IsNullOrEmpty ();
			}
		}

		public Collection<CustomAttributeNamedArgument> Properties {
			get {
				Resolve ();

				if (properties == null)
					properties = new Collection<CustomAttributeNamedArgument> ();

				return properties;
			}
		}

		internal bool HasImage {
			get { return constructor != null && constructor.HasImage; }
		}

		internal ModuleDefinition Module {
			get { return constructor.Module; }
		}

		internal CustomAttribute (uint signature, MethodReference constructor)
		{
			this.signature = signature;
			this.constructor = constructor;
			this.resolved = false;
		}

		public CustomAttribute (MethodReference constructor)
		{
			this.constructor = constructor;
			this.resolved = true;
		}

		public byte [] GetBlob ()
		{
			if (!HasImage || signature == 0)
				throw new NotSupportedException ();

			return Module.Read (this, (attribute, reader) => reader.ReadCustomAttributeBlob (attribute.signature)); ;
		}

		void Resolve ()
		{
			if (resolved || !HasImage)
				return;

			Module.Read (this, (attribute, reader) => {
				reader.ReadCustomAttributeSignature (attribute);
				return this;
			});

			resolved = true;
		}
	}

	static partial class Mixin {

		public static void CheckName (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("Empty name");
		}
	}
}
