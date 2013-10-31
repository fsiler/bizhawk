﻿using System;
using LuaInterface;
using BizHawk.Client.Common;

namespace BizHawk.MultiClient
{
	//TODO: this needs a major refactor, as well as MemoryLuaLibrary, and this shoudl inherit memorylua library and extend it
	public class MainMemoryLuaLibrary : LuaLibraryBase
	{
		public MainMemoryLuaLibrary(Lua lua)
			: base()
		{
			_lua = lua;
		}

		public override string Name { get { return "mainmemory"; } }
		public override string[] Functions
		{
			get
			{
				return new[]
				{
					"getname",
					"readbyte",
					"readbyterange",
					"readfloat",
					"writebyte",
					"writebyterange",
					"writefloat",

					"read_s8",
					"read_u8",
					"read_s16_le",
					"read_s24_le",
					"read_s32_le",
					"read_u16_le",
					"read_u24_le",
					"read_u32_le",
					"read_s16_be",
					"read_s24_be",
					"read_s32_be",
					"read_u16_be",
					"read_u24_be",
					"read_u32_be",
					"write_s8",
					"write_u8",
					"write_s16_le",
					"write_s24_le",
					"write_s32_le",
					"write_u16_le",
					"write_u24_le",
					"write_u32_le",
					"write_s16_be",
					"write_s24_be",
					"write_s32_be",
					"write_u16_be",
					"write_u24_be",
					"write_u32_be",
				};
			}
		}

		private Lua _lua;

		#region Main Memory Library Helpers

		private static int U2S(uint u, int size)
		{
			int s = (int)u;
			s <<= 8 * (4 - size);
			s >>= 8 * (4 - size);
			return s;
		}

		private int MM_R_S_LE(int addr, int size)
		{
			return U2S(MM_R_U_LE(addr, size), size);
		}

		private uint MM_R_U_LE(int addr, int size)
		{
			uint v = 0;
			for (int i = 0; i < size; ++i)
				v |= MM_R_U8(addr + i) << 8 * i;
			return v;
		}

		private int MM_R_S_BE(int addr, int size)
		{
			return U2S(MM_R_U_BE(addr, size), size);
		}

		private uint MM_R_U_BE(int addr, int size)
		{
			uint v = 0;
			for (int i = 0; i < size; ++i)
				v |= MM_R_U8(addr + i) << 8 * (size - 1 - i);
			return v;
		}

		private void MM_W_S_LE(int addr, int v, int size)
		{
			MM_W_U_LE(addr, (uint)v, size);
		}

		private void MM_W_U_LE(int addr, uint v, int size)
		{
			for (int i = 0; i < size; ++i)
				MM_W_U8(addr + i, (v >> (8 * i)) & 0xFF);
		}

		private void MM_W_S_BE(int addr, int v, int size)
		{
			MM_W_U_BE(addr, (uint)v, size);
		}

		private void MM_W_U_BE(int addr, uint v, int size)
		{
			for (int i = 0; i < size; ++i)
				MM_W_U8(addr + i, (v >> (8 * (size - 1 - i))) & 0xFF);
		}

		private uint MM_R_U8(int addr)
		{
			return Global.Emulator.MainMemory.PeekByte(addr);
		}

		private void MM_W_U8(int addr, uint v)
		{
			Global.Emulator.MainMemory.PokeByte(addr, (byte)v);
		}

		

		#endregion

		public string mainmemory_getname()
		{
			return Global.Emulator.MainMemory.Name;
		}

		public uint mainmemory_readbyte(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_U8(addr);
		}

		public LuaTable mainmemory_readbyterange(object address, object length)
		{
			int l = LuaCommon.LuaInt(length);
			int addr = LuaCommon.LuaInt(address);
			int last_addr = l + addr;
			LuaTable table = _lua.NewTable();
			for (int i = addr; i <= last_addr; i++)
			{
				string a = String.Format("{0:X2}", i);
				byte v = Global.Emulator.MainMemory.PeekByte(i);
				string vs = String.Format("{0:X2}", (int)v);
				table[a] = vs;
			}
			return table;
		}

		public float mainmemory_readfloat(object lua_addr, bool bigendian)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			uint val = Global.Emulator.MainMemory.PeekDWord(addr, bigendian ? Endian.Big : Endian.Little);

			byte[] bytes = BitConverter.GetBytes(val);
			float _float = BitConverter.ToSingle(bytes, 0);
			return _float;
		}

