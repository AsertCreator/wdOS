using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Core
{
	public unsafe static class DynamicLinkerManager
	{
		public static T CallInUsermode<T>(void* addr)
		{
			var func = ((delegate* unmanaged[Cdecl]<T>)(addr));
			return func();
		}
		public static T Call<T>(void* addr)
		{
			return ((delegate* unmanaged[Cdecl]<T>)(addr))();
		}
	}
}
