using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

using Neo.Cryptography;
using Neo.IO;
using Neo.SmartContract;
using Neo.VM;

using Survey.Common;

namespace Survey.Contract.Tests
{
    public static class TestHelper
    {
        private const string SurveryContractFilePath = @"Survey.Contract.avm";

        internal static readonly byte[] TestSurveyCreatorKey = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
        internal static readonly byte[] TestSurveyResponderKey = new byte[] { 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

        #region Survey Management Helper Methods

        internal static byte[] CreateSurvey(byte[] creator, uint blockDuration, string description)
        {
            using (var engine = new ExecutionEngine(null, Crypto.Default))
            {
                // Load smart contract script into engine
                engine.LoadScript(SurveryContractFilePath);

                // Get arguments script
                var argumentsScript = GetArgumentsScript(Operations.CREATE, creator, blockDuration, description);

                // Load arguments script into engine
                engine.LoadScript(argumentsScript);

                // Start execution
                engine.Execute();

                // Get result
                var result = engine.EvaluationStack.Peek();

                // TODO Get survey identifier
                var surveyId = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };


                // Return survey identifier
                return surveyId;
            }
        }

        internal static bool CloseSurvey(byte[] creator, byte[] surveyId)
        {
            using (var engine = new ExecutionEngine(null, Crypto.Default))
            {
                // Load smart contract script into engine
                engine.LoadScript(SurveryContractFilePath);

                // Get arguments script
                var argumentsScript = GetArgumentsScript(Operations.CLOSE, creator, surveyId);

                // Load arguments script into engine
                engine.LoadScript(argumentsScript);

                // Start execution
                engine.Execute();

                // Get result
                var result = engine.EvaluationStack.Peek();

                // TODO Get whether survey was closed successfully
                var closedSuccessfully = false;


                return closedSuccessfully;
            }
        }

        internal static bool DeleteSurvey(byte[] creator, byte[] surveyId)
        {
            using (var engine = new ExecutionEngine(null, Crypto.Default))
            {
                // Load smart contract script into engine
                engine.LoadScript(SurveryContractFilePath);

                // Get arguments script
                var argumentsScript = GetArgumentsScript(Operations.DELETE, creator, surveyId);

                // Load arguments script into engine
                engine.LoadScript(argumentsScript);

                // Start execution
                engine.Execute();

                // Get result
                var result = engine.EvaluationStack.Peek();

                // TODO Get whether survey was closed successfully
                var closedSuccessfully = false;


                return closedSuccessfully;
            }
        }

        #endregion Survey Management Helper Methods

        #region Survey Response Helper Methods

        internal static bool RespondToSurvey(byte[] responder, byte[] surveyId)
        {
            using (var engine = new ExecutionEngine(null, Crypto.Default))
            {
                // Load smart contract script into engine
                engine.LoadScript(SurveryContractFilePath);

                // Get arguments script
                var argumentsScript = GetArgumentsScript(Operations.RESPOND, responder, surveyId);

                // Load arguments script into engine
                engine.LoadScript(argumentsScript);

                // Start execution
                engine.Execute();

                // Get result
                var result = engine.EvaluationStack.Peek();

                // TODO Get whether survey was closed successfully
                var respondedSuccessfully = false;


                return respondedSuccessfully;
            }
        }

        #endregion Survey Response Helper Methods

        #region Script Helper Methods

        private static void LoadScript(this ExecutionEngine engine, string compiledContractPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(compiledContractPath));

            var contractScriptBytes = File.ReadAllBytes(compiledContractPath);

            engine.LoadScript(contractScriptBytes);
        }

        private static byte[] GetArgumentsScript(string operationName, params object[] args)
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

        #endregion Script Helper Methods
    }
}