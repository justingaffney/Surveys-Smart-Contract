using Neo.SmartContract.Framework.Services.Neo;

using Neo.SmartContract.Framework;

using Survey.Common;

namespace Survey.Contract
{
    public class MainContract : SmartContract
    {
        public static object Main(string operation, params object[] args)
        {
            // Management operations
            if (operation == Operations.CREATE)
            {
                return CreateSurvey((byte[]) args[0], (uint)args[1], (string) args[2]);
            }
            else if (operation == Operations.CLOSE)
            {
                return CloseSurvey();
            }
            else if (operation == Operations.DELETE)
            {
                return DeleteSurvey();
            }

            // Response operations
            else if (operation == Operations.RESPOND)
            {
                return RespondToSurvey();
            }

            // Querying operations
            else if (operation == Operations.GET_SURVEYS)
            {
                return GetSurveys();
            }
            else if (operation == Operations.GET_SURVEY_DETAILS)
            {
                return GetSurveyDetails();
            }
            else if (operation == Operations.GET_SURVEY_RESPONSES)
            {
                return GetSurveyResponses();
            }

            else
            {
                return 0;
            }
        }

        #region Survey management operations

        private static byte[] CreateSurvey(byte[] creator, uint blockDuration, string description)//, string[] questions)
        {
            // TODO Verify creator can create survey


            // Generate survey identifier
            var height = Blockchain.GetHeight();

            var endBlock = height + blockDuration;
            
            var surveyId = Sha1(Helper.Concat(Helper.Concat(creator, height.AsByteArray()), Helper.AsByteArray(description)));

            // Encode survey information
            var surveyInfoBytes = EncodeSurveyInfo(endBlock, description);//, questions);

            Storage.Put(Storage.CurrentContext, surveyId, surveyInfoBytes);

            return surveyId;
        }

        private static byte[] EncodeSurveyInfo(uint endBlock, string description)
        {
            // TODO Implement

            return new byte[] { };
        }

        private static int CloseSurvey()
        {
            // TODO Implement
            return 2;
        }

        private static int DeleteSurvey()
        {
            // TODO Implement
            return 4;
        }

        #endregion Survey management operations

        #region Survey response operations
        
        private static int RespondToSurvey()
        {
            // TODO Implement
            return 5;
        }

        #endregion Survey response operations

        #region Survey querying methods
        
        private static int GetSurveys()
        {
            // TODO 
            return 7;
        }

        private static int GetSurveyDetails()
        {
            // TODO Implement
            return 8;
        }

        private static int GetSurveyResponses()
        {
            // TODO Implement
            return 9;
        }

        #endregion Survey querying methods
    }
}