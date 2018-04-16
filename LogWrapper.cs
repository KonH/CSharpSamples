using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CSharpSamples {
	public static class LogWrapper {
		public interface ILogics {
			void DoSimpleWork();
			void DoWorkWithArg(string arg);
		}

		class RealWorker : ILogics {
			public void DoSimpleWork() {
				Console.WriteLine("Some work");
			}

			public void DoWorkWithArg(string arg) {
				Console.WriteLine("Some work with arg");
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
				typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object) });

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
			if ( parameters > 1 ) {
				throw new NotSupportedException();
			}
			gen.Emit(OpCodes.Ldarg_0); // this
			if ( parameters > 0 ) {
				gen.Emit(OpCodes.Ldstr, $"{method.Name}('{{0}}'): start"); // string literal
				gen.Emit(OpCodes.Ldarg_1); // arg 0
				gen.Emit(OpCodes.Call, stringFormatMethod);
			} else {
				gen.Emit(OpCodes.Ldstr, $"{method.Name}: start"); // string literal
			}
			gen.Emit(OpCodes.Call, consoleWriteLineMethod); // Console.WriteLine
			gen.Emit(OpCodes.Ldfld, instanceField); // _instance
			if ( parameters > 0 ) {
				gen.Emit(OpCodes.Ldarg_1); // arg 0
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
		}
	}
}
