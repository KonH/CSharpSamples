using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CSharpSamples {
	public static class LogWrapper {
		public interface ILogics {
			void DoSimpleWork();
			// TODO
			//void DoWorkWithArg(string arg);
		}

		class RealWorker : ILogics {
			public void DoSimpleWork() {
				Console.WriteLine("Some work");
			}

			/*public void DoWork(string arg) {
				Console.WriteLine("Some work with arg");
			}*/
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
			var constGen = constBuilder.GetILGenerator();

			constGen.Emit(OpCodes.Ldarg_0); // this
			constGen.Emit(OpCodes.Ldarg_1); // instance
			constGen.Emit(OpCodes.Stfld, instanceField); // _instance = instance
			constGen.Emit(OpCodes.Ret);

			var consoleWriteLineMethod = 
				typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });

			var methods = originType.GetMethods();
			foreach ( var method in methods ) {
				var methodBuilder = typeBuilder.DefineMethod(
					method.Name,
					MethodAttributes.Public | MethodAttributes.Virtual,
					CallingConventions.HasThis,
					method.ReturnType,
					method.GetParameters().Select(pInfo => pInfo.ParameterType).ToArray()
				);
				var gen = methodBuilder.GetILGenerator();

				gen.Emit(OpCodes.Ldarg_0); // this
				gen.Emit(OpCodes.Ldstr, $"{method.Name}: start"); // string literal
				gen.Emit(OpCodes.Call, consoleWriteLineMethod); // Console.WriteLine
				gen.Emit(OpCodes.Ldfld, instanceField); // _instance
				gen.EmitCall(OpCodes.Callvirt, method, null); // call wrapped method
				gen.Emit(OpCodes.Ldstr, $"{method.Name}: end"); // string literal
				gen.Emit(OpCodes.Call, consoleWriteLineMethod); // Console.WriteLine
				gen.Emit(OpCodes.Ret);

				typeBuilder.DefineMethodOverride(methodBuilder, method);
			}
			var wrapperType = typeBuilder.CreateType();
			var wrapperInstance = Activator.CreateInstance(wrapperType, instance) as T;
			return wrapperInstance;
		}

		public static void Test() {
			var wrapper = CreateLogWrapper<ILogics>(new RealWorker());
			wrapper.DoSimpleWork();
		}
	}
}
