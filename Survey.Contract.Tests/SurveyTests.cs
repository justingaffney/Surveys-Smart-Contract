using Xunit;

using Neo.VM;
using Neo.Cryptography;

using Survey.Common;

namespace Survey.Contract.Tests
{
    public class SurveyTests
    {
        private const string SurveryContractFilePath = @"Survey.Contract.avm";

        private static readonly byte[] TestSurveyCreatorKey = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };

        [Fact]
        [Trait("Survey", "Management")]
        public void CreateSurveyTest()
        {
            using (var engine = new ExecutionEngine(null, Crypto.Default))
            {
                // Load smart contract script into engine
                engine.LoadScript(SurveryContractFilePath);

                // Get arguments script
                var argumentsScript = TestHelper.GetArgumentsScript(Operations.CREATE,
                    TestSurveyCreatorKey, (uint) 10, "Test description");

                // Load arguments script into engine
                engine.LoadScript(argumentsScript);

                // Start execution
                engine.Execute();

                // Get result
                var result = engine.EvaluationStack.Peek();
            }
        }
    }
}