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
        private const string SurveyContractFilePath = @"Survey.Contract.avm";

        // Test survey creator and responder public keys
        internal static readonly byte[] TestSurveyCreatorKey = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
        internal static readonly byte[] TestSurveyResponderKey = new byte[] { 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

        // Test survey attributes
        internal const uint TestSurveyBlockDuration = Constants.SurveyMinimumBlockDuration;
        internal const string TestSurveyName = "Test Survey";
        internal const string TestSurveyDescription = "Test Survey Description";

        // Encoded test survey
        internal static readonly byte[] TestSurveyContents = new byte[] { 43, 141, 124, 86, 123, 168, 22, 1, 56, 248, 211, 253, 4, 19, 104, 23 }; // TODO Implement survey contents encoding format

        // Encoded test response
        internal static readonly byte[] TestSurveyResponse = new byte[] { 12, 46, 104, 111, 244, 1, 96, 32, 54, 20, 3, 65, 76, 34, 29, 55, 34, 234, 44 }; // TODO Implement survey response encoding format



        #region Survey Management Helper Methods

        internal static byte[] CreateSurvey(byte[] creator, uint blockDuration, string name, string description, byte[] survey)
        {
            using (var engine = LoadContractScript())
            {
                // Get arguments script
                var argumentsScript = GetArgumentsScript(Operations.CREATE, creator, blockDuration, name, description, survey);

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
            using (var engine = LoadContractScript())
            {
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
            using (var engine = LoadContractScript())
            {
                // Get arguments script
                var argumentsScript = GetArgumentsScript(Operations.DELETE, creator, surveyId);

                // Load arguments script into engine
                engine.LoadScript(argumentsScript);

                // Start execution
                engine.Execute();

                // Get result
                var result = engine.EvaluationStack.Peek();

                // TODO Get whether survey was deleted successfully
                var deletedSuccessfully = false;


                return deletedSuccessfully;
            }
        }

        #endregion Survey Management Helper Methods

        #region Survey Response Helper Methods

        internal static bool RespondToSurvey(byte[] responder, byte[] surveyId, byte[] response)
        {
            using (var engine = LoadContractScript())
            {
                // Get arguments script
                var argumentsScript = GetArgumentsScript(Operations.RESPOND, responder, surveyId, response);

                // Load arguments script into engine
                engine.LoadScript(argumentsScript);

                // Start execution
                engine.Execute();

                // Get result
                var result = engine.EvaluationStack.Peek();

                // TODO Get whether survey response was successful
                var respondedSuccessfully = false;


                return respondedSuccessfully;
            }
        }

        #endregion Survey Response Helper Methods

        #region Script Helper Methods

        private static ExecutionEngine LoadContractScript()
        {
            var engine = new ExecutionEngine(null, Crypto.Default);

            // Load smart contract script into engine
            var contractScriptBytes = File.ReadAllBytes(SurveyContractFilePath);

            engine.LoadScript(contractScriptBytes);

            return engine;
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