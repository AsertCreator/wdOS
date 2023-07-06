using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Core
{
	public static class SystemCalls
	{
		public static Dictionary<ushort, SystemCallHandler> SystemCallHandlers = new();
		private static bool initialized;

		public static void Initialize()
		{
			if (!initialized)
			{
				// Self Terminate syscall
				SystemCallHandlers[0x1000] = delegate (ref SystemCallInfo x)
				{
					// todo: terminate
					return 0;
				};

				INTs.SetIntHandler(0x80, HandleSWI);
				initialized = true;
			}
		}

		private static void HandleSWI(ref INTs.IRQContext ctx)
		{
			SystemCallInfo info = new();

			info.FuncNumber = (ushort)ctx.EAX;
			info.RegisterEAX = ctx.EAX;
			info.RegisterEBX = ctx.EBX;
			info.RegisterECX = ctx.ECX;
			info.RegisterEDX = ctx.EDX;
			info.RegisterESI = ctx.ESI;
			info.RegisterEDI = ctx.EDI;
			info.RegisterESP = ctx.ESP;
			info.RegisterEBP = ctx.EBP;
			info.RegisterEFL = (uint)ctx.EFlags;
			info.RegisterEIP = ctx.EIP;

			if (!SystemCallHandlers.ContainsKey(info.FuncNumber))
			{
				GCImplementation.Free(info);

				// todo: terminate that awful process

				return;
			}

			int res = SystemCallHandlers[info.FuncNumber](ref info);

			if (res == 0)
			{
				ctx.EAX = info.RegisterEAX;
				ctx.EBX = info.RegisterEBX;
				ctx.ECX = info.RegisterECX;
				ctx.EDX = info.RegisterEDX;
				ctx.ESI = info.RegisterESI;
				ctx.EDI = info.RegisterEDI;
				ctx.ESP = info.RegisterESP;
				ctx.EBP = info.RegisterEBP;
				ctx.EFlags = (INTs.EFlagsEnum)info.RegisterEFL;
				ctx.EIP = info.RegisterEIP;

				GCImplementation.Free(info);
			}
			else
			{
				GCImplementation.Free(info);

				// todo: terminate that awful process

				return;
			}
		}
	}
	public class SystemCallInfo
	{
		public Process Executor;
		public ushort FuncNumber;
		public uint RegisterEAX;
		public uint RegisterEBX;
		public uint RegisterECX;
		public uint RegisterEDX;
		public uint RegisterESI;
		public uint RegisterEDI;
		public uint RegisterESP;
		public uint RegisterEBP;
		public uint RegisterEIP;
		public uint RegisterEFL;
	}
	public delegate int SystemCallHandler(ref SystemCallInfo info);
}
