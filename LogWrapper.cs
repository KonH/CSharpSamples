using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CSharpSamples {
	public static class LogWrapper {
		public interface ILogics {
			void DoSimpleWork();
			void DoWorkWithArg(string arg);
			void DoWorkWithArgs(string arg0, string arg1, string arg2);
		}

		class RealWorker : ILogics {
			public void DoSimpleWork() {
				Console.WriteLine("Some work");
			}

			public void DoWorkWithArg(string arg) {
				Console.WriteLine("Some work with arg");
			}

			public void DoWorkWithArgs(string arg0, string arg1, string arg2) {
				Console.WriteLine("Some work with another args");
			}
		}

		// Very annoying boilerplate
		class LogWorker : ILogics {
			ILogics _worker;

			public LogWorker(ILogics worker) {
				_worker = worker;
			}

			public void DoSimpleWork() {
				Console.WriteLine("DoSimpleWork: start");
				_worker.DoSimpleWork();
				Console.WriteLine("DoSimpleWork: end");
			}

			public void DoWorkWithArg(string arg) {
				Console.WriteLine($"DoWorkWithArg('{arg}'): start");
				_worker.DoWorkWithArg(arg);
				Console.WriteLine("DoWorkWithArg: end");
			}

			public void DoWorkWithArgs(string arg0, string arg1, string arg2) {
				var args = new object[3];
				args[0] = arg0;
				args[1] = arg1;
				args[2] = arg2;
				var str = string.Format("{0}; {1}; {2}", args);
				Console.WriteLine($"DoWorkWithArgs('{str}'): start");
				_worker.DoWorkWithArgs(arg0, arg1, arg2);
				Console.WriteLine("DoWorkWithArgs: end");
			}
		}

		static T CreateLogWrapper<T>(T instance) where T : class {
			var originType = typeof(T);
			var wrapperName = $"{originType.Name}_Wrapper";
			var assemblyName = new AssemblyName() { Name = wrapperName };
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(wrapperName);
			var typeBuilder = 
				moduleBuilder.DefineType(
					wrapperName,
					TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class,
					typeof(object),
					new Type[] { originType }
				);

			var instanceField = typeBuilder.DefineField("_instance", typeof(T), FieldAttributes.Private);

			var constBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new Type[] { originType });
			CreateConstructor(constBuilder.GetILGenerator(), instanceField);

			var consoleWriteLineMethod = 
				typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });
			var stringFormatMethod =
				typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object[]) });

			var methods = originType.GetMethods();
			foreach ( var method in methods ) {
				var methodBuilder = typeBuilder.DefineMethod(
					method.Name,
					MethodAttributes.Public | MethodAttributes.Virtual,
					CallingConventions.HasThis,
					method.ReturnType,
					method.GetParameters().Select(pInfo => pInfo.ParameterType).ToArray()
				);
				CreateMethodWrapper(methodBuilder.GetILGenerator(), method, consoleWriteLineMethod, stringFormatMethod, instanceField);
				typeBuilder.DefineMethodOverride(methodBuilder, method);
			}
			var wrapperType = typeBuilder.CreateType();
			var wrapperInstance = Activator.CreateInstance(wrapperType, instance) as T;
			return wrapperInstance;
		}

		static void CreateConstructor(ILGenerator constGen, FieldInfo instanceField) {
			constGen.Emit(OpCodes.Ldarg_0); // this
			constGen.Emit(OpCodes.Ldarg_1); // instance
			constGen.Emit(OpCodes.Stfld, instanceField); // _instance = instance
			constGen.Emit(OpCodes.Ret);
		}

		static void CreateMethodWrapper(
			ILGenerator gen, MethodInfo method, MethodInfo consoleWriteLineMethod, MethodInfo stringFormatMethod, FieldBuilder instanceField
		) {
			var parameters = method.GetParameters().Length;
			LocalBuilder paramValues = null;
			if ( parameters > 0 ) {
				paramValues = gen.DeclareLocal(typeof(object[]));
			}
			gen.Emit(OpCodes.Ldarg_0); // this
			if ( parameters > 0 ) {
				var formatStr = "";
				for ( var i = 0; i < parameters; i++ ) {
					formatStr += $"'{{{i}}}'; ";
				}
				formatStr = formatStr.Substring(0, formatStr.Length - 2);

				gen.Emit(OpCodes.Ldc_I4_S, parameters); // make params array
				gen.Emit(OpCodes.Newarr, typeof(object));
				gen.Emit(OpCodes.Stloc, paramValues);

				// fill array
				for ( var i = 0; i < parameters; i++ ) {
					gen.Emit(OpCodes.Ldloc_0); // load array
					gen.Emit(OpCodes.Ldc_I4, i); // load array index
					gen.Emit(OpCodes.Ldarg, 1 + i); // load arg i
					gen.Emit(OpCodes.Stelem_Ref); // set value
				}

				gen.Emit(OpCodes.Ldstr, $"{method.Name}({formatStr}): start"); // string literal
				gen.Emit(OpCodes.Ldloc_0); // load params array
				gen.Emit(OpCodes.Call, stringFormatMethod);
			} else {
				gen.Emit(OpCodes.Ldstr, $"{method.Name}: start"); // string literal
			}
			gen.Emit(OpCodes.Call, consoleWriteLineMethod); // Console.WriteLine
			gen.Emit(OpCodes.Ldfld, instanceField); // _instance
			for ( var i = 0; i < parameters; i++ ) {
				gen.Emit(OpCodes.Ldarg, 1 + i); // arg i
			}
			gen.EmitCall(OpCodes.Callvirt, method, null); // call wrapped method
			gen.Emit(OpCodes.Ldstr, $"{method.Name}: end"); // string literal
			gen.Emit(OpCodes.Call, consoleWriteLineMethod); // Console.WriteLine
			gen.Emit(OpCodes.Ret);
		}

		public static void Test() {
			var wrapper = CreateLogWrapper<ILogics>(new RealWorker());
			wrapper.DoSimpleWork();
			wrapper.DoWorkWithArg("my arg");
			wrapper.DoWorkWithArgs("my arg 0", "my arg 1", "my arg 2");
		}
	}
}
