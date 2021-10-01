﻿// MIT License - Copyright (c) Malte Rupprecht
// This file is subject to the terms and conditions defined in
// LICENSE, which is part of this source code package

using System;
using System.Globalization;
using System.IO;

namespace LibreLancer.Ini
{
	public class StringValue : IValue
	{
		private int valuePointer;
		private string value;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
		public StringValue(BinaryReader reader, BiniStringBlock stringBlock)
		{
			if (reader == null) throw new ArgumentNullException("reader");
			if (stringBlock == null) throw new ArgumentNullException("stringBlock");

			this.valuePointer = reader.ReadInt32();
            this.value = stringBlock.Get(valuePointer);
        }

		public StringValue(string value)
		{
			if (value == null) throw new ArgumentNullException("value");
			this.valuePointer = -1;
			this.value = value;
		}

		public static implicit operator string(StringValue operand)
		{
			if (operand == null) return null;
			else return operand.value;
		}

		public bool ToBoolean()
		{
			bool result;
			if (bool.TryParse(value, out result)) return result;
			else return !string.IsNullOrEmpty(value);
		}

		public int ToInt32()
		{
			int result;
            uint result2;
			if (int.TryParse(value, out result)) return result;
			else if (uint.TryParse(value, out result2)) return (int) result2;
            else return -1;
		}

        public long ToInt64()
        {
            long result;
            if (long.TryParse(value, out result)) return result;
            else return -1;
        }

        public float ToSingle()
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
			float result;
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result)) return result;
            else
            {
                FLLog.Error("Ini", $"Failed to parse float {value}");
                return 0;
            }
        }

		public override string ToString()
		{
			return value;
		}
		public StringKeyValue ToKeyValue()
		{
			throw new InvalidCastException ();
		}
	}
}