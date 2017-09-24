using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

using Survey.Common;

namespace Survey.Contract
{
    public class MainContract : SmartContract
    {
        private const string ResponsesKey = "responses";

        public static object Main(string operation, params object[] args)
        {
            // Management operations
            if (operation == Operations.CREATE)
            {
                return CreateSurvey((byte[]) args[0], (byte[]) args[1], (uint) args[2], (string) args[3]);
            }
            else if (operation == Operations.CLOSE)
            {
                return CloseSurvey((byte[]) args[0], (byte[])args[1], (byte[]) args[2]);
            }
            else if (operation == Operations.DELETE)
            {
                return DeleteSurvey((byte[]) args[0], (byte[])args[1], (byte[]) args[2]);
            }

            // Response operations
            else if (operation == Operations.RESPOND)
            {
                return RespondToSurvey((byte[]) args[0], (byte[])args[1], (byte[]) args[2]);
            }

            // Querying operations
            else if (operation == Operations.GET_SURVEYS)
            {
                return GetSurveys();
            }
            else if (operation == Operations.GET_SURVEY_METADATA)
            {
                return GetSurveyMetadata((byte[]) args[0]);
            }
            else if (operation == Operations.GET_SURVEY_RESPONSES)
            {
                return GetSurveyResponses((byte[]) args[0]);
            }

            else
            {
                return 0;
            }
        }

        #region Survey management operations

        private static byte[] CreateSurvey(byte[] creator, byte[] signature, uint blockDuration, string description)
        {
            // Verify creator chose to create survey
            if (!VerifySignature(signature, creator)) return new byte[0];


            // Generate survey identifier
            var height = Blockchain.GetHeight();
            
            var surveyId = Sha1(Helper.Concat(Helper.Concat(creator, EncodingHelper.AsByteArray(height)), Helper.AsByteArray(description)));


            var endBlock = height + blockDuration;
            
            // Encode survey metadata
            var surveyMetadataBytes = EncodingHelper.EncodeSurveyMetadata(false, endBlock, description);


            // Put encoded survey metadata in contract storage
            Storage.Put(Storage.CurrentContext, surveyId, surveyMetadataBytes);


            // Survey successfully created
            return surveyId;
        }

        private static bool CloseSurvey(byte[] creator, byte[] signature, byte[] surveyId)
        {
            // Verify creator chose to close survey
            if (!VerifySignature(signature, creator)) return false;
            

            // Check survey exists
            var surveyMetadataBytes = Storage.Get(Storage.CurrentContext, surveyId);

            if (surveyMetadataBytes == null || surveyMetadataBytes.Length == 0) return false;


            // Check if survey has already been closed
            var isClosed = EncodingHelper.GetSurveyIsClosed(surveyMetadataBytes);

            if (isClosed) return true;


            // Update survey metadata
            surveyMetadataBytes = EncodingHelper.SetSurveyIsClosed(surveyMetadataBytes);


            // Put updated survey metadata in contract storage
            Storage.Put(Storage.CurrentContext, surveyId, surveyMetadataBytes);


            // Survey successfully closed
            return true;
        }

        private static bool DeleteSurvey(byte[] creator, byte[] signature, byte[] surveyId)
        {
            // Verify creator chose to delete survey
            if (!VerifySignature(signature, creator)) return false;


            // TODO Should this operation check if the survey exists first?


            // Delete survey metadata from contract storage
            Storage.Delete(Storage.CurrentContext, surveyId);


            // Delete survey responses
            var responsesKey = GetSurveyResponsesKey(surveyId);

            Storage.Delete(Storage.CurrentContext, responsesKey);


            // Survey successfully deleted
            return true;
        }

        #endregion Survey management operations

        #region Survey response operations
        
        private static bool RespondToSurvey(byte[] responder, byte[] signature, byte[] surveyId)
        {
            // Verify responder chose to respond
            if (!VerifySignature(signature, responder)) return false;


            var responsesKey = GetSurveyResponsesKey(surveyId);


            // TODO Implement


            return false;
        }

        #endregion Survey response operations

        // TODO Should there be query operations in this smart contract?
        //      Aren't these expensive transactions for retrieving data?
        #region Survey querying methods
        
        private static byte[] GetSurveys()
        {
            // TODO Implement


            return new byte[0];
        }

        private static byte[] GetSurveyMetadata(byte[] surveyId)
        {
            // TODO Authenticate request


            return Storage.Get(Storage.CurrentContext, surveyId);
        }

        private static byte[] GetSurveyResponses(byte[] surveyId)
        {
            // TODO Authenticate request


            var responsesKey = GetSurveyResponsesKey(surveyId);
            

            return Storage.Get(Storage.CurrentContext, responsesKey);
        }

        #endregion Survey querying methods

        #region Helper Methods

        private static byte[] GetSurveyResponsesKey(byte[] surveyId)
        {
            return surveyId.Concat(ResponsesKey.AsByteArray());
        }

        #endregion Helper Methods
    }
}