		public void mainmemory_writebyte(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			uint v = LuaCommon.LuaUInt(lua_v);
			MM_W_U8(addr, v);
		}

		public void mainmemory_writebyterange(LuaTable memoryblock)
		{
			foreach (var address in memoryblock.Keys)
			{
				int a = LuaCommon.LuaInt(address);
				int v = LuaCommon.LuaInt(memoryblock[address]);

				Global.Emulator.MainMemory.PokeByte(a, (byte)v);
			}
		}

		public void mainmemory_writefloat(object lua_addr, object lua_v, bool bigendian)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			float dv = (float)(double)lua_v;
			byte[] bytes = BitConverter.GetBytes(dv);
			uint v = BitConverter.ToUInt32(bytes, 0);
			Global.Emulator.MainMemory.PokeDWord(addr, v, bigendian ? Endian.Big : Endian.Little);
		}


		public int mainmemory_read_s8(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return (sbyte)MM_R_U8(addr);
		}

		public uint mainmemory_read_u8(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_U8(addr);
		}

		public int mainmemory_read_s16_le(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_S_LE(addr, 2);
		}

		public int mainmemory_read_s24_le(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_S_LE(addr, 3);
		}

		public int mainmemory_read_s32_le(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_S_LE(addr, 4);
		}

		public uint mainmemory_read_u16_le(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_U_LE(addr, 2);
		}

		public uint mainmemory_read_u24_le(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_U_LE(addr, 3);
		}

		public uint mainmemory_read_u32_le(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_U_LE(addr, 4);
		}

		public int mainmemory_read_s16_be(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_S_BE(addr, 2);
		}

		public int mainmemory_read_s24_be(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_S_BE(addr, 3);
		}

		public int mainmemory_read_s32_be(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_S_BE(addr, 4);
		}

		public uint mainmemory_read_u16_be(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_U_BE(addr, 2);
		}

		public uint mainmemory_read_u24_be(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_U_BE(addr, 3);
		}

		public uint mainmemory_read_u32_be(object lua_addr)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			return MM_R_U_BE(addr, 4);
		}

		public void mainmemory_write_s8(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			int v = LuaCommon.LuaInt(lua_v);
			MM_W_U8(addr, (uint)v);
		}

		public void mainmemory_write_u8(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			uint v = LuaCommon.LuaUInt(lua_v);
			MM_W_U8(addr, v);
		}

		public void mainmemory_write_s16_le(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			int v = LuaCommon.LuaInt(lua_v);
			MM_W_S_LE(addr, v, 2);
		}

		public void mainmemory_write_s24_le(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			int v = LuaCommon.LuaInt(lua_v);
			MM_W_S_LE(addr, v, 3);
		}

		public void mainmemory_write_s32_le(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			int v = LuaCommon.LuaInt(lua_v);
			MM_W_S_LE(addr, v, 4);
		}

		public void mainmemory_write_u16_le(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			uint v = LuaCommon.LuaUInt(lua_v);
			MM_W_U_LE(addr, v, 2);
		}

		public void mainmemory_write_u24_le(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			uint v = LuaCommon.LuaUInt(lua_v);
			MM_W_U_LE(addr, v, 3);
		}

		public void mainmemory_write_u32_le(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			uint v = LuaCommon.LuaUInt(lua_v);
			MM_W_U_LE(addr, v, 4);
		}

		public void mainmemory_write_s16_be(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			int v = LuaCommon.LuaInt(lua_v);
			MM_W_S_BE(addr, v, 2);
		}

		public void mainmemory_write_s24_be(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			int v = LuaCommon.LuaInt(lua_v);
			MM_W_S_BE(addr, v, 3);
		}

		public void mainmemory_write_s32_be(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			int v = LuaCommon.LuaInt(lua_v);
			MM_W_S_BE(addr, v, 4);
		}

		public void mainmemory_write_u16_be(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			uint v = LuaCommon.LuaUInt(lua_v);
			MM_W_U_BE(addr, v, 2);
		}

		public void mainmemory_write_u24_be(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			uint v = LuaCommon.LuaUInt(lua_v);
			MM_W_U_BE(addr, v, 3);
		}

		public void mainmemory_write_u32_be(object lua_addr, object lua_v)
		{
			int addr = LuaCommon.LuaInt(lua_addr);
			uint v = LuaCommon.LuaUInt(lua_v);
			MM_W_U_BE(addr, v, 4);
		}
	}
}
