using Xunit;

namespace Survey.Contract.Tests
{
    public class SurveyManagementTests
    {
        #region Create Tests

        [Fact(DisplayName = "Create_Successful")]
        [Trait("Contract", "Management")]
        public void CreateSurveySuccessfullyTest()
        {
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, 10, "Test description");
            Assert.NotNull(surveyId);
        }

        [Fact(DisplayName = "Create_Unsuccessful",
            Skip = "TODO Create unsuccessful parameters")]
        [Trait("Contract", "Management")]
        public void CreateSurveyUnsuccessfullyTest()
        {
            // TODO Create unsuccessful parameters
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, 10, "Test description");
            Assert.NotNull(surveyId);
        }

        #endregion Create Tests

        #region Close Tests

        [Fact(DisplayName = "Close_Successful")]
        [Trait("Contract", "Management")]
        public void CloseSurveySuccessfullyTest()
        {
            // Create survey first
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, 10, "Test description");
            Assert.NotNull(surveyId);

            // Close survey
            var closeSuccessfully = TestHelper.CloseSurvey(TestHelper.TestSurveyCreatorKey, surveyId);
            Assert.True(closeSuccessfully);
        }

        [Fact(DisplayName = "Close_Unsuccessful",
            Skip = "TODO Create unsuccessful parameters")]
        [Trait("Contract", "Management")]
        public void CloseSurveyUnsuccessfullyTest()
        {
            // Create survey first
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, 10, "Test description");
            Assert.NotNull(surveyId);

            // Close survey
            // TODO Create unsuccessful parameters
            var closeSuccessfully = TestHelper.CloseSurvey(TestHelper.TestSurveyCreatorKey, surveyId);
            Assert.False(closeSuccessfully);
        }

        #endregion Close Tests

        #region Delete Tests

        [Fact(DisplayName = "Delete_Successful_Closing_First")]
        [Trait("Contract", "Management")]
        public void DeleteSurveySuccessfullyClosingFirstTest()
        {
            // Create survey first
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, 10, "Test description");
            Assert.NotNull(surveyId);

            // Close survey
            var closeSuccessfully = TestHelper.CloseSurvey(TestHelper.TestSurveyCreatorKey, surveyId);
            Assert.True(closeSuccessfully);

            // Delete survey
            var deletedSuccessfully = TestHelper.DeleteSurvey(TestHelper.TestSurveyCreatorKey, surveyId);
            Assert.True(deletedSuccessfully);
        }

        [Fact(DisplayName = "Delete_Unsuccessful_Closing_First",
            Skip = "TODO Create unsuccessful parameters")]
        [Trait("Contract", "Management")]
        public void DeleteSurveyUnsuccessfullyClosingFirstTest()
        {
            // Create survey first
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, 10, "Test description");
            Assert.NotNull(surveyId);

            // Close survey
            var closeSuccessfully = TestHelper.CloseSurvey(TestHelper.TestSurveyCreatorKey, surveyId);
            Assert.True(closeSuccessfully);

            // Delete survey
            // TODO Create unsuccessful parameters
            var deletedSuccessfully = TestHelper.DeleteSurvey(TestHelper.TestSurveyCreatorKey, surveyId);
            Assert.False(deletedSuccessfully);
        }

        [Fact(DisplayName = "Delete_Successful_Not_Closing_First")]
        [Trait("Contract", "Management")]
        public void DeleteSurveySuccessfullyNotClosingFirstTest()
        {
            // Create survey first
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, 10, "Test description");
            Assert.NotNull(surveyId);

            // Delete survey
            var deletedSuccessfully = TestHelper.DeleteSurvey(TestHelper.TestSurveyCreatorKey, surveyId);
            Assert.True(deletedSuccessfully);
        }

        [Fact(DisplayName = "Delete_Unsuccessful_Not_Closing_First",
            Skip = "TODO Create unsuccessful parameters")]
        [Trait("Contract", "Management")]
        public void DeleteSurveyUnsuccessfullyNotClosingFirstTest()
        {
            // Create survey first
            var surveyId = TestHelper.CreateSurvey(TestHelper.TestSurveyCreatorKey, 10, "Test description");
            Assert.NotNull(surveyId);

            // Delete survey
            // TODO Create unsuccessful parameters
            var deletedSuccessfully = TestHelper.DeleteSurvey(TestHelper.TestSurveyCreatorKey, surveyId);
            Assert.False(deletedSuccessfully);
        }

        #endregion Delete Tests
    }
}