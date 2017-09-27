using Xunit;

using Survey.Common;

namespace Survey.Contract.Tests
{
    public class SurveyResponseTests
    {
        [Fact(DisplayName = "Response_Successful")]
        [Trait("Contract", "Response")]
        public void RespondToSurveySuccessfullyTest()
        {
            // Create survey first
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, TestHelper.TestSurveyBlockDuration, TestHelper.TestSurveyName, TestHelper.TestSurveyDescription, TestHelper.TestSurveyContents);
            Assert.NotNull(surveyId);

            // Respond to survey
            var respondedSuccessfully = TestHelper.RespondToSurvey(TestHelper.TestSurveyResponderKey, surveyId, TestHelper.TestSurveyResponse);
            Assert.True(respondedSuccessfully);
        }

        [Fact(DisplayName = "Response_Unsuccessful_Incorrect_Survey_ID")]
        [Trait("Contract", "Response")]
        public void RespondToSurveyUnsuccessfullyIncorrectSurveyIdTest()
        {
            // Create survey first
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, TestHelper.TestSurveyBlockDuration, TestHelper.TestSurveyName, TestHelper.TestSurveyDescription, TestHelper.TestSurveyContents);
            Assert.NotNull(surveyId);

            // Respond to survey that does not exist
            var respondedSuccessfully = TestHelper.RespondToSurvey(TestHelper.TestSurveyResponderKey, new byte[Constants.SurveyIdLengthBytes], TestHelper.TestSurveyResponse);
            Assert.False(respondedSuccessfully);
        }

        [Fact(DisplayName = "Response_Unsuccessful_Empty_Survey_Response")]
        [Trait("Contract", "Response")]
        public void RespondToSurveyUnsuccessfullyEmptyResponseTest()
        {
            // Create survey first
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, TestHelper.TestSurveyBlockDuration, TestHelper.TestSurveyName, TestHelper.TestSurveyDescription, TestHelper.TestSurveyContents);
            Assert.NotNull(surveyId);

            // Respond to survey
            var respondedSuccessfully = TestHelper.RespondToSurvey(TestHelper.TestSurveyResponderKey, surveyId, new byte[0]);
            Assert.False(respondedSuccessfully);
        }
    }
}