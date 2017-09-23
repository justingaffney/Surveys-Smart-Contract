using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

using Neo.IO;
using Neo.SmartContract;
using Neo.VM;

namespace Survey.Contract.Tests
{
    public static class TestHelper
    {
        public static void LoadScript(this ExecutionEngine engine, string compiledContractPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(compiledContractPath));

            var contractScriptBytes = File.ReadAllBytes(compiledContractPath);

            engine.LoadScript(contractScriptBytes);
        }

        public static byte[] GetArgumentsScript(string operationName, params object[] args)
        {
            Debug.Assert(operationName != null && args != null);

            using (var builder = new ScriptBuilder())
            {
                // Last argument is pushed first
                foreach (var arg in args.Reverse())
                {
                    EmitPush(builder, arg);
                }

                // Push operation name last as it is the first argument
                builder.EmitPush(operationName);

                return builder.ToArray();
            }
        }

        private static void EmitPush(ScriptBuilder builder, object arg)
        {
            if (arg is bool)
            {
                builder.EmitPush((bool)arg);
            }
            else if (arg is byte[])
            {
                builder.EmitPush((byte[])arg);
            }
            else if (arg is string)
            {
                builder.EmitPush((string)arg);
            }
            else if (arg is BigInteger)
            {
                builder.EmitPush((BigInteger)arg);
            }
            else if (arg is ContractParameter)
            {
                builder.EmitPush((ContractParameter)arg);
            }
            else if (arg is ISerializable)
            {
                builder.EmitPush((ISerializable)arg);
            }
            else
            {
                builder.EmitPush(arg);
            }
        }
    }
}