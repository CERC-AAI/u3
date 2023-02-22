using UnityEngine;
using System;
using System.Text;

public class BinarizeData
{
	static public bool FromByteArray(Type type, byte[] rawValue, int index, out int size, ref object data)
	{
		//Pack length info in the first 4 bytes
		if (type == typeof(byte[]))
		{
			size = BitConverter.ToInt32(rawValue, index);
			
			byte[] tempData = new byte[size];
			
			Array.Copy(rawValue, index + 4, tempData, 0, size);
			
			size += 4;
			
			data = (object)tempData;
			
			return true;
		}
		//Strings need to be handled seperately
		else if (type == typeof(string))
		{
			for (int i = index; i < rawValue.Length; i++)
			{
				if (rawValue[i] == 0)
				{
					//Found end of string​
					size = i - index + 1;
					
					data = (object)Encoding.ASCII.GetString(rawValue, index, i - index);
					
					return true;
				}
			}
			
			size = 0;
			
			return false;
		}
		else
		{
			if (type == typeof(int))
			{
				size = sizeof(int);
				data = BitConverter.ToInt32(rawValue, index);
			}
			else if (type == typeof(bool))
			{
				size = sizeof(bool);
				data = BitConverter.ToBoolean(rawValue, index);
			}
			else if (type == typeof(char))
			{
				size = sizeof(char);
				data = BitConverter.ToChar(rawValue, index);
			}
			else if (type == typeof(double))
			{
				size = sizeof(double);
				data = BitConverter.ToDouble(rawValue, index);
			}
			else if (type == typeof(float))
			{
				size = sizeof(float);
				data = BitConverter.ToSingle(rawValue, index);
			}
			else if (type == typeof(long))
			{
				size = sizeof(long);
				data = BitConverter.ToInt64(rawValue, index);
			}
			else if (type == typeof(short))
			{
				size = sizeof(short);
				data = BitConverter.ToInt16(rawValue, index);
			}
			else if (type == typeof(uint))
			{
				size = sizeof(uint);
				data = BitConverter.ToUInt32(rawValue, index);
			}
			else if (type == typeof(ulong))
			{
				size = sizeof(ulong);
				data = BitConverter.ToUInt64(rawValue, index);
			}
			else if (type == typeof(ushort))
			{
				size = sizeof(ushort);
				data = BitConverter.ToUInt16(rawValue, index);
			}
			else if (type == typeof(Vector3))
			{
				size = sizeof(float) * 3;
				data = new Vector3(BitConverter.ToSingle(rawValue, index), BitConverter.ToSingle(rawValue, index + sizeof(float)), BitConverter.ToSingle(rawValue, index + sizeof(float) * 2));
			}
			else
			{
				size = 0;
				Debug.Log("Cannot convert variable of type: " + type);
				return false;
			}
			
			return true;
		}
	}
	
	static public byte[] ToByteArray(object value, int maxLength)
	{
		//Pack length info in the first 4 bytes
		if (value.GetType() == typeof(byte[]))
		{
			int tempLength = ((byte[])value).Length;
			byte[] tempByteArray = new byte[tempLength + 4];
			
			byte[] lengthBytes = BitConverter.GetBytes((Int32)tempLength);

			Array.Copy(lengthBytes, 0, tempByteArray, 0, 4);
			Array.Copy((byte[])value, 0, tempByteArray, 4, tempLength);

			return tempByteArray;
		}
		//Strings need to be handled seperately
		else if (value.GetType() == typeof(string))
		{
			byte[] rawdata = Encoding.ASCII.GetBytes((string)value + "\0");

			return rawdata;
		}
		else
		{
			byte[] rawdata;
			if (value.GetType() == typeof(int))
			{
				rawdata = BitConverter.GetBytes((int)value);
			}
			else if (value.GetType() == typeof(bool))
			{
				rawdata = BitConverter.GetBytes((bool)value);
			}
			else if (value.GetType() == typeof(char))
			{
				rawdata = BitConverter.GetBytes((char)value);
			}
			else if (value.GetType() == typeof(double))
			{
				rawdata = BitConverter.GetBytes((double)value);
			}
			else if (value.GetType() == typeof(float))
			{
				rawdata = BitConverter.GetBytes((float)value);
			}
			else if (value.GetType() == typeof(long))
			{
				rawdata = BitConverter.GetBytes((long)value);
			}
			else if (value.GetType() == typeof(short))
			{
				rawdata = BitConverter.GetBytes((short)value);
			}
			else if (value.GetType() == typeof(uint))
			{
				rawdata = BitConverter.GetBytes((uint)value);
			}
			else if (value.GetType() == typeof(ulong))
			{
				rawdata = BitConverter.GetBytes((ulong)value);
			}
			else if (value.GetType() == typeof(ushort))
			{
				rawdata = BitConverter.GetBytes((ushort)value);
			}
			else if (value.GetType() == typeof(Vector3))
			{
				rawdata = new byte[sizeof(float) * 3];

				Array.Copy(BitConverter.GetBytes(((Vector3)value).x), 0, rawdata, 0, sizeof(float));
				Array.Copy(BitConverter.GetBytes(((Vector3)value).y), 0, rawdata, sizeof(float), sizeof(float));
				Array.Copy(BitConverter.GetBytes(((Vector3)value).z), 0, rawdata, sizeof(float) * 2, sizeof(float));
			}
			else
			{
				Debug.Log("Cannot convert variable of type: " + value.GetType());
				return null;
			}

			if (maxLength < rawdata.Length)
			{
				byte[] temp = new byte[maxLength];
				Array.Copy(rawdata, temp, maxLength);
				return temp;
			}
			else
			{
				return rawdata;
			}
		}
	}
}