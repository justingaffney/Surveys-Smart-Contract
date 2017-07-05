using AntShares.SmartContract.Framework;
using AntShares.SmartContract.Framework.Services.AntShares;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AntShares.SmartContract
{
    public class Poll : FunctionCode
    {
        private const string POLL_ID_LIST_KEY = "polls";

        #region Operation constants
        // Poll management operations
        private const string CREATE_OPERATION = "create";
        private const string EDIT_OPERATION = "edit";
        private const string CLOSE_OPERATION = "close";
        private const string DELETE_OPERATION = "delete";

        // Poll response operations
        private const string RESPOND_OPERATION = "respond";
        private const string UPDATE_RESPONSE_OPERATION = "update_response";

        // Poll query operations
        private const string GET_POLLS_OPERATION = "get_polls";
        private const string GET_POLL_RESPONSES_OPERATION = "get_poll_responses";
        #endregion Operation constants

        private const int MAX_QUESTION_LENGTH = 128;
        private const int MAX_POLL_RESPONSE_OPTIONS = 30; // TODO Should probably be a smaller value

        public static object Main(string operation, params object[] args)
        {
            switch (operation)
            {
                // Poll management operations
                case CREATE_OPERATION:
                    return CreatePoll((byte[]) args[0], (byte[]) args[1], (string) args[2], (string[]) args[3]);
                case CLOSE_OPERATION:
                    return ClosePoll((byte[]) args[0], (byte[]) args[1], (byte[]) args[2]);
                case EDIT_OPERATION:
                    return EditPoll((byte[]) args[0], (byte[]) args[1], (byte[]) args[2]);
                case DELETE_OPERATION:
                    return DeletePoll((byte[]) args[0], (byte[]) args[1], (byte[]) args[2]);

                // Poll response operations
                case RESPOND_OPERATION:
                    return RespondToPoll((byte[]) args[0], (byte[]) args[1], (byte[]) args[2], (byte) args[3]);
                case UPDATE_RESPONSE_OPERATION:
                    return UpdatePollResponse((byte[])args[0], (byte[])args[1], (byte[])args[2], (byte)args[3]);

                // Poll querying operations
                case GET_POLLS_OPERATION:
                    return GetPolls();
                case GET_POLL_RESPONSES_OPERATION:
                    return GetPollResponses((byte[]) args[0]);

                default:
                    return false;
            }
        }

        #region Poll management operations
        /// <summary>
        /// Create a new poll
        /// </summary>
        /// <param name="poller">Poll conductor's address</param>
        /// <param name="signature">Poller's signature</param>
        /// <param name="question">Poll question</param>
        /// <param name="options">Poll response options</param>
        /// <returns>Poll identifier if successfully created, otherwise error</returns>
        private static byte[] CreatePoll(byte[] poller, byte[] signature, string question, string[] options)
        {
            // TODO Return more specific error values

            if (!VerifySignature(poller, signature)) return null;

            // Validate question length
            if (question.Length > MAX_QUESTION_LENGTH) return null;

            // Validate number of response options
            if (options.Length > MAX_POLL_RESPONSE_OPTIONS) return null;


            // Poll is valid, get poll identifier and create poll

            // Hash question to create identifier
            var pollId = Sha256(Encoding.UTF8.GetBytes(question));

            // Ensure it is a unique identifier
            var uniqueIdFound = false;
            while (!uniqueIdFound)
            {
                var pollWithId = Storage.Get(Storage.CurrentContext, pollId);

                if (pollWithId != null)
                {
                    // Identifier is being used

                    // Try get a unique identifier by hashing hash
                    pollId = Sha256(pollId);
                }
                else
                {
                    uniqueIdFound = true;
                }
            }

            // Unique poll identifier generated, create poll

            // Assign identifiers to response options
            var optionsWithIds = new Dictionary<byte, string>();
            byte optionId = 0;
            foreach (var option in options)
            {
                optionsWithIds.Add(optionId, option);
                optionId++;
            }

            // Encode poll
            var poll = PollEncoder.Encode(true, poller, question, optionsWithIds);

            // Check encoding was successful
            if (poll == null) return null;

            // Store poll
            Storage.Put(Storage.CurrentContext, pollId, poll);

            // TODO Add to list of polls

            // Poll created, return poll identifier
            return pollId;
        }

        private static bool EditPoll(byte[] poller, byte[] signature, byte[] pollId)
        {
            // TODO Return more specific error values

            var poll = Storage.Get(Storage.CurrentContext, pollId);

            if (poll == null) return false;

            if (!VerifySignature(poller, signature)) return false;

            // TODO Implement


            return true;
        }

        private static bool ClosePoll(byte[] poller, byte[] signature, byte[] pollId)
        {
            // TODO Return more specific error values

            var poll = Storage.Get(Storage.CurrentContext, pollId);

            if (poll == null) return false;

            if (!VerifySignature(poller, signature)) return false;

            // TODO Implement


            return true;
        }

        private static bool DeletePoll(byte[] poller, byte[] signature, byte[] pollId)
        {
            // TODO Return more specific error values

            var poll = Storage.Get(Storage.CurrentContext, pollId);

            if (poll == null) return false;

            if (!VerifySignature(poller, signature)) return false;

            Storage.Delete(Storage.CurrentContext, pollId);

            // TODO Remove from list of polls

            return true;
        }
        #endregion Poll management operations

        #region Poll response operations
        private static bool RespondToPoll(byte[] responder, byte[] signature, byte[] pollId, byte response)
        {
            // TODO Implement


        }
                
        private static bool UpdatePollResponse(byte[] responder, byte[] signature, byte[] pollId, byte newResponse)
        {
            // TODO Implement


        }
        #endregion Poll response operations
        
        #region Poll querying methods
        private static byte[] GetPollResponses(byte[] pollId)
        {
            var poll = Storage.Get(Storage.CurrentContext, pollId);

            if (poll == null) return null;

            // TODO Get poll responses
            bool isOpen;
            byte[] poller;
            string question;
            Dictionary<byte, string> options;
            Dictionary<byte[], byte> responses;

            var decodedSuccessfully = PollEncoder.Decode(poll, out isOpen, out poller, out question, out options, out responses);

            if (!decodedSuccessfully) return null;

            // Count number of responses for each option
            var responseCounts = new Dictionary<byte, uint>();

            foreach (var response in responses)
            {
                if (!responseCounts.ContainsKey(response.Value))
                {
                    // Add first response for option
                    responseCounts.Add(response.Value, 1);
                }
                else
                {
                    // Increment response count for option
                    responseCounts[response.Value]++;
                }
            }

            // Encode response counts
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    // Write number of response counts
                    writer.Write(responseCounts.Count);

                    foreach (var responseCount in responseCounts)
                    {
                        // Write option identifier
                        writer.Write(responseCount.Key);

                        // Write response count
                        writer.Write(responseCount.Value);
                    }

                    // Encoded successfully
                    return stream.ToArray();
                }
            }
        }

        private static byte[] GetPolls()
        {
            var encodedPollIdList = Storage.Get(Storage.CurrentContext, POLL_ID_LIST_KEY);

            // TODO Should it return an empty list instead?
            if (encodedPollIdList == null) return null;

            // Decode to list of poll identifiers
            // TODO Implement

            // Build list of poll identifiers and their corresponding questions
            // TODO Implement


            // Successfully retrieved polls
            return polls;
        }
        #endregion Poll querying methods
    }
}