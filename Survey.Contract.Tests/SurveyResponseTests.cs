using Xunit;

namespace Survey.Contract.Tests
{
    public class SurveyResponseTests
    {
        [Fact(DisplayName = "Response_Successful")]
        [Trait("Contract", "Response")]
        public void RespondToSurveySuccessfullyTest()
        {
            // Create survey first
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, 10, "Test description");
            Assert.NotNull(surveyId);

            // Respond to survey
            var respondedSuccessfully = TestHelper.RespondToSurvey(TestHelper.TestSurveyResponderKey, surveyId);
            Assert.True(respondedSuccessfully);
        }

        [Fact(DisplayName = "Response_Unsuccessful")]
        [Trait("Contract", "Response")]
        public void RespondToSurveyUnsuccessfullyTest()
        {
            // Create survey first
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, 10, "Test description");
            Assert.NotNull(surveyId);

            // Close survey
            // TODO Create unsuccessful parameters
            var respondedSuccessfully = TestHelper.RespondToSurvey(TestHelper.TestSurveyResponderKey, surveyId);
            Assert.False(respondedSuccessfully);
        }
    }
}