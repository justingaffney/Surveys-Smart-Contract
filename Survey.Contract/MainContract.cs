using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;

using Survey.Common;

namespace Survey.Contract
{
    public class MainContract : SmartContract
    {
        private const string MetadataKey = "metadata";
        private const string RespondersKey = "responders";

        public static object Main(string operation, params object[] args)
        {
            // Management operations
            if (operation == Operations.CREATE)
            {
                return CreateSurvey((byte[]) args[0], (uint) args[1], (string) args[2], (string) args[3], (byte[]) args[4]);
            }
            else if (operation == Operations.CLOSE)
            {
                return CloseSurvey((byte[]) args[0], (byte[]) args[1]);
            }
            else if (operation == Operations.DELETE)
            {
                return DeleteSurvey((byte[]) args[0], (byte[])args[1]);
            }

            // Response operations
            else if (operation == Operations.RESPOND)
            {
                return RespondToSurvey((byte[]) args[0], (byte[])args[1], (byte[])args[2]);
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

        private static byte[] CreateSurvey(byte[] creator, uint blockDuration, string name, string description, byte[] survey)
        {
            var emptyBytes = new byte[0];

            // Validate parameters
            if (blockDuration < Constants.SurveyMinimumBlockDuration) return emptyBytes;

            if (survey == null || survey.Length == 0) return emptyBytes;


            // Verify creator chose to create survey
            if (!Runtime.CheckWitness(creator)) return new byte[0];


            // Generate survey identifier
            var height = Blockchain.GetHeight();
            
            var surveyId = GenerateSurveyId(creator, height, description);


            // Encode survey metadata
            var endBlock = height + blockDuration;

            var surveyMetadataBytes = EncodingHelper.EncodeSurveyMetadata(false, endBlock, description);


            // Put survey in contract storage
            Storage.Put(Storage.CurrentContext, surveyId, survey);


            // Put encoded survey metadata in contract storage
            var surveyMetadataKey = GetSurveyMetadataKey(surveyId);

            Storage.Put(Storage.CurrentContext, surveyMetadataKey, surveyMetadataBytes);


            // Survey successfully created
            return surveyId;
        }

        private static bool CloseSurvey(byte[] creator, byte[] surveyId)
        {
            // Validate survey identifier
            if (surveyId == null || surveyId.Length != Constants.SurveyIdLengthBytes) return false;


            // Verify creator chose to close survey
            if (!Runtime.CheckWitness(creator)) return false;


            // Check survey exists
            var surveyMetadataKey = GetSurveyMetadataKey(surveyId);

            var surveyMetadataBytes = Storage.Get(Storage.CurrentContext, surveyMetadataKey);

            if (surveyMetadataBytes == null || surveyMetadataBytes.Length == 0) return false;


            // Check if survey has already been closed
            var isClosed = EncodingHelper.GetSurveyIsClosed(surveyMetadataBytes);

            if (isClosed) return true;


            // Update survey metadata
            surveyMetadataBytes = EncodingHelper.SetSurveyIsClosed(surveyMetadataBytes);


            // Put updated survey metadata in contract storage
            Storage.Put(Storage.CurrentContext, surveyMetadataKey, surveyMetadataBytes);


            // Survey successfully closed
            return true;
        }

        private static bool DeleteSurvey(byte[] creator, byte[] surveyId)
        {
            // Validate survey identifier
            if (surveyId == null || surveyId.Length != Constants.SurveyIdLengthBytes) return false;


            // Verify creator chose to delete survey
            if (!Runtime.CheckWitness(creator)) return false;


            // Check the survey exists
            var surveyMetadataKey = GetSurveyMetadataKey(surveyId);

            var surveyMetadataBytes = Storage.Get(Storage.CurrentContext, surveyMetadataKey);

            if (surveyMetadataBytes == null || surveyMetadataBytes.Length == 0) return true;


            // Get list of responders
            var respondersKey = GetSurveyRespondersKey(surveyId);

            var responders = GetSurveyResponders(surveyId);


            // Delete survey responses
            int currentIndex = 0;
            int i = 0;

            while ((responders.Length - currentIndex) >= Constants.PublicKeyLengthBytes)
            {
                var responder = responders.Range(currentIndex, Constants.PublicKeyLengthBytes);
                
                var responseKey = GetSurveyResponseKey(surveyId, responder);

                // Delete response
                Storage.Delete(Storage.CurrentContext, responseKey);


                currentIndex += Constants.PublicKeyLengthBytes;
                i++;
            }
                        
            // Delete list of responders
            Storage.Delete(Storage.CurrentContext, respondersKey);

            // Delete survey
            Storage.Delete(Storage.CurrentContext, surveyId);

            // Delete survey metadata from contract storage
            Storage.Delete(Storage.CurrentContext, surveyMetadataKey);


            // Survey successfully deleted
            return true;
        }

        #endregion Survey management operations

        #region Survey response operations
        
        private static bool RespondToSurvey(byte[] responder, byte[] surveyId, byte[] response)
        {
            // Validate parameters
            if (surveyId == null || surveyId.Length != Constants.SurveyIdLengthBytes) return false;

            if (response == null || response.Length == 0) return false;


            // Check response input is valid
            if (response == null || response.Length == 0) return false;


            // Verify user chose to respond
            if (!Runtime.CheckWitness(responder)) return false;


            // Check if survey exists
            var surveyMetadataKey = GetSurveyMetadataKey(surveyId);

            var surveyMetadataBytes = Storage.Get(Storage.CurrentContext, surveyMetadataKey);

            if (surveyMetadataBytes == null || surveyMetadataBytes.Length == 0) return false;


            // Check if survey is closed
            var isClosed = EncodingHelper.GetSurveyIsClosed(surveyMetadataBytes);

            if (isClosed) return false;
            

            // Check if user has already responded to survey
            var responseKey = GetSurveyResponseKey(surveyId, responder);

            var retrievedResponse = Storage.Get(Storage.CurrentContext, responseKey);

            if (retrievedResponse != null && retrievedResponse.Length > 0) return false;


            // User has not responded to survey, store response in contract
            AddSurveyResponder(surveyId, responder);
            
            // TODO Encrypt response so only the creator can read them

            Storage.Put(Storage.CurrentContext, responseKey, response);

            
            // User successfully responded
            return true;
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
            // Validate survey identifier
            if (surveyId == null || surveyId.Length != Constants.SurveyIdLengthBytes) return new byte[0];


            // TODO Authenticate request

            var surveyMetadataKey = GetSurveyMetadataKey(surveyId);

            return Storage.Get(Storage.CurrentContext, surveyMetadataKey);
        }

        private static byte[] GetSurveyResponses(byte[] surveyId)
        {
            // Validate survey identifier
            if (surveyId == null || surveyId.Length != Constants.SurveyIdLengthBytes) return new byte[0];


            // TODO Authenticate request


            var responses = new byte[0];


            // Get list of responders
            var responders = GetSurveyResponders(surveyId);


            // Get survey responses
            int currentIndex = 0;
            int i = 0;

            while ((responders.Length - currentIndex) >= Constants.PublicKeyLengthBytes)
            {
                var responder = responders.Range(currentIndex, Constants.PublicKeyLengthBytes);

                var responseKey = GetSurveyResponseKey(surveyId, responder);

                // Get response
                var response = Storage.Get(Storage.CurrentContext, responseKey);

                // TODO Add to list of responses



                currentIndex += Constants.PublicKeyLengthBytes;
                i++;
            }

            return responses;
        }

        #endregion Survey querying methods

        #region Storage Methods

        private static void AddSurveyResponder(byte[] surveyId, byte[] responder)
        {
            var responders = GetSurveyResponders(surveyId);

            responders = responders.Concat(responder);
            
            SetSurveyResponders(surveyId, responders);
        }

        private static byte[] GetSurveyResponders(byte[] surveyId)
        {
            var respondersKey = GetSurveyRespondersKey(surveyId);

            var responders = Storage.Get(Storage.CurrentContext, respondersKey);

            if (responders == null) responders = new byte[0];

            return responders;
        }

        private static void SetSurveyResponders(byte[] surveyId, byte[] responders)
        {
            var respondersKey = GetSurveyRespondersKey(surveyId);

            Storage.Put(Storage.CurrentContext, respondersKey, responders);
        }

        #endregion Storage Methods

        #region Helper Methods

        private static byte[] GenerateSurveyId(byte[] creator, uint height, string description)
        {
            return Sha1(Helper.Concat(Helper.Concat(creator, height.AsByteArray()), description.AsByteArray()));
        }

        private static byte[] GetSurveyMetadataKey(byte[] surveyId)
        {
            return surveyId.Concat(MetadataKey.AsByteArray());
        }

        private static byte[] GetSurveyRespondersKey(byte[] surveyId)
        {
            return surveyId.Concat(RespondersKey.AsByteArray());
        }

        private static byte[] GetSurveyResponseKey(byte[] surveyId, byte[] responder)
        {
            return surveyId.Concat(responder);
        }

        #endregion Helper Methods
    }
